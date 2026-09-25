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
        public const string ModVersion = "0.9.0";
        internal static ILog Log { get; private set; }
        public static Setting Settings { get; private set; }

        public static bool DiagnosticLoggingEnabled =>
            Settings?.EnableDiagnosticLogging ?? false;

        public static void LogDiagnosticInfo(string message)
        {
            if (!DiagnosticLoggingEnabled || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                Log?.Info(message);
            }
            catch
            {
                // A logging failure must not interrupt the brush.
            }
        }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log = LogManager.GetLogger("AdvancedForestBrush").SetShowsErrorsInUI(false);
            Log.Info($"Advanced Forest Brush {ModVersion} loading");

            Settings = new Setting(this);
            Settings.RegisterKeyBindings();
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
            // Suppress the ObjectTool brush before it processes a rotation drag.
            updateSystem.UpdateBefore<ForestBrushShapeSystem, ObjectToolSystem>(
                SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAfter<ForestBrushPreviewSystem, ObjectToolSystem>(
                SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ForestBrushOverlaySystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<AdvancedForestBrushUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<ForestBrushTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateBefore<ForestPlacementFilterSystem>(SystemUpdatePhase.Modification1);
            updateSystem.UpdateBefore<ForestSpeciesPlacementSystem, GenerateObjectsSystem>(
                SystemUpdatePhase.Modification1);
        }

        public void OnDispose()
        {
            Log?.Info("Advanced Forest Brush disposed");
            ForestBrushInput.Deactivate();

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
