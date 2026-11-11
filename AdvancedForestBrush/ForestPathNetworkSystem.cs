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
    public partial class ForestPathNetworkSystem : GameSystemBase
    {
        private const float CurveTolerance = 0.15f;
        private const float CachePadding = 128f;
        private const float CacheMovement = 96f;
        // Pre-filter margin for the cheap bounding-box test; no road is wider than this.
        private const float MaximumRoadHalfWidth = 100f;
        private const int MaximumGridSize = 256;

        // Plain floats: Mono does not inline float2 operators in the hot loop.
        private struct Segment
        {
            public float Ax, Ay, Dx, Dy;
            public float HalfWidth, InverseLengthSquared;
        }

        private EntityQuery m_Networks;
        private readonly List<Bezier4x3> m_Curves = new List<Bezier4x3>();
        private readonly List<float> m_HalfWidths = new List<float>();
        private Segment[] m_Segments = new Segment[256];
        private int m_SegmentCount;
        private int[] m_CellStart = new int[1];
        private int[] m_CellItems = new int[256];
        private float2 m_GridMin;
        private float m_CellSize = 16f, m_InverseCellSize = 1f / 16f;
        private int m_GridSize;
        private float m_QueryExtent;
        private float2 m_Center;
        private float m_Radius;
        private float m_NextScan;
        private int m_Gap, m_Width;
        private uint m_Signature;
        private bool m_Valid;
        public int Version { get; private set; }
        // Changes only when the roads themselves changed, not when the cache follows the brush.
        public int RoadsVersion { get; private set; }
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
            ForestBrushProfiler.Frame();
            if (!ForestBrushState.PanelVisible || !ForestBrushState.PathForest)
            {
                if (m_Valid)
                {
                    m_Valid = false;
                    m_SegmentCount = 0;
                    m_GridSize = 0;
                    PathCount = 0;
                    Version++;
                    RoadsVersion++;
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
            ForestBrushProfiler.Begin(ForestBrushProfiler.RoadCache);

            // Padding also covers movement between refreshes and roads just outside the brush.
            float2 scanCenter = moved ? xz : m_Center;
            float extent = radius + ForestBrushState.PathGap + ForestBrushState.PathWidth + CachePadding;
            uint signature = CollectNearbyCurves(scanCenter, extent);
            if (!moved && signature == m_Signature)
            {
                ForestBrushProfiler.End(ForestBrushProfiler.RoadCache);
                return;
            }
            // Same place, different roads: cached preview distances are stale.
            if (!moved) RoadsVersion++;
            m_Center = scanCenter;
            m_Radius = radius;
            m_Gap = ForestBrushState.PathGap;
            m_Width = ForestBrushState.PathWidth;
            m_Signature = signature;
            PathCount = m_Curves.Count;

            float reach = m_Gap + m_Width;
            m_QueryExtent = radius + CachePadding;
            float gridExtent = m_QueryExtent + reach + 1f;
            m_CellSize = math.clamp(reach / 8f, 8f, 32f);
            m_CellSize = math.max(m_CellSize, gridExtent * 2f / MaximumGridSize);
            m_InverseCellSize = 1f / m_CellSize;
            m_GridSize = math.max(1, (int)math.ceil(gridExtent * 2f * m_InverseCellSize));
            m_GridMin = scanCenter - gridExtent;

            m_SegmentCount = 0;
            for (int i = 0; i < m_Curves.Count; i++)
            {
                Bezier4x3 curve = m_Curves[i];
                Flatten(curve.a.xz, curve.b.xz, curve.c.xz, curve.d.xz, m_HalfWidths[i], 0);
            }
            BuildGrid();
            m_Curves.Clear();
            m_HalfWidths.Clear();
            m_Valid = true;
            Version++;
            ForestBrushProfiler.CountCacheRebuild();
            ForestBrushProfiler.End(ForestBrushProfiler.RoadCache);
            if (Mod.DiagnosticLoggingEnabled)
                Mod.LogDiagnosticInfo($"Path forest: {PathCount} nearby roads/paths, {m_SegmentCount} curve segments, " +
                    $"{m_GridSize}x{m_GridSize} cells of {m_CellSize:F1}m.");
        }

        // Walks the component arrays chunk by chunk. Distant edges are rejected by a
        // bounding-box test before any per-entity lookup, so the cost follows the
        // number of nearby roads and not the size of the city.
        private uint CollectNearbyCurves(float2 scanCenter, float extent)
        {
            m_Curves.Clear();
            m_HalfWidths.Clear();
            uint signature = 2166136261u;
            float2 coarseMin = scanCenter - extent - MaximumRoadHalfWidth;
            float2 coarseMax = scanCenter + extent + MaximumRoadHalfWidth;

            m_Networks.CompleteDependency();
            EntityTypeHandle entityType = EntityManager.GetEntityTypeHandle();
            ComponentTypeHandle<Curve> curveType = EntityManager.GetComponentTypeHandle<Curve>(true);
            ComponentTypeHandle<PrefabRef> prefabType = EntityManager.GetComponentTypeHandle<PrefabRef>(true);
            ComponentTypeHandle<Game.Net.Elevation> elevationType =
                EntityManager.GetComponentTypeHandle<Game.Net.Elevation>(true);
            ComponentTypeHandle<Composition> compositionType =
                EntityManager.GetComponentTypeHandle<Composition>(true);

            using NativeArray<ArchetypeChunk> chunks = m_Networks.ToArchetypeChunkArray(Allocator.Temp);
            for (int c = 0; c < chunks.Length; c++)
            {
                ArchetypeChunk chunk = chunks[c];
                NativeArray<Entity> entities = chunk.GetNativeArray(entityType);
                NativeArray<Curve> curves = chunk.GetNativeArray(ref curveType);
                NativeArray<PrefabRef> prefabs = chunk.GetNativeArray(ref prefabType);
                bool hasElevation = chunk.Has(ref elevationType);
                bool hasComposition = chunk.Has(ref compositionType);
                NativeArray<Game.Net.Elevation> elevations = hasElevation
                    ? chunk.GetNativeArray(ref elevationType) : default;
                NativeArray<Composition> compositions = hasComposition
                    ? chunk.GetNativeArray(ref compositionType) : default;

                for (int i = 0; i < curves.Length; i++)
                {
                    Bezier4x3 curve = curves[i].m_Bezier;
                    float2 min = math.min(math.min(curve.a.xz, curve.b.xz), math.min(curve.c.xz, curve.d.xz));
                    float2 max = math.max(math.max(curve.a.xz, curve.b.xz), math.max(curve.c.xz, curve.d.xz));
                    // Also false for NaN coordinates, which the finite check below would reject anyway.
                    if (!(math.all(min <= coarseMax) && math.all(max >= coarseMin))) continue;

                    // Bridges and tunnels must not seed a forest on the ground below/above them.
                    if (hasElevation && math.cmax(math.abs(elevations[i].m_Elevation)) > 2f) continue;

                    Entity prefab = prefabs[i].m_Prefab;
                    if ((!EntityManager.HasComponent<RoadData>(prefab) &&
                         !EntityManager.HasComponent<PathwayData>(prefab)) ||
                        !EntityManager.TryGetComponent(prefab, out NetGeometryData geometry)) continue;

                    float width = geometry.m_DefaultWidth;
                    if (hasComposition &&
                        EntityManager.TryGetComponent(compositions[i].m_Edge, out NetCompositionData data))
                        width = math.max(width, data.m_Width);
                    if (!math.isfinite(width) || width <= 0f ||
                        !math.all(math.isfinite(curve.a)) || !math.all(math.isfinite(curve.b)) ||
                        !math.all(math.isfinite(curve.c)) || !math.all(math.isfinite(curve.d))) continue;
                    if (math.any(min > scanCenter + extent + width * 0.5f) ||
                        math.any(max < scanCenter - extent - width * 0.5f)) continue;

                    m_Curves.Add(curve);
                    m_HalfWidths.Add(width * 0.5f + CurveTolerance);
                    signature = math.hash(new uint4(signature, math.hash(curve.a), math.hash(curve.b), math.hash(curve.c)));
                    signature = math.hash(new uint4(signature, math.hash(curve.d), math.asuint(width),
                        (uint)entities[i].Index));
                }
            }
            return signature;
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
            // Road surfaces completely outside the grid cannot reach any queried point.
            float2 min = math.min(a, b) - halfWidth, max = math.max(a, b) + halfWidth;
            float2 gridMax = m_GridMin + m_GridSize * m_CellSize;
            if (math.any(max < m_GridMin) || math.any(min > gridMax)) return;
            if (m_SegmentCount == m_Segments.Length)
                System.Array.Resize(ref m_Segments, m_SegmentCount * 2);
            float2 delta = b - a;
            m_Segments[m_SegmentCount++] = new Segment
            {
                Ax = a.x,
                Ay = a.y,
                Dx = delta.x,
                Dy = delta.y,
                HalfWidth = halfWidth,
                InverseLengthSquared = 1f / math.max(0.000001f, math.lengthsq(delta))
            };
        }

        private void BuildGrid()
        {
            int cellCount = m_GridSize * m_GridSize;
            if (m_CellStart.Length < cellCount + 1) m_CellStart = new int[cellCount + 1];
            System.Array.Clear(m_CellStart, 0, cellCount + 1);

            for (int i = 0; i < m_SegmentCount; i++)
            {
                CellRange(i, out int x0, out int y0, out int x1, out int y1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                        m_CellStart[y * m_GridSize + x + 1]++;
            }
            for (int i = 0; i < cellCount; i++) m_CellStart[i + 1] += m_CellStart[i];
            int total = m_CellStart[cellCount];
            if (m_CellItems.Length < total) m_CellItems = new int[math.max(total, m_CellItems.Length * 2)];

            // Fill from the back of each cell; afterwards m_CellStart[i] is the cell's first item again.
            for (int i = cellCount; i > 0; i--) m_CellStart[i] = m_CellStart[i - 1];
            m_CellStart[0] = 0;
            for (int i = 0; i < m_SegmentCount; i++)
            {
                CellRange(i, out int x0, out int y0, out int x1, out int y1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                        m_CellItems[m_CellStart[y * m_GridSize + x + 1]++] = i;
            }
        }

        private void CellRange(int index, out int x0, out int y0, out int x1, out int y1)
        {
            Segment segment = m_Segments[index];
            float bx = segment.Ax + segment.Dx, by = segment.Ay + segment.Dy;
            int last = m_GridSize - 1;
            x0 = math.clamp((int)math.floor((math.min(segment.Ax, bx) - segment.HalfWidth - m_GridMin.x) * m_InverseCellSize), 0, last);
            x1 = math.clamp((int)math.floor((math.max(segment.Ax, bx) + segment.HalfWidth - m_GridMin.x) * m_InverseCellSize), 0, last);
            y0 = math.clamp((int)math.floor((math.min(segment.Ay, by) - segment.HalfWidth - m_GridMin.y) * m_InverseCellSize), 0, last);
            y1 = math.clamp((int)math.floor((math.max(segment.Ay, by) + segment.HalfWidth - m_GridMin.y) * m_InverseCellSize), 0, last);
        }

        private static float Distance(float2 p, float2 a, float2 b)
        {
            float2 delta = b - a;
            float t = math.saturate(math.dot(p - a, delta) / math.max(0.000001f, math.lengthsq(delta)));
            return math.distance(p, a + delta * t);
        }

        internal struct PatternSample
        {
            public float Edge, Transition, Multiplier;

            public float Probability(float inward)
            {
                if (inward <= 0f || inward >= Edge) return 0f;
                float fade = Transition > 0f
                    ? math.smoothstep(0f, Transition, inward) *
                        (1f - math.smoothstep(Edge - Transition, Edge, inward))
                    : 1f;
                return math.saturate(fade * Multiplier);
            }
        }

        // True where InwardDistance can see every road that matters for the point.
        internal bool Covers(float2 point)
        {
            return m_Valid && math.abs(point.x - m_Center.x) <= m_QueryExtent &&
                math.abs(point.y - m_Center.y) <= m_QueryExtent;
        }

        // Only this part depends on the nearby road geometry.
        internal float InwardDistance(float2 point)
        {
            if (!m_Valid || m_SegmentCount == 0) return 0f;
            float px = point.x, py = point.y;
            if (math.abs(px - m_Center.x) > m_QueryExtent ||
                math.abs(py - m_Center.y) > m_QueryExtent) return 0f;

            float gap = ForestBrushState.PathGap;
            float limit = gap + ForestBrushState.PathWidth;
            int last = m_GridSize - 1;
            int cx = math.clamp((int)((px - m_GridMin.x) * m_InverseCellSize), 0, last);
            int cy = math.clamp((int)((py - m_GridMin.y) * m_InverseCellSize), 0, last);
            float distance = float.MaxValue;

            // Ring 0 is the cell of the point, ring 1 its eight neighbours, and so on.
            // Every road not seen after ring k is at least k cells away, so the search
            // ends as soon as the nearest road found is closer than that.
            for (int ring = 0; ; ring++)
            {
                int x0 = cx - ring, x1 = cx + ring, y0 = cy - ring, y1 = cy + ring;
                int yStart = math.max(y0, 0), yEnd = math.min(y1, last);
                int xStart = math.max(x0, 0), xEnd = math.min(x1, last);
                for (int y = yStart; y <= yEnd; y++)
                {
                    int row = y * m_GridSize;
                    if (y == y0 || y == y1)
                    {
                        for (int x = xStart; x <= xEnd; x++)
                            ScanCell(row + x, px, py, ref distance);
                    }
                    else
                    {
                        if (x0 >= 0) ScanCell(row + x0, px, py, ref distance);
                        if (x1 <= last) ScanCell(row + x1, px, py, ref distance);
                    }
                }
                if (distance <= gap) return 0f;
                float covered = ring * m_CellSize;
                if (distance <= covered || covered >= limit) break;
                if (x0 <= 0 && y0 <= 0 && x1 >= last && y1 >= last) break;
            }

            float inward = distance - gap;
            if (inward <= 0f || inward >= ForestBrushState.PathWidth) return 0f;

            return inward;
        }

        private void ScanCell(int cell, float px, float py, ref float distance)
        {
            int end = m_CellStart[cell + 1];
            for (int i = m_CellStart[cell]; i < end; i++)
            {
                ref Segment segment = ref m_Segments[m_CellItems[i]];
                float t = ((px - segment.Ax) * segment.Dx + (py - segment.Ay) * segment.Dy) *
                    segment.InverseLengthSquared;
                t = t < 0f ? 0f : t > 1f ? 1f : t;
                float dx = px - (segment.Ax + segment.Dx * t);
                float dy = py - (segment.Ay + segment.Dy * t);
                float d = math.sqrt(dx * dx + dy * dy) - segment.HalfWidth;
                if (d < distance) distance = d;
            }
        }

        internal static PatternSample SamplePattern(float2 point)
        {
            uint seed = math.hash(new uint2((uint)ForestBrushState.PathSeed, 0x9e3779b9u));
            float2 offset = new float2(seed & 0xffffu, seed >> 16) * 0.0317f;
            float2 p = point / math.max(10f, ForestBrushState.NoiseScale);
            float broad = noise.snoise(p + offset) * 0.5f + 0.5f;
            float detail = noise.snoise(p * 3.17f - offset) * 0.5f + 0.5f;
            float irregularity = ForestBrushState.NoiseStrength / 100f;
            float edge = ForestBrushState.PathWidth * (1f - broad * 0.25f * irregularity);
            float transition = math.min(ForestBrushState.PathTransition, edge * 0.45f);
            float multiplier = math.lerp(1f, 0.3f + broad * 0.5f + detail * 0.2f, irregularity);
            if (ForestBrushState.PathClearings > 0)
            {
                float clearing = noise.snoise(point / ForestBrushState.PathClearingSize +
                    offset + new float2(71.3f, -23.7f)) * 0.5f + 0.5f;
                float threshold = 0.12f + ForestBrushState.PathClearings / 100f * 0.75f;
                multiplier *= math.smoothstep(threshold, threshold + 0.12f, clearing);
            }
            return new PatternSample { Edge = edge, Transition = transition, Multiplier = multiplier };
        }

        public float Probability(float2 point, float2 patternPoint, bool detailed = true)
        {
            float inward = InwardDistance(point);
            if (inward <= 0f) return 0f;
            return detailed ? SamplePattern(patternPoint).Probability(inward) : 1f;
        }

        public bool Keep(float3 position, float3 shapeCenter)
        {

            float density = math.saturate(ForestBrushState.DensityPercent / 100f);
            float chance = Probability(position.xz,
                ForestBrushState.PatternPosition(position, shapeCenter)) * density;
            return ForestBrushState.PlacementRandom(position, (uint)ForestBrushState.PathSeed) < chance;
        }
    }
}