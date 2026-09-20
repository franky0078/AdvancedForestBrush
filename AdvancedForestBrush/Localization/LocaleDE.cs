using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleDE : IDictionarySource
    {
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Advanced Forest Brush öffnen" },
                { "AdvancedForestBrush.UI.Close", "Advanced Forest Brush schließen" },
                { "AdvancedForestBrush.UI.BrushSize", "Pinselgröße" },
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
