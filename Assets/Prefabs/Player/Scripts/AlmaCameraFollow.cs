using UnityEngine;

namespace AlmaGame.Player
{
    // Alma's camera, the same prefab in every scene (Prefabs/Player/Camera_Alma):
    // - Horizontal: the view goes ahead of where Alma LOOKS. Facing right she sits near the left edge
    //   (`Alma From Edge`) and the rest of the screen shows what is ahead; facing left, the other way round.
    //   Turning swings the view across, smooth but quick.
    // - Vertical: a tall dead zone, so climbing steps and platforms or jumping doesn't move the view; it only
    //   eases up or down when Alma gets near the top or bottom of the screen (big climbs, pits). A level can
    //   instead lock the height completely (horizontal levels) through its CameraBounds2D.
    // - Framing: at the start Alma's ground sits low on the screen (`Alma Screen Height`), showing more above.
    // - Bounds: with a CameraBounds2D in the scene the view never shows past the level's edges.
    // - Centre zones: inside a CameraCenterZone2D there is no look-ahead, Alma stays in the centre.
    // - Vertical zones: inside a CameraVerticalZone2D the view looks up or down (Alma low or high).
    [RequireComponent(typeof(Camera))]
    public sealed class AlmaCameraFollow : MonoBehaviour
    {
        public AlmaMotor2D Target;

        [Header("Horizontal")]
        [Tooltip("Where Alma sits on the screen, measured from the edge BEHIND her (fraction of the screen width): 0.25 = a quarter of the screen behind her, three quarters ahead.")]
        [SerializeField, Range(0.05f, 0.5f)] private float _almaFromEdge = 0.1f;
        [Tooltip("Seconds Alma must keep looking the new way before the view swings (a quick tap back and forth doesn't move it).")]
        [SerializeField, Min(0f)] private float _turnDelay = 0.08f;
        [Tooltip("Smoothing of the horizontal follow (seconds): low = snappier.")]
        [SerializeField, Min(0.01f)] private float _horizontalSmoothTime = 0.22f;
        [Tooltip("Top horizontal camera speed, as a multiple of Alma's run speed (high enough to swing across quickly when she turns).")]
        [SerializeField, Min(1f)] private float _maxSpeedFactor = 4f;

        [Header("Vertical")]
        [Tooltip("The view stays at the level's start height. It only rises or drops as much as needed to keep Alma between `Lock Margins` of the screen, and returns to the start height as soon as she is back in range.")]
        [SerializeField] private bool _lockVertical = true;
        [Tooltip("Lowest and highest screen position (0 = bottom, 1 = top) Alma may reach before the locked view moves.")]
        [SerializeField] private Vector2 _lockMargins = new Vector2(0.08f, 0.72f);
        [Tooltip("Where Alma is placed on the screen at the start and after respawning: 0 = bottom, 1 = top.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _almaScreenHeight = 0.17f;
        [Tooltip("Vertical dead zone, as screen heights from the bottom: inside it the camera doesn't move vertically.")]
        [SerializeField, Range(0f, 0.5f)] private float _deadZoneBottom = 0.15f;
        [SerializeField, Range(0.5f, 1f)] private float _deadZoneTop = 0.5f;
        [SerializeField, Min(0.01f)] private float _verticalSmoothTime = 0.2f;

        private Camera _camera;
        private CameraBounds2D _bounds;
        private int _facing = 1;          // the side the view looks to
        private int _pendingFacing = 1;
        private float _pendingSince;
        private float _viewY;
        private float _velocityX;
        private float _velocityY;
        private Vector3 _followPosition;
        private float _depth;
        private float _shakeAmplitude;
        private float _shakeDuration;
        private float _shakeEndsAt;
        private float _baseSize = 8f;
        private Vector3 _viewPosition;
        // Camera shot (tutorials, story moments): a point and a zoom the view blends towards by `_shotWeight`.
        private Vector2 _shotPoint;
        private float _shotSize = 8f;
        private float _shotWeight;
        private float _startY;
        private bool _startYSet;
        // Boss arenas: a point (the boss) framed together with Alma, see SetFocus.
        private System.Func<Vector2> _focus;
        private float _focusMargin = 0.18f;
        private float _focusSmoothTime = 0.35f;
        // Height of the ground Alma last stood on (or lower, while falling): vertical zones follow it.
        private float _groundY;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 8f;
            _baseSize = _camera.orthographicSize;
            _depth = transform.position.z;
            _followPosition = _viewPosition = transform.position;
        }

        private void Start()
        {
            if (Target == null) Target = FindAnyObjectByType<AlmaMotor2D>();   // the prefab finds Alma by itself
            _bounds = FindAnyObjectByType<CameraBounds2D>();
            if (Target != null) Target.Respawned += SnapToTarget;
            SnapToTarget();
        }

        private void OnDestroy()
        {
            if (Target != null) Target.Respawned -= SnapToTarget;
        }

        // Where the view is without the shake offset (the HUD sticks to it, so it never shakes).
        public Vector3 ViewPosition => _viewPosition;
        public float AlmaFromEdge => _almaFromEdge;

        // Blends the view towards `point` with an orthographic size of `size`; `weight` 0 = normal follow,
        // 1 = fully on the shot (callers animate the weight, on unscaled time if the game is paused).
        public void SetShot(Vector2 point, float size, float weight)
        {
            _shotPoint = point;
            _shotSize = Mathf.Max(1f, size);
            _shotWeight = Mathf.Clamp01(weight);
        }

        public void ClearShot() => _shotWeight = 0f;

        // Boss fights: horizontally the view centres between Alma and `focus` (the boss), so what attacks
        // her is always on screen; when they are too far apart Alma stays `almaMargin` (fraction of the
        // screen width) from the edge and the rest of the view shows the boss's side. Replaces the look-ahead.
        public void SetFocus(System.Func<Vector2> focus, float almaMargin = 0.18f, float smoothTime = 0.35f)
        {
            _focus = focus;
            _focusMargin = Mathf.Clamp(almaMargin, 0.05f, 0.45f);
            _focusSmoothTime = Mathf.Max(0.01f, smoothTime);
        }

        public void ClearFocus() => _focus = null;

        // Short shake that fades linearly to zero; a new call replaces the current one.
        public void Shake(float amplitude, float duration)
        {
            _shakeAmplitude = amplitude;
            _shakeDuration = duration;
            _shakeEndsAt = Time.time + duration;
        }

        // Jump straight to Alma (level start, respawn) instead of gliding across the level.
        public void SnapToTarget()
        {
            if (Target == null) return;
            Vector3 alma = Target.transform.position;
            _facing = _pendingFacing = Target.FacingDirection < 0 ? -1 : 1;
            _viewY = FixedHeight(out float fixedY) ? fixedY : alma.y + (0.5f - _almaScreenHeight) * _baseSize * 2f;
            if (!_startYSet) { _startY = _viewY; _startYSet = true; }
            if (_lockVertical) _viewY = LockedY(alma.y);
            _groundY = alma.y;
            CameraVerticalZone2D zone = CameraVerticalZone2D.At(alma);
            if (zone != null) _viewY = ZoneY(zone, alma.y);
            _velocityX = _velocityY = 0f;
            _followPosition = Clamp(new Vector3(alma.x + _facing * LeadAt(alma), _viewY, _depth));
            transform.position = _followPosition;
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            Vector3 alma = Target.transform.position;

            // The side she looks to (after a short delay, so a quick tap back and forth doesn't swing the view).
            int looking = Target.FacingDirection < 0 ? -1 : 1;
            if (looking != _pendingFacing) { _pendingFacing = looking; _pendingSince = Time.time; }
            if (_pendingFacing != _facing && Time.time - _pendingSince >= _turnDelay) _facing = _pendingFacing;

            // Vertical zone (looking up or down), else the dead zone (or a locked height): the view height
            // only moves when Alma leaves the band.
            if (Target.IsGrounded || alma.y < _groundY) _groundY = alma.y;
            CameraVerticalZone2D zone = CameraVerticalZone2D.At(alma);
            if (zone != null) _viewY = ZoneY(zone, alma.y);
            else if (FixedHeight(out float fixedY)) _viewY = fixedY;
            else if (_lockVertical) _viewY = LockedY(alma.y);
            else
            {
                float height = _camera.orthographicSize * 2f;
                float bottom = _viewY - height * 0.5f;
                if (alma.y > bottom + height * _deadZoneTop) _viewY = alma.y - height * (_deadZoneTop - 0.5f);
                else if (alma.y < bottom + height * _deadZoneBottom) _viewY = alma.y + height * (0.5f - _deadZoneBottom);
            }

            // Follow is smoothed on its own position so the shake offset never feeds back into it.
            float goalX = alma.x + _facing * LeadAt(alma);
            float smoothX = _horizontalSmoothTime;
            if (_focus != null)
            {
                float halfWidth = _baseSize * _camera.aspect;
                float reach = halfWidth - 2f * halfWidth * _focusMargin;   // how far from the centre Alma may be
                goalX = Mathf.Clamp((alma.x + _focus().x) * 0.5f, alma.x - reach, alma.x + reach);
                smoothX = _focusSmoothTime;
            }
            Vector3 goal = Clamp(new Vector3(goalX, _viewY, _depth));
            // Capped near Alma's run speed, so catching up after a turn never whips the view across.
            float maxSpeedX = Target.Settings.MoveSpeed * _maxSpeedFactor;
            float x = Mathf.SmoothDamp(_followPosition.x, goal.x, ref _velocityX, smoothX, maxSpeedX);
            float y = Mathf.SmoothDamp(_followPosition.y, goal.y, ref _velocityY, zone != null ? zone.SmoothTime : _verticalSmoothTime);
            _followPosition = new Vector3(x, y, _depth);
            if (_shotWeight > 0f)
            {
                float k = _shotWeight * _shotWeight * (3f - 2f * _shotWeight);
                _camera.orthographicSize = Mathf.Lerp(_baseSize, _shotSize, k);
                _viewPosition = Clamp(Vector3.Lerp(_followPosition, new Vector3(_shotPoint.x, _shotPoint.y, _depth), k));
            }
            else
            {
                _camera.orthographicSize = _baseSize;
                _viewPosition = _followPosition;
            }
            transform.position = _viewPosition + ShakeOffset();
        }

        // How far ahead of Alma the view centre goes: she sits `Alma From Edge` from the edge behind her.
        private float Lead => (0.5f - _almaFromEdge) * 2f * _baseSize * (_camera != null ? _camera.aspect : 16f / 9f);

        // No look-ahead inside a CameraCenterZone2D: Alma stays in the centre, both sides in view.
        private float LeadAt(Vector2 alma) => CameraCenterZone2D.Contains(alma) ? 0f : Lead;

        // View height inside a vertical zone: Alma's ground `Alma From Edge` from the top (looking down) or
        // the bottom (looking up), but never letting Alma herself (high jumps, long falls) leave the screen.
        private float ZoneY(CameraVerticalZone2D zone, float almaY)
        {
            float h = _baseSize * 2f;
            float y = _groundY + zone.Look * (0.5f - zone.AlmaFromEdge) * h;
            return Mathf.Clamp(y, almaY - h * 0.38f, almaY + h * 0.38f);
        }

        // Start height, moved only as far as needed to keep Alma inside the lock margins.
        private float LockedY(float almaY)
        {
            float h = _baseSize * 2f;
            float lowest = almaY + h * (0.5f - _lockMargins.y);    // Alma at the top margin
            float highest = almaY + h * (0.5f - _lockMargins.x);   // Alma at the bottom margin
            return Mathf.Clamp(_startY, lowest, highest);
        }

        private bool FixedHeight(out float centerY)
        {
            centerY = _bounds != null ? _bounds.FixedCameraY : 0f;
            return _bounds != null && _bounds.LockHeight;
        }

        private Vector3 Clamp(Vector3 position)
        {
            if (_bounds == null) return position;
            Rect area = _bounds.Area;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            position.x = area.width > halfWidth * 2f ? Mathf.Clamp(position.x, area.xMin + halfWidth, area.xMax - halfWidth) : area.center.x;
            position.y = area.height > halfHeight * 2f ? Mathf.Clamp(position.y, area.yMin + halfHeight, area.yMax - halfHeight) : area.center.y;
            return position;
        }

        private Vector3 ShakeOffset()
        {
            float remaining = _shakeEndsAt - Time.time;
            if (remaining <= 0f || _shakeDuration <= 0f) return Vector3.zero;
            return (Vector3)(Random.insideUnitCircle * (_shakeAmplitude * remaining / _shakeDuration));
        }

        private void OnDrawGizmosSelected()
        {
            // Where Alma sits when looking right / left, and the vertical dead-zone band, on the current view.
            var cam = GetComponent<Camera>();
            if (cam == null) return;
            float height = cam.orthographicSize * 2f, width = height * cam.aspect;
            float bottom = transform.position.y - height * 0.5f;
            float bandBottom = bottom + height * _deadZoneBottom;
            float bandTop = bottom + height * _deadZoneTop;
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.9f);
            Gizmos.DrawWireCube(new Vector3(transform.position.x, (bandBottom + bandTop) * 0.5f, 0f), new Vector3(width, bandTop - bandBottom, 0f));
            float left = transform.position.x - width * 0.5f;
            Gizmos.DrawLine(new Vector3(left + width * _almaFromEdge, bottom), new Vector3(left + width * _almaFromEdge, bottom + height));
            Gizmos.DrawLine(new Vector3(left + width * (1f - _almaFromEdge), bottom), new Vector3(left + width * (1f - _almaFromEdge), bottom + height));
        }
    }
}
