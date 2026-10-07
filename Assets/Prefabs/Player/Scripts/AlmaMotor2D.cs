using UnityEngine;

namespace AlmaGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class AlmaMotor2D : MonoBehaviour
    {
        [SerializeField] private AlmaMovementSettings _settings;
        [SerializeField] private float _fallRespawnY = -12f;
        // Abilities are unlocked by altars (and restored from the save by GameProgress). They default
        // to unlocked so the prefab can be tested in any scene without the progression system.
        [SerializeField] private bool _doubleJumpUnlocked = false;
        [SerializeField] private bool _groundPoundUnlocked = false;
        [SerializeField] private bool _dashUnlocked = false;
        [SerializeField] private bool _roarUnlocked = false;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[8];
        private readonly Collider2D[] _roarHits = new Collider2D[16];
        private readonly System.Collections.Generic.List<IRoarTarget> _roarTargets = new();
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
        private bool _groundPoundQueued;
        private float _groundPoundStartedAt;
        private bool _dashQueued;
        private bool _dashAvailable = true;
        private int _dashDirection = 1;
        private float _dashStartedAt;
        private float _dashReadyAt;
        private bool _roarQueued;
        private float _roarStartedAt;
        private bool _airJumpAvailable;
        private bool _bounceQueued;
        private float _bounceSpeed;
        private float _bounceHeldMultiplier;
        private bool _bounceRising;
        // Wind: zones add their acceleration each physics step (consumed and cleared next step).
        // The horizontal push lives in its own velocity so the run controller doesn't cancel it.
        private const float WindDrag = 4f;
        private Vector2 _windAcceleration;
        private float _windGravityCompensation;
        private float _windVelocityX;
        private float _appliedWindVx;

        public AlmaMovementSettings Settings => _settings;
        public Vector2 Velocity => _body != null ? _body.linearVelocity : Vector2.zero;
        public bool IsGrounded { get; private set; }
        public int FacingDirection { get; private set; } = 1;
        public bool IsGroundPounding { get; private set; }
        // Raised on the physics step the ground check confirms a Pisotón impact.
        public event System.Action GroundPoundLanded;
        public bool IsDashing { get; private set; }
        public bool IsRoaring { get; private set; }
        public bool IsDead { get; private set; }
        public Vector2 RespawnPosition => _spawnPosition;
        // Raised when Alma dies; a listener (AlmaDeathFx) plays the sequence and then calls Respawn().
        public event System.Action Died;
        // Raised after Alma is placed back at RespawnPosition; level pieces use it to reset themselves.
        public event System.Action Respawned;
        // Raised on the physics step the double jump impulse is applied.
        public event System.Action DoubleJumped;
        public bool DoubleJumpUnlocked { get => _doubleJumpUnlocked; set => _doubleJumpUnlocked = value; }

        public bool IsUnlocked(AlmaAbility ability) => ability switch
        {
            AlmaAbility.DoubleJump => _doubleJumpUnlocked,
            AlmaAbility.GroundPound => _groundPoundUnlocked,
            AlmaAbility.Dash => _dashUnlocked,
            AlmaAbility.Roar => _roarUnlocked,
            _ => false,
        };

        public void SetUnlocked(AlmaAbility ability, bool unlocked)
        {
            switch (ability)
            {
                case AlmaAbility.DoubleJump: _doubleJumpUnlocked = unlocked; break;
                case AlmaAbility.GroundPound: _groundPoundUnlocked = unlocked; break;
                case AlmaAbility.Dash: _dashUnlocked = unlocked; break;
                case AlmaAbility.Roar: _roarUnlocked = unlocked; break;
            }
        }

        // Checkpoints move where Alma reappears; Respawn() places her there.
        public void SetRespawnPosition(Vector2 position) => _spawnPosition = position;

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
            if (IsDead) return;
            _moveInput = Mathf.Clamp(move, -1f, 1f);
            _jumpHeld = jumpHeld;
            if (jumpPressed)
            {
                _jumpQueued = true;
                _jumpPressedAt = Time.time;
            }
            if (!IsDashing && !IsRoaring && Mathf.Abs(_moveInput) > 0.1f)
                FacingDirection = _moveInput > 0f ? 1 : -1;
        }

        // Consumed on the next physics step; ignored unless Alma is airborne.
        public void RequestGroundPound() => _groundPoundQueued = true;

        // Wind zones call this every physics step while Alma is inside. Dash and Pisotón ignore wind.
        public void AddWind(Vector2 acceleration, float gravityCompensation)
        {
            _windAcceleration += acceleration;
            _windGravityCompensation = Mathf.Max(_windGravityCompensation, gravityCompensation);
        }

        // Springs (bouncy mushroom): launches Alma up on the next physics step. Holding jump multiplies
        // the speed, and releasing jump doesn't cut the rise short. Optionally restores air abilities.
        public void Bounce(float speed, float heldMultiplier, bool refillAirAbilities)
        {
            _bounceQueued = true;
            _bounceSpeed = speed;
            _bounceHeldMultiplier = heldMultiplier;
            if (refillAirAbilities) RefillAirAbilities(true, true);
        }

        // Pickups (Dash refill spore): returns true only if something was actually restored.
        public bool RefillAirAbilities(bool dash, bool doubleJump)
        {
            bool refilled = false;
            if (dash && _dashUnlocked && (!_dashAvailable || Time.time < _dashReadyAt))
            {
                _dashAvailable = true;
                _dashReadyAt = 0f;
                refilled = true;
            }
            if (doubleJump && _doubleJumpUnlocked && !_airJumpAvailable)
            {
                _airJumpAvailable = true;
                refilled = true;
            }
            return refilled;
        }

        // Consumed on the next physics step; needs an air charge and the cooldown to be over.
        public void RequestDash() => _dashQueued = true;

        // Consumed on the next physics step; movement stays free while roaring, only facing is held.
        public void RequestRoar() => _roarQueued = true;

        private void FixedUpdate()
        {
            if (IsDead) return;
            RefreshGrounded();
            if (IsGrounded) _airJumpAvailable = true;
            Vector2 velocity = _body.linearVelocity;
            // Take last step's wind out, update it, and add it back after the run controller.
            bool windBlocked = IsDashing || IsGroundPounding;
            velocity.x -= _appliedWindVx;
            float windAccelX = windBlocked ? 0f : _windAcceleration.x;
            _windVelocityX = windBlocked ? 0f
                : _windVelocityX + (windAccelX - _windVelocityX * WindDrag) * Time.fixedDeltaTime;
            if (!windBlocked)
                velocity.y += (_windAcceleration.y + Mathf.Abs(Physics2D.gravity.y) * _settings.GravityScale
                    * _windGravityCompensation) * Time.fixedDeltaTime;
            _windAcceleration = Vector2.zero;
            _windGravityCompensation = 0f;
            // Alma stands still while roaring on the ground; air control is kept for aerial roars.
            float move = IsRoaring && IsGrounded ? 0f : _moveInput;
            float moveTime = !IsGrounded ? _settings.AirAccelerationTime
                : Mathf.Abs(move) < 0.01f ? _settings.BrakingTime : _settings.AccelerationTime;
            velocity.x = Mathf.MoveTowards(velocity.x, move * _settings.MoveSpeed,
                _settings.MoveSpeed / moveTime * Time.fixedDeltaTime);
            velocity.x += _windVelocityX;
            _appliedWindVx = _windVelocityX;

            if (_jumpQueued && Time.time - _jumpPressedAt > _settings.JumpBufferTime)
                _jumpQueued = false;

            // Bounce from a spring: replaces landing and ends a Pisotón that hit it.
            if (_bounceQueued)
            {
                _bounceQueued = false;
                if (!IsDashing)
                {
                    velocity.y = Mathf.Max(velocity.y, _bounceSpeed * (_jumpHeld ? _bounceHeldMultiplier : 1f));
                    IsGroundPounding = false;
                    IsGrounded = false;
                    _jumpQueued = false;
                    _jumpConsumed = true;
                    _lastGroundedAt = float.NegativeInfinity;
                    _ignoreGroundUntil = Time.time + 0.1f;
                    _bounceRising = true;
                }
            }
            if (velocity.y <= 0f || IsGrounded) _bounceRising = false;

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

            // Double jump: one air impulse that never weakens a stronger upward bounce.
            if (_jumpQueued && _doubleJumpUnlocked && _airJumpAvailable && !IsGrounded
                && !IsDashing && !IsGroundPounding)
            {
                velocity.y = Mathf.Max(velocity.y, _settings.DoubleJumpSpeed);
                _jumpQueued = false;
                _airJumpAvailable = false;
                DoubleJumped?.Invoke();
            }

            float gravityMultiplier = velocity.y < -0.01f ? _settings.FallGravityMultiplier
                : velocity.y > 0.01f && !_jumpHeld && !_bounceRising ? _settings.ReleasedJumpGravityMultiplier : 1f;
            float gravity = _settings.GravityScale * gravityMultiplier;
            // Limit this step's gravity as well, so terminal speed is actually respected.
            float remainingFallSpeed = Mathf.Max(0f, velocity.y + _settings.MaxFallSpeed);
            float fullStep = Mathf.Abs(Physics2D.gravity.y) * gravity * Time.fixedDeltaTime;
            _body.gravityScale = fullStep > remainingFallSpeed && fullStep > 0f
                ? gravity * remainingFallSpeed / fullStep : gravity;
            velocity.y = Mathf.Max(velocity.y, -_settings.MaxFallSpeed);
            _body.linearVelocity = velocity;
            UpdateGroundPound();
            UpdateDash();
            UpdateRoar();

            if (_body.position.y < _fallRespawnY) Die();
        }

        private void RefreshGrounded()
        {
            IsGrounded = false;
            // Moving up usually means a jump, so the ground isn't checked; but if Alma was on the ground
            // last step (walking up a slope or a tilted seesaw) she stays grounded. Jumps clear that.
            bool wasGrounded = Time.time - _lastGroundedAt <= Time.fixedDeltaTime * 1.5f;
            if ((_body.linearVelocity.y > 0.1f && !wasGrounded) || Time.time < _ignoreGroundUntil) return;
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

        // Wind-up holds Alma still, then she drops straight down until the ground check reports impact.
        private void UpdateGroundPound()
        {
            if (_groundPoundQueued && _groundPoundUnlocked && !IsGroundPounding && !IsDashing && !IsGrounded)
            {
                IsGroundPounding = true;
                _groundPoundStartedAt = Time.time;
                _lastGroundedAt = float.NegativeInfinity;
            }
            _groundPoundQueued = false;
            if (!IsGroundPounding) return;
            if (IsGrounded)
            {
                IsGroundPounding = false;
                GroundPoundLanded?.Invoke();
                return;
            }

            bool windingUp = Time.time - _groundPoundStartedAt < _settings.GroundPoundWindupTime;
            _body.gravityScale = 0f;
            _body.linearVelocity = new Vector2(0f, windingUp ? 0f : -_settings.GroundPoundSpeed);
        }

        // Air dash: covers DashDistance in DashDuration along the facing locked at start, without gravity.
        private void UpdateDash()
        {
            if (IsGrounded) _dashAvailable = true;
            if (_dashQueued && _dashUnlocked && !IsDashing && !IsGroundPounding && !IsGrounded
                && _dashAvailable && Time.time >= _dashReadyAt)
            {
                IsDashing = true;
                _dashAvailable = false;
                _dashDirection = FacingDirection;
                _dashStartedAt = Time.time;
                _lastGroundedAt = float.NegativeInfinity;
            }
            _dashQueued = false;
            if (!IsDashing) return;
            if (Time.time - _dashStartedAt >= _settings.DashDuration)
            {
                // Leave at run speed so the dash doesn't carry extra distance.
                IsDashing = false;
                // A refill picked up mid-dash (spore) lets the next Dash go right away.
                _dashReadyAt = _dashAvailable ? Time.time : Time.time + _settings.DashCooldown;
                Vector2 exit = _body.linearVelocity;
                exit.x = Mathf.Clamp(exit.x, -_settings.MoveSpeed, _settings.MoveSpeed);
                _body.linearVelocity = exit;
                return;
            }

            _body.gravityScale = 0f;
            _body.linearVelocity = new Vector2(_dashDirection * _settings.DashDistance / _settings.DashDuration, 0f);
        }

        private void UpdateRoar()
        {
            if (_roarQueued && _roarUnlocked && !IsRoaring && !IsDashing && !IsGroundPounding)
            {
                IsRoaring = true;
                _roarStartedAt = Time.time;
                EmitRoar();
            }
            _roarQueued = false;
            if (IsRoaring && Time.time - _roarStartedAt >= _settings.RoarDuration) IsRoaring = false;
        }

        // Frontal cone along the facing at roar start: RoarRange for normal targets,
        // RoarResonanceRange for resonant ones. Each target is hit once per roar.
        private void EmitRoar()
        {
            Vector2 origin = _body.position;
            var facing = new Vector2(FacingDirection, 0f);
            var filter = new ContactFilter2D { useTriggers = true };
            float reach = Mathf.Max(_settings.RoarRange, _settings.RoarResonanceRange);
            int count = Physics2D.OverlapCircle(origin, reach, filter, _roarHits);
            _roarTargets.Clear();
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _roarHits[i];
                if (hit.attachedRigidbody == _body) continue;
                var target = hit.GetComponentInParent<IRoarTarget>();
                if (target == null || _roarTargets.Contains(target)) continue;
                Vector2 offset = hit.ClosestPoint(origin) - origin;
                float range = target.ResonatesWithRoar ? _settings.RoarResonanceRange : _settings.RoarRange;
                if (offset.magnitude > range) continue;
                if (offset.sqrMagnitude > 0.0001f && Vector2.Angle(facing, offset) > _settings.RoarHalfAngle) continue;
                _roarTargets.Add(target);
                target.ReceiveRoar(origin, FacingDirection);
            }
        }

        // Lethal hazards and falling out of the level call this. Physics and control stop until
        // Respawn(); with no Died listener on the prefab, Alma respawns immediately.
        public void Die()
        {
            if (IsDead) return;
            if (Died == null)
            {
                Respawn();
                return;
            }
            IsDead = true;
            _body.linearVelocity = Vector2.zero;
            _body.simulated = false;
            _moveInput = 0f;
            _jumpHeld = _jumpQueued = false;
            _groundPoundQueued = IsGroundPounding = false;
            _dashQueued = IsDashing = false;
            _roarQueued = IsRoaring = false;
            Died.Invoke();
        }

        public void Respawn()
        {
            IsDead = false;
            _body.simulated = true;
            _body.position = _spawnPosition;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.gravityScale = _settings.GravityScale;
            _moveInput = 0f;
            _jumpHeld = _jumpQueued = _jumpConsumed = false;
            _groundPoundQueued = IsGroundPounding = false;
            _dashQueued = IsDashing = false;
            _dashAvailable = true;
            _dashReadyAt = 0f;
            _roarQueued = IsRoaring = false;
            _airJumpAvailable = false;
            _bounceQueued = _bounceRising = false;
            _windAcceleration = Vector2.zero;
            _windGravityCompensation = _windVelocityX = _appliedWindVx = 0f;
            _lastGroundedAt = float.NegativeInfinity;
            _ignoreGroundUntil = Time.time + 0.08f;
            IsGrounded = false;
            Respawned?.Invoke();
        }

        private void OnDisable()
        {
            _moveInput = 0f;
            _jumpHeld = _jumpQueued = false;
            _groundPoundQueued = IsGroundPounding = false;
            _dashQueued = IsDashing = false;
            _roarQueued = IsRoaring = false;
        }
    }
}
