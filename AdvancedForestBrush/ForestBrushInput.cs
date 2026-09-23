using Game.Input;

namespace AdvancedForestBrush
{
    internal static class ForestBrushInput
    {
        private static ProxyAction s_RotateAction;

        internal static bool IsRotating =>
            s_RotateAction != null && s_RotateAction.IsPressed();

        internal static void Activate()
        {
            if (Mod.Settings == null)
            {
                return;
            }

            ProxyAction rotate = Mod.Settings.GetAction(Setting.kRotateAction);
            if (rotate == null)
            {
                Mod.Log?.Error("Advanced Forest Brush rotation action is unavailable");
                return;
            }

            s_RotateAction = rotate;
            rotate.shouldBeEnabled = true;
        }

        internal static void Deactivate()
        {
            if (s_RotateAction != null)
            {
                s_RotateAction.shouldBeEnabled = false;
            }

            s_RotateAction = null;
        }
    }
}
