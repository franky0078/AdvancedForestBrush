using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleDE : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleDE(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Advanced Forest Brush" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Allgemein" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDefaultsGroup), "Standardwerte" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "Informationen" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShape)), "Standardform" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShape)), "Form, die beim Laden von Advanced Forest Brush ausgewählt wird." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Circle]", "Kreis" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Square]", "Quadrat" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Rectangle]", "Rechteck" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Polygon]", "Multipoint-Polygon" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Standard-Kreisgröße (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Anfängliche Größe des Kreispinsels in Metern." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShapeWidth)), "Standardbreite der Form (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShapeWidth)), "Anfängliche Breite von Quadrat und Rechteck in Metern." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRectangleLength)), "Standardlänge des Rechtecks (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRectangleLength)), "Anfängliche Länge des Rechtecks in Metern." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRotation)), "Standarddrehung (°)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRotation)), "Anfängliche Drehung von Quadrat und Rechteck." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultDensity)), "Standarddichte (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultDensity)), "Anfängliche Platzierungsdichte von 10 bis 300 Prozent." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseMode)), "Standardverteilung" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseMode)), "Verteilung, die beim Laden von Advanced Forest Brush ausgewählt wird." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Uniform]", "Gleichmäßig" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Natural]", "Natürlich" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clusters]", "Gruppen" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clearings]", "Lichtungen" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Edge]", "Waldrand" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseScale)), "Standard-Noise-Größe (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseScale)), "Anfängliche Größe des Verteilungsmusters." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseStrength)), "Standard-Unregelmäßigkeit (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseStrength)), "Anfängliche Stärke der feinen Zufallsabweichungen." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Standardwerte zurücksetzen" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)), "Setzt alle Standardwerte von Advanced Forest Brush zurück." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "Zeigt die aktuell installierte Version an." },

                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Advanced Forest Brush öffnen" },
                { "AdvancedForestBrush.UI.Close", "Advanced Forest Brush schließen" },
                { "AdvancedForestBrush.UI.Back", "Zurück zum Tree Controller" },
                { "AdvancedForestBrush.UI.Shape", "Form" },
                { "AdvancedForestBrush.UI.Circle", "Kreis" },
                { "AdvancedForestBrush.UI.CircleTooltip", "Normaler runder Pinsel." },
                { "AdvancedForestBrush.UI.Square", "Quadrat" },
                { "AdvancedForestBrush.UI.SquareTooltip", "Quadratischer Pinsel. Zum Drehen die rechte Maustaste halten und die Maus waagerecht bewegen." },
                { "AdvancedForestBrush.UI.Rectangle", "Rechteck" },
                { "AdvancedForestBrush.UI.RectangleTooltip", "Rechteckiger Pinsel mit getrennter Breite und Länge. Drehbar." },
                { "AdvancedForestBrush.UI.Polygon", "Multipoint-Polygon" },
                { "AdvancedForestBrush.UI.PolygonTooltip", "Linksklick setzt Punkte. Klick auf den ersten Punkt oder Doppelklick schließt das Polygon. Die fertige Form hängt danach wie ein Stempel am Mauszeiger. Rechtsklick bearbeitet sie, Esc setzt sie zurück." },
                { "AdvancedForestBrush.UI.BrushSize", "Pinselgröße" },
                { "AdvancedForestBrush.UI.Size", "Größe" },
                { "AdvancedForestBrush.UI.Width", "Breite" },
                { "AdvancedForestBrush.UI.Length", "Länge" },
                { "AdvancedForestBrush.UI.Rotation", "Drehung" },
                { "AdvancedForestBrush.UI.PolygonDrawing", "Wird gezeichnet" },
                { "AdvancedForestBrush.UI.PolygonReady", "Bestätigt" },
                { "AdvancedForestBrush.UI.PolygonPoints", "Punkte" },
                { "AdvancedForestBrush.UI.PolygonReset", "Polygon zurücksetzen" },
                { "AdvancedForestBrush.UI.Density", "Dichte" },
                { "AdvancedForestBrush.UI.Distribution", "Verteilung" },
                { "AdvancedForestBrush.UI.NoiseSize", "Noise-Größe" },
                { "AdvancedForestBrush.UI.NoiseSizeTooltip", "Bestimmt die Größe der Noise-Strukturen. Kleine Werte erzeugen häufige, kleine Wechsel; große Werte erzeugen weitläufige Gruppen und Lichtungen." },
                { "AdvancedForestBrush.UI.Irregularity", "Unregelmäßigkeit" },
                { "AdvancedForestBrush.UI.IrregularityTooltip", "Bestimmt, wie stark feine Zufallsdetails die Grundverteilung verändern. Höhere Werte erzeugen unregelmäßigere und natürlichere Ränder." },
                { "AdvancedForestBrush.UI.Decrease", "Wert verringern" },
                { "AdvancedForestBrush.UI.Increase", "Wert erhöhen" },
                { "AdvancedForestBrush.UI.Uniform", "Gleichmäßig" },
                { "AdvancedForestBrush.UI.UniformTooltip", "Gleichmäßige Verteilung ohne Noise." },
                { "AdvancedForestBrush.UI.Natural", "Natürlich" },
                { "AdvancedForestBrush.UI.NaturalTooltip", "Weiche, natürliche Dichteschwankungen." },
                { "AdvancedForestBrush.UI.Clusters", "Gruppen" },
                { "AdvancedForestBrush.UI.ClustersTooltip", "Vegetation wird in deutlich erkennbaren Gruppen platziert." },
                { "AdvancedForestBrush.UI.Clearings", "Lichtungen" },
                { "AdvancedForestBrush.UI.ClearingsTooltip", "Erzeugt freie Flächen innerhalb des Waldes." },
                { "AdvancedForestBrush.UI.ForestEdge", "Waldrand" },
                { "AdvancedForestBrush.UI.ForestEdgeTooltip", "Erzeugt dichtere Übergänge und natürlich wirkende Waldränder." },
            };
        }

        public void Unload() { }
    }
}
