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
    public partial class ForestSinglePlantRotationSystem : GameSystemBase
    {
        private EntityQuery m_PlacedObjects;
        private EntityQuery m_PreviewObjects;
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private ForestSpeciesPlacementSystem m_SpeciesPlacement;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            m_SpeciesPlacement = World.GetOrCreateSystemManaged<ForestSpeciesPlacementSystem>();
            m_PlacedObjects = SystemAPI.QueryBuilder()
                .WithAll<Applied, Created, PrefabRef, Transform>()
                .WithNone<Temp, Deleted, Overridden, Owner>()
                .Build();
            m_PreviewObjects = SystemAPI.QueryBuilder()
                .WithAll<Temp, PrefabRef, Transform>()
                .WithNone<Deleted, Overridden, Owner>()
                .Build();
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible || !ForestBrushState.SingleTreeMode ||
                !Mod.SingleTreeAddonEnabled || m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Create ||
                m_ToolSystem.activePrefab == null ||
                !m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity activePrefab))
            {
                return;
            }

            Setting settings = Mod.Settings;
            if (!m_PlacedObjects.IsEmptyIgnoreFilter)
            {
                using NativeArray<Entity> placedObjects = m_PlacedObjects.ToEntityArray(Allocator.Temp);
                foreach (Entity entity in placedObjects)
                {
                    if (EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab != activePrefab)
                    {
                        continue;
                    }

                    quaternion rotation = m_SpeciesPlacement.GetSingleTreeRotation(settings, out bool randomized);
                    WriteRotation(entity, rotation);
                    if (Mod.DiagnosticLoggingEnabled)
                    {
                        float3 forward = math.mul(rotation, new float3(0f, 0f, 1f));
                        float angle = (math.degrees(math.atan2(forward.x, forward.z)) + 360f) % 360f;
                        Mod.LogDiagnosticInfo($"Single vegetation placement {entity}: applied yaw {angle:F1}°.");
                    }

                    m_SpeciesPlacement.AdvanceSingleTreeRotation(
                        randomized, settings?.RandomVegetationRotation == true);
                }
            }

            if (m_PreviewObjects.IsEmptyIgnoreFilter)
            {
                return;
            }

            // A stationary preview can survive a tool update without a fresh definition.
            quaternion previewRotation = m_SpeciesPlacement.GetSingleTreeRotation(settings, out _);
            using NativeArray<Entity> previewObjects = m_PreviewObjects.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in previewObjects)
            {
                Temp temp = EntityManager.GetComponentData<Temp>(entity);
                if (EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab == activePrefab &&
                    temp.m_Original == Entity.Null && (temp.m_Flags & TempFlags.Create) != 0 &&
                    (temp.m_Flags & (TempFlags.Delete | TempFlags.Cancel)) == 0)
                {
                    WriteRotation(entity, previewRotation);
                }
            }
        }

        private void WriteRotation(Entity entity, quaternion rotation)
        {
            Transform transform = EntityManager.GetComponentData<Transform>(entity);
            if (math.abs(math.dot(transform.m_Rotation.value, rotation.value)) >= 0.9999999f)
            {
                return;
            }

            transform.m_Rotation = rotation;
            EntityManager.SetComponentData(entity, transform);
            if (!EntityManager.HasComponent<BatchesUpdated>(entity))
            {
                EntityManager.AddComponent<BatchesUpdated>(entity);
            }
        }
    }
}
