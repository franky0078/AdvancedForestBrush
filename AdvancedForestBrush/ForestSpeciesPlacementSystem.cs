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
        private readonly System.Random m_SingleTreeRandom = new System.Random();
        private int m_SingleTreeSeed;
        private float m_SingleTreeChanceRoll;
        private float m_SingleTreeAngleRoll;
        private float m_SingleTreeManualYaw;

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
            m_SingleTreeSeed = m_SingleTreeRandom.Next(1, ushort.MaxValue + 1);
            m_SingleTreeChanceRoll = (float)m_SingleTreeRandom.NextDouble();
            m_SingleTreeAngleRoll = (float)m_SingleTreeRandom.NextDouble();
        }

        internal void AdvanceSingleTreeRotation(bool randomized, bool randomRotationEnabled)
        {
            m_SingleTreeSeed = m_SingleTreeRandom.Next(1, ushort.MaxValue + 1);
            m_SingleTreeChanceRoll = (float)m_SingleTreeRandom.NextDouble();
            if (randomized)
            {
                // A jittered golden-angle step spreads several consecutive placements across the range.
                float step = 0.381966f + ((float)m_SingleTreeRandom.NextDouble() - 0.5f) * 0.03f;
                m_SingleTreeAngleRoll = math.frac(m_SingleTreeAngleRoll + step);
            }

            if (randomRotationEnabled)
            {
                ResetSingleTreeManualRotation();
            }
        }

        internal void RotateSingleTree(float degrees)
        {
            m_SingleTreeManualYaw = math.frac((m_SingleTreeManualYaw + degrees) / 360f) * 360f;
        }

        internal void ResetSingleTreeManualRotation()
        {
            m_SingleTreeManualYaw = 0f;
        }

        internal quaternion GetSingleTreeRotation(Setting settings, out bool randomized)
        {
            randomized = settings?.RandomVegetationRotation == true &&
                PassesRotationChance(settings, m_SingleTreeChanceRoll);
            float baseAngle = randomized
                ? math.lerp(settings.RandomRotationMin, settings.RandomRotationMax, m_SingleTreeAngleRoll)
                : 0f;
            return quaternion.RotateY(math.radians(baseAngle + m_SingleTreeManualYaw));
        }

        private static bool PassesRotationChance(Setting settings, float roll) =>
            settings.RandomRotationChance == 100 || roll < settings.RandomRotationChance / 100f;

        private static quaternion GetRotation(Setting settings, float fraction)
        {
            float degrees = math.lerp(settings.RandomRotationMin, settings.RandomRotationMax, fraction);
            return quaternion.RotateY(math.radians(degrees));
        }

        protected override void OnUpdate()
        {
            bool singleTree = ForestBrushState.SingleTreeMode && Mod.SingleTreeAddonEnabled &&
                m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Create;
            if (!ForestBrushState.PanelVisible ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                (!singleTree && m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush))
            {
                return;
            }

            bool usePalette = !singleTree && ForestSpeciesPalette.Active && ForestSpeciesPalette.Prefabs.Count > 0;
            Setting settings = Mod.Settings;
            var ages = (AgeMask)(settings?.TreeAgeMask ?? (int)AgeMask.Mature);
            bool randomRotation = settings?.RandomVegetationRotation == true && settings.RandomRotationChance > 0;
            quaternion singleRotation = singleTree ? GetSingleTreeRotation(settings, out _) : default;
            using NativeArray<Entity> entities = m_Definitions.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition creation) ||
                    !EntityManager.TryGetComponent(entity, out ObjectDefinition definition) ||
                    creation.m_Original != Entity.Null ||
                    (creation.m_Flags & CreationFlags.Delete) != 0 ||
                    (!EntityManager.HasComponent<TreeData>(creation.m_Prefab) &&
                     !EntityManager.HasComponent<PlantData>(creation.m_Prefab)))
                {
                    continue;
                }

                if (singleTree && creation.m_RandomSeed != m_SingleTreeSeed)
                {
                    // Keep the model variation stable when the tool rebuilds the preview.
                    creation.m_RandomSeed = m_SingleTreeSeed;
                    EntityManager.SetComponentData(entity, creation);
                }

                uint hash = singleTree ? (uint)m_SingleTreeSeed :
                    math.hash(new uint3(math.asuint(definition.m_Position.x),
                        math.asuint(definition.m_Position.z), (uint)creation.m_RandomSeed));
                if (usePalette)
                {
                    Entity replacement = ForestSpeciesPalette.Select(hash);
                    if (replacement == Entity.Null)
                    {
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

                bool isTree = EntityManager.HasComponent<TreeData>(creation.m_Prefab);
                if (isTree)
                {
                    // Resolve the age for the final species just before object generation.
                    var ageRandom = new Random(math.max(1u, hash ^ 0x9e3779b9u));
                    definition.m_Age = ToolUtils.GetRandomAge(ref ageRandom, ages);
                }

                bool rotate = singleTree ||
                    (randomRotation && PassesRotationChance(settings, ForestBrushState.PlacementRandom(
                        definition.m_Position, (uint)creation.m_RandomSeed ^ 0x813c6a7du)));
                if (rotate)
                {
                    definition.m_Rotation = singleTree ? singleRotation :
                        GetRotation(settings, ForestBrushState.PlacementRandom(
                            definition.m_Position, (uint)creation.m_RandomSeed ^ 0x4a39b70du));
                    if (singleTree)
                    {
                        definition.m_LocalRotation = singleRotation;
                    }
                }

                if (isTree || rotate)
                {
                    EntityManager.SetComponentData(entity, definition);
                }
            }
        }
    }
}
