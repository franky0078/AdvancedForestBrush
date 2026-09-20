using Colossal.UI.Binding;
using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Unity.Entities;
using Unity.Mathematics;

namespace AdvancedForestBrush
{
    public partial class AdvancedForestBrushUISystem : UISystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;

        public override GameMode gameMode => GameMode.GameOrEditor;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            AddUpdateBinding(new GetterValueBinding<bool>(Mod.Id, "IsVisible", IsVegetationContext));
            AddUpdateBinding(new GetterValueBinding<bool>(Mod.Id, "PanelVisible", () => ForestBrushState.PanelVisible));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "Density", () => ForestBrushState.DensityPercent));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "NoiseMode", () => (int)ForestBrushState.NoiseMode));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "NoiseScale", () => ForestBrushState.NoiseScale));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "NoiseStrength", () => ForestBrushState.NoiseStrength));

            AddBinding(new TriggerBinding(Mod.Id, "TogglePanel", TogglePanel));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetDensity", value => ForestBrushState.SetDensity(value)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "StepDensity", direction => ForestBrushState.StepDensity(direction)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetNoiseMode", value => ForestBrushState.NoiseMode = (ForestNoiseMode)math.clamp(value, 0, 4)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetNoiseScale", value => ForestBrushState.NoiseScale = math.clamp(value, 10, 200)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetNoiseStrength", value => ForestBrushState.NoiseStrength = math.clamp(value, 0, 100)));
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (IsVegetationBrush())
            {
                m_ObjectToolSystem.brushStrength = ForestBrushState.DensityPercent / 100f;
            }
        }

        private void TogglePanel()
        {
            ForestBrushState.PanelVisible = !ForestBrushState.PanelVisible;

            if (!ForestBrushState.PanelVisible)
            {
                return;
            }

            m_ToolSystem.selected = Entity.Null;
            m_ObjectToolSystem.mode = ObjectToolSystem.Mode.Brush;
            m_ToolSystem.activeTool = m_ObjectToolSystem;
        }

        private bool IsVegetationBrush()
        {
            return IsVegetationContext() &&
                m_ToolSystem.activeTool == m_ObjectToolSystem &&
                m_ObjectToolSystem.actualMode == ObjectToolSystem.Mode.Brush;
        }

        private bool IsVegetationContext()
        {
            if ((m_ToolSystem.activeTool != m_ObjectToolSystem &&
                 m_ToolSystem.activeTool?.toolID != "Tree Controller Tool") ||
                m_ToolSystem.activePrefab == null ||
                !m_PrefabSystem.TryGetEntity(m_ToolSystem.activePrefab, out Entity prefabEntity))
            {
                return false;
            }

            return EntityManager.HasComponent<PlantData>(prefabEntity);
        }
    }
}
