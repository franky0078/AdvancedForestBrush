using Colossal.Mathematics;
using Game;
using Game.Rendering;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace AdvancedForestBrush
{
    public partial class ForestBrushOverlaySystem : GameSystemBase
    {
        private OverlayRenderSystem m_OverlayRenderSystem;
        private readonly List<float3> m_Points = new List<float3>();

        protected override void OnCreate()
        {
            base.OnCreate();
            m_OverlayRenderSystem =
                World.GetOrCreateSystemManaged<OverlayRenderSystem>();
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible ||
                !ForestBrushState.HasValidCursor ||
                ForestBrushState.Shape != ForestBrushShape.Polygon ||
                ForestBrushState.PolygonClosed)
            {
                return;
            }

            BuildPreviewPoints();
            if (m_Points.Count < 2)
            {
                return;
            }

            NativeArray<float3> points = new NativeArray<float3>(
                m_Points.Count,
                Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < m_Points.Count; i++)
            {
                points[i] = m_Points[i] + new float3(0f, 0.22f, 0f);
            }

            OverlayRenderSystem.Buffer buffer =
                m_OverlayRenderSystem.GetBuffer(out JobHandle dependencies);

            DrawShapeJob job = new DrawShapeJob
            {
                OverlayBuffer = buffer,
                Points = points,
                Closed = false,
                Polygon = true,
                ActiveColor = new UnityEngine.Color(0.2f, 0.82f, 1f, 1f),
                ConfirmedColor = new UnityEngine.Color(0.25f, 0.95f, 0.45f, 1f)
            };

            JobHandle handle = job.Schedule(
                JobHandle.CombineDependencies(Dependency, dependencies));
            m_OverlayRenderSystem.AddBufferWriter(handle);
            Dependency = handle;
        }

        private void BuildPreviewPoints()
        {
            m_Points.Clear();

            m_Points.AddRange(ForestBrushState.PolygonPoints);
            if (ForestBrushState.HasValidCursor)
            {
                m_Points.Add(ForestBrushState.CursorPosition);
            }
        }

        private struct DrawShapeJob : IJob
        {
            public OverlayRenderSystem.Buffer OverlayBuffer;

            [DeallocateOnJobCompletion]
            public NativeArray<float3> Points;

            public bool Closed;
            public bool Polygon;
            public UnityEngine.Color ActiveColor;
            public UnityEngine.Color ConfirmedColor;

            public void Execute()
            {
                UnityEngine.Color color = Closed
                    ? ConfirmedColor
                    : ActiveColor;

                for (int i = 0; i + 1 < Points.Length; i++)
                {
                    OverlayBuffer.DrawLine(
                        color,
                        new Line3.Segment(Points[i], Points[i + 1]),
                        0.7f);
                }

                if (Closed && Points.Length >= 3)
                {
                    OverlayBuffer.DrawLine(
                        color,
                        new Line3.Segment(Points[Points.Length - 1], Points[0]),
                        0.7f);
                }

                if (!Polygon)
                {
                    return;
                }

                int markerCount = Closed ? Points.Length : Points.Length - 1;
                for (int i = 0; i < markerCount; i++)
                {
                    OverlayBuffer.DrawCircle(
                        color,
                        color,
                        0.1f,
                        0f,
                        new float2(0f, 1f),
                        Points[i],
                        1.4f);
                }
            }
        }
    }
}
