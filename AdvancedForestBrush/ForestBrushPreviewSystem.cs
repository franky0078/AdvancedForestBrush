using Colossal.Entities;
using Game;
using Game.Common;
using Game.Prefabs;
using Game.Rendering;
using Game.Simulation;
using Game.Tools;
using System.Collections.Generic;
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
        internal static bool DetailedPreview => ForestBrushState.PathForest
            ? Mod.Settings?.RoadPreview == RoadPreviewMode.Detailed
            : Mod.Settings?.ForestPreview == ForestPreviewMode.Detailed;
        internal static bool UsePatternPreview => ForestBrushState.PathForest
            ? (Mod.Settings?.RoadPreview ?? RoadPreviewMode.Normal) != RoadPreviewMode.Legacy
            : DetailedPreview;

        private const int PreviewTextureSize = 128;
        private const float HaloPixels = 12f;
        private const float InnerPixels =
            PreviewTextureSize - HaloPixels * 2f;
        private const float PreviewScale =
            PreviewTextureSize / InnerPixels;

        private EntityQuery m_SettingsQuery;
        private PrefabSystem m_PrefabSystem;
        private TerrainSystem m_TerrainSystem;
        private System.Action<ScriptableRenderContext, List<Camera>> m_NativeBrushRender;
        private bool m_OwnsBrushRenderCallback;
        private Mesh m_Mesh;
        private MaterialPropertyBlock m_Properties;
        private ForestPathNetworkSystem m_PathNetwork;
        private Texture2D m_PathTexture;
        private uint m_PathSignature;
        private uint m_PatternSignature;
        private bool m_PatternBuilt;
        private bool[] m_PatternInside;
        private int[] m_PatternRoadIndices;
        private const int RoadMaskSize = 64;
        private readonly float2[] m_RoadSamplePositions = new float2[RoadMaskSize * RoadMaskSize];
        private readonly float[] m_RoadDistances = new float[RoadMaskSize * RoadMaskSize];
        private readonly bool[] m_RoadSampleNeeded = new bool[RoadMaskSize * RoadMaskSize];
        private int m_RoadDistanceIndex;
        private ForestPathNetworkSystem.PatternSample[] m_PathPattern;
        private Color32[] m_PatternPixels;
        private uint m_PendingPathSignature;
        private uint m_PendingPathGeometry;
        private bool m_PathPending;
        private int m_PathPixelIndex;
        private float3 m_PendingPathCenter;
        private float m_PendingPathWidth, m_PendingPathLength, m_PendingPathRotation;
        private float m_PendingPathSine, m_PendingPathCosine;
        private readonly System.Diagnostics.Stopwatch m_PathPreviewTimer = new System.Diagnostics.Stopwatch();
        private const double PathPreviewBudgetMs = 1.25;
        private bool m_PathBuilt;
        private float3 m_PathCenter;
        private float m_PathRotation, m_PathWidth, m_PathLength;
        private Color32[] m_PathPixels = new Color32[PreviewTextureSize * PreviewTextureSize];
        private Texture2D m_RectangleTexture;
        private Texture2D m_PolygonTexture;
        private int m_BuiltPolygonVersion = -1;
        private int m_BrushTexture;
        private int m_BrushOpacity;
        private float m_PreviewOpacity = 0.8f;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_PathNetwork = World.GetOrCreateSystemManaged<ForestPathNetworkSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_SettingsQuery = GetEntityQuery(
                ComponentType.ReadOnly<OverlayConfigurationData>());
            m_BrushTexture = Shader.PropertyToID("_BrushTexture");
            m_BrushOpacity = Shader.PropertyToID("_BrushOpacity");
            BrushRenderSystem brushRenderer = World.GetOrCreateSystemManaged<BrushRenderSystem>();
            var renderMethod = typeof(BrushRenderSystem).GetMethod("Render",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, new[] { typeof(ScriptableRenderContext), typeof(List<Camera>) }, null);
            if (renderMethod == null)
            {
                Mod.Log?.Error("Native brush Render callback was not found; keeping the game renderer unchanged.");
                return;
            }
            m_NativeBrushRender = (System.Action<ScriptableRenderContext, List<Camera>>)
                System.Delegate.CreateDelegate(typeof(System.Action<ScriptableRenderContext, List<Camera>>),
                    brushRenderer, renderMethod);
            RenderPipelineManager.beginContextRendering -= m_NativeBrushRender;
            RenderPipelineManager.beginContextRendering += Render;
            m_OwnsBrushRenderCallback = true;
        }

        protected override void OnUpdate()
        {
            if (!m_OwnsBrushRenderCallback) return;
            bool customPreview =
                ForestBrushState.PanelVisible &&
                (UsePatternPreview || ForestBrushState.Shape != ForestBrushShape.Circle);

            if (!customPreview)
            {
                m_PathBuilt = false;
                m_PathPending = false;
                return;
            }

            if (UsePatternPreview && ForestBrushState.HasValidCursor)
            {
                if (ForestBrushState.PathForest) m_PathNetwork.EnsureCache(ForestBrushState.CursorPosition);
                UpdatePathTexture();
            }
            else
            {
                m_PathBuilt = false;
                m_PathPending = false;
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

        private void Render(
            ScriptableRenderContext context,
            List<Camera> cameras)
        {

            if (!ForestBrushState.PanelVisible ||
                (!UsePatternPreview && ForestBrushState.Shape == ForestBrushShape.Circle))
            {
                m_NativeBrushRender(context, cameras);
                return;
            }
            if (!ForestBrushState.HasValidCursor ||
                (!UsePatternPreview && ForestBrushState.Shape == ForestBrushShape.Circle) ||
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

            if (UsePatternPreview)
            {
                if (!m_PathBuilt) return;
                width = m_PathWidth;
                length = m_PathLength;
                rotation = m_PathRotation;
                texture = m_PathTexture;
            }
            else if (ForestBrushState.Shape == ForestBrushShape.Polygon)
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
            float3 cursor = UsePatternPreview ? m_PathCenter : ForestBrushState.CursorPosition;
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

        private void UpdatePathTexture()
        {
            if (ForestBrushState.Shape == ForestBrushShape.Polygon && !ForestBrushState.PolygonClosed)
            {
                m_PathBuilt = false;
                m_PathPending = false;
                return;
            }
            float width = ForestBrushState.Shape == ForestBrushShape.Circle
                ? ForestBrushState.CircleBrushRadius * 2f
                : ForestBrushState.Shape == ForestBrushShape.Polygon
                    ? ForestBrushState.PolygonHalfWidth * 2f : ForestBrushState.ShapeWidth;
            float length = ForestBrushState.Shape == ForestBrushShape.Rectangle
                ? ForestBrushState.ShapeLength
                : ForestBrushState.Shape == ForestBrushShape.Polygon
                    ? ForestBrushState.PolygonHalfLength * 2f : width;
            float rotation = ForestBrushState.Shape == ForestBrushShape.Circle ? 0f : ForestBrushState.RotationDegrees;
            float3 center = ForestBrushState.CursorPosition;
            // Moving or rotating the brush must not regenerate its noise/clearing pattern.
            uint settings = math.hash(new float3(width, length, (float)ForestBrushState.Shape));
            settings = math.hash(new uint3(settings,
                (uint)ForestBrushState.PolygonVersion, ForestBrushState.PathForest ? 1u : 0u));
            settings = math.hash(new uint2(settings, DetailedPreview ? 1u : 0u));
            if (DetailedPreview)
            {
                settings = math.hash(new uint4(settings, (uint)ForestBrushState.NoiseScale,
                    (uint)ForestBrushState.NoiseStrength, (uint)ForestBrushState.DensityPercent));
                settings = math.hash(new uint4(settings, (uint)ForestBrushState.NoiseMode,
                    (uint)ForestBrushState.Seed, (uint)ForestBrushState.PathSeed));
                settings = math.hash(new uint4(settings, (uint)ForestBrushState.PathTransition,
                    (uint)ForestBrushState.PathClearings, (uint)ForestBrushState.PathClearingSize));
                settings = math.hash(new uint2(settings, (uint)ForestBrushState.PathWidth));
            }
            if (!m_PatternBuilt || settings != m_PatternSignature)
            {
                BuildPattern(width, length);
                m_PatternSignature = settings;
                m_PatternBuilt = true;
                m_PathPending = false;
                m_PathBuilt = false;
            }

            if (!ForestBrushState.PathForest)
            {
                // The overlay material already projects onto terrain.
                if (!m_PathBuilt)
                {
                    m_PathTexture.SetPixels32(m_PatternPixels);
                    m_PathTexture.Apply(false, false);
                }
                m_PathCenter = center;
                m_PathRotation = rotation;
                m_PathWidth = width;
                m_PathLength = length;
                m_PathBuilt = true;
                m_PathPending = false;
                return;
            }

            uint geometry = math.hash(new uint3((uint)m_PathNetwork.Version,
                (uint)ForestBrushState.PathGap, (uint)ForestBrushState.PathWidth));
            // Do not combine rows evaluated against two different road snapshots.
            if (m_PathPending && geometry != m_PendingPathGeometry)
                m_PathPending = false;
            // Sub-pixel mouse jitter does not warrant another complete road mask.
            float movementStep = math.max(0.125f,
                math.min(width, length) * PreviewScale / RoadMaskSize * 0.25f);
            float2 cell = math.floor(center.xz / movementStep);
            uint signature = math.hash(new uint3(settings, math.asuint(cell.x), math.asuint(cell.y)));
            signature = math.hash(new uint3(signature, geometry, math.asuint(rotation)));
            if (!m_PathPending)
            {
                if (m_PathBuilt && signature == m_PathSignature) return;
                m_PendingPathCenter = center;
                m_PendingPathWidth = width;
                m_PendingPathLength = length;
                m_PendingPathRotation = rotation;
                m_PendingPathSine = math.sin(math.radians(rotation));
                m_PendingPathCosine = math.cos(math.radians(rotation));
                m_PendingPathSignature = signature;
                m_PendingPathGeometry = geometry;
                m_PathPixelIndex = 0;
                m_RoadDistanceIndex = 0;
                m_PathPending = true;
            }
            m_PathPreviewTimer.Restart();
            for (; m_RoadDistanceIndex < m_RoadDistances.Length; m_RoadDistanceIndex++)
            {
                if ((m_RoadDistanceIndex & 63) == 0 &&
                    m_PathPreviewTimer.Elapsed.TotalMilliseconds >= PathPreviewBudgetMs) break;
                if (!m_RoadSampleNeeded[m_RoadDistanceIndex])
                {
                    m_RoadDistances[m_RoadDistanceIndex] = 0f;
                    continue;
                }
                float2 local = m_RoadSamplePositions[m_RoadDistanceIndex];
                float2 world = m_PendingPathCenter.xz + new float2(
                    local.x * m_PendingPathCosine + local.y * m_PendingPathSine,
                    -local.x * m_PendingPathSine + local.y * m_PendingPathCosine);
                m_RoadDistances[m_RoadDistanceIndex] = m_PathNetwork.InwardDistance(world);
            }
            if (m_RoadDistanceIndex < m_RoadDistances.Length)
            {
                m_PathPreviewTimer.Stop();
                return;
            }
            float density = DetailedPreview ? math.min(1f, ForestBrushState.DensityPercent / 100f) : 1f;
            for (; m_PathPixelIndex < m_PathPixels.Length; m_PathPixelIndex++)
            {
                if ((m_PathPixelIndex & 63) == 0 &&
                    m_PathPreviewTimer.Elapsed.TotalMilliseconds >= PathPreviewBudgetMs) break;
                float chance = 0f;
                if (m_PatternInside[m_PathPixelIndex])
                {
                    // Nearest sampling preserves zero road exclusions instead of
                    // interpolating positive distances across a road or a gap.
                    float inward = m_RoadDistances[m_PatternRoadIndices[m_PathPixelIndex]];
                    chance = DetailedPreview ? m_PathPattern[m_PathPixelIndex].Probability(inward)
                        : inward > 0f ? 1f : 0f;
                }
                m_PathPixels[m_PathPixelIndex] = new Color32(255, 255, 255,
                    (byte)math.round(chance * density * 255f));
            }
            m_PathPreviewTimer.Stop();
            if (m_PathPixelIndex < m_PathPixels.Length) return;
            m_PathTexture.SetPixels32(m_PathPixels);
            m_PathTexture.Apply(false, false);
            m_PathCenter = m_PendingPathCenter;
            m_PathRotation = m_PendingPathRotation;
            m_PathWidth = m_PendingPathWidth;
            m_PathLength = m_PendingPathLength;
            m_PathSignature = m_PendingPathSignature;
            m_PathBuilt = true;
            m_PathPending = false;
        }

        private void BuildPattern(float width, float length)
        {
            int textureSize = DetailedPreview ? PreviewTextureSize : RoadMaskSize;
            int count = textureSize * textureSize;
            if (m_PathTexture != null && m_PathTexture.width != textureSize)
            {
                Object.Destroy(m_PathTexture);
                m_PathTexture = null;
            }
            if (m_PathTexture == null)
            {
                m_PathTexture = new Texture2D(textureSize, textureSize,
                    TextureFormat.RGBA32, false, true)
                {
                    name = "Advanced Forest Brush Path Forest",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }
            if (m_PatternPixels == null || m_PatternPixels.Length != count)
            {
                m_PatternInside = new bool[count];
                m_PatternRoadIndices = new int[count];
                m_PathPattern = new ForestPathNetworkSystem.PatternSample[count];
                m_PatternPixels = new Color32[count];
                m_PathPixels = new Color32[count];
            }

            for (int i = 0; i < m_RoadSamplePositions.Length; i++)
                m_RoadSamplePositions[i] = new float2(
                    ((i % RoadMaskSize + 0.5f) / RoadMaskSize - 0.5f) * width * PreviewScale,
                    ((i / RoadMaskSize + 0.5f) / RoadMaskSize - 0.5f) * length * PreviewScale);
            System.Array.Clear(m_RoadSampleNeeded, 0, m_RoadSampleNeeded.Length);

            float density = DetailedPreview ? math.min(1f, ForestBrushState.DensityPercent / 100f) : 1f;
            for (int i = 0; i < count; i++)
            {
                float2 local = new float2(((i % textureSize + 0.5f) / textureSize - 0.5f) * width * PreviewScale,
                    ((i / textureSize + 0.5f) / textureSize - 0.5f) * length * PreviewScale);
                bool inside = ForestBrushState.Shape == ForestBrushShape.Circle
                    ? math.lengthsq(local) <= width * width * 0.25f
                    : ForestBrushState.Shape == ForestBrushShape.Polygon
                        ? ForestBrushState.IsPointInsideLocalPolygon(local)
                        : math.abs(local.x) <= width * 0.5f && math.abs(local.y) <= length * 0.5f;
                m_PatternRoadIndices[i] = (i / textureSize * RoadMaskSize / textureSize) * RoadMaskSize +
                    (i % textureSize * RoadMaskSize / textureSize);
                m_PatternInside[i] = inside;
                if (inside) m_RoadSampleNeeded[m_PatternRoadIndices[i]] = true;
                float chance = 0f;
                if (inside)
                {
                    if (ForestBrushState.PathForest && DetailedPreview)
                        m_PathPattern[i] = ForestPathNetworkSystem.SamplePattern(local);
                    else if (!ForestBrushState.PathForest)
                        chance = ForestPlacementFilterSystem.NoiseProbability(local);
                }
                m_PatternPixels[i] = new Color32(255, 255, 255,
                    (byte)math.round(chance * density * 255f));
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
            if (m_OwnsBrushRenderCallback)
            {
                RenderPipelineManager.beginContextRendering -= Render;
                RenderPipelineManager.beginContextRendering += m_NativeBrushRender;
                m_OwnsBrushRenderCallback = false;
            }

            if (m_Mesh != null)
            {
                Object.Destroy(m_Mesh);
            }

            if (m_PolygonTexture != null)
            {
                Object.Destroy(m_PolygonTexture);
            }

            if (m_PathTexture != null) Object.Destroy(m_PathTexture);

            if (m_RectangleTexture != null)
            {
                Object.Destroy(m_RectangleTexture);
            }

            base.OnDestroy();
        }
    }
}
