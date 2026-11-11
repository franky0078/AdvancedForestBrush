using System.Diagnostics;
using System.Globalization;

namespace AdvancedForestBrush
{
    // Developer-only timing of the mod's main-thread work.
 
    internal static class ForestBrushProfiler
    {
        internal const int Preview = 0;
        internal const int RoadCache = 1;
        internal const int Filter = 2;
        internal const int Species = 3;
        private const int SectionCount = 4;

        private static readonly string[] s_Names = { "preview", "road cache", "filter", "species" };
        private static readonly long[] s_Start = new long[SectionCount];
        private static readonly long[] s_Ticks = new long[SectionCount];
        private static readonly long[] s_Worst = new long[SectionCount];
        private static readonly double s_TickMs = 1000.0 / Stopwatch.Frequency;
        private static int s_Frames;
        private static float s_FrameSeconds, s_WorstFrameSeconds, s_NextReport;
        private static int s_RoadSamples, s_PreviewsFinished, s_CacheRebuilds, s_PatternBuilds;

        [Conditional("AFB_PROFILING")]
        internal static void Begin(int section)
        {
            s_Start[section] = Mod.DiagnosticLoggingEnabled ? Stopwatch.GetTimestamp() : 0L;
        }

        [Conditional("AFB_PROFILING")]
        internal static void End(int section)
        {
            long start = s_Start[section];
            if (start == 0L) return;
            s_Start[section] = 0L;
            long elapsed = Stopwatch.GetTimestamp() - start;
            s_Ticks[section] += elapsed;
            if (elapsed > s_Worst[section]) s_Worst[section] = elapsed;
            // Preview and filter refresh the road cache themselves; do not count that time twice.
            for (int i = 0; i < SectionCount; i++)
            {
                if (i != section && s_Start[i] != 0L) s_Start[i] += elapsed;
            }
        }

        [Conditional("AFB_PROFILING")]
        internal static void CountRoadSample() { s_RoadSamples++; }

        [Conditional("AFB_PROFILING")]
        internal static void CountPreviewFinished() { s_PreviewsFinished++; }

        [Conditional("AFB_PROFILING")]
        internal static void CountCacheRebuild() { s_CacheRebuilds++; }

        [Conditional("AFB_PROFILING")]
        internal static void CountPatternBuild() { s_PatternBuilds++; }

        // Called once per frame. Writes one summary line per second.
        [Conditional("AFB_PROFILING")]
        internal static void Frame()
        {
            if (!Mod.DiagnosticLoggingEnabled || !ForestBrushState.PanelVisible)
            {
                Reset();
                return;
            }

            float delta = UnityEngine.Time.unscaledDeltaTime;
            s_Frames++;
            s_FrameSeconds += delta;
            if (delta > s_WorstFrameSeconds) s_WorstFrameSeconds = delta;
            float now = UnityEngine.Time.unscaledTime;
            if (now < s_NextReport) return;
            s_NextReport = now + 1f;

            CultureInfo culture = CultureInfo.InvariantCulture;
            var text = new System.Text.StringBuilder(256);
            text.Append("Timing over ").Append(s_Frames).Append(" frames (frame avg ")
                .Append((s_FrameSeconds * 1000f / s_Frames).ToString("F1", culture)).Append(" ms, worst ")
                .Append((s_WorstFrameSeconds * 1000f).ToString("F1", culture)).Append(" ms), mod ms per frame avg/worst:");
            double total = 0.0;
            for (int i = 0; i < SectionCount; i++)
            {
                double average = s_Ticks[i] * s_TickMs / s_Frames;
                total += average;
                text.Append(' ').Append(s_Names[i]).Append('=')
                    .Append(average.ToString("F3", culture)).Append('/')
                    .Append((s_Worst[i] * s_TickMs).ToString("F2", culture)).Append(';');
            }
            text.Append(" mod total avg=").Append(total.ToString("F3", culture))
                .Append(" ms; road samples=").Append(s_RoadSamples)
                .Append(", previews finished=").Append(s_PreviewsFinished)
                .Append(", cache rebuilds=").Append(s_CacheRebuilds)
                .Append(", pattern builds=").Append(s_PatternBuilds)
                .Append(", preview mode=").Append(ForestBrushState.PathForest ? "path " : "forest ")
                .Append(ForestBrushPreviewSystem.DetailedPreview ? "detailed"
                    : ForestBrushPreviewSystem.UsePatternPreview ? "normal" : "legacy/off").Append('.');
            Mod.LogDiagnosticInfo(text.ToString());
            Reset();
        }

        private static void Reset()
        {
            for (int i = 0; i < SectionCount; i++)
            {
                s_Ticks[i] = 0L;
                s_Worst[i] = 0L;
            }
            s_Frames = 0;
            s_FrameSeconds = s_WorstFrameSeconds = 0f;
            s_RoadSamples = s_PreviewsFinished = s_CacheRebuilds = s_PatternBuilds = 0;
        }
    }
}