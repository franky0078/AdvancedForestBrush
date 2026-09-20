using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleEN : IDictionarySource
    {
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Open Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Close", "Close Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.BrushSize", "Brush size" },
                { "AdvancedForestBrush.UI.Density", "Density" },
                { "AdvancedForestBrush.UI.Distribution", "Distribution" },
                { "AdvancedForestBrush.UI.NoiseSize", "Noise size" },
                { "AdvancedForestBrush.UI.NoiseSizeTooltip", "Controls the size of the noise pattern. Small values create frequent small variations; large values create broad clusters and clearings." },
                { "AdvancedForestBrush.UI.Irregularity", "Irregularity" },
                { "AdvancedForestBrush.UI.IrregularityTooltip", "Controls how strongly fine random details alter the base distribution. Higher values create more irregular and natural edges." },
                { "AdvancedForestBrush.UI.Decrease", "Decrease value" },
                { "AdvancedForestBrush.UI.Increase", "Increase value" },
                { "AdvancedForestBrush.UI.Uniform", "Uniform" },
                { "AdvancedForestBrush.UI.UniformTooltip", "Even distribution without noise." },
                { "AdvancedForestBrush.UI.Natural", "Natural" },
                { "AdvancedForestBrush.UI.NaturalTooltip", "Soft, natural density variation." },
                { "AdvancedForestBrush.UI.Clusters", "Clusters" },
                { "AdvancedForestBrush.UI.ClustersTooltip", "Vegetation in distinct clusters." },
                { "AdvancedForestBrush.UI.Clearings", "Clearings" },
                { "AdvancedForestBrush.UI.ClearingsTooltip", "Open spaces inside the forest." },
                { "AdvancedForestBrush.UI.ForestEdge", "Forest edge" },
                { "AdvancedForestBrush.UI.ForestEdgeTooltip", "Denser transitions and forest edges." },
            };
        }

        public void Unload() { }
    }
}
