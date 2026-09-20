using Colossal.Logging;
using AdvancedForestBrush.Localization;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Tools;

namespace AdvancedForestBrush
{
    public sealed class Mod : IMod
    {
        public const string Id = "AdvancedForestBrush";
        internal static ILog Log { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log = LogManager.GetLogger("AdvancedForestBrush").SetShowsErrorsInUI(false);
            Log.Info("Advanced Forest Brush 0.1.0 loading");
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN());
            GameManager.instance.localizationManager.AddSource("de-DE", new LocaleDE());
            updateSystem.UpdateAt<ForestBrushShapeSystem>(SystemUpdatePhase.ToolUpdate);
            // ObjectTool writes the vanilla Brush component during ToolUpdate.
            // Run afterwards so the circular preview cannot reappear on top of
            // the square, rectangle or polygon preview.
            updateSystem.UpdateAfter<ForestBrushPreviewSystem, ObjectToolSystem>(
                SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ForestBrushOverlaySystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<AdvancedForestBrushUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateBefore<ForestPlacementFilterSystem>(SystemUpdatePhase.Modification1);
        }

        public void OnDispose()
        {
            Log?.Info("Advanced Forest Brush disposed");
        }
    }
}
