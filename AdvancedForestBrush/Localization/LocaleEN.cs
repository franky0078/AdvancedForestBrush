using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Advanced Forest Brush" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDefaultsGroup), "Default values" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "Information" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShape)), "Default shape" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShape)), "Shape selected when Advanced Forest Brush is loaded." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Circle]", "Circle" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Square]", "Square" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Rectangle]", "Rectangle" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Polygon]", "Multipoint polygon" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Default circle size (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Initial size of the circular brush in metres." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShapeWidth)), "Default shape width (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShapeWidth)), "Initial width of the square and rectangle in metres." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRectangleLength)), "Default rectangle length (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRectangleLength)), "Initial length of the rectangle in metres." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRotation)), "Default rotation (°)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRotation)), "Initial rotation of square and rectangle." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultDensity)), "Default density (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultDensity)), "Initial placement density from 10 to 300 percent." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseMode)), "Default distribution" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribution selected when Advanced Forest Brush is loaded." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Uniform]", "Uniform" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Natural]", "Natural" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clusters]", "Clusters" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clearings]", "Clearings" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Edge]", "Forest edge" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseScale)), "Default noise size (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseScale)), "Initial size of the distribution pattern." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseStrength)), "Default irregularity (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseStrength)), "Initial strength of the fine random variation." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Reset default values" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)), "Restores all Advanced Forest Brush defaults." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "Shows the currently installed version." },

                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Open Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Close", "Close Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Back", "Back to Tree Controller" },
                { "AdvancedForestBrush.UI.Shape", "Shape" },
                { "AdvancedForestBrush.UI.Circle", "Circle" },
                { "AdvancedForestBrush.UI.CircleTooltip", "Normal round brush." },
                { "AdvancedForestBrush.UI.Square", "Square" },
                { "AdvancedForestBrush.UI.SquareTooltip", "Square brush. Hold the right mouse button and move the mouse horizontally to rotate." },
                { "AdvancedForestBrush.UI.Rectangle", "Rectangle" },
                { "AdvancedForestBrush.UI.RectangleTooltip", "Rectangular brush with separate width and length. Rotatable." },
                { "AdvancedForestBrush.UI.Polygon", "Multipoint polygon" },
                { "AdvancedForestBrush.UI.PolygonTooltip", "Left-click to set points. Close the polygon by clicking the first point or double-clicking. The finished shape then follows the mouse pointer like a stamp. Right-click edits it; Esc resets it." },
                { "AdvancedForestBrush.UI.BrushSize", "Brush size" },
                { "AdvancedForestBrush.UI.Size", "Size" },
                { "AdvancedForestBrush.UI.Width", "Width" },
                { "AdvancedForestBrush.UI.Length", "Length" },
                { "AdvancedForestBrush.UI.Rotation", "Rotation" },
                { "AdvancedForestBrush.UI.PolygonDrawing", "Drawing" },
                { "AdvancedForestBrush.UI.PolygonReady", "Ready" },
                { "AdvancedForestBrush.UI.PolygonPoints", "points" },
                { "AdvancedForestBrush.UI.PolygonReset", "Reset polygon" },
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
