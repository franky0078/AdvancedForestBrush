using Unity.Mathematics;

namespace AdvancedForestBrush
{
    public enum ForestNoiseMode
    {
        Uniform,
        Natural,
        Clusters,
        Clearings,
        Edge
    }

    public static class ForestBrushState
    {
        public static bool PanelVisible = false;
        public static int DensityPercent = 100;
        public static ForestNoiseMode NoiseMode = ForestNoiseMode.Uniform;
        public static int NoiseScale = 45;
        public static int NoiseStrength = 50;
        public static int Seed = 1977;

        public static void SetDensity(int value)
        {
            DensityPercent = math.clamp(value, 10, 300);
        }

        public static int StepDensity(int direction)
        {
            int step = DensityPercent < 100 ? 5 : 10;
            if (direction < 0 && DensityPercent == 100)
            {
                step = 5;
            }

            SetDensity(DensityPercent + (direction < 0 ? -step : step));
            return DensityPercent;
        }
    }
}
