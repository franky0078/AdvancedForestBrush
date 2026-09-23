using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AdvancedForestBrush
{
    public partial class ForestBrushShapeSystem : GameSystemBase
    {
        private const float CloseDistance = 3f;
        private const float MinimumPointDistance = 0.75f;
        private const float DoubleClickSeconds = 0.35f;

        private ToolSystem m_ToolSystem;
        private ObjectToolSystem m_ObjectToolSystem;
        private ToolRaycastSystem m_ToolRaycastSystem;
        private PrefabSystem m_PrefabSystem;
        private InputAction m_ApplyAction;
        private InputAction m_PointerDeltaAction;
        private float m_LastClickTime = -10f;
        private float3 m_LastClickPosition;
        private bool m_ReleasePolygonSuppression;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ObjectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            m_ToolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            m_ApplyAction = new InputAction(
                "AdvancedForestBrushApply",
                InputActionType.Button,
                "<Mouse>/leftButton");
            m_PointerDeltaAction = new InputAction(
                "AdvancedForestBrushPointerDelta",
                InputActionType.Value,
                "<Mouse>/delta");

            m_ApplyAction.Enable();
            m_PointerDeltaAction.Enable();
        }

        protected override void OnUpdate()
        {
            if (!ForestBrushState.PanelVisible)
            {
                ForestBrushState.HasValidCursor = false;
                return;
            }

            if (m_ReleasePolygonSuppression)
            {
                ForestBrushState.SuppressPolygonPlacement = false;
                m_ReleasePolygonSuppression = false;
            }

            if (!IsVegetationBrush())
            {
                ForestBrushState.HasValidCursor = false;
                return;
            }

            if (!m_ToolRaycastSystem.GetRaycastResult(out RaycastResult result))
            {
                ForestBrushState.HasValidCursor = false;
                return;
            }

            ForestBrushState.CursorPosition = result.m_Hit.m_Position;
            ForestBrushState.HasValidCursor = true;

            if (ForestBrushState.PointerOverUI)
            {
                return;
            }

            if (ForestBrushState.Shape == ForestBrushShape.Polygon)
            {
                UpdatePolygonInput();
                return;
            }

            if ((ForestBrushState.Shape == ForestBrushShape.Square ||
                 ForestBrushState.Shape == ForestBrushShape.Rectangle) &&
                IsRotationGesture())
            {
                // ObjectTool runs later in ToolUpdate; keep its brush from
                // erasing vegetation during Ctrl + right-drag rotation.
                m_ObjectToolSystem.brushStrength = 0f;
                RotateFromPointerDelta();
            }
        }

        private void UpdatePolygonInput()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ForestBrushState.ResetPolygon();
                return;
            }

            if (ForestBrushState.PolygonClosed)
            {
                if (IsRotationGesture())
                {
                    m_ObjectToolSystem.brushStrength = 0f;
                    RotateFromPointerDelta();
                }

                return;
            }

            // Backspace undoes the last point. Right click is reserved for
            // the game's normal vegetation erase action.
            if (Keyboard.current != null &&
                Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                int count = ForestBrushState.PolygonPoints.Count;
                if (count > 0)
                {
                    ForestBrushState.PolygonPoints.RemoveAt(count - 1);
                }

                ForestBrushState.SuppressPolygonPlacement = true;
                return;
            }

            if (!m_ApplyAction.WasPressedThisFrame())
            {
                return;
            }

            float3 position = ForestBrushState.CursorPosition;
            int pointCount = ForestBrushState.PolygonPoints.Count;
            bool nearFirst = pointCount >= 3 &&
                math.distance(
                    new float2(position.x, position.z),
                    new float2(
                        ForestBrushState.PolygonPoints[0].x,
                        ForestBrushState.PolygonPoints[0].z)) <= CloseDistance;

            bool doubleClick =
                pointCount >= 3 &&
                UnityEngine.Time.unscaledTime - m_LastClickTime <= DoubleClickSeconds &&
                math.distance(
                    new float2(position.x, position.z),
                    new float2(m_LastClickPosition.x, m_LastClickPosition.z)) <=
                    CloseDistance;

            if ((nearFirst || doubleClick) &&
                IsValidPolygon() &&
                ForestBrushState.FinalizePolygon())
            {
                ForestBrushState.SuppressPolygonPlacement = true;
                m_ReleasePolygonSuppression = true;
                return;
            }

            if (pointCount == 0 ||
                math.distance(
                    new float2(position.x, position.z),
                    new float2(
                        ForestBrushState.PolygonPoints[pointCount - 1].x,
                        ForestBrushState.PolygonPoints[pointCount - 1].z)) >=
                    MinimumPointDistance)
            {
                ForestBrushState.PolygonPoints.Add(position);
            }

            ForestBrushState.SuppressPolygonPlacement = true;
            m_LastClickTime = UnityEngine.Time.unscaledTime;
            m_LastClickPosition = position;
        }

        private static bool IsRotationGesture()
        {
            return Keyboard.current != null && Mouse.current != null &&
                (Keyboard.current.leftCtrlKey.isPressed ||
                 Keyboard.current.rightCtrlKey.isPressed) &&
                Mouse.current.rightButton.isPressed;
        }

        private void RotateFromPointerDelta()
        {
            Vector2 delta = m_PointerDeltaAction.ReadValue<Vector2>();
            if (math.abs(delta.x) > 0.01f)
            {
                ForestBrushState.SetRotation(
                    ForestBrushState.RotationDegrees + delta.x * 0.35f);
            }
        }

        private static bool IsValidPolygon()
        {
            int count = ForestBrushState.PolygonPoints.Count;
            if (count < 3)
            {
                return false;
            }

            float twiceArea = 0f;
            for (int i = 0; i < count; i++)
            {
                float3 current = ForestBrushState.PolygonPoints[i];
                float3 next = ForestBrushState.PolygonPoints[(i + 1) % count];
                twiceArea += current.x * next.z - next.x * current.z;
            }

            if (math.abs(twiceArea) < 0.5f)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                float2 a0 = ToXZ(ForestBrushState.PolygonPoints[i]);
                float2 a1 = ToXZ(ForestBrushState.PolygonPoints[(i + 1) % count]);

                for (int j = i + 1; j < count; j++)
                {
                    if (j == i ||
                        (j + 1) % count == i ||
                        (i + 1) % count == j)
                    {
                        continue;
                    }

                    float2 b0 = ToXZ(ForestBrushState.PolygonPoints[j]);
                    float2 b1 = ToXZ(ForestBrushState.PolygonPoints[(j + 1) % count]);
                    if (SegmentsIntersect(a0, a1, b0, b1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static float2 ToXZ(float3 point)
        {
            return new float2(point.x, point.z);
        }

        private static bool SegmentsIntersect(float2 a, float2 b, float2 c, float2 d)
        {
            float abC = Cross(b - a, c - a);
            float abD = Cross(b - a, d - a);
            float cdA = Cross(d - c, a - c);
            float cdB = Cross(d - c, b - c);
            return abC * abD < 0f && cdA * cdB < 0f;
        }

        private static float Cross(float2 a, float2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private bool IsVegetationBrush()
        {
            if (m_ToolSystem.activeTool != m_ObjectToolSystem ||
                m_ObjectToolSystem.actualMode != ObjectToolSystem.Mode.Brush ||
                m_ToolSystem.activePrefab == null ||
                !m_PrefabSystem.TryGetEntity(
                    m_ToolSystem.activePrefab,
                    out Entity prefabEntity))
            {
                return false;
            }

            return EntityManager.HasComponent<PlantData>(prefabEntity);
        }

        protected override void OnDestroy()
        {
            m_ApplyAction?.Dispose();
            m_PointerDeltaAction?.Dispose();
            base.OnDestroy();
        }
    }
}
