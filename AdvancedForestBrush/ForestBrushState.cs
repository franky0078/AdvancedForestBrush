using System.Collections.Generic;
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

    public enum ForestBrushShape
    {
        Circle,
        Square,
        Rectangle,
        Polygon
    }

    public enum ForestSpeciesGrouping
    {
        Off,
        Weak,
        Medium,
        Strong
    }

    public static class ForestBrushState
    {
        public static bool PanelVisible;
        public static bool PointerOverUI;
        public static int DensityPercent = 100;
        public static ForestNoiseMode NoiseMode = ForestNoiseMode.Uniform;
        public static int NoiseScale = 45;
        public static int NoiseStrength = 50;
        // 0 = off, 1 = weak, 2 = medium, 3 = strong.
        public static int SpeciesGrouping;
        public static int Seed = 1977;

        public static ForestBrushShape Shape = ForestBrushShape.Circle;
        public static int ShapeWidth = 100;
        public static int ShapeLength = 150;
        public static float RotationDegrees;
        public static float CircleBrushSize = 100f;
        public static float3 CursorPosition;
        public static bool HasValidCursor;
        public static bool PolygonClosed;
        public static bool SuppressPolygonPlacement;
        public static readonly List<float3> PolygonPoints = new List<float3>();
        public static readonly List<float2> PolygonLocalPoints = new List<float2>();
        public static float PolygonHalfWidth;
        public static float PolygonHalfLength;
        public static float PolygonBoundingRadius;
        public static int PolygonVersion;
        public static float PolygonFeedbackUntil;
        public static bool PolygonFeedbackValid;

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

        public static void SetShape(int value)
        {
            ForestBrushShape next = (ForestBrushShape)math.clamp(value, 0, 3);
            if (Shape == next)
            {
                return;
            }

            Shape = next;
            ResetPolygon();
        }

        public static void SetRotation(float value)
        {
            RotationDegrees = math.fmod(value % 360f + 360f, 360f);
        }

        public static void ResetPolygon()
        {
            PolygonPoints.Clear();
            PolygonLocalPoints.Clear();
            PolygonHalfWidth = 0f;
            PolygonHalfLength = 0f;
            PolygonBoundingRadius = 0f;
            PolygonClosed = false;
            PolygonFeedbackUntil = 0f;
            SuppressPolygonPlacement = Shape == ForestBrushShape.Polygon;
            PolygonVersion++;
        }

        public static bool FinalizePolygon()
        {
            if (PolygonPoints.Count < 3)
            {
                return false;
            }

            float2 minimum = new float2(float.MaxValue);
            float2 maximum = new float2(float.MinValue);
            foreach (float3 point in PolygonPoints)
            {
                float2 xz = new float2(point.x, point.z);
                minimum = math.min(minimum, xz);
                maximum = math.max(maximum, xz);
            }

            float2 center = (minimum + maximum) * 0.5f;
            PolygonHalfWidth = math.max(0.5f, (maximum.x - minimum.x) * 0.5f);
            PolygonHalfLength = math.max(0.5f, (maximum.y - minimum.y) * 0.5f);
            PolygonBoundingRadius = 0f;
            PolygonLocalPoints.Clear();

            foreach (float3 point in PolygonPoints)
            {
                float2 local = new float2(point.x, point.z) - center;
                PolygonLocalPoints.Add(local);
                PolygonBoundingRadius = math.max(
                    PolygonBoundingRadius,
                    math.length(local));
            }

            PolygonClosed = true;
            PolygonVersion++;
            return true;
        }

        public static void ReopenPolygonAtCursor()
        {
            if (!PolygonClosed || !HasValidCursor)
            {
                return;
            }

            PolygonPoints.Clear();
            foreach (float2 local in PolygonLocalPoints)
            {
                PolygonPoints.Add(new float3(
                    CursorPosition.x + local.x,
                    CursorPosition.y,
                    CursorPosition.z + local.y));
            }

            PolygonLocalPoints.Clear();
            PolygonHalfWidth = 0f;
            PolygonHalfLength = 0f;
            PolygonBoundingRadius = 0f;
            PolygonClosed = false;
            PolygonFeedbackUntil = 0f;
            SuppressPolygonPlacement = true;
            PolygonVersion++;
        }

        public static bool Contains(float3 worldPosition)
        {
            return HasValidCursor && Contains(worldPosition, CursorPosition);
        }

        public static bool Contains(float3 worldPosition, float3 shapeCenter)
        {
            float2 point = new float2(worldPosition.x, worldPosition.z);

            if (Shape == ForestBrushShape.Circle)
            {
                return true;
            }

            float2 offset = point - new float2(shapeCenter.x, shapeCenter.z);
            float radians = math.radians(RotationDegrees);
            float sine = math.sin(radians);
            float cosine = math.cos(radians);
            float2 local = new float2(
                offset.x * cosine - offset.y * sine,
                offset.x * sine + offset.y * cosine);

            if (Shape == ForestBrushShape.Polygon)
            {
                if (!PolygonClosed)
                {
                    return false;
                }

                return IsPointInsideLocalPolygon(local);
            }

            float halfWidth = ShapeWidth * 0.5f;
            float halfLength = Shape == ForestBrushShape.Square
                ? halfWidth
                : ShapeLength * 0.5f;

            return math.abs(local.x) <= halfWidth &&
                   math.abs(local.y) <= halfLength;
        }

        public static bool IsPointInsideLocalPolygon(float2 point)
        {
            bool inside = false;
            int count = PolygonLocalPoints.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                float2 a = PolygonLocalPoints[i];
                float2 b = PolygonLocalPoints[j];
                bool crosses = (a.y > point.y) != (b.y > point.y);

                if (crosses)
                {
                    float intersectionX = a.x +
                        (point.y - a.y) * (b.x - a.x) /
                        (b.y - a.y);

                    if (point.x < intersectionX)
                    {
                        inside = !inside;
                    }
                }
            }

            return inside;
        }
    }
}
