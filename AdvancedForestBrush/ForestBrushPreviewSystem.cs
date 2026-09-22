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
        private const int PreviewTextureSize = 128;
        private const float HaloPixels = 12f;
        private const float InnerPixels =
            PreviewTextureSize - HaloPixels * 2f;
        private const float PreviewScale =
            PreviewTextureSize / InnerPixels;

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
        private Texture2D m_RectangleTexture;
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

                // BrushRenderSystem explicitly excludes Hidden entities
                if (!EntityManager.HasComponent<Hidden>(entity))
                {
                    EntityManager.AddComponent<Hidden>(entity);
                    m_HiddenVanillaBrushes.Add(entity);
                }
            }

            if ((ForestBrushState.Shape == ForestBrushShape.Square ||
                 ForestBrushState.Shape == ForestBrushShape.Rectangle) &&
                m_RectangleTexture == null)
            {
                BuildRectangleTexture();
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
                rotation = ForestBrushState.RotationDegrees;
                texture = m_PolygonTexture;
            }
            else
            {
                width = ForestBrushState.ShapeWidth;
                length = ForestBrushState.Shape == ForestBrushShape.Square
                    ? width
                    : ForestBrushState.ShapeLength;
                rotation = ForestBrushState.RotationDegrees;
                texture = m_RectangleTexture;
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
                new Vector3(
                    width * PreviewScale * 0.5f,
                    height,
                    length * PreviewScale * 0.5f));

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
                    PreviewTextureSize,
                    PreviewTextureSize,
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
                new Color32[PreviewTextureSize * PreviewTextureSize];
            float halfWidth = math.max(0.5f, ForestBrushState.PolygonHalfWidth);
            float halfLength = math.max(0.5f, ForestBrushState.PolygonHalfLength);
            float textureHalfWidth = halfWidth * PreviewScale;
            float textureHalfLength = halfLength * PreviewScale;
            float haloWidth = math.max(
                0.25f,
                math.min(halfWidth, halfLength) *
                (HaloPixels * 2f / InnerPixels));

            for (int y = 0; y < PreviewTextureSize; y++)
            {
                float localZ =
                    math.lerp(
                        -textureHalfLength,
                        textureHalfLength,
                        (y + 0.5f) / PreviewTextureSize);

                for (int x = 0; x < PreviewTextureSize; x++)
                {
                    float localX =
                        math.lerp(
                            -textureHalfWidth,
                            textureHalfWidth,
                            (x + 0.5f) / PreviewTextureSize);
                    float2 point = new float2(localX, localZ);
                    bool inside = ForestBrushState.IsPointInsideLocalPolygon(
                        point);
                    pixels[y * PreviewTextureSize + x] = GetPreviewPixel(
                        inside,
                        DistanceToPolygonEdge(point),
                        haloWidth);
                }
            }

            m_PolygonTexture.SetPixels32(pixels);
            m_PolygonTexture.Apply(false, false);
            m_BuiltPolygonVersion = ForestBrushState.PolygonVersion;
        }

        private void BuildRectangleTexture()
        {
            m_RectangleTexture = new Texture2D(
                PreviewTextureSize,
                PreviewTextureSize,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = "Advanced Forest Brush Rectangle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color32[] pixels =
                new Color32[PreviewTextureSize * PreviewTextureSize];

            for (int y = 0; y < PreviewTextureSize; y++)
            {
                for (int x = 0; x < PreviewTextureSize; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    bool inside =
                        px >= HaloPixels &&
                        px <= PreviewTextureSize - HaloPixels &&
                        py >= HaloPixels &&
                        py <= PreviewTextureSize - HaloPixels;
                    float2 outside = new float2(
                        math.max(
                            math.max(HaloPixels - px, 0f),
                            px - (PreviewTextureSize - HaloPixels)),
                        math.max(
                            math.max(HaloPixels - py, 0f),
                            py - (PreviewTextureSize - HaloPixels)));
                    pixels[y * PreviewTextureSize + x] = GetPreviewPixel(
                        inside,
                        math.length(outside),
                        HaloPixels);
                }
            }

            m_RectangleTexture.SetPixels32(pixels);
            m_RectangleTexture.Apply(false, true);
        }

        private static Color32 GetPreviewPixel(
            bool inside,
            float distanceToEdge,
            float haloWidth)
        {
            if (inside)
            {
                return new Color32(255, 255, 255, 255);
            }

            float opacity = 1f - math.smoothstep(
                0f,
                haloWidth,
                math.max(0f, distanceToEdge));
            byte alpha = (byte)math.round(math.saturate(opacity) * 255f);
            return new Color32(255, 255, 255, alpha);
        }

        private static float DistanceToPolygonEdge(float2 point)
        {
            float minimum = float.MaxValue;
            int count = ForestBrushState.PolygonLocalPoints.Count;

            for (int i = 0; i < count; i++)
            {
                float2 start = ForestBrushState.PolygonLocalPoints[i];
                float2 end = ForestBrushState.PolygonLocalPoints[(i + 1) % count];
                float2 segment = end - start;
                float lengthSquared = math.lengthsq(segment);
                float amount = lengthSquared > 0.0001f
                    ? math.saturate(math.dot(point - start, segment) / lengthSquared)
                    : 0f;
                minimum = math.min(
                    minimum,
                    math.distance(point, start + segment * amount));
            }

            return minimum;
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

            if (m_RectangleTexture != null)
            {
                Object.Destroy(m_RectangleTexture);
            }

            base.OnDestroy();
        }
    }
}
