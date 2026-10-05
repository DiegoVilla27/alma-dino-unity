using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Cave bat: flies back and forth between two points with a gentle bob, flipping to face its way.
    // Touching its body always kills Alma. When Alma is within range below it (left or right), it
    // stops, turns to her, trembles and blinks red as a warning, then swoops down in a U-shaped arc
    // through where she was and climbs back to its patrol height on the other side. It hovers there
    // to rest, then resumes patrolling; if Alma is still in range it attacks again.
    // The arc doesn't follow Alma after it starts, so it can be read and dodged (e.g. double jump).
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class CaveBat2D : MonoBehaviour
    {
        [Header("Patrol (X offsets from the start position)")]
        [SerializeField] private float _pointA = -3f;
        [SerializeField] private float _pointB = 3f;
        [SerializeField, Min(0.1f)] private float _flySpeed = 2.5f;
        [SerializeField, Min(0f)] private float _bobHeight = 0.15f;
        [SerializeField, Min(0f)] private float _bobFrequency = 0.8f;

        [Header("Detection (relative to the bat)")]
        [SerializeField, Min(0.1f)] private float _detectRange = 4f;
        [SerializeField, Min(0.1f)] private float _detectDepth = 6f;
        [SerializeField] private float _detectAbove = 0.5f;

        [Header("Attack")]
        [SerializeField, Min(0.05f)] private float _warningTime = 0.65f;
        [SerializeField] private Color _warningTint = new Color(1f, 0.6f, 0.6f, 1f);
        [SerializeField, Min(0.2f)] private float _diveTime = 1.6f;
        [SerializeField, Min(0f)] private float _minDiveDepth = 1f;
        [SerializeField, Min(0.5f)] private float _maxDiveDepth = 6f;
        [SerializeField, Min(0f)] private float _maxTilt = 35f;
        [SerializeField, Min(0f)] private float _restTime = 2f;

        [Header("Danger (local, facing right)")]
        [SerializeField] private Vector2 _bodyOffset = new Vector2(0.6f, -0.4f);
        [SerializeField] private Vector2 _bodySize = new Vector2(1.3f, 1.3f);

        private enum State { Patrol, Warning, Dive, Rest }

        private readonly Collider2D[] _hits = new Collider2D[8];
        private SpriteRenderer _renderer;
        private AlmaMotor2D _player;
        private State _state = State.Patrol;
        private float _stateStartedAt;
        private float _startX;
        private float _patrolY;
        private int _facing = 1;
        private Vector2 _diveStart;
        private Vector2 _diveControl;
        private Vector2 _diveEnd;
        private Vector3 _hoverAt;

        private float MinX => _startX + Mathf.Min(_pointA, _pointB);
        private float MaxX => _startX + Mathf.Max(_pointA, _pointB);

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _startX = transform.position.x;
            _patrolY = transform.position.y;
            _player = FindAnyObjectByType<AlmaMotor2D>();
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Patrol:
                    if (TryFindAlma(out float dx)) BeginWarning(dx);
                    else Patrol();
                    break;

                case State.Warning:
                    // Hover in place, trembling and blinking red.
                    transform.position = _hoverAt + new Vector3(Mathf.Sin(Time.time * 60f) * 0.05f, 0f, 0f);
                    _renderer.color = Color.Lerp(Color.white, _warningTint, Mathf.PingPong(elapsed * 8f, 1f));
                    if (elapsed >= _warningTime) BeginDive();
                    break;

                case State.Dive:
                    Dive(elapsed / _diveTime);
                    break;

                case State.Rest:
                    Hover(_hoverAt);
                    Tilt(0f, 8f);
                    if (elapsed >= _restTime) Enter(State.Patrol);
                    break;
            }
        }

        private void FixedUpdate()
        {
            // Its body is always lethal; the wings are not, to keep contact fair.
            if (_player == null || _player.IsDead) return;
            Vector2 scale = Abs(transform.lossyScale);
            Vector2 offset = Vector2.Scale(new Vector2(_bodyOffset.x * _facing, _bodyOffset.y), scale);
            Vector2 center = (Vector2)transform.position + (Vector2)(transform.rotation * offset);
            int count = Physics2D.OverlapBox(center, Vector2.Scale(_bodySize, scale),
                transform.eulerAngles.z, ContactFilter2D.noFilter, _hits);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = _hits[i].attachedRigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
            }
        }

        // Flies towards the current end; outside A–B (after a dive) it heads back into range.
        private void Patrol()
        {
            Vector3 position = transform.position;
            if (position.x >= MaxX) SetFacing(-1);
            else if (position.x <= MinX) SetFacing(1);
            position.x += _facing * _flySpeed * Time.deltaTime;
            position.y = Mathf.MoveTowards(position.y, _patrolY + Bob(), _flySpeed * Time.deltaTime);
            transform.position = position;
            Tilt(0f, 8f);
        }

        // Alma within range horizontally and below (or slightly above) the bat.
        private bool TryFindAlma(out float dx)
        {
            dx = 0f;
            if (_player == null || _player.IsDead) return false;
            Vector2 offset = (Vector2)(_player.transform.position - transform.position);
            dx = offset.x;
            return Mathf.Abs(offset.x) <= _detectRange && offset.y <= _detectAbove && offset.y >= -_detectDepth;
        }

        private void BeginWarning(float dx)
        {
            if (Mathf.Abs(dx) > 0.1f) SetFacing(dx > 0f ? 1 : -1);
            _hoverAt = transform.position;
            Enter(State.Warning);
        }

        // U-shaped quadratic arc: starts at the bat, passes through Alma's position at its midpoint
        // and ends mirrored on the other side at patrol height.
        private void BeginDive()
        {
            _renderer.color = Color.white;
            _diveStart = transform.position;
            Vector2 target = _player != null ? (Vector2)_player.transform.position : _diveStart + Vector2.down * _minDiveDepth;
            target.y = Mathf.Clamp(target.y, _patrolY - _maxDiveDepth, _diveStart.y - _minDiveDepth);
            _diveEnd = new Vector2(2f * target.x - _diveStart.x, _patrolY);
            _diveControl = 2f * target - 0.5f * (_diveStart + _diveEnd);
            int side = _diveEnd.x >= _diveStart.x ? 1 : -1;
            SetFacing(side);
            Enter(State.Dive);
        }

        private void Dive(float t)
        {
            float clamped = Mathf.Clamp01(t);
            float u = 1f - clamped;
            Vector2 position = u * u * _diveStart + 2f * u * clamped * _diveControl + clamped * clamped * _diveEnd;
            Vector2 velocity = 2f * u * (_diveControl - _diveStart) + 2f * clamped * (_diveEnd - _diveControl);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            if (Mathf.Abs(velocity.x) > 0.01f) SetFacing(velocity.x > 0f ? 1 : -1);
            // Nose follows the arc: down while diving, up while climbing.
            float angle = Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg * _facing;
            Tilt(Mathf.Clamp(angle, -_maxTilt, _maxTilt), 20f);

            if (t < 1f) return;
            _hoverAt = transform.position;
            Enter(State.Rest);
        }

        private void Hover(Vector3 around)
        {
            transform.position = new Vector3(around.x, around.y + Bob(), around.z);
        }

        private float Bob() => Mathf.Sin(Time.time * Mathf.PI * 2f * _bobFrequency) * _bobHeight;

        private void Tilt(float angle, float sharpness)
        {
            float current = transform.eulerAngles.z;
            if (current > 180f) current -= 360f;
            float next = Mathf.Lerp(current, angle, 1f - Mathf.Exp(-sharpness * Time.deltaTime));
            transform.rotation = Quaternion.Euler(0f, 0f, next);
        }

        private void SetFacing(int facing)
        {
            _facing = facing;
            _renderer.flipX = facing < 0;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private static Vector2 Abs(Vector3 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        private void OnDrawGizmosSelected()
        {
            Vector3 position = transform.position;
            float startX = Application.isPlaying ? _startX : position.x;
            float patrolY = Application.isPlaying ? _patrolY : position.y;
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.9f);
            Vector3 a = new Vector3(startX + _pointA, patrolY, 0f);
            Vector3 b = new Vector3(startX + _pointB, patrolY, 0f);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.15f);
            Gizmos.DrawWireSphere(b, 0.15f);

            // Detection zone around the current position.
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            float top = _detectAbove;
            float bottom = -_detectDepth;
            Gizmos.DrawWireCube(position + new Vector3(0f, (top + bottom) * 0.5f, 0f),
                new Vector3(_detectRange * 2f, top - bottom, 0f));

            Vector2 scale = Abs(transform.lossyScale);
            int facing = _renderer != null && _renderer.flipX ? -1 : 1;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(position + (Vector3)Vector2.Scale(new Vector2(_bodyOffset.x * facing, _bodyOffset.y), scale),
                Vector2.Scale(_bodySize, scale));
        }
    }
}
