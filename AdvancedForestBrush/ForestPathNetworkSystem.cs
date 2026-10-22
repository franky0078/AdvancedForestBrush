using System.Collections.Generic;
using Colossal.Entities;
using Colossal.Mathematics;
using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AdvancedForestBrush
{
    // A local, spatially indexed snapshot shared by the preview and placement filter.
    // No network scans or ECS access take place in the per-candidate distance test.
    public partial class ForestPathNetworkSystem : GameSystemBase
    {
        private const float CellSize = 64f;
        private const float CurveTolerance = 0.15f;
        private const float CachePadding = 128f;
        private const float CacheMovement = 96f;
        private struct Segment
        {
            public float2 A, B, Delta;
            public float HalfWidth, InverseLengthSquared;
        }
        private EntityQuery m_Networks;
        private readonly List<Segment> m_Segments = new List<Segment>();
        private readonly Dictionary<int2, List<int>> m_Cells = new Dictionary<int2, List<int>>();
        private float2 m_Center;
        private float m_Radius;
        private float m_NextScan;
        private int m_Gap, m_Width;
        private uint m_Signature;
        private bool m_Valid;
        public int Version { get; private set; }
        public int PathCount { get; private set; }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Networks = GetEntityQuery(
                ComponentType.ReadOnly<Edge>(),
                ComponentType.ReadOnly<Curve>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Overridden>(),
                ComponentType.Exclude<Temp>());
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible || !ForestBrushState.PathForest)
            {
                if (m_Valid)
                {
                    m_Valid = false;
                    m_Cells.Clear();
                    m_Segments.Clear();
                    PathCount = 0;
                    Version++;
                }
            }
        }

        public void EnsureCache(float3 center)
        {
            float radius = ForestBrushState.Shape == ForestBrushShape.Circle
                ? ForestBrushState.CircleBrushSize * 0.5f
                : ForestBrushState.Shape == ForestBrushShape.Polygon
                    ? ForestBrushState.PolygonBoundingRadius
                    : math.length(new float2(ForestBrushState.ShapeWidth,
                        ForestBrushState.Shape == ForestBrushShape.Square
                            ? ForestBrushState.ShapeWidth : ForestBrushState.ShapeLength)) * 0.5f;
            float2 xz = center.xz;
            bool moved = !m_Valid || math.distance(m_Center, xz) > CacheMovement ||
                math.abs(radius - m_Radius) > 0.1f ||
                m_Gap != ForestBrushState.PathGap || m_Width != ForestBrushState.PathWidth;
            if (!moved && UnityEngine.Time.unscaledTime < m_NextScan) return;
            m_NextScan = UnityEngine.Time.unscaledTime + 1f;

            // Padding also covers movement between refreshes and roads just outside the brush.
            float2 scanCenter = moved ? xz : m_Center;
            float extent = radius + ForestBrushState.PathGap + ForestBrushState.PathWidth + CachePadding;
            var curves = new List<Bezier4x3>();
            var widths = new List<float>();
            uint signature = 2166136261u;
            using NativeArray<Entity> entities = m_Networks.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                Entity prefab = EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;
                if ((!EntityManager.HasComponent<RoadData>(prefab) &&
                     !EntityManager.HasComponent<PathwayData>(prefab)) ||
                    !EntityManager.TryGetComponent(prefab, out NetGeometryData geometry)) continue;

                // Bridges and tunnels must not seed a forest on the ground below/above them.
                if (EntityManager.TryGetComponent(entity, out Game.Net.Elevation elevation) &&
                    math.cmax(math.abs(elevation.m_Elevation)) > 2f) continue;

                Bezier4x3 curve = EntityManager.GetComponentData<Curve>(entity).m_Bezier;
                float width = geometry.m_DefaultWidth;
                if (EntityManager.TryGetComponent(entity, out Composition composition) &&
                    EntityManager.TryGetComponent(composition.m_Edge, out NetCompositionData data))
                    width = math.max(width, data.m_Width);
                if (!math.isfinite(width) || width <= 0f ||
                    !math.all(math.isfinite(curve.a)) || !math.all(math.isfinite(curve.b)) ||
                    !math.all(math.isfinite(curve.c)) || !math.all(math.isfinite(curve.d))) continue;
                float2 min = math.min(math.min(curve.a.xz, curve.b.xz), math.min(curve.c.xz, curve.d.xz));
                float2 max = math.max(math.max(curve.a.xz, curve.b.xz), math.max(curve.c.xz, curve.d.xz));
                if (math.any(min > scanCenter + extent + width * 0.5f) ||
                    math.any(max < scanCenter - extent - width * 0.5f)) continue;

                curves.Add(curve);
                widths.Add(width * 0.5f + CurveTolerance);
                signature = math.hash(new uint4(signature, math.hash(curve.a), math.hash(curve.b), math.hash(curve.c)));
                signature = math.hash(new uint4(signature, math.hash(curve.d), math.asuint(width), (uint)entity.Index));
            }
            if (!moved && signature == m_Signature) return;
            m_Center = scanCenter;
            m_Radius = radius;
            m_Gap = ForestBrushState.PathGap;
            m_Width = ForestBrushState.PathWidth;
            m_Signature = signature;
            m_Cells.Clear();
            m_Segments.Clear();
            PathCount = curves.Count;
            for (int i = 0; i < curves.Count; i++)
            {
                Bezier4x3 curve = curves[i];
                Flatten(curve.a.xz, curve.b.xz, curve.c.xz, curve.d.xz, widths[i], 0);
            }
            m_Valid = true;
            Version++;
            Mod.LogDiagnosticInfo($"Path forest: {PathCount} nearby roads/paths, {m_Segments.Count} curve segments.");
        }

        private void Flatten(float2 a, float2 b, float2 c, float2 d, float halfWidth, int depth)
        {
            if (depth >= 12 || (math.distancesq(a, d) <= 32f * 32f &&
                Distance(b, a, d) <= CurveTolerance && Distance(c, a, d) <= CurveTolerance))
            {
                AddSegment(a, d, halfWidth);
                return;
            }
            float2 ab = (a + b) * 0.5f, bc = (b + c) * 0.5f, cd = (c + d) * 0.5f;
            float2 abc = (ab + bc) * 0.5f, bcd = (bc + cd) * 0.5f;
            float2 mid = (abc + bcd) * 0.5f;
            Flatten(a, ab, abc, mid, halfWidth, depth + 1);
            Flatten(mid, bcd, cd, d, halfWidth, depth + 1);
        }

        private void AddSegment(float2 a, float2 b, float halfWidth)
        {
            float reach = halfWidth + m_Gap + m_Width;
            float2 min = math.min(a, b) - reach, max = math.max(a, b) + reach;
            // Only index cells that can be reached by this brush before the next rebuild.
            min = math.max(min, m_Center - m_Radius - CachePadding);
            max = math.min(max, m_Center + m_Radius + CachePadding);
            if (math.any(min > max)) return;
            int index = m_Segments.Count;
            float2 delta = b - a;
            m_Segments.Add(new Segment { A = a, B = b, Delta = delta, HalfWidth = halfWidth,
                InverseLengthSquared = 1f / math.max(0.000001f, math.lengthsq(delta)) });
            int2 lo = Cell(min), hi = Cell(max);
            for (int y = lo.y; y <= hi.y; y++)
            for (int x = lo.x; x <= hi.x; x++)
            {
                var key = new int2(x, y);
                if (!m_Cells.TryGetValue(key, out List<int> list))
                {
                    list = new List<int>();
                    m_Cells.Add(key, list);
                }
                list.Add(index);
            }
        }

        private static int2 Cell(float2 p) => (int2)math.floor(p / CellSize);
        private static float Distance(float2 p, float2 a, float2 b)
        {
            float2 delta = b - a;
            float t = math.saturate(math.dot(p - a, delta) / math.max(0.000001f, math.lengthsq(delta)));
            return math.distance(p, a + delta * t);
        }

        public float Probability(float2 point, bool detailed = true)
        {
            if (!m_Valid || !m_Cells.TryGetValue(Cell(point), out List<int> candidates)) return 0f;
            float distance = float.MaxValue;
            foreach (int index in candidates)
            {
                Segment segment = m_Segments[index];
                float t = math.saturate(math.dot(point - segment.A, segment.Delta) * segment.InverseLengthSquared);
                distance = math.min(distance, math.distance(point, segment.A + segment.Delta * t) - segment.HalfWidth);
                if (distance <= ForestBrushState.PathGap) return 0f;
            }
            float inward = distance - ForestBrushState.PathGap;
            if (inward <= 0f || inward >= ForestBrushState.PathWidth) return 0f;

            if (!detailed) return 1f;

            uint seed = math.hash(new uint2((uint)ForestBrushState.PathSeed, 0x9e3779b9u));
            float2 offset = new float2(seed & 0xffffu, seed >> 16) * 0.0317f;
            float2 p = point / math.max(10f, ForestBrushState.NoiseScale);
            float broad = noise.snoise(p + offset) * 0.5f + 0.5f;
            float detail = noise.snoise(p * 3.17f - offset) * 0.5f + 0.5f;
            float irregularity = ForestBrushState.NoiseStrength / 100f;
            float edge = ForestBrushState.PathWidth * (1f - broad * 0.25f * irregularity);
            float transition = math.min(ForestBrushState.PathTransition, edge * 0.45f);
            float chance = math.smoothstep(0f, transition, inward) *
                (1f - math.smoothstep(edge - transition, edge, inward));
            chance *= math.lerp(1f, 0.3f + broad * 0.5f + detail * 0.2f, irregularity);
            if (ForestBrushState.PathClearings > 0)
            {
                float clearing = noise.snoise(point / ForestBrushState.PathClearingSize +
                    offset + new float2(71.3f, -23.7f)) * 0.5f + 0.5f;
                float threshold = 0.12f + ForestBrushState.PathClearings / 100f * 0.75f;
                chance *= math.smoothstep(threshold, threshold + 0.12f, clearing);
            }
            return math.saturate(chance);
        }

        public bool Keep(float3 position)
        {
            // One random test for the combined probability, rather than intersecting
            // two coordinate-based random masks at low density.
            float density = math.saturate(ForestBrushState.DensityPercent / 100f);
            float chance = Probability(position.xz) * density;
            return ForestBrushState.PlacementRandom(position, (uint)ForestBrushState.PathSeed) < chance;
        }
    }
}
