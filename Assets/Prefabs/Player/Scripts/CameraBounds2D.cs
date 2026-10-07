using UnityEngine;

namespace AlmaGame.Player
{
    // Camera settings of a level: put one in each level, sized to the playable area (left/right walls,
    // lowest ground, highest point). AlmaCameraFollow keeps its whole view inside it, so nothing past the
    // level (under the floor, beyond the ends) is ever shown. Horizontal levels can lock the camera height,
    // so the view only ever moves sideways.
    [DisallowMultipleComponent]
    public sealed class CameraBounds2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(40f, 24f);
        [Tooltip("Keep the camera at one height for the whole level (horizontal levels): it only moves sideways.")]
        [SerializeField] private bool _lockHeight;
        [Tooltip("Camera centre height when the height is locked (world units). Yellow line in the Scene view.")]
        [SerializeField] private float _fixedCameraY = 5f;

        public Rect Area => new Rect((Vector2)transform.position - _size * 0.5f, _size);
        public bool LockHeight => _lockHeight;
        public float FixedCameraY => _fixedCameraY;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.9f);
            Gizmos.DrawWireCube(transform.position, _size);
            if (!_lockHeight) return;
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.9f);
            Gizmos.DrawLine(new Vector3(transform.position.x - _size.x * 0.5f, _fixedCameraY, 0f),
                new Vector3(transform.position.x + _size.x * 0.5f, _fixedCameraY, 0f));
        }
    }
}
