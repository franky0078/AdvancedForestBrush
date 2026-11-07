using Colossal.Entities;
using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using AgeMask = Game.Tools.AgeMask;

namespace AdvancedForestBrush
{
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
            RequireForUpdate(m_Definitions);
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            bool usePalette = ForestSpeciesPalette.Active && ForestSpeciesPalette.Prefabs.Count > 0;
            var ages = (AgeMask)(Mod.Settings?.TreeAgeMask ?? (int)AgeMask.Mature);
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
                if (usePalette)
                {
                    Entity replacement = ForestSpeciesPalette.Select(hash);
                    if (replacement == Entity.Null)
                    {
                        // Do not fall back to the game's selection when all entries are disabled.
                        EntityManager.DestroyEntity(entity);
                        continue;
                    }
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

                if (creation.m_Original == Entity.Null &&
                    (creation.m_Flags & CreationFlags.Delete) == 0 &&
                    EntityManager.HasComponent<TreeData>(creation.m_Prefab))
                {
                    // Resolve the brush age for the final species just before object generation.
                    var ageRandom = new Random(math.max(1u, hash ^ 0x9e3779b9u));
                    definition.m_Age = ToolUtils.GetRandomAge(ref ageRandom, ages);
                    EntityManager.SetComponentData(entity, definition);
                }
            }
        }
    }
}
