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
