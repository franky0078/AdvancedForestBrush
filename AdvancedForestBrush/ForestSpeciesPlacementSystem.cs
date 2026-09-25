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
    // Runs after Tree Controller's early Modification1 pass, so the saved
    // species is the final prefab used by the game's placement consumers.
    public partial class ForestSpeciesPlacementSystem : GameSystemBase
    {
        private EntityQuery m_Definitions;
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_Definitions = SystemAPI.QueryBuilder()
                .WithAll<CreationDefinition, ObjectDefinition, Updated>()
                .WithNone<Deleted, Overridden>()
                .Build();
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible || !ForestSpeciesPalette.Active ||
                ForestSpeciesPalette.Prefabs.Count == 0 ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            using NativeArray<Entity> entities = m_Definitions.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition creation) ||
                    !EntityManager.TryGetComponent(entity, out ObjectDefinition definition) ||
                    (!EntityManager.HasComponent<PlantData>(creation.m_Prefab) &&
                     !EntityManager.HasComponent<TreeData>(creation.m_Prefab)))
                {
                    continue;
                }

                uint hash = math.hash(new uint3(math.asuint(definition.m_Position.x),
                    math.asuint(definition.m_Position.z), (uint)creation.m_RandomSeed));
                Entity replacement = ForestSpeciesPalette.Prefabs[
                    (int)(hash % (uint)ForestSpeciesPalette.Prefabs.Count)];
                if (!EntityManager.Exists(replacement) ||
                    (!EntityManager.HasComponent<PlantData>(replacement) &&
                     !EntityManager.HasComponent<TreeData>(replacement)))
                {
                    continue;
                }

                if (ForestBrushState.SpeciesGrouping != 0 &&
                    !ForestPlacementFilterSystem.KeepSpecies(
                        definition.m_Position, replacement, creation.m_RandomSeed))
                {
                    EntityManager.DestroyEntity(entity);
                    continue;
                }

                creation.m_Prefab = replacement;
                EntityManager.SetComponentData(entity, creation);
            }
        }
    }
}
