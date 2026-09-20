using Colossal.Entities;
using Game;
using Game.Common;
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
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_DefinitionQuery = SystemAPI.QueryBuilder()
                .WithAll<CreationDefinition, ObjectDefinition, Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();
            RequireForUpdate(m_DefinitionQuery);
        }

        protected override void OnUpdate()
        {
            if (ForestBrushState.NoiseMode == ForestNoiseMode.Uniform ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            using NativeArray<Entity> entities = m_DefinitionQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition creation) ||
                    !EntityManager.HasComponent<PlantData>(creation.m_Prefab) ||
                    !EntityManager.TryGetComponent(entity, out ObjectDefinition definition))
                {
                    continue;
                }

                if (!Keep(definition.m_Position, creation.m_RandomSeed))
                {
                    EntityManager.DestroyEntity(entity);
                }
            }
        }

        private static bool Keep(float3 position, int randomSeed)
        {
            float scale = math.max(10f, ForestBrushState.NoiseScale);
            float2 p = new float2(position.x, position.z) / scale;
            float seed = ForestBrushState.Seed * 0.0137f;
            float baseNoise = noise.snoise(p + new float2(seed, -seed)) * 0.5f + 0.5f;
            float detail = noise.snoise((p * 2.37f) + new float2(-seed, seed * 0.5f)) * 0.5f + 0.5f;
            float value = math.lerp(baseNoise, baseNoise * detail, ForestBrushState.NoiseStrength / 100f);

            switch (ForestBrushState.NoiseMode)
            {
                case ForestNoiseMode.Natural:
                    return value > 0.28f;
                case ForestNoiseMode.Clusters:
                    return value > 0.52f;
                case ForestNoiseMode.Clearings:
                    return value < 0.38f || value > 0.57f;
                case ForestNoiseMode.Edge:
                    uint hash = math.hash(new uint3((uint)math.abs(position.x * 10f), (uint)math.abs(position.z * 10f), (uint)randomSeed));
                    float random = (hash & 0x00ffffffu) / 16777215f;
                    return random < math.saturate(0.35f + value * 0.65f);
                default:
                    return true;
            }
        }
    }
}
