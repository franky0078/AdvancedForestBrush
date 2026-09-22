using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleIT : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleIT(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Advanced Forest Brush" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Generale" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDefaultsGroup), "Valori predefiniti" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "Informazioni" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShape)), "Forma predefinita" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShape)), "Forma selezionata al caricamento di Advanced Forest Brush." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Circle]", "Cerchio" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Square]", "Quadrato" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Rectangle]", "Rettangolo" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Polygon]", "Poligono multipunto" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Dimensione predefinita del cerchio (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Dimensione iniziale del pennello circolare in metri." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShapeWidth)), "Larghezza predefinita della forma (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShapeWidth)), "Larghezza iniziale del quadrato e del rettangolo in metri." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRectangleLength)), "Lunghezza predefinita del rettangolo (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRectangleLength)), "Lunghezza iniziale del rettangolo in metri." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRotation)), "Rotazione predefinita (°)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRotation)), "Rotazione iniziale di quadrato, rettangolo e poligono chiuso." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultDensity)), "Densità predefinita (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultDensity)), "Densità iniziale di posizionamento dal 10 al 300 percento." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribuzione predefinita" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribuzione selezionata al caricamento di Advanced Forest Brush." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Uniform]", "Uniforme" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Natural]", "Naturale" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clusters]", "Gruppi" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clearings]", "Radura" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Edge]", "Margine del bosco" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseScale)), "Dimensione predefinita del rumore (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseScale)), "Dimensione iniziale del modello di distribuzione." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseStrength)), "Irregolarità predefinita (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseStrength)), "Intensità iniziale della variazione casuale fine." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Ripristina valori predefiniti" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)), "Ripristina tutti i valori predefiniti di Advanced Forest Brush." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Versione" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "Mostra la versione attualmente installata." },

                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Apri Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Close", "Chiudi Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Back", "Torna a Tree Controller" },
                { "AdvancedForestBrush.UI.Shape", "Forma" },
                { "AdvancedForestBrush.UI.Circle", "Cerchio" },
                { "AdvancedForestBrush.UI.CircleTooltip", "Normale pennello circolare." },
                { "AdvancedForestBrush.UI.Square", "Quadrato" },
                { "AdvancedForestBrush.UI.SquareTooltip", "Pennello quadrato. Tieni premuto il pulsante destro del mouse e muovi il mouse orizzontalmente per ruotarlo." },
                { "AdvancedForestBrush.UI.Rectangle", "Rettangolo" },
                { "AdvancedForestBrush.UI.RectangleTooltip", "Pennello rettangolare con larghezza e lunghezza separate. Ruotabile." },
                { "AdvancedForestBrush.UI.Polygon", "Poligono multipunto" },
                { "AdvancedForestBrush.UI.PolygonTooltip", "Fai clic con il pulsante sinistro per impostare i punti. Chiudi il poligono facendo clic sul primo punto o con un doppio clic. Poi tieni premuto il pulsante destro del mouse e muovilo orizzontalmente per ruotare il poligono completato. Esc lo reimposta." },
                { "AdvancedForestBrush.UI.BrushSize", "Dimensione pennello" },
                { "AdvancedForestBrush.UI.Size", "Dimensione" },
                { "AdvancedForestBrush.UI.Width", "Larghezza" },
                { "AdvancedForestBrush.UI.Length", "Lunghezza" },
                { "AdvancedForestBrush.UI.Rotation", "Rotazione" },
                { "AdvancedForestBrush.UI.PolygonDrawing", "Disegno" },
                { "AdvancedForestBrush.UI.PolygonReady", "Pronto" },
                { "AdvancedForestBrush.UI.PolygonPoints", "punti" },
                { "AdvancedForestBrush.UI.PolygonReset", "Reimposta poligono" },
                { "AdvancedForestBrush.UI.Density", "Densità" },
                { "AdvancedForestBrush.UI.Distribution", "Distribuzione" },
                { "AdvancedForestBrush.UI.NoiseSize", "Dimensione rumore" },
                { "AdvancedForestBrush.UI.NoiseSizeTooltip", "Controlla la dimensione del modello di rumore. Valori piccoli creano variazioni piccole e frequenti; valori grandi creano gruppi e radure estesi." },
                { "AdvancedForestBrush.UI.Irregularity", "Irregolarità" },
                { "AdvancedForestBrush.UI.IrregularityTooltip", "Controlla quanto i dettagli casuali fini modificano la distribuzione di base. Valori più alti creano bordi più irregolari e naturali." },
                { "AdvancedForestBrush.UI.TreeAge", "Età / dimensione" },
                { "AdvancedForestBrush.UI.PreserveAge", "Mantieni età" },
                { "AdvancedForestBrush.UI.PreserveAgeTooltip", "Impedisce agli alberi appena posizionati di continuare a invecchiare e crescere." },
                { "AdvancedForestBrush.UI.NaturalAge", "Combinazione naturale" },
                { "AdvancedForestBrush.UI.NaturalAgeTooltip", "Seleziona insieme alberelli, alberi giovani, maturi e vecchi. Tree Controller distribuisce casualmente queste età." },
                { "AdvancedForestBrush.UI.Sapling", "Alberello" },
                { "AdvancedForestBrush.UI.SaplingTooltip", "Posiziona alberi nella fase di alberello." },
                { "AdvancedForestBrush.UI.YoungTree", "Giovane" },
                { "AdvancedForestBrush.UI.YoungTreeTooltip", "Posiziona alberi giovani ancora in crescita." },
                { "AdvancedForestBrush.UI.MatureTree", "Maturo" },
                { "AdvancedForestBrush.UI.MatureTreeTooltip", "Posiziona alberi maturi." },
                { "AdvancedForestBrush.UI.ElderlyTree", "Vecchio" },
                { "AdvancedForestBrush.UI.ElderlyTreeTooltip", "Posiziona alberi vecchi." },
                { "AdvancedForestBrush.UI.Decrease", "Diminuisci valore" },
                { "AdvancedForestBrush.UI.Increase", "Aumenta valore" },
                { "AdvancedForestBrush.UI.Uniform", "Uniforme" },
                { "AdvancedForestBrush.UI.UniformTooltip", "Distribuzione uniforme senza rumore." },
                { "AdvancedForestBrush.UI.Natural", "Naturale" },
                { "AdvancedForestBrush.UI.NaturalTooltip", "Variazione della densità morbida e naturale." },
                { "AdvancedForestBrush.UI.Clusters", "Gruppi" },
                { "AdvancedForestBrush.UI.ClustersTooltip", "Vegetazione in gruppi ben distinti." },
                { "AdvancedForestBrush.UI.Clearings", "Radure" },
                { "AdvancedForestBrush.UI.ClearingsTooltip", "Spazi aperti all'interno del bosco." },
                { "AdvancedForestBrush.UI.ForestEdge", "Margine del bosco" },
                { "AdvancedForestBrush.UI.ForestEdgeTooltip", "Transizioni e margini del bosco più densi." },
            };
        }

        public void Unload() { }
    }
}
