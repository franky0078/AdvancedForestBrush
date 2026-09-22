using Colossal.Entities;
using Colossal.Mathematics;
using Game;
using Game.Common;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AdvancedForestBrush
{
    public partial class ForestPlacementFilterSystem : GameSystemBase
    {
        private EntityQuery m_DefinitionQuery;
        private EntityQuery m_BrushDefinitionQuery;
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
            m_BrushDefinitionQuery = SystemAPI.QueryBuilder()
                .WithAll<CreationDefinition, BrushDefinition, Updated>()
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

            if (!customShape &&
                ForestBrushState.NoiseMode == ForestNoiseMode.Uniform)
            {
                return;
            }

            using NativeArray<BrushDefinition> brushDefinitions =
                customShape
                    ? m_BrushDefinitionQuery.ToComponentDataArray<BrushDefinition>(Allocator.Temp)
                    : new NativeArray<BrushDefinition>(0, Allocator.Temp);

            // A BrushDefinition is converted into the visible circular Brush
            // later in Modification1. The custom preview replaces that circle,
            // so remove only the visual brush definition before it is generated.
            // ObjectDefinition candidates have already been created separately.
            if (customShape)
            {
                using NativeArray<Entity> brushDefinitionEntities =
                    m_BrushDefinitionQuery.ToEntityArray(Allocator.Temp);

                foreach (Entity entity in brushDefinitionEntities)
                {
                    if (EntityManager.Exists(entity))
                    {
                        EntityManager.DestroyEntity(entity);
                    }
                }
            }

            using NativeArray<Entity> entities = m_DefinitionQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in entities)
            {
                if (!EntityManager.TryGetComponent(entity, out CreationDefinition creation) ||
                    !EntityManager.TryGetComponent(entity, out ObjectDefinition definition))
                {
                    continue;
                }

                // Brush-created vegetation definitions are optional. This
                // remains valid after Tree Controller replaces the prefab and
                // avoids depending on its internal vegetation component type.
                if ((creation.m_Flags & CreationFlags.Optional) == 0)
                {
                    continue;
                }

                bool outsideShape =
                    ForestBrushState.SuppressPolygonPlacement ||
                    (customShape &&
                     !ContainsAnyBrushStamp(
                         definition.m_Position,
                         brushDefinitions));

                bool rejectedByNoise =
                    ForestBrushState.NoiseMode != ForestNoiseMode.Uniform &&
                    !Keep(definition.m_Position, creation.m_RandomSeed);

                if (outsideShape || rejectedByNoise)
                {
                    EntityManager.DestroyEntity(entity);
                }
            }
        }

        private static bool ContainsAnyBrushStamp(
            float3 position,
            NativeArray<BrushDefinition> brushDefinitions)
        {
            for (int i = 0; i < brushDefinitions.Length; i++)
            {
                BrushDefinition brush = brushDefinitions[i];
                float length = MathUtils.Length(brush.m_Line);
                float spacing = math.max(2.5f, brush.m_Size * 0.25f);
                int stampCount = 1 + (int)math.floor(length / spacing);

                for (int stamp = 1; stamp <= stampCount; stamp++)
                {
                    float3 center = MathUtils.Position(
                        brush.m_Line,
                        (float)stamp / stampCount);

                    if (ForestBrushState.Contains(position, center))
                    {
                        return true;
                    }
                }
            }

            // Stationary stamps can reach the object-definition query one
            // update before their BrushDefinition becomes visible. The last
            // valid tool position is the exact fallback for that case.
            return brushDefinitions.Length == 0 &&
                   ForestBrushState.HasValidCursor &&
                   ForestBrushState.Contains(
                       position,
                       ForestBrushState.CursorPosition);
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
