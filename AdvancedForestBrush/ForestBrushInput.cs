using Game.Input;
using UnityEngine;

namespace AdvancedForestBrush
{
    internal static class ForestBrushInput
    {
        private static ProxyAction s_RotateAction;
        private static ProxyAction s_AddSpeciesAction;
        private static float s_LastAddSpeciesPressTime = -10f;
        private static bool s_MissingAddActionLogged;

        internal static bool IsRotating =>
            s_RotateAction != null && s_RotateAction.IsPressed();

        internal static bool IsAddingSpecies =>
            s_AddSpeciesAction != null &&
            (s_AddSpeciesAction.IsPressed() ||
             s_AddSpeciesAction.WasPerformedThisFrame() ||
             Time.realtimeSinceStartup - s_LastAddSpeciesPressTime < 0.35f);

        internal static void Update()
        {
            if (s_AddSpeciesAction != null && s_AddSpeciesAction.WasPerformedThisFrame())
            {
                s_LastAddSpeciesPressTime = Time.realtimeSinceStartup;
            }
        }

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
            s_AddSpeciesAction = Mod.Settings.GetAction(Setting.kAddSpeciesAction);
            if (s_AddSpeciesAction != null) s_AddSpeciesAction.shouldBeEnabled = true;
            else if (!s_MissingAddActionLogged)
            {
                Mod.Log?.Warn("Advanced Forest Brush add-to-list action is unavailable");
                s_MissingAddActionLogged = true;
            }
        }

        internal static void Deactivate()
        {
            if (s_RotateAction != null)
            {
                s_RotateAction.shouldBeEnabled = false;
            }

            if (s_AddSpeciesAction != null) s_AddSpeciesAction.shouldBeEnabled = false;

            s_RotateAction = null;
            s_AddSpeciesAction = null;
            s_LastAddSpeciesPressTime = -10f;
        }
    }
}
