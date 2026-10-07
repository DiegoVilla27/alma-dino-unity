using UnityEngine;

namespace AlmaGame.Level
{
    // One parallax layer. It moves with the camera by `Follow`: 1 = stays put on screen (a far sky),
    // 0 = fixed in the world like the level, negative = slides past faster than the level (foreground).
    // Horizontally, a Tiled strip re-centres itself under the camera by whole tiles (`Wrap`), and a single
    // sprite can repeat every `Wrap Period` units, so neither runs out however long the level is.
    // Vertically, a layer can be pinned to the bottom or top edge of the screen whatever the device aspect
    // (backgrounds: bottom edge on the screen's bottom edge, so their top is always above the view), or kept
    // covering the whole view while following the camera by `Follow.y`.
    [DisallowMultipleComponent]
    public sealed class ParallaxLayer2D : MonoBehaviour
    {
        public enum ScreenAnchor { None, Bottom, Top }

        [Tooltip("How much the layer follows the camera on each axis: 1 = moves with it, 0 = world-fixed, < 0 = foreground.")]
        [SerializeField] private Vector2 _follow = new Vector2(0.9f, 0.9f);
        [Tooltip("Repeat a Tiled strip horizontally under the camera (needs a seamless strip).")]
        [SerializeField] private bool _wrap = true;
        [Tooltip("For a single sprite: it reappears every this many units of camera travel (0 = no repeat).")]
        [SerializeField, Min(0f)] private float _wrapPeriod;

        [Header("Vertical")]
        [Tooltip("Pin the sprite to a screen edge (foreground plants). Overrides the vertical follow.")]
        [SerializeField] private ScreenAnchor _anchor = ScreenAnchor.None;
        [Tooltip("Distance from that edge to the sprite's edge (units): negative = partly off-screen.")]
        [SerializeField] private float _anchorOffset;
        [Tooltip("Keep the layer covering the whole view height, so its top and bottom edges never show.")]
        [SerializeField] private bool _coverView = true;

        [SerializeField] private Camera _camera;

        private Vector3 _start;
        private Vector3 _cameraStart;
        private float _tileWidth;
        private Vector2 _extents;

        private void Start()
        {
            if (_camera == null) _camera = Camera.main;
            _start = transform.position;
            if (_camera != null) _cameraStart = _camera.transform.position;
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
            {
                _tileWidth = sprite.sprite.bounds.size.x * transform.lossyScale.x;
                // A wrapping strip must stay wider than the view plus one tile on each side, whatever the
                // device aspect: grow the Tiled size to enough whole tiles.
                if (_wrap && _wrapPeriod <= 0f && sprite.drawMode == SpriteDrawMode.Tiled && _camera != null)
                {
                    float viewWidth = _camera.orthographicSize * 2f * _camera.aspect;
                    int tiles = Mathf.CeilToInt(viewWidth / _tileWidth) + 2;
                    sprite.size = new Vector2(sprite.sprite.bounds.size.x * tiles, sprite.size.y);
                }
                _extents = sprite.bounds.extents;
                // A background pinned to a screen edge must be at least as tall as the view, or its other edge
                // would show: scale it up if needed (foreground pieces with a negative offset are exempt).
                if (_anchor != ScreenAnchor.None && _anchorOffset >= 0f && _camera != null)
                {
                    float viewHeight = _camera.orthographicSize * 2f;
                    if (_extents.y * 2f < viewHeight)
                    {
                        transform.localScale *= viewHeight / (_extents.y * 2f);
                        _tileWidth = sprite.sprite.bounds.size.x * transform.lossyScale.x;
                        _extents = sprite.bounds.extents;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            Vector3 cam = _camera.transform.position;
            Vector3 moved = cam - _cameraStart;
            Vector3 position = _start + new Vector3(moved.x * _follow.x, moved.y * _follow.y, 0f);

            if (_wrap && _tileWidth > 0f && _wrapPeriod <= 0f)
                position.x += Mathf.Round((cam.x - position.x) / _tileWidth) * _tileWidth;
            else if (_wrapPeriod > 0f)
                position.x += Mathf.Round((cam.x - position.x) / _wrapPeriod) * _wrapPeriod;

            float halfHeight = _camera.orthographicSize;
            switch (_anchor)
            {
                case ScreenAnchor.Bottom:
                    position.y = cam.y - halfHeight + _extents.y + _anchorOffset;
                    break;
                case ScreenAnchor.Top:
                    position.y = cam.y + halfHeight - _extents.y - _anchorOffset;
                    break;
                default:
                    if (_coverView && _extents.y > 0f)
                    {
                        // Taller than the view: clamp so both edges stay off-screen. Shorter: centre it.
                        position.y = _extents.y >= halfHeight
                            ? Mathf.Clamp(position.y, cam.y + halfHeight - _extents.y, cam.y - halfHeight + _extents.y)
                            : cam.y;
                    }
                    break;
            }
            transform.position = position;
        }
    }
}
