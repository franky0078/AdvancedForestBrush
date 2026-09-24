using Colossal.Mathematics;
using Game;
using Game.Rendering;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
                (ForestBrushState.PolygonClosed &&
                 UnityEngine.Time.unscaledTime >= ForestBrushState.PolygonFeedbackUntil))
            {
                return;
            }

            bool closed = ForestBrushState.PolygonClosed;
            bool snapping = !closed &&
                ForestBrushShapeSystem.IsNearFirst(ForestBrushState.CursorPosition);
            bool invalid = false;
            if (!closed && ForestBrushState.PolygonPoints.Count >= 2)
            {
                float3 last = ForestBrushState.PolygonPoints[
                    ForestBrushState.PolygonPoints.Count - 1];
                bool newCorner = math.distancesq(
                    new float2(last.x, last.z),
                    new float2(ForestBrushState.CursorPosition.x,
                        ForestBrushState.CursorPosition.z)) >=
                    ForestBrushShapeSystem.MinimumPointDistance *
                    ForestBrushShapeSystem.MinimumPointDistance;
                invalid = snapping
                    ? !ForestBrushShapeSystem.IsValidPolygon()
                    : newCorner &&
                      !ForestBrushShapeSystem.IsValidPolygon(
                          ForestBrushState.CursorPosition);
            }

            bool feedback = UnityEngine.Time.unscaledTime < ForestBrushState.PolygonFeedbackUntil;
            BuildPreviewPoints(closed, snapping);
            if (m_Points.Count < 2)
            {
                return;
            }

            NativeArray<float3> points = new NativeArray<float3>(
                m_Points.Count,
                Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

            const float previewDepthOffset = 0.22f;
            Camera camera = Camera.main;
            float3 cameraPosition = default;
            bool hasCamera = camera != null;

            if (hasCamera)
            {
                Vector3 position = camera.transform.position;
                cameraPosition = new float3(
                    position.x,
                    position.y,
                    position.z);
            }

            for (int i = 0; i < m_Points.Count; i++)
            {
                float3 point = m_Points[i];
                float3 offsetDirection = hasCamera
                    ? math.normalizesafe(
                        cameraPosition - point,
                        new float3(0f, 1f, 0f))
                    : new float3(0f, 1f, 0f);

                points[i] = point + offsetDirection * previewDepthOffset;
            }

            OverlayRenderSystem.Buffer buffer =
                m_OverlayRenderSystem.GetBuffer(out JobHandle dependencies);

            DrawShapeJob job = new DrawShapeJob
            {
                OverlayBuffer = buffer,
                Points = points,
                Closed = closed || snapping,
                StoredPointCount = ForestBrushState.PolygonPoints.Count,
                Invalid = feedback ? !ForestBrushState.PolygonFeedbackValid : invalid,
                Green = feedback ? ForestBrushState.PolygonFeedbackValid : snapping && !invalid,
                ActiveColor = new UnityEngine.Color(0.2f, 0.82f, 1f, 1f),
                ConfirmedColor = new UnityEngine.Color(0.25f, 0.95f, 0.45f, 1f),
                InvalidColor = new UnityEngine.Color(1f, 0.85f, 0.15f, 1f)
            };

            JobHandle handle = job.Schedule(
                JobHandle.CombineDependencies(Dependency, dependencies));
            m_OverlayRenderSystem.AddBufferWriter(handle);
            Dependency = handle;
        }

        private void BuildPreviewPoints(bool closed, bool snapping)
        {
            m_Points.Clear();

            m_Points.AddRange(ForestBrushState.PolygonPoints);
            if (!closed && !snapping)
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
            public int StoredPointCount;
            public bool Invalid;
            public bool Green;
            public UnityEngine.Color ActiveColor;
            public UnityEngine.Color ConfirmedColor;
            public UnityEngine.Color InvalidColor;

            public void Execute()
            {
                UnityEngine.Color color = Invalid
                    ? InvalidColor
                    : Green ? ConfirmedColor : ActiveColor;

                for (int i = 0; i + 1 < Points.Length; i++)
                {
                    OverlayBuffer.DrawLine(
                        color,
                        new Line3.Segment(Points[i], Points[i + 1]),
                        0.7f);
                }

                if (Points.Length >= 3 &&
                    (Closed || StoredPointCount >= 3))
                {
                    OverlayBuffer.DrawLine(
                        color,
                        new Line3.Segment(Points[Points.Length - 1], Points[0]),
                        0.18f);
                }

                int markerCount = math.min(StoredPointCount, Points.Length);
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
