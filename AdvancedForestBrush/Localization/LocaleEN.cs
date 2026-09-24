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
                { m_Setting.GetOptionTabLocaleID(Setting.kControlsSection), "Key bindings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kControlsGroup), "Key bindings" },
                { m_Setting.GetBindingMapLocaleID(), "Advanced Forest Brush" },
                { m_Setting.GetBindingKeyLocaleID(Setting.kRotateAction), "Rotate brush" },
                { m_Setting.GetBindingKeyHintLocaleID(Setting.kRotateAction), "Rotate brush" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RotateMouse)), "Rotate brush (mouse)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RotateMouse)), "Hold the assigned mouse buttons and move horizontally to rotate a square, rectangle or closed polygon." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RotateKeyboard)), "Rotate brush (keyboard)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RotateKeyboard)), "Hold the assigned key and move the mouse horizontally to rotate a square, rectangle or closed polygon." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetBindings)), "Reset bindings" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetBindings)), "Restores the default rotation bindings." },
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
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRotation)), "Initial rotation of square, rectangle and a closed polygon." },
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
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultSpeciesGrouping)), "Default species grouping" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultSpeciesGrouping)), "0 = off, 1 = weak, 2 = medium, 3 = strong. Changes the spatial placement of species selected in Tree Controller." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Off]", "Off" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Weak]", "Weak" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Medium]", "Medium" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Strong]", "Strong" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Reset default values" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)), "Restores all Advanced Forest Brush defaults." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "Shows the currently installed version." },

                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Open Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Close", "Close Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Anarchy", "Anarchy" },
                { "AdvancedForestBrush.UI.AnarchyTooltip", "Toggles Anarchy for brush placement. Requires the Anarchy mod." },
                { "AdvancedForestBrush.UI.Back", "Back to Tree Controller" },
                { "AdvancedForestBrush.UI.Shape", "Shape" },
                { "AdvancedForestBrush.UI.Circle", "Circle" },
                { "AdvancedForestBrush.UI.CircleTooltip", "Normal round brush." },
                { "AdvancedForestBrush.UI.Square", "Square" },
                { "AdvancedForestBrush.UI.SquareTooltip", "Square brush. Hold the rotation control configured in Options and move the mouse horizontally. The erase control removes vegetation." },
                { "AdvancedForestBrush.UI.Rectangle", "Rectangle" },
                { "AdvancedForestBrush.UI.RectangleTooltip", "Rectangular brush with separate width and length. Use the rotation control configured in Options to rotate." },
                { "AdvancedForestBrush.UI.Polygon", "Multipoint polygon" },
                { "AdvancedForestBrush.UI.PolygonTooltip", "Left-click to set points; Backspace removes the last point. Click the first point or double-click to close. Use the rotation control configured in Options to rotate the closed polygon. The erase control removes vegetation. Esc resets it." },
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
                { "AdvancedForestBrush.UI.SpeciesGrouping", "Species grouping" },
                { "AdvancedForestBrush.UI.SpeciesGroupingTooltip", "Favors patches of the selected species. Stronger grouping keeps fewer trees; increase density if needed." },
                { "AdvancedForestBrush.UI.GroupingOff", "Off" },
                { "AdvancedForestBrush.UI.GroupingWeak", "Weak" },
                { "AdvancedForestBrush.UI.GroupingMedium", "Medium" },
                { "AdvancedForestBrush.UI.GroupingStrong", "Strong" },
                { "AdvancedForestBrush.UI.TreeAge", "Age / size" },
                { "AdvancedForestBrush.UI.PreserveAge", "Preserve age" },
                { "AdvancedForestBrush.UI.PreserveAgeTooltip", "Prevents newly placed trees from continuing to age and grow." },
                { "AdvancedForestBrush.UI.NaturalAge", "Natural mix" },
                { "AdvancedForestBrush.UI.NaturalAgeTooltip", "Selects sapling, young, mature and old trees together. Tree Controller distributes these ages randomly." },
                { "AdvancedForestBrush.UI.Sapling", "Sapling" },
                { "AdvancedForestBrush.UI.SaplingTooltip", "Places trees in the sapling growth phase." },
                { "AdvancedForestBrush.UI.YoungTree", "Young" },
                { "AdvancedForestBrush.UI.YoungTreeTooltip", "Places young, still-growing trees." },
                { "AdvancedForestBrush.UI.MatureTree", "Mature" },
                { "AdvancedForestBrush.UI.MatureTreeTooltip", "Places mature trees." },
                { "AdvancedForestBrush.UI.ElderlyTree", "Old" },
                { "AdvancedForestBrush.UI.ElderlyTreeTooltip", "Places old trees." },
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
