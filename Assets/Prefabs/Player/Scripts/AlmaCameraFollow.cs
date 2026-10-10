using UnityEngine;

namespace AlmaGame.Player
{
    // Platformer camera that moves little, slowly and on purpose, so the level and its parallax stay calm:
    // - Dead zone: Alma moves freely inside a box around the screen centre; the camera only follows when she
    //   pushes against its edge, so small steps and turns don't move the view.
    // - Look-ahead: after Alma has been running one way for a moment, the view eases ahead to show what's
    //   coming; turning around eases it back the other way (never a jump).
    // - Vertical: a tall dead zone, so climbing steps and platforms or jumping doesn't move the view; it only
    //   eases up or down when Alma gets near the top or bottom of the screen (big climbs, pits). A level can
    //   instead lock the height completely (horizontal levels) through its CameraBounds2D.
    // - Framing: at the start Alma's ground sits low on the screen (`Alma Screen Height`), showing more above.
    // - Bounds: with a CameraBounds2D in the scene the view never shows past the level's edges.
    [RequireComponent(typeof(Camera))]
    public sealed class AlmaCameraFollow : MonoBehaviour
    {
        public AlmaMotor2D Target;

        [Header("Horizontal")]
        [Tooltip("Width of the box around the screen centre where Alma moves without moving the camera (units).")]
        [SerializeField, Min(0f)] private float _deadZoneWidth = 0f;
        [Tooltip("How far the view eases ahead in the running direction (units).")]
        [SerializeField, Min(0f)] private float _lookAheadDistance = 1f;
        [Tooltip("Seconds of running to reach the full look-ahead (and to swing it to the other side when turning).")]
        [SerializeField, Min(0.05f)] private float _lookAheadTime = 0.05f;
        [Tooltip("Seconds Alma must keep running one way before the look-ahead starts moving (ignores short steps).")]
        [SerializeField, Min(0f)] private float _lookAheadDelay = 1f;
        [Tooltip("Fraction of the run speed that counts as running for the look-ahead.")]
        [SerializeField, Range(0f, 1f)] private float _lookAheadSpeedThreshold = 0f;
        [SerializeField, Min(0.01f)] private float _horizontalSmoothTime = 0.01f;
        [Tooltip("Top horizontal camera speed, as a multiple of Alma's run speed.")]
        [SerializeField, Min(1f)] private float _maxSpeedFactor = 2f;

        [Header("Vertical")]
        [Tooltip("Where Alma is placed on the screen at the start and after respawning: 0 = bottom, 1 = top.")]
        [SerializeField, Range(0.1f, 0.9f)] private float _almaScreenHeight = 0.17f;
        [Tooltip("Vertical dead zone, as screen heights from the bottom: inside it the camera doesn't move vertically.")]
        [SerializeField, Range(0f, 0.5f)] private float _deadZoneBottom = 0.15f;
        [SerializeField, Range(0.5f, 1f)] private float _deadZoneTop = 0.5f;
        [SerializeField, Min(0.01f)] private float _verticalSmoothTime = 0.2f;

        private Camera _camera;
        private CameraBounds2D _bounds;
        private float _focusX;
        private float _lookAhead;
        private float _runTime;
        private float _runDirection;
        private float _viewY;
        private float _velocityX;
        private float _velocityY;
        private Vector3 _followPosition;
        private float _depth;
        private float _shakeAmplitude;
        private float _shakeDuration;
        private float _shakeEndsAt;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _camera.orthographicSize = 8f;
            _depth = transform.position.z;
            _followPosition = transform.position;
        }

        private void Start()
        {
            _bounds = FindAnyObjectByType<CameraBounds2D>();
            if (Target != null) Target.Respawned += SnapToTarget;
            SnapToTarget();
        }

        private void OnDestroy()
        {
            if (Target != null) Target.Respawned -= SnapToTarget;
        }

        // Where the view is without the shake offset (the HUD sticks to it, so it never shakes).
        public Vector3 ViewPosition => _followPosition;

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
            _focusX = alma.x;
            _lookAhead = 0f;
            _runTime = 0f;
            _viewY = FixedHeight(out float fixedY) ? fixedY : alma.y + (0.5f - _almaScreenHeight) * _camera.orthographicSize * 2f;
            _velocityX = _velocityY = 0f;
            _followPosition = Clamp(new Vector3(_focusX, _viewY, _depth));
            transform.position = _followPosition;
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            Vector3 alma = Target.transform.position;

            // Dead zone: the focus only moves when Alma pushes past the edge of the box.
            float half = _deadZoneWidth * 0.5f;
            if (alma.x > _focusX + half) _focusX = alma.x - half;
            else if (alma.x < _focusX - half) _focusX = alma.x + half;

            // Look-ahead eases towards the running direction; standing still keeps the current one.
            float vx = Target.Velocity.x;
            bool running = Mathf.Abs(vx) >= Target.Settings.MoveSpeed * _lookAheadSpeedThreshold;
            if (running && Mathf.Sign(vx) == _runDirection) _runTime += Time.deltaTime;
            else _runTime = 0f;
            _runDirection = running ? Mathf.Sign(vx) : 0f;
            if (running && _runTime >= _lookAheadDelay)
            {
                float rate = _lookAheadDistance * 2f / _lookAheadTime;
                _lookAhead = Mathf.MoveTowards(_lookAhead, Mathf.Sign(vx) * _lookAheadDistance, rate * Time.deltaTime);
            }

            // Vertical dead zone (or a locked height): the view height only moves when Alma leaves the band.
            if (FixedHeight(out float fixedY)) _viewY = fixedY;
            else
            {
                float height = _camera.orthographicSize * 2f;
                float bottom = _viewY - height * 0.5f;
                if (alma.y > bottom + height * _deadZoneTop) _viewY = alma.y - height * (_deadZoneTop - 0.5f);
                else if (alma.y < bottom + height * _deadZoneBottom) _viewY = alma.y + height * (0.5f - _deadZoneBottom);
            }

            // Follow is smoothed on its own position so the shake offset never feeds back into it.
            Vector3 goal = Clamp(new Vector3(_focusX + _lookAhead, _viewY, _depth));
            // Capped near Alma's run speed, so catching up after a turn never whips the view across.
            float maxSpeedX = Target.Settings.MoveSpeed * _maxSpeedFactor;
            float x = Mathf.SmoothDamp(_followPosition.x, goal.x, ref _velocityX, _horizontalSmoothTime, maxSpeedX);
            float y = Mathf.SmoothDamp(_followPosition.y, goal.y, ref _velocityY, _verticalSmoothTime);
            _followPosition = new Vector3(x, y, _depth);
            transform.position = _followPosition + ShakeOffset();
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
            // Horizontal dead zone and the vertical dead-zone band, on the current view.
            var cam = GetComponent<Camera>();
            if (cam == null) return;
            float height = cam.orthographicSize * 2f;
            float bottom = transform.position.y - height * 0.5f;
            float bandBottom = bottom + height * _deadZoneBottom;
            float bandTop = bottom + height * _deadZoneTop;
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.9f);
            Gizmos.DrawWireCube(new Vector3(transform.position.x, (bandBottom + bandTop) * 0.5f, 0f),
                new Vector3(_deadZoneWidth, bandTop - bandBottom, 0f));
        }
    }
}
