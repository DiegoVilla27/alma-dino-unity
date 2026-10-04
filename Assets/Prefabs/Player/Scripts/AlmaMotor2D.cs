using UnityEngine;

namespace AlmaGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class AlmaMotor2D : MonoBehaviour
    {
        [SerializeField] private AlmaMovementSettings _settings;
        [SerializeField] private float _fallRespawnY = -12f;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[8];
        private Rigidbody2D _body;
        private CapsuleCollider2D _collider;
        private Vector2 _spawnPosition;
        private float _moveInput;
        private bool _jumpHeld;
        private bool _jumpQueued;
        private float _jumpPressedAt;
        private float _lastGroundedAt = float.NegativeInfinity;
        private bool _jumpConsumed;
        private float _ignoreGroundUntil;

        public AlmaMovementSettings Settings => _settings;
        public Vector2 Velocity => _body != null ? _body.linearVelocity : Vector2.zero;
        public bool IsGrounded { get; private set; }
        public int FacingDirection { get; private set; } = 1;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CapsuleCollider2D>();
            _spawnPosition = _body.position;
            if (_settings == null)
            {
                Debug.LogError("Alma needs movement settings assigned on her prefab.", this);
                enabled = false;
                return;
            }
            _body.gravityScale = _settings.GravityScale;
        }

        // Keyboard, gamepad, touch or tests feed the same input contract.
        // A quick press/release between physics steps still produces one jump.
        public void SetInput(float move, bool jumpPressed, bool jumpHeld)
        {
            _moveInput = Mathf.Clamp(move, -1f, 1f);
            _jumpHeld = jumpHeld;
            if (jumpPressed)
            {
                _jumpQueued = true;
                _jumpPressedAt = Time.time;
            }
            if (Mathf.Abs(_moveInput) > 0.1f)
                FacingDirection = _moveInput > 0f ? 1 : -1;
        }

        private void FixedUpdate()
        {
            RefreshGrounded();
            Vector2 velocity = _body.linearVelocity;
            float moveTime = !IsGrounded ? _settings.AirAccelerationTime
                : Mathf.Abs(_moveInput) < 0.01f ? _settings.BrakingTime : _settings.AccelerationTime;
            velocity.x = Mathf.MoveTowards(velocity.x, _moveInput * _settings.MoveSpeed,
                _settings.MoveSpeed / moveTime * Time.fixedDeltaTime);

            if (_jumpQueued && Time.time - _jumpPressedAt > _settings.JumpBufferTime)
                _jumpQueued = false;

            if (_jumpQueued && !_jumpConsumed
                && (IsGrounded || Time.time - _lastGroundedAt <= _settings.CoyoteTime))
            {
                velocity.y = _settings.JumpSpeed;
                _jumpQueued = false;
                _jumpConsumed = true;
                _lastGroundedAt = float.NegativeInfinity;
                _ignoreGroundUntil = Time.time + 0.08f;
                IsGrounded = false;
            }

            float gravityMultiplier = velocity.y < -0.01f ? _settings.FallGravityMultiplier
                : velocity.y > 0.01f && !_jumpHeld ? _settings.ReleasedJumpGravityMultiplier : 1f;
            float gravity = _settings.GravityScale * gravityMultiplier;
            // Limit this step's gravity as well, so terminal speed is actually respected.
            float remainingFallSpeed = Mathf.Max(0f, velocity.y + _settings.MaxFallSpeed);
            float fullStep = Mathf.Abs(Physics2D.gravity.y) * gravity * Time.fixedDeltaTime;
            _body.gravityScale = fullStep > remainingFallSpeed && fullStep > 0f
                ? gravity * remainingFallSpeed / fullStep : gravity;
            velocity.y = Mathf.Max(velocity.y, -_settings.MaxFallSpeed);
            _body.linearVelocity = velocity;

            if (_body.position.y < _fallRespawnY) Respawn();
        }

        private void RefreshGrounded()
        {
            IsGrounded = false;
            if (_body.linearVelocity.y > 0.1f || Time.time < _ignoreGroundUntil) return;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(_settings.GroundLayers);
            int count = _collider.Cast(Vector2.down, filter, _groundHits, _settings.GroundProbeDistance);
            for (int i = 0; i < count; i++)
            {
                var hit = _groundHits[i];
                if (hit.collider == null || hit.rigidbody == _body
                    || hit.normal.y < _settings.MinimumGroundNormal) continue;
                IsGrounded = true;
                _lastGroundedAt = Time.time;
                _jumpConsumed = false;
                break;
            }
        }

        public void Respawn()
        {
            _body.position = _spawnPosition;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.gravityScale = _settings.GravityScale;
            _moveInput = 0f;
            _jumpHeld = _jumpQueued = _jumpConsumed = false;
            _lastGroundedAt = float.NegativeInfinity;
            _ignoreGroundUntil = Time.time + 0.08f;
            IsGrounded = false;
        }

        private void OnDisable()
        {
            _moveInput = 0f;
            _jumpHeld = _jumpQueued = false;
        }
    }
}
