using Colossal.Entities;
using Game;
using Game.Common;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace AdvancedForestBrush
{
    [UpdateAfter(typeof(ObjectToolSystem))]
    [UpdateAfter(typeof(ForestBrushShapeSystem))]
    public partial class ForestBrushPreviewSystem : GameSystemBase
    {
        private const int PolygonTextureSize = 256;

        private EntityQuery m_BrushQuery;
        private EntityQuery m_SettingsQuery;
        private PrefabSystem m_PrefabSystem;
        private TerrainSystem m_TerrainSystem;
        private readonly Dictionary<Entity, float> m_OriginalOpacities =
            new Dictionary<Entity, float>();
        private readonly HashSet<Entity> m_HiddenVanillaBrushes =
            new HashSet<Entity>();
        private Mesh m_Mesh;
        private MaterialPropertyBlock m_Properties;
        private Texture2D m_PolygonTexture;
        private int m_BuiltPolygonVersion = -1;
        private int m_BrushTexture;
        private int m_BrushOpacity;
        private float m_PreviewOpacity = 0.8f;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_BrushQuery = GetEntityQuery(
                ComponentType.ReadWrite<Brush>(),
                ComponentType.Exclude<Deleted>());
            m_SettingsQuery = GetEntityQuery(
                ComponentType.ReadOnly<OverlayConfigurationData>());
            m_BrushTexture = Shader.PropertyToID("_BrushTexture");
            m_BrushOpacity = Shader.PropertyToID("_BrushOpacity");
            RenderPipelineManager.beginContextRendering += Render;
        }

        protected override void OnUpdate()
        {
            bool customPreview =
                ForestBrushState.PanelVisible &&
                ForestBrushState.Shape != ForestBrushShape.Circle;

            if (!customPreview)
            {
                RestoreVanillaBrush();
                return;
            }

            using NativeArray<Entity> entities =
                m_BrushQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out Brush brush))
                {
                    continue;
                }

                if (brush.m_Opacity > 0.001f)
                {
                    m_OriginalOpacities[entity] = brush.m_Opacity;
                    m_PreviewOpacity = brush.m_Opacity;
                }

                if (brush.m_Opacity != 0f)
                {
                    brush.m_Opacity = 0f;
                    EntityManager.SetComponentData(entity, brush);
                }

                // BrushRenderSystem explicitly excludes Hidden entities. This
                // is more reliable than opacity alone because its render
                // callback can run before our own callback.
                if (!EntityManager.HasComponent<Hidden>(entity))
                {
                    EntityManager.AddComponent<Hidden>(entity);
                    m_HiddenVanillaBrushes.Add(entity);
                }
            }

            if (ForestBrushState.Shape == ForestBrushShape.Polygon &&
                ForestBrushState.PolygonClosed &&
                m_BuiltPolygonVersion != ForestBrushState.PolygonVersion)
            {
                BuildPolygonTexture();
            }
        }

        private void RestoreVanillaBrush()
        {
            foreach (Entity entity in m_HiddenVanillaBrushes)
            {
                if (EntityManager.Exists(entity) &&
                    EntityManager.HasComponent<Hidden>(entity))
                {
                    EntityManager.RemoveComponent<Hidden>(entity);
                }
            }

            m_HiddenVanillaBrushes.Clear();

            if (m_OriginalOpacities.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<Entity, float> pair in m_OriginalOpacities)
            {
                if (!EntityManager.Exists(pair.Key) ||
                    !EntityManager.HasComponent<Brush>(pair.Key))
                {
                    continue;
                }

                Brush brush = EntityManager.GetComponentData<Brush>(pair.Key);
                if (brush.m_Opacity == 0f)
                {
                    brush.m_Opacity = pair.Value;
                    EntityManager.SetComponentData(pair.Key, brush);
                }
            }

            m_OriginalOpacities.Clear();
        }

        private void Render(
            ScriptableRenderContext context,
            List<Camera> cameras)
        {
            if (!ForestBrushState.PanelVisible ||
                !ForestBrushState.HasValidCursor ||
                ForestBrushState.Shape == ForestBrushShape.Circle ||
                (ForestBrushState.Shape == ForestBrushShape.Polygon &&
                 !ForestBrushState.PolygonClosed) ||
                !m_PrefabSystem.TryGetSingletonPrefab<OverlayConfigurationPrefab>(
                    m_SettingsQuery,
                    out OverlayConfigurationPrefab overlayPrefab))
            {
                return;
            }

            float width;
            float length;
            float rotation;
            Texture texture;

            if (ForestBrushState.Shape == ForestBrushShape.Polygon)
            {
                width = ForestBrushState.PolygonHalfWidth * 2f;
                length = ForestBrushState.PolygonHalfLength * 2f;
                rotation = 0f;
                texture = m_PolygonTexture;
            }
            else
            {
                width = ForestBrushState.ShapeWidth;
                length = ForestBrushState.Shape == ForestBrushShape.Square
                    ? width
                    : ForestBrushState.ShapeLength;
                rotation = ForestBrushState.RotationDegrees;
                texture = Texture2D.whiteTexture;
            }

            if (texture == null || width <= 0f || length <= 0f)
            {
                return;
            }

            float bottom = m_TerrainSystem.heightScaleOffset.y - 50f;
            float height = m_TerrainSystem.heightScaleOffset.x + 100f;
            float3 cursor = ForestBrushState.CursorPosition;
            Matrix4x4 matrix = Matrix4x4.TRS(
                new Vector3(cursor.x, bottom, cursor.z),
                Quaternion.Euler(0f, rotation, 0f),
                new Vector3(width * 0.5f, height, length * 0.5f));

            MaterialPropertyBlock properties = GetProperties();
            properties.Clear();
            properties.SetTexture(m_BrushTexture, texture);
            properties.SetFloat(m_BrushOpacity, math.max(0.15f, m_PreviewOpacity));

            foreach (Camera camera in cameras)
            {
                if (camera.cameraType == CameraType.Game ||
                    camera.cameraType == CameraType.SceneView)
                {
                    Graphics.DrawMesh(
                        GetMesh(),
                        matrix,
                        overlayPrefab.m_ObjectBrushMaterial,
                        0,
                        camera,
                        0,
                        properties,
                        ShadowCastingMode.Off,
                        false);
                }
            }
        }

        private void BuildPolygonTexture()
        {
            if (m_PolygonTexture == null)
            {
                m_PolygonTexture = new Texture2D(
                    PolygonTextureSize,
                    PolygonTextureSize,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name = "Advanced Forest Brush Polygon",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }

            Color32[] pixels =
                new Color32[PolygonTextureSize * PolygonTextureSize];
            float halfWidth = math.max(0.5f, ForestBrushState.PolygonHalfWidth);
            float halfLength = math.max(0.5f, ForestBrushState.PolygonHalfLength);

            for (int y = 0; y < PolygonTextureSize; y++)
            {
                float localZ =
                    math.lerp(-halfLength, halfLength, (y + 0.5f) / PolygonTextureSize);

                for (int x = 0; x < PolygonTextureSize; x++)
                {
                    float localX =
                        math.lerp(-halfWidth, halfWidth, (x + 0.5f) / PolygonTextureSize);
                    bool inside = ForestBrushState.IsPointInsideLocalPolygon(
                        new float2(localX, localZ));
                    pixels[y * PolygonTextureSize + x] = inside
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(0, 0, 0, 0);
                }
            }

            m_PolygonTexture.SetPixels32(pixels);
            m_PolygonTexture.Apply(false, false);
            m_BuiltPolygonVersion = ForestBrushState.PolygonVersion;
        }

        private Mesh GetMesh()
        {
            if (m_Mesh != null)
            {
                return m_Mesh;
            }

            m_Mesh = new Mesh
            {
                name = "Advanced Forest Brush Preview",
                vertices = new[]
                {
                    new Vector3(-1f, 0f, -1f),
                    new Vector3(-1f, 0f, 1f),
                    new Vector3(1f, 0f, 1f),
                    new Vector3(1f, 0f, -1f),
                    new Vector3(-1f, 1f, -1f),
                    new Vector3(-1f, 1f, 1f),
                    new Vector3(1f, 1f, 1f),
                    new Vector3(1f, 1f, -1f)
                },
                triangles = new[]
                {
                    0, 1, 5, 5, 4, 0, 3, 7, 6, 6, 2, 3,
                    0, 3, 2, 2, 1, 0, 4, 5, 6, 6, 7, 4,
                    0, 4, 7, 7, 3, 0, 1, 2, 6, 6, 5, 1
                }
            };
            return m_Mesh;
        }

        private MaterialPropertyBlock GetProperties()
        {
            m_Properties ??= new MaterialPropertyBlock();
            return m_Properties;
        }

        protected override void OnDestroy()
        {
            RenderPipelineManager.beginContextRendering -= Render;
            RestoreVanillaBrush();

            if (m_Mesh != null)
            {
                Object.Destroy(m_Mesh);
            }

            if (m_PolygonTexture != null)
            {
                Object.Destroy(m_PolygonTexture);
            }

            base.OnDestroy();
        }
    }
}
