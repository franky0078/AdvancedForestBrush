using System;
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
        public static readonly string ModVersion =
            typeof(Mod).Assembly.GetName().Version.ToString(3);
        internal static ILog Log { get; private set; }
        public static Setting Settings { get; private set; }
        private static Func<bool> s_SingleTreeAddonEnabled;

        internal static bool SingleTreeAddonEnabled => s_SingleTreeAddonEnabled?.Invoke() == true;

        public static void RegisterSingleTreeAddon(Func<bool> isEnabled)
        {
            s_SingleTreeAddonEnabled = isEnabled;
        }

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
            GameManager.instance.localizationManager.AddSource("ru-RU", new LocaleRU(Settings));
            GameManager.instance.localizationManager.AddSource("pl-PL", new LocalePL(Settings));
            GameManager.instance.localizationManager.AddSource("pt-BR", new LocalePTBR(Settings));
            GameManager.instance.localizationManager.AddSource("ja-JP", new LocaleJA(Settings));
            GameManager.instance.localizationManager.AddSource("ko-KR", new LocaleKO(Settings));
            GameManager.instance.localizationManager.AddSource("zh-HANS", new LocaleZHHANS(Settings));
            GameManager.instance.localizationManager.AddSource("zh-HANT", new LocaleZHHANT(Settings));

            AssetDatabase.global.LoadSettings(
                nameof(AdvancedForestBrush),
                Settings,
                new Setting(this));

            Settings.ApplyToState();
            updateSystem.UpdateBefore<ForestPathNetworkSystem, ForestBrushShapeSystem>(
                SystemUpdatePhase.ToolUpdate);
            // Suppress the ObjectTool brush before it processes a rotation drag.
            updateSystem.UpdateBefore<ForestBrushShapeSystem, ObjectToolSystem>(
                SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAfter<ForestBrushPreviewSystem, ObjectToolSystem>(
                SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ForestBrushOverlaySystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<AdvancedForestBrushUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<ForestBrushTooltipSystem>(SystemUpdatePhase.UITooltip);
            updateSystem.UpdateBefore<ForestPlacementFilterSystem, ForestSpeciesPlacementSystem>(
                SystemUpdatePhase.Modification1);
            updateSystem.UpdateBefore<ForestSpeciesPlacementSystem, GenerateObjectsSystem>(
                SystemUpdatePhase.Modification1);
            updateSystem.UpdateBefore<ForestTreePreserveAgeSystem, ApplyObjectsSystem>(
                SystemUpdatePhase.ApplyTool);
            updateSystem.UpdateBefore<ForestSinglePlantRotationSystem>(SystemUpdatePhase.PostTool);
        }

        public void OnDispose()
        {
            Log?.Info("Advanced Forest Brush disposed");
            ForestBrushInput.Deactivate();
            s_SingleTreeAddonEnabled = null;

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
