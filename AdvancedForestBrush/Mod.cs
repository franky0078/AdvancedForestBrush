using Colossal.Logging;
using Colossal.IO.AssetDatabase;
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
        public const string ModVersion = "0.6.0";
        internal static ILog Log { get; private set; }
        public static Setting Settings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log = LogManager.GetLogger("AdvancedForestBrush").SetShowsErrorsInUI(false);
            Log.Info($"Advanced Forest Brush {ModVersion} loading");

            Settings = new Setting(this);
            Settings.RegisterInOptionsUI();

            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            GameManager.instance.localizationManager.AddSource("de-DE", new LocaleDE(Settings));
            GameManager.instance.localizationManager.AddSource("es-ES", new LocaleES(Settings));
            GameManager.instance.localizationManager.AddSource("it-IT", new LocaleIT(Settings));
            GameManager.instance.localizationManager.AddSource("fr-FR", new LocaleFR(Settings));

            AssetDatabase.global.LoadSettings(
                nameof(AdvancedForestBrush),
                Settings,
                new Setting(this));

            Settings.ApplyToState();
            // Suppress the ObjectTool brush before it processes Ctrl + right-drag.
            updateSystem.UpdateBefore<ForestBrushShapeSystem, ObjectToolSystem>(
                SystemUpdatePhase.ToolUpdate);
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

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
