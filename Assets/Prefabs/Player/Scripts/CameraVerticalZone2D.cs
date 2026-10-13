using System.Collections.Generic;
using UnityEngine;

namespace AlmaGame.Player
{
    public enum CameraLook { Down = -1, Up = 1 }

    // Camera zone of a level: while Alma is inside its box the view looks up (climbs) or down (descents):
    // Alma sits `Alma From Edge` from the bottom (Up) or the top (Down) of the screen and the rest of the
    // view shows where she is going. The height follows the ground she stands on, so a jump doesn't move
    // the view; falling does, at once. Only the height changes: sideways the normal look-ahead stays
    // (Alma low/high on the left or right), and together with a CameraCenterZone2D she is low/high in the
    // middle. Leaving the box brings the normal height back. Purple box in the Scene view.
    [DisallowMultipleComponent]
    public sealed class CameraVerticalZone2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(20f, 30f);
        [SerializeField] private CameraLook _look = CameraLook.Down;
        [Tooltip("Where Alma sits, measured from the screen edge she is near (top for Down, bottom for Up), as a fraction of the screen height.")]
        [SerializeField, Range(0.05f, 0.5f)] private float _almaFromEdge = 0.2f;
        [Tooltip("Smoothing of the height while inside (seconds): low = snappier.")]
        [SerializeField, Min(0.01f)] private float _smoothTime = 0.3f;

        private static readonly List<CameraVerticalZone2D> Active = new List<CameraVerticalZone2D>();

        public Rect Area => new Rect((Vector2)transform.position - _size * 0.5f, _size);
        public int Look => (int)_look;
        public float AlmaFromEdge => _almaFromEdge;
        public float SmoothTime => _smoothTime;

        public static CameraVerticalZone2D At(Vector2 point)
        {
            for (int i = 0; i < Active.Count; i++)
                if (Active[i].Area.Contains(point)) return Active[i];
            return null;
        }

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        public static readonly Color GizmoColor = new Color(0.75f, 0.4f, 1f, 0.9f);

#if UNITY_EDITOR
        internal static CameraVerticalZone2D AtInEditor(Vector2 point)
        {
            foreach (var zone in FindObjectsByType<CameraVerticalZone2D>(FindObjectsSortMode.None))
                if (zone.isActiveAndEnabled && zone.Area.Contains(point)) return zone;
            return null;
        }

        private void OnDrawGizmos()
        {
            Vector3 c = transform.position;
            CameraZoneGizmos.Box(c, _size, GizmoColor, _look == CameraLook.Down ? "Cámara: abajo ↓" : "Cámara: arriba ↑");
            // Arrow towards where the view looks.
            float s = (int)_look, a = Mathf.Min(_size.y * 0.25f, 2f);
            Gizmos.color = GizmoColor;
            Gizmos.DrawLine(c - Vector3.up * s * a, c + Vector3.up * s * a);
            Gizmos.DrawLine(c + Vector3.up * s * a, c + new Vector3(-0.4f, s * (a - 0.5f), 0f));
            Gizmos.DrawLine(c + Vector3.up * s * a, c + new Vector3(0.4f, s * (a - 0.5f), 0f));
        }

        // Preview: the screen with Alma standing at the zone centre (centred if a centre zone covers her).
        private void OnDrawGizmosSelected()
        {
            Vector2 alma = transform.position;
            CameraZoneGizmos.Frame(alma, new Vector2(CameraZoneGizmos.ViewX(alma), CameraZoneGizmos.ViewY(alma.y, Look, _almaFromEdge)), GizmoColor);
        }
#endif
    }
}
