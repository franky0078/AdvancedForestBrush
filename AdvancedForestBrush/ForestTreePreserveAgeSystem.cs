using Game;
using Game.Common;
using Game.Objects;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;

namespace AdvancedForestBrush
{
    public partial class ForestTreePreserveAgeSystem : GameSystemBase
    {
        private EntityQuery m_NewTrees;
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_NewTrees = SystemAPI.QueryBuilder()
                .WithAll<Temp, Tree>()
                .WithNone<Deleted, Overridden, Owner>()
                .Build();
            RequireForUpdate(m_NewTrees);
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                (m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush &&
                 !(ForestBrushState.SingleTreeMode && Mod.SingleTreeAddonEnabled &&
                   m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create)) ||
                !m_ObjectToolSystem.decorationMode)
            {
                return;
            }

            // Mark the generated tree before the game commits the placement.
            using NativeArray<Entity> trees = m_NewTrees.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in trees)
            {
                Temp temp = EntityManager.GetComponentData<Temp>(entity);
                if (temp.m_Original != Entity.Null ||
                    (temp.m_Flags & TempFlags.Create) == 0 ||
                    (temp.m_Flags & (TempFlags.Delete | TempFlags.Cancel)) != 0)
                {
                    continue;
                }

                if (EntityManager.HasComponent<Decoration>(entity))
                {
                    EntityManager.SetComponentEnabled<Decoration>(entity, true);
                }
                else
                {
                    EntityManager.AddComponent<Decoration>(entity);
                }
            }
        }
    }
}
