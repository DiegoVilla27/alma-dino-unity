using System.Collections.Generic;
using UnityEngine;

namespace AlmaGame.Player
{
    // Camera zone of a level: while Alma is inside its box the view keeps her in the centre of the screen
    // instead of going ahead of where she looks, so both sides are visible (arenas, rooms where danger comes
    // from either side). Leaving the box brings the normal look-ahead back. The change is eased by the camera's
    // own smoothing. Put one per area, sized to it (blue box in the Scene view).
    [DisallowMultipleComponent]
    public sealed class CameraCenterZone2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(20f, 16f);

        private static readonly List<CameraCenterZone2D> Active = new List<CameraCenterZone2D>();

        public Rect Area => new Rect((Vector2)transform.position - _size * 0.5f, _size);

        public static bool Contains(Vector2 point)
        {
            for (int i = 0; i < Active.Count; i++)
                if (Active[i].Area.Contains(point)) return true;
            return false;
        }

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        public static readonly Color GizmoColor = new Color(0.3f, 0.75f, 1f, 0.9f);

#if UNITY_EDITOR
        // The Active list only fills in Play mode, so the Scene view preview searches the scene.
        internal static bool ContainsInEditor(Vector2 point)
        {
            foreach (var zone in FindObjectsByType<CameraCenterZone2D>(FindObjectsSortMode.None))
                if (zone.isActiveAndEnabled && zone.Area.Contains(point)) return true;
            return false;
        }

        private void OnDrawGizmos() => CameraZoneGizmos.Box(transform.position, _size, GizmoColor, "Cámara: centro");

        // Preview: the screen with Alma standing at the zone centre.
        private void OnDrawGizmosSelected()
        {
            Vector2 alma = transform.position;
            CameraVerticalZone2D vertical = CameraVerticalZone2D.AtInEditor(alma);
            float y = vertical != null ? CameraZoneGizmos.ViewY(alma.y, vertical.Look, vertical.AlmaFromEdge) : alma.y + 0.33f * 16f;
            CameraZoneGizmos.Frame(alma, new Vector2(alma.x, y), GizmoColor);
        }
#endif
    }
}
