using Game.Input;
using Game.Tools;
using Game.UI.Tooltip;

namespace AdvancedForestBrush
{
    // Render the same input-hint widget the game uses for Paint and Erase.
    public partial class ForestBrushTooltipSystem : TooltipSystemBase
    {
        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private InputHintTooltip m_RotateHint;
        private ProxyAction m_RotateAction;
        private string m_RotateBindings;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible ||
                !ForestBrushState.HasValidCursor ||
                ForestBrushState.PointerOverUI ||
                m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush)
            {
                return;
            }

            if (ForestBrushState.Shape == ForestBrushShape.Square ||
                ForestBrushState.Shape == ForestBrushShape.Rectangle ||
                (ForestBrushState.Shape == ForestBrushShape.Polygon &&
                 ForestBrushState.PolygonClosed))
            {
                ProxyAction action = Mod.Settings?.GetAction(Setting.kRotateAction);
                if (action == null)
                {
                    return;
                }

                // Refresh() only rebuilds the game's cached hint when its label changes.
                // A rebind keeps the same ProxyAction and label, so compare the actual
                // binding paths and modifiers and replace the widget when they change.
                string bindings = action.ToString();
                if (m_RotateHint == null || m_RotateAction != action ||
                    m_RotateBindings != bindings)
                {
                    m_RotateAction = action;
                    m_RotateBindings = bindings;
                    m_RotateHint = new InputHintTooltip(action);
                }

                m_RotateHint.Refresh(InputManager.DeviceType.Mouse);
                AddMouseTooltip(m_RotateHint);
            }
        }
    }
}
