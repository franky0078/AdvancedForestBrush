using Colossal;
using System.Collections.Generic;

namespace AdvancedForestBrush.Localization
{
    public sealed class LocaleFR : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleFR(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Advanced Forest Brush" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Général" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDefaultsGroup), "Valeurs par défaut" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "Informations" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShape)), "Forme par défaut" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShape)), "Forme sélectionnée au chargement d’Advanced Forest Brush." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Circle]", "Cercle" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Square]", "Carré" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Rectangle]", "Rectangle" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTBRUSHSHAPE[Polygon]", "Polygone multipoint" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Taille du cercle par défaut (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultCircleBrushSize)), "Taille initiale de la brosse circulaire en mètres." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultShapeWidth)), "Largeur de forme par défaut (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultShapeWidth)), "Largeur initiale du carré et du rectangle en mètres." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRectangleLength)), "Longueur du rectangle par défaut (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRectangleLength)), "Longueur initiale du rectangle en mètres." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultRotation)), "Rotation par défaut (°)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultRotation)), "Rotation initiale du carré, du rectangle et d’un polygone fermé." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultDensity)), "Densité par défaut (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultDensity)), "Densité initiale de placement de 10 à 300 pour cent." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribution par défaut" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseMode)), "Distribution sélectionnée au chargement d’Advanced Forest Brush." },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Uniform]", "Uniforme" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Natural]", "Naturelle" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clusters]", "Groupes" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Clearings]", "Clairières" },
                { "Options.AdvancedForestBrush.AdvancedForestBrush.Mod.FORESTNOISEMODE[Edge]", "Lisière de forêt" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseScale)), "Taille du bruit par défaut (m)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseScale)), "Taille initiale du motif de distribution." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DefaultNoiseStrength)), "Irrégularité par défaut (%)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DefaultNoiseStrength)), "Intensité initiale de la variation aléatoire fine." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Rétablir les valeurs par défaut" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)), "Rétablit toutes les valeurs par défaut d’Advanced Forest Brush." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "Affiche la version actuellement installée." },

                { "AdvancedForestBrush.UI.Title", "Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Open", "Ouvrir Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Close", "Fermer Advanced Forest Brush" },
                { "AdvancedForestBrush.UI.Back", "Retour à Tree Controller" },
                { "AdvancedForestBrush.UI.Shape", "Forme" },
                { "AdvancedForestBrush.UI.Circle", "Cercle" },
                { "AdvancedForestBrush.UI.CircleTooltip", "Brosse ronde normale." },
                { "AdvancedForestBrush.UI.Square", "Carré" },
                { "AdvancedForestBrush.UI.SquareTooltip", "Brosse carrée. Maintenez le bouton droit de la souris et déplacez-la horizontalement pour effectuer une rotation." },
                { "AdvancedForestBrush.UI.Rectangle", "Rectangle" },
                { "AdvancedForestBrush.UI.RectangleTooltip", "Brosse rectangulaire avec largeur et longueur distinctes. Peut être tournée." },
                { "AdvancedForestBrush.UI.Polygon", "Polygone multipoint" },
                { "AdvancedForestBrush.UI.PolygonTooltip", "Cliquez avec le bouton gauche pour placer des points. Fermez le polygone en cliquant sur le premier point ou en double-cliquant. Maintenez ensuite le bouton droit de la souris et déplacez-la horizontalement pour faire pivoter le polygone terminé. Échap le réinitialise." },
                { "AdvancedForestBrush.UI.BrushSize", "Taille de la brosse" },
                { "AdvancedForestBrush.UI.Size", "Taille" },
                { "AdvancedForestBrush.UI.Width", "Largeur" },
                { "AdvancedForestBrush.UI.Length", "Longueur" },
                { "AdvancedForestBrush.UI.Rotation", "Rotation" },
                { "AdvancedForestBrush.UI.PolygonDrawing", "Tracé" },
                { "AdvancedForestBrush.UI.PolygonReady", "Prêt" },
                { "AdvancedForestBrush.UI.PolygonPoints", "points" },
                { "AdvancedForestBrush.UI.PolygonReset", "Réinitialiser le polygone" },
                { "AdvancedForestBrush.UI.Density", "Densité" },
                { "AdvancedForestBrush.UI.Distribution", "Distribution" },
                { "AdvancedForestBrush.UI.NoiseSize", "Taille du bruit" },
                { "AdvancedForestBrush.UI.NoiseSizeTooltip", "Contrôle la taille du motif de bruit. Les petites valeurs créent de petites variations fréquentes ; les grandes valeurs créent de vastes groupes et clairières." },
                { "AdvancedForestBrush.UI.Irregularity", "Irrégularité" },
                { "AdvancedForestBrush.UI.IrregularityTooltip", "Contrôle l’influence des détails aléatoires fins sur la distribution de base. Des valeurs élevées créent des bordures plus irrégulières et naturelles." },
                { "AdvancedForestBrush.UI.TreeAge", "Âge / taille" },
                { "AdvancedForestBrush.UI.PreserveAge", "Conserver l’âge" },
                { "AdvancedForestBrush.UI.PreserveAgeTooltip", "Empêche les arbres nouvellement placés de continuer à vieillir et à grandir." },
                { "AdvancedForestBrush.UI.NaturalAge", "Mélange naturel" },
                { "AdvancedForestBrush.UI.NaturalAgeTooltip", "Sélectionne ensemble des jeunes plants, de jeunes arbres, des arbres adultes et de vieux arbres. Tree Controller répartit ces âges aléatoirement." },
                { "AdvancedForestBrush.UI.Sapling", "Jeune plant" },
                { "AdvancedForestBrush.UI.SaplingTooltip", "Place les arbres au stade de jeune plant." },
                { "AdvancedForestBrush.UI.YoungTree", "Jeune" },
                { "AdvancedForestBrush.UI.YoungTreeTooltip", "Place de jeunes arbres encore en croissance." },
                { "AdvancedForestBrush.UI.MatureTree", "Adulte" },
                { "AdvancedForestBrush.UI.MatureTreeTooltip", "Place des arbres adultes." },
                { "AdvancedForestBrush.UI.ElderlyTree", "Vieux" },
                { "AdvancedForestBrush.UI.ElderlyTreeTooltip", "Place de vieux arbres." },
                { "AdvancedForestBrush.UI.Decrease", "Diminuer la valeur" },
                { "AdvancedForestBrush.UI.Increase", "Augmenter la valeur" },
                { "AdvancedForestBrush.UI.Uniform", "Uniforme" },
                { "AdvancedForestBrush.UI.UniformTooltip", "Distribution uniforme sans bruit." },
                { "AdvancedForestBrush.UI.Natural", "Naturelle" },
                { "AdvancedForestBrush.UI.NaturalTooltip", "Variation de densité douce et naturelle." },
                { "AdvancedForestBrush.UI.Clusters", "Groupes" },
                { "AdvancedForestBrush.UI.ClustersTooltip", "Végétation répartie en groupes distincts." },
                { "AdvancedForestBrush.UI.Clearings", "Clairières" },
                { "AdvancedForestBrush.UI.ClearingsTooltip", "Espaces ouverts à l’intérieur de la forêt." },
                { "AdvancedForestBrush.UI.ForestEdge", "Lisière de forêt" },
                { "AdvancedForestBrush.UI.ForestEdgeTooltip", "Transitions et lisières de forêt plus denses." },
            };
        }

        public void Unload() { }
    }
}
