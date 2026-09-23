using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleES(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Advanced Forest Brush" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "General" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDefaultsGroup), "Valores predeterminados" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "Información" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShape)), "Forma predeterminada" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShape)), "Forma seleccionada al cargar Advanced Forest Brush." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Circle]", "Círculo" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Square]", "Cuadrado" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Rectangle]", "Rectángulo" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Polygon]", "Polígono multipunto" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Tamaño predeterminado del círculo (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Tamaño inicial del pincel circular en metros." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShapeWidth)), "Anchura predeterminada de la forma (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShapeWidth)), "Anchura inicial del cuadrado y del rectángulo en metros." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRectangleLength)), "Longitud predeterminada del rectángulo (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRectangleLength)), "Longitud inicial del rectángulo en metros." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRotation)), "Rotación predeterminada (°)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRotation)), "Rotación inicial del cuadrado, del rectángulo y de un polígono cerrado." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultDensity)), "Densidad predeterminada (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultDensity)), "Densidad inicial de colocación del 10 al 300 por ciento." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribución predeterminada" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribución seleccionada al cargar Advanced Forest Brush." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Uniform]", "Uniforme" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Natural]", "Natural" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clusters]", "Grupos" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clearings]", "Claros" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Edge]", "Borde del bosque" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseScale)), "Tamaño predeterminado del ruido (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseScale)), "Tamaño inicial del patrón de distribución." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseStrength)), "Irregularidad predeterminada (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseStrength)), "Intensidad inicial de la variación aleatoria fina." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultSpeciesGrouping)), "Agrupación de especies predeterminada" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultSpeciesGrouping)), "0 = desactivada, 1 = suave, 2 = media, 3 = fuerte. Distribuye espacialmente las especies seleccionadas en Tree Controller." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Off]", "Desactivada" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Weak]", "Suave" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Medium]", "Media" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTSPECIESGROUPING[Strong]", "Fuerte" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Restablecer valores predeterminados" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)), "Restablece todos los valores predeterminados de Advanced Forest Brush." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Versión" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "Muestra la versión instalada actualmente." },

                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Abrir Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Close", "Cerrar Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Back", "Volver a Tree Controller" },
                { "AdvancedForestBrush.UI.Shape", "Forma" },
                { "AdvancedForestBrush.UI.Circle", "Círculo" },
                { "AdvancedForestBrush.UI.CircleTooltip", "Pincel redondo normal." },
                { "AdvancedForestBrush.UI.Square", "Cuadrado" },
                { "AdvancedForestBrush.UI.SquareTooltip", "Pincel cuadrado. Mantén Ctrl y el botón derecho del ratón y muévelo horizontalmente para girar. El clic derecho borra vegetación." },
                { "AdvancedForestBrush.UI.Rectangle", "Rectángulo" },
                { "AdvancedForestBrush.UI.RectangleTooltip", "Pincel rectangular con anchura y longitud independientes. Mantén Ctrl y arrastra con el botón derecho para girar." },
                { "AdvancedForestBrush.UI.Polygon", "Polígono multipunto" },
                { "AdvancedForestBrush.UI.PolygonTooltip", "Clic izquierdo para colocar puntos; Retroceso elimina el último. Clic en el primer punto o doble clic para cerrar. Mantén Ctrl y arrastra con el botón derecho para girar el polígono cerrado. El clic derecho borra vegetación. Esc lo restablece." },
                { "AdvancedForestBrush.UI.BrushSize", "Tamaño del pincel" },
                { "AdvancedForestBrush.UI.Size", "Tamaño" },
                { "AdvancedForestBrush.UI.Width", "Anchura" },
                { "AdvancedForestBrush.UI.Length", "Longitud" },
                { "AdvancedForestBrush.UI.Rotation", "Rotación" },
                { "AdvancedForestBrush.UI.PolygonDrawing", "Dibujando" },
                { "AdvancedForestBrush.UI.PolygonReady", "Listo" },
                { "AdvancedForestBrush.UI.PolygonPoints", "puntos" },
                { "AdvancedForestBrush.UI.PolygonReset", "Restablecer polígono" },
                { "AdvancedForestBrush.UI.Density", "Densidad" },
                { "AdvancedForestBrush.UI.Distribution", "Distribución" },
                { "AdvancedForestBrush.UI.NoiseSize", "Tamaño del ruido" },
                { "AdvancedForestBrush.UI.NoiseSizeTooltip", "Controla el tamaño del patrón de ruido. Los valores pequeños crean variaciones pequeñas y frecuentes; los valores grandes crean grupos y claros amplios." },
                { "AdvancedForestBrush.UI.Irregularity", "Irregularidad" },
                { "AdvancedForestBrush.UI.IrregularityTooltip", "Controla cuánto modifican los detalles aleatorios finos la distribución base. Los valores más altos crean bordes más irregulares y naturales." },
                { "AdvancedForestBrush.UI.SpeciesGrouping", "Agrupación de especies" },
                { "AdvancedForestBrush.UI.SpeciesGroupingTooltip", "Agrupa las especies seleccionadas en zonas. Una agrupación mayor reduce el número de árboles; aumenta la densidad si es necesario." },
                { "AdvancedForestBrush.UI.GroupingOff", "No" },
                { "AdvancedForestBrush.UI.GroupingWeak", "Suave" },
                { "AdvancedForestBrush.UI.GroupingMedium", "Media" },
                { "AdvancedForestBrush.UI.GroupingStrong", "Fuerte" },
                { "AdvancedForestBrush.UI.TreeAge", "Edad / tamaño" },
                { "AdvancedForestBrush.UI.PreserveAge", "Conservar edad" },
                { "AdvancedForestBrush.UI.PreserveAgeTooltip", "Impide que los árboles recién colocados sigan envejeciendo y creciendo." },
                { "AdvancedForestBrush.UI.NaturalAge", "Mezcla natural" },
                { "AdvancedForestBrush.UI.NaturalAgeTooltip", "Selecciona conjuntamente árboles recién plantados, jóvenes, maduros y viejos. Tree Controller distribuye estas edades al azar." },
                { "AdvancedForestBrush.UI.Sapling", "Plantón" },
                { "AdvancedForestBrush.UI.SaplingTooltip", "Coloca árboles en la fase de plantón." },
                { "AdvancedForestBrush.UI.YoungTree", "Joven" },
                { "AdvancedForestBrush.UI.YoungTreeTooltip", "Coloca árboles jóvenes que todavía están creciendo." },
                { "AdvancedForestBrush.UI.MatureTree", "Maduro" },
                { "AdvancedForestBrush.UI.MatureTreeTooltip", "Coloca árboles maduros." },
                { "AdvancedForestBrush.UI.ElderlyTree", "Viejo" },
                { "AdvancedForestBrush.UI.ElderlyTreeTooltip", "Coloca árboles viejos." },
                { "AdvancedForestBrush.UI.Decrease", "Disminuir valor" },
                { "AdvancedForestBrush.UI.Increase", "Aumentar valor" },
                { "AdvancedForestBrush.UI.Uniform", "Uniforme" },
                { "AdvancedForestBrush.UI.UniformTooltip", "Distribución uniforme sin ruido." },
                { "AdvancedForestBrush.UI.Natural", "Natural" },
                { "AdvancedForestBrush.UI.NaturalTooltip", "Variación de densidad suave y natural." },
                { "AdvancedForestBrush.UI.Clusters", "Grupos" },
                { "AdvancedForestBrush.UI.ClustersTooltip", "Vegetación en grupos bien diferenciados." },
                { "AdvancedForestBrush.UI.Clearings", "Claros" },
                { "AdvancedForestBrush.UI.ClearingsTooltip", "Espacios abiertos dentro del bosque." },
                { "AdvancedForestBrush.UI.ForestEdge", "Borde del bosque" },
                { "AdvancedForestBrush.UI.ForestEdgeTooltip", "Transiciones y bordes del bosque más densos." },
            };
        }

        public void Unload() { }
    }
}
