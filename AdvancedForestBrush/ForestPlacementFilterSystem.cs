using Colossal.Entities;
using Game;
using Game.Common;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AdvancedForestBrush
{
    public partial class ForestPlacementFilterSystem : GameSystemBase
    {
        private EntityQuery m_DefinitionQuery;
        private ForestPathNetworkSystem m_PathNetwork;
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private ToolRaycastSystem m_ToolRaycastSystem;
        private float m_NextDiagnostic;
        private int m_DiagnosticFrames, m_DiagnosticCandidates, m_DiagnosticKept;
        private int m_DiagnosticShape, m_DiagnosticPath, m_DiagnosticDensity;
        private int m_DiagnosticNoise, m_DiagnosticSpecies;
        private float m_DiagnosticDistance;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_PathNetwork = World.GetOrCreateSystemManaged<ForestPathNetworkSystem>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_ToolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            m_DefinitionQuery = SystemAPI.QueryBuilder()
                .WithAll<CreationDefinition, ObjectDefinition, Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            bool customShape =
                ForestBrushState.Shape != ForestBrushShape.Circle;

            bool detailedPreview = ForestBrushPreviewSystem.DetailedPreview;

            if (!detailedPreview && !ForestBrushState.PathForest && !customShape &&
                ForestBrushState.NoiseMode == ForestNoiseMode.Uniform &&
                ForestBrushState.SpeciesGrouping == 0 &&
                ForestBrushState.DensityPercent >= 100)
            {
                return;
            }

            if (m_DefinitionQuery.IsEmptyIgnoreFilter)
            {
                LogPlacementCounts(0, 0, 0, 0, 0, 0, 0);
                return;
            }

            bool hasShapeCenter =
                m_ToolRaycastSystem.GetRaycastResult(out RaycastResult raycastResult) &&
                !EntityManager.HasComponent<Deleted>(raycastResult.m_Owner);
            float3 shapeCenter = hasShapeCenter
                ? raycastResult.m_Hit.m_Position
                : ForestBrushState.CursorPosition;


            if (ForestBrushState.PathForest) m_PathNetwork.EnsureCache(shapeCenter);

            using NativeArray<Entity> entities = m_DefinitionQuery.ToEntityArray(Allocator.Temp);
            using EntityCommandBuffer commands = new EntityCommandBuffer(Allocator.Temp);
            int candidates = 0, kept = 0, shapeRejected = 0, pathRejected = 0;
            int densityRejected = 0, noiseRejected = 0, speciesRejected = 0;
            float maximumDistance = 0f;
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition creation) ||
                    !EntityManager.TryGetComponent(entity, out ObjectDefinition definition))
                {
                    continue;
                }

                if (!EntityManager.HasComponent<PlantData>(creation.m_Prefab) &&
                    !EntityManager.HasComponent<TreeData>(creation.m_Prefab))
                {
                    continue;
                }
                candidates++;
                if (Mod.DiagnosticLoggingEnabled)
                    maximumDistance = math.max(maximumDistance,
                        math.distance(definition.m_Position.xz, shapeCenter.xz));

                bool outsideShape =
                    ForestBrushState.SuppressPolygonPlacement ||
                    ((customShape || detailedPreview || ForestBrushState.PathForest) &&
                     ((!hasShapeCenter && !ForestBrushState.HasValidCursor) ||
                      !ForestBrushState.Contains(
                          definition.m_Position,
                          shapeCenter)));

                if (outsideShape)
                {
                    shapeRejected++;
                    commands.DestroyEntity(entity);
                    continue;
                }
                if (ForestBrushState.PathForest && !m_PathNetwork.Keep(definition.m_Position, shapeCenter))
                {
                    pathRejected++;
                    commands.DestroyEntity(entity);
                    continue;
                }
                if (!ForestBrushState.PathForest && !KeepDensity(definition.m_Position, creation.m_RandomSeed))
                {
                    densityRejected++;
                    commands.DestroyEntity(entity);
                    continue;
                }
                if (!ForestBrushState.PathForest && ForestBrushState.NoiseMode != ForestNoiseMode.Uniform &&
                    !Keep(definition.m_Position, creation.m_RandomSeed, shapeCenter))
                {
                    noiseRejected++;
                    commands.DestroyEntity(entity);
                    continue;
                }
                bool rejectedBySpecies =
                    ForestBrushState.SpeciesGrouping != 0 &&
                    !(ForestSpeciesPalette.Active && ForestSpeciesPalette.Prefabs.Count > 0) &&
                    !KeepSpecies(definition.m_Position, creation.m_Prefab,
                        creation.m_RandomSeed);

                if (rejectedBySpecies)
                {
                    speciesRejected++;
                    commands.DestroyEntity(entity);
                    continue;
                }
                kept++;
            }
            commands.Playback(EntityManager);
            LogPlacementCounts(candidates, kept, shapeRejected, pathRejected,
                densityRejected, noiseRejected, speciesRejected, maximumDistance);
        }

        private void LogPlacementCounts(int candidates, int kept, int shape, int path,
            int density, int noise, int species, float maximumDistance = 0f)
        {
            if (!Mod.DiagnosticLoggingEnabled) return;
            m_DiagnosticFrames++;
            m_DiagnosticCandidates += candidates;
            m_DiagnosticKept += kept;
            m_DiagnosticShape += shape;
            m_DiagnosticPath += path;
            m_DiagnosticDensity += density;
            m_DiagnosticNoise += noise;
            m_DiagnosticSpecies += species;
            m_DiagnosticDistance = math.max(m_DiagnosticDistance, maximumDistance);
            if (UnityEngine.Time.unscaledTime < m_NextDiagnostic) return;
            m_NextDiagnostic = UnityEngine.Time.unscaledTime + 1f;
            Mod.LogDiagnosticInfo($"Forest brush candidates over {m_DiagnosticFrames} updates: " +
                $"input={m_DiagnosticCandidates}, kept={m_DiagnosticKept}, " +
                $"rejected shape={m_DiagnosticShape}, road={m_DiagnosticPath}, density={m_DiagnosticDensity}, " +
                $"noise={m_DiagnosticNoise}, species={m_DiagnosticSpecies}; max cursor distance={m_DiagnosticDistance:F1}m; " +
                $"cursor={ForestBrushState.CursorPosition.x:F1},{ForestBrushState.CursorPosition.z:F1}.");
            m_DiagnosticFrames = m_DiagnosticCandidates = m_DiagnosticKept = 0;
            m_DiagnosticShape = m_DiagnosticPath = m_DiagnosticDensity = 0;
            m_DiagnosticNoise = m_DiagnosticSpecies = 0;
            m_DiagnosticDistance = 0f;
        }

        private static bool KeepDensity(float3 position, int randomSeed)
        {
            if (ForestBrushState.DensityPercent >= 100)
            {
                return true;
            }

            float random = ForestBrushState.PlacementRandom(position, (uint)randomSeed ^ 0x6d2b79f5u);
            return random < ForestBrushState.DensityPercent / 100f;
        }

        // Filtering rather than replacing the prefab preserves its selected mix.
        internal static bool KeepSpecies(float3 position, Entity prefab, int randomSeed)
        {
            uint species = math.hash(new uint2((uint)prefab.Index,
                (uint)prefab.Version));
            float2 offset = new float2((species & 0xffffu) * 0.0137f,
                ((species >> 16) & 0xffffu) * 0.0179f);
            float2 p = new float2(position.x, position.z) /
                math.max(10f, ForestBrushState.NoiseScale);
            float field = noise.snoise(p + offset) * 0.5f + 0.5f;
            float grouping = ForestBrushState.SpeciesGrouping == 1 ? 0.35f :
                ForestBrushState.SpeciesGrouping == 2 ? 0.65f : 0.9f;
            float chance = math.lerp(1f,
                math.smoothstep(0.12f, 0.88f, field), grouping);
            uint hash = math.hash(new uint4(math.asuint(position.x),
                math.asuint(position.z), (uint)randomSeed, species));
            float random = (hash & 0x00ffffffu) / 16777215f;
            return random < chance;
        }

        private static bool Keep(float3 position, int randomSeed, float3 shapeCenter)
        {
            float chance = NoiseProbability(ForestBrushState.PatternPosition(position, shapeCenter));
            if (ForestBrushState.NoiseMode == ForestNoiseMode.Clusters)
            {
                uint hash = math.hash(new uint4(math.asuint(position.x),
                    math.asuint(position.z), (uint)randomSeed, 0xa511e9b3u));
                return (hash & 0x00ffffffu) / 16777216f < chance;
            }
            if (ForestBrushState.NoiseMode == ForestNoiseMode.Edge)
            {
                uint hash = math.hash(new uint3((uint)math.abs(position.x * 10f),
                    (uint)math.abs(position.z * 10f), (uint)randomSeed));
                return (hash & 0x00ffffffu) / 16777215f < chance;
            }
            return chance > 0f;
        }

        internal static float NoiseProbability(float2 position)
        {
            if (ForestBrushState.NoiseMode == ForestNoiseMode.Uniform) return 1f;
            float scale = math.max(10f, ForestBrushState.NoiseScale);
            float2 p = position / scale;
            float seed = ForestBrushState.Seed * 0.0137f;
            float baseNoise = noise.snoise(p + new float2(seed, -seed)) * 0.5f + 0.5f;
            float detail = noise.snoise((p * 2.37f) + new float2(-seed, seed * 0.5f)) * 0.5f + 0.5f;
            float value = math.lerp(baseNoise, baseNoise * detail, ForestBrushState.NoiseStrength / 100f);

            switch (ForestBrushState.NoiseMode)
            {
                case ForestNoiseMode.Natural:
                    return value > 0.28f ? 1f : 0f;
                case ForestNoiseMode.Clusters:
                    float clusterValue = math.lerp(baseNoise, value, 0.35f);
                    float clusterChance = math.lerp(0.05f, 1f,
                        math.smoothstep(0.25f, 0.65f, clusterValue));
                    return clusterChance;
                case ForestNoiseMode.Clearings:
                    return value < 0.38f || value > 0.57f ? 1f : 0f;
                case ForestNoiseMode.Edge:
                    return math.saturate(0.35f + value * 0.65f);
                default:
                    return 1f;
            }
        }
    }
}
