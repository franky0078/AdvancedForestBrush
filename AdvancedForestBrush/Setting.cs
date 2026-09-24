using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Input;
using Game.Settings;
using Unity.Mathematics;

namespace AdvancedForestBrush
{
    [FileLocation(nameof(AdvancedForestBrush))]
    [SettingsUITabOrder(kSection, kControlsSection)]
    [SettingsUIGroupOrder(kDefaultsGroup, kControlsGroup, kAboutGroup)]
    [SettingsUIShowGroupName(kDefaultsGroup, kControlsGroup, kAboutGroup)]
    [SettingsUIMouseAction(kRotateAction, ActionType.Button,
        SettingsUIInputActionAttribute.kDefaultRebindOptions,
        ModifierOptions.Allow, true, usages: new[] { Usages.kToolUsage })]
    [SettingsUIKeyboardAction(kRotateAction, ActionType.Button,
        SettingsUIInputActionAttribute.kDefaultRebindOptions,
        ModifierOptions.Allow, true, usages: new[] { Usages.kToolUsage })]
    public sealed class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kControlsSection = "Controls";
        public const string kDefaultsGroup = "Defaults";
        public const string kControlsGroup = "Bindings";
        public const string kAboutGroup = "About";
        public const string kRotateAction = "RotateBrush";

        [SettingsUISection(kControlsSection, kControlsGroup)]
        [SettingsUIMouseBinding(BindingMouse.Right, kRotateAction, ctrl: true)]
        public ProxyBinding RotateMouse { get; set; }

        [SettingsUISection(kControlsSection, kControlsGroup)]
        [SettingsUIKeyboardBinding(BindingKeyboard.None, kRotateAction)]
        public ProxyBinding RotateKeyboard { get; set; }

        [SettingsUISection(kControlsSection, kControlsGroup)]
        public bool ResetBindings
        {
            set
            {
                if (value)
                {
                    ResetKeyBindings();
                }
            }
        }

        private ForestBrushShape m_DefaultShape;
        private int m_DefaultCircleBrushSize;
        private int m_DefaultShapeWidth;
        private int m_DefaultRectangleLength;
        private int m_DefaultRotation;
        private int m_DefaultDensity;
        private ForestNoiseMode m_DefaultNoiseMode;
        private int m_DefaultNoiseScale;
        private int m_DefaultNoiseStrength;
        private ForestSpeciesGrouping m_DefaultSpeciesGrouping;
        private bool m_EnableDiagnosticLogging;

        public Setting(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUISection(kSection, kDefaultsGroup)]
        public ForestBrushShape DefaultShape
        {
            get => m_DefaultShape;
            set
            {
                m_DefaultShape = (ForestBrushShape)math.clamp((int)value, 0, 3);
                ForestBrushState.SetShape((int)m_DefaultShape);
            }
        }

        [SettingsUISlider(min = 10, max = 1000, step = 10)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultCircleBrushSize
        {
            get => m_DefaultCircleBrushSize;
            set
            {
                m_DefaultCircleBrushSize = math.clamp(value, 10, 1000);
                ForestBrushState.CircleBrushSize = m_DefaultCircleBrushSize;
            }
        }

        [SettingsUISlider(min = 10, max = 1000, step = 10)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultShapeWidth
        {
            get => m_DefaultShapeWidth;
            set
            {
                m_DefaultShapeWidth = math.clamp(value, 10, 1000);
                ForestBrushState.ShapeWidth = m_DefaultShapeWidth;
            }
        }

        [SettingsUISlider(min = 10, max = 1000, step = 10)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultRectangleLength
        {
            get => m_DefaultRectangleLength;
            set
            {
                m_DefaultRectangleLength = math.clamp(value, 10, 1000);
                ForestBrushState.ShapeLength = m_DefaultRectangleLength;
            }
        }

        [SettingsUISlider(min = 0, max = 355, step = 5)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultRotation
        {
            get => m_DefaultRotation;
            set
            {
                m_DefaultRotation = math.clamp(value, 0, 355);
                ForestBrushState.SetRotation(m_DefaultRotation);
            }
        }

        [SettingsUISlider(min = 10, max = 300, step = 5)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultDensity
        {
            get => m_DefaultDensity;
            set
            {
                m_DefaultDensity = math.clamp(value, 10, 300);
                ForestBrushState.SetDensity(m_DefaultDensity);
            }
        }

        [SettingsUISection(kSection, kDefaultsGroup)]
        public ForestNoiseMode DefaultNoiseMode
        {
            get => m_DefaultNoiseMode;
            set
            {
                m_DefaultNoiseMode = (ForestNoiseMode)math.clamp((int)value, 0, 4);
                ForestBrushState.NoiseMode = m_DefaultNoiseMode;
            }
        }

        [SettingsUISlider(min = 10, max = 200, step = 5)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultNoiseScale
        {
            get => m_DefaultNoiseScale;
            set
            {
                m_DefaultNoiseScale = math.clamp(value, 10, 200);
                ForestBrushState.NoiseScale = m_DefaultNoiseScale;
            }
        }

        [SettingsUISlider(min = 0, max = 100, step = 5)]
        [SettingsUISection(kSection, kDefaultsGroup)]
        public int DefaultNoiseStrength
        {
            get => m_DefaultNoiseStrength;
            set
            {
                m_DefaultNoiseStrength = math.clamp(value, 0, 100);
                ForestBrushState.NoiseStrength = m_DefaultNoiseStrength;
            }
        }

        [SettingsUISection(kSection, kDefaultsGroup)]
        public ForestSpeciesGrouping DefaultSpeciesGrouping
        {
            get => m_DefaultSpeciesGrouping;
            set
            {
                m_DefaultSpeciesGrouping = (ForestSpeciesGrouping)math.clamp((int)value, 0, 3);
                ForestBrushState.SpeciesGrouping = (int)m_DefaultSpeciesGrouping;
            }
        }

        [SettingsUISection(kSection, kDefaultsGroup)]
        public bool ResetToDefaults
        {
            set => SetDefaults();
        }

        [SettingsUISection(kSection, kAboutGroup)]
        public bool EnableDiagnosticLogging
        {
            get => m_EnableDiagnosticLogging;
            set
            {
                if (m_EnableDiagnosticLogging == value)
                {
                    return;
                }

                m_EnableDiagnosticLogging = value;
                Mod.Log?.Info($"Advanced Forest Brush diagnostic logging {(value ? "enabled" : "disabled")}.");
            }
        }

        [SettingsUISection(kSection, kAboutGroup)]
        public string Version => Mod.ModVersion;

        public override void SetDefaults()
        {
            DefaultShape = ForestBrushShape.Circle;
            DefaultCircleBrushSize = 100;
            DefaultShapeWidth = 100;
            DefaultRectangleLength = 150;
            DefaultRotation = 0;
            DefaultDensity = 100;
            DefaultNoiseMode = ForestNoiseMode.Uniform;
            DefaultNoiseScale = 45;
            DefaultNoiseStrength = 50;
            DefaultSpeciesGrouping = ForestSpeciesGrouping.Off;
            EnableDiagnosticLogging = false;
        }

        public void ApplyToState()
        {
            ForestBrushState.SetShape((int)DefaultShape);
            ForestBrushState.CircleBrushSize = DefaultCircleBrushSize;
            ForestBrushState.ShapeWidth = DefaultShapeWidth;
            ForestBrushState.ShapeLength = DefaultRectangleLength;
            ForestBrushState.SetRotation(DefaultRotation);
            ForestBrushState.SetDensity(DefaultDensity);
            ForestBrushState.NoiseMode = DefaultNoiseMode;
            ForestBrushState.NoiseScale = DefaultNoiseScale;
            ForestBrushState.NoiseStrength = DefaultNoiseStrength;
            ForestBrushState.SpeciesGrouping = (int)DefaultSpeciesGrouping;
        }
    }
}
