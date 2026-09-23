using Colossal.UI.Binding;
using Game;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace AdvancedForestBrush
{
    public partial class AdvancedForestBrushUISystem : UISystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private PrefabSystem m_PrefabSystem;
        private ToolBaseSystem m_PreviousTool;
        private ObjectToolSystem.Mode m_PreviousMode;
        private Entity m_PreviousSelected;
        private float m_PreviousBrushSize;
        private float m_PreviousBrushStrength;
        private bool m_OwnsObjectToolState;

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
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "SpeciesGrouping", () => ForestBrushState.SpeciesGrouping));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "Shape", () => (int)ForestBrushState.Shape));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "ShapeWidth", () => ForestBrushState.ShapeWidth));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "ShapeLength", () => ForestBrushState.ShapeLength));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "CircleBrushSize", () => (int)math.round(ForestBrushState.CircleBrushSize)));
            AddUpdateBinding(new GetterValueBinding<float>(Mod.Id, "Rotation", () => ForestBrushState.RotationDegrees));
            AddUpdateBinding(new GetterValueBinding<int>(Mod.Id, "PolygonPointCount", () => ForestBrushState.PolygonPoints.Count));
            AddUpdateBinding(new GetterValueBinding<bool>(Mod.Id, "PolygonClosed", () => ForestBrushState.PolygonClosed));

            AddBinding(new TriggerBinding(Mod.Id, "TogglePanel", TogglePanel));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetDensity", value => ForestBrushState.SetDensity(value)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "StepDensity", direction => ForestBrushState.StepDensity(direction)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetNoiseMode", value => ForestBrushState.NoiseMode = (ForestNoiseMode)math.clamp(value, 0, 4)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetNoiseScale", value => ForestBrushState.NoiseScale = math.clamp(value, 10, 200)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetNoiseStrength", value => ForestBrushState.NoiseStrength = math.clamp(value, 0, 100)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetSpeciesGrouping", value => ForestBrushState.SpeciesGrouping = math.clamp(value, 0, 3)));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetShape", SetShape));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetShapeWidth", SetShapeWidth));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetShapeLength", SetShapeLength));
            AddBinding(new TriggerBinding<int>(Mod.Id, "SetCircleBrushSize", SetCircleBrushSize));
            AddBinding(new TriggerBinding<float>(Mod.Id, "SetRotation", value => ForestBrushState.SetRotation(value)));
            AddBinding(new TriggerBinding(Mod.Id, "ResetPolygon", ForestBrushState.ResetPolygon));
            AddBinding(new TriggerBinding<bool>(Mod.Id, "SetPointerOverUI", value => ForestBrushState.PointerOverUI = value));
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (!ForestBrushState.PanelVisible || !m_OwnsObjectToolState)
            {
                return;
            }


            // new tool selection from UIUpdate.
            if (!IsVegetationBrush())
            {
                ClosePanel(m_ToolSystem.activeTool == m_ObjectToolSystem);
                return;
            }

            if (ForestBrushState.Shape == ForestBrushShape.Circle)
            {
                m_ObjectToolSystem.brushSize =
                    math.max(10f, ForestBrushState.CircleBrushSize);
            }
            else if (ForestBrushState.Shape == ForestBrushShape.Square ||
                     ForestBrushState.Shape == ForestBrushShape.Rectangle)
            {
                ApplyShapeBrushBounds();
            }
            else if (ForestBrushState.Shape == ForestBrushShape.Polygon &&
                     ForestBrushState.PolygonClosed)
            {
                ApplyPolygonBrushBounds();
            }

            bool polygonReady =
                ForestBrushState.Shape != ForestBrushShape.Polygon ||
                (ForestBrushState.PolygonClosed &&
                 !ForestBrushState.SuppressPolygonPlacement);

            bool rotating = ForestBrushState.HasValidCursor &&
                !ForestBrushState.PointerOverUI &&
                (ForestBrushState.Shape == ForestBrushShape.Square ||
                 ForestBrushState.Shape == ForestBrushShape.Rectangle ||
                 (ForestBrushState.Shape == ForestBrushShape.Polygon &&
                  ForestBrushState.PolygonClosed)) &&
                Keyboard.current != null && Mouse.current != null &&
                (Keyboard.current.leftCtrlKey.isPressed ||
                 Keyboard.current.rightCtrlKey.isPressed) &&
                Mouse.current.rightButton.isPressed;

            m_ObjectToolSystem.brushStrength = polygonReady && !rotating
                ? ForestBrushState.DensityPercent / 100f
                : 0f;
        }

        private void TogglePanel()
        {
            ForestBrushState.PanelVisible = !ForestBrushState.PanelVisible;
            if (!ForestBrushState.PanelVisible)
            {
                ClosePanel(true);
                return;
            }

            AcquireObjectToolState();
        }

        private void AcquireObjectToolState()
        {
            if (m_OwnsObjectToolState)
            {
                return;
            }

            m_PreviousTool = m_ToolSystem.activeTool;
            m_PreviousMode = m_ObjectToolSystem.mode;
            m_PreviousSelected = m_ToolSystem.selected;
            m_PreviousBrushSize = m_ObjectToolSystem.brushSize;
            m_PreviousBrushStrength = m_ObjectToolSystem.brushStrength;
            m_OwnsObjectToolState = true;

            m_ToolSystem.selected = Entity.Null;
            m_ObjectToolSystem.mode = ObjectToolSystem.Mode.Brush;
            m_ToolSystem.activeTool = m_ObjectToolSystem;
        }

        private void ClosePanel(bool restorePreviousTool)
        {
            ForestBrushState.PanelVisible = false;
            ForestBrushState.PointerOverUI = false;
            ForestBrushState.HasValidCursor = false;

            if (!m_OwnsObjectToolState)
            {
                return;
            }


            // values into the normal controller after closing.
            m_ObjectToolSystem.brushSize = m_PreviousBrushSize;
            m_ObjectToolSystem.brushStrength = m_PreviousBrushStrength;
            m_ObjectToolSystem.mode = m_PreviousMode;

            if (restorePreviousTool &&
                m_ToolSystem.activeTool == m_ObjectToolSystem &&
                m_PreviousTool != null)
            {
                m_ToolSystem.selected = m_PreviousSelected;
                m_ToolSystem.activeTool = m_PreviousTool;
            }

            m_PreviousTool = null;
            m_OwnsObjectToolState = false;
        }

        private void SetShape(int value)
        {
            ForestBrushState.SetShape(value);

            if (!ForestBrushState.PanelVisible || !m_OwnsObjectToolState)
            {
                return;
            }

            if (ForestBrushState.Shape == ForestBrushShape.Circle ||
                (ForestBrushState.Shape == ForestBrushShape.Polygon &&
                 !ForestBrushState.PolygonClosed))
            {
                m_ObjectToolSystem.brushSize =
                    math.max(10f, ForestBrushState.CircleBrushSize);
            }
            else if (ForestBrushState.Shape == ForestBrushShape.Polygon)
            {
                ApplyPolygonBrushBounds();
            }
            else
            {
                ApplyShapeBrushBounds();
            }
        }

        private void SetShapeWidth(int value)
        {
            ForestBrushState.ShapeWidth = math.clamp(value, 10, 1000);
            if (ForestBrushState.PanelVisible && m_OwnsObjectToolState)
            {
                ApplyShapeBrushBounds();
            }
        }

        private void SetShapeLength(int value)
        {
            ForestBrushState.ShapeLength = math.clamp(value, 10, 1000);
            if (ForestBrushState.PanelVisible && m_OwnsObjectToolState)
            {
                ApplyShapeBrushBounds();
            }
        }

        private void SetCircleBrushSize(int value)
        {
            ForestBrushState.CircleBrushSize = math.clamp(value, 10, 1000);
            if (ForestBrushState.PanelVisible &&
                ForestBrushState.Shape == ForestBrushShape.Circle &&
                m_OwnsObjectToolState)
            {
                m_ObjectToolSystem.brushSize = ForestBrushState.CircleBrushSize;
            }
        }

        private void ApplyShapeBrushBounds()
        {
            float width = ForestBrushState.ShapeWidth;
            float length = ForestBrushState.Shape == ForestBrushShape.Square
                ? width
                : ForestBrushState.ShapeLength;

            m_ObjectToolSystem.brushSize =
                math.sqrt(width * width + length * length) * 1.5f;
        }

        private void ApplyPolygonBrushBounds()
        {
            m_ObjectToolSystem.brushSize =
                math.max(10f, ForestBrushState.PolygonBoundingRadius * 3f);
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

        protected override void OnDestroy()
        {
            ClosePanel(true);
            base.OnDestroy();
        }
    }
}
