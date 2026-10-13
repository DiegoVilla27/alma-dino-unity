#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AlmaGame.Player
{
    // Scene view look of the camera zones (editor only): a tinted box with its name, handles on the edges to
    // resize it by dragging, and, while selected, a dashed frame of the phone screen as the camera would
    // show it with Alma standing at the white marker (drag the zone, or the marker follows its centre).
    internal static class CameraZoneGizmos
    {
        private const float ViewHeight = 16f;   // orthographic size 8 of Camera_Alma
        private static readonly BoxBoundsHandle Handle = new BoxBoundsHandle { axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y };

        public static float Aspect => Camera.main != null ? Camera.main.aspect : 9f / 19.5f;

        public static float AlmaFromEdge
        {
            get
            {
                var follow = Object.FindAnyObjectByType<AlmaCameraFollow>();
                return follow != null ? follow.AlmaFromEdge : 0.1f;
            }
        }

        public static void Box(Vector3 centre, Vector2 size, Color color, string label)
        {
            Gizmos.color = new Color(color.r, color.g, color.b, 0.07f);
            Gizmos.DrawCube(centre, size);
            Gizmos.color = color;
            Gizmos.DrawWireCube(centre, size);
            var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } };
            Handles.Label(centre + new Vector3(-size.x * 0.5f + 0.2f, size.y * 0.5f - 0.2f), label, style);
        }

        // The screen frame with Alma's feet at `alma`, `viewCentre` being where the camera would sit.
        public static void Frame(Vector2 alma, Vector2 viewCentre, Color color)
        {
            Vector2 half = new Vector2(ViewHeight * Aspect, ViewHeight) * 0.5f;
            Vector3 a = viewCentre - half, b = new Vector2(viewCentre.x + half.x, viewCentre.y - half.y);
            Vector3 c = viewCentre + half, d = new Vector2(viewCentre.x - half.x, viewCentre.y + half.y);
            Handles.color = color;
            Handles.DrawDottedLines(new[] { a, b, b, c, c, d, d, a }, 4f);
            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(new Rect(alma.x - 0.45f, alma.y, 0.9f, 1.8f), new Color(1f, 1f, 1f, 0.35f), Color.white);
            Handles.Label(new Vector3(alma.x - 0.45f, alma.y - 0.15f), "Alma");
        }

        public static float ViewY(float almaY, int look, float fromEdge) => almaY + look * (0.5f - fromEdge) * ViewHeight;

        // Horizontal view centre with Alma at `alma`: centred inside a centre zone, else looking right.
        public static float ViewX(Vector2 alma) =>
            CameraCenterZone2D.ContainsInEditor(alma) ? alma.x : alma.x + (0.5f - AlmaFromEdge) * ViewHeight * Aspect;

        public static void EditBox(Component zone, Color color)
        {
            var serialized = new SerializedObject(zone);
            SerializedProperty size = serialized.FindProperty("_size");
            Handle.center = zone.transform.position;
            Handle.size = size.vector2Value;
            Handle.SetColor(color);
            EditorGUI.BeginChangeCheck();
            Handle.DrawHandle();
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(zone.transform, "Resize camera zone");
            zone.transform.position = new Vector3(Handle.center.x, Handle.center.y, zone.transform.position.z);
            size.vector2Value = Handle.size;
            serialized.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(CameraCenterZone2D))]
    internal sealed class CameraCenterZone2DEditor : Editor
    {
        private void OnSceneGUI() => CameraZoneGizmos.EditBox((Component)target, CameraCenterZone2D.GizmoColor);
    }

    [CustomEditor(typeof(CameraVerticalZone2D))]
    internal sealed class CameraVerticalZone2DEditor : Editor
    {
        private void OnSceneGUI() => CameraZoneGizmos.EditBox((Component)target, CameraVerticalZone2D.GizmoColor);
    }
}
#endif
