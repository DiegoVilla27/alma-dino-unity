using System;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Models;
using AlmaDino.Features.Player.ScriptableObjects;
using AlmaDino.Features.Player.Services;
using AlmaDino.Features.Player.Services.States;
using AlmaDino.Shared.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaDino.Features.Player.Controllers
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(GroundDetector2D))]
    public class PlayerController : MonoBehaviour, IPlayerRespawnable, IAbilityUnlockable, IBounceable2D, IDashRefillable2D, IWindAffected2D, IRoarEmitter2D
    {
        public event Action<Vector2, Vector2> OnRoared;
        public void EmitRoar(Vector2 origin, Vector2 direction) => OnRoared?.Invoke(origin, direction);

        [Header("Config")]
        [SerializeField] private AlmaPhysicsConfigSO _config;

        [Header("Events")]
        [SerializeField] private CameraShakeEventChannelSO _cameraShakeChannel;
        [SerializeField] private AbilityUnlockedEventChannelSO _abilityUnlockedChannel;

        [Header("Unlocked Abilities")]
        [SerializeField] private bool _doubleJumpUnlocked = false;
        [SerializeField] private bool _groundPoundUnlocked = false;
        [SerializeField] private bool _dashUnlocked = false;
        [SerializeField] private bool _roarUnlocked = false;

        [Header("Fall Death Boundary")]
        [SerializeField] private float _fallDeathY = -8.0f;

        [Header("References")]
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private GroundDetector2D _groundDetector;
        [SerializeField] private Transform _visualRoot;

        private PlayerStateMachine _stateMachine;
        private PlayerFrameInput _pendingInput;
        private PlayerFrameInput _physicsInput;
        private bool _processingPhysics;
        private FacingDirection2D _facingDirection = FacingDirection2D.Right;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _dashCooldownTimer;
        private bool _hasDoubleJump;
        private bool _canAirDash;
        private bool _isBouncing;

        private Vector2 _activeCheckpointPosition;

        public AlmaPhysicsConfigSO Config => _config;
        public PlayerFrameInput Input => _processingPhysics ? _physicsInput : _inputReader.CurrentInput;
        public Rigidbody2D Rigidbody => _rigidbody;
        public GroundDetector2D GroundDetector => _groundDetector;
        public FacingDirection2D FacingDirection => _facingDirection;
        public PlayerStateMachine StateMachine => _stateMachine;

        public bool IsBouncing => _isBouncing;
        public bool IgnoresWind => _stateMachine != null && _stateMachine.CurrentStateType == PlayerStateEnum.Dash;
        public bool HasDoubleJump { get => _hasDoubleJump; set => _hasDoubleJump = value; }
        public bool CanAirDash => _dashUnlocked && _canAirDash && _dashCooldownTimer <= 0f
            && (_config == null || _config.CanDash);
        public float CoyoteTimer => _coyoteTimer;
        public float JumpBufferTimer => _jumpBufferTimer;

        public bool IsDoubleJumpUnlocked => _doubleJumpUnlocked;
        public bool IsGroundPoundUnlocked => _groundPoundUnlocked;
        public bool IsDashUnlocked => _dashUnlocked;
        public bool IsRoarUnlocked => _roarUnlocked;

        public event Action<Vector2> OnRespawned;
        public event Action<PlayerStateEnum> OnStateChanged;
        public event Action<AbilityType> OnAbilityUnlocked;

        public void UnlockAbility(AbilityType type)
        {
            switch (type)
            {
                case AbilityType.DoubleJump:
                    _doubleJumpUnlocked = true;
                    _hasDoubleJump = true;
                    break;
                case AbilityType.GroundPound:
                    _groundPoundUnlocked = true;
                    break;
                case AbilityType.Dash:
                    _dashUnlocked = true;
                    _canAirDash = true;
                    break;
                case AbilityType.Roar:
                    _roarUnlocked = true;
                    break;
            }

            // Persistir de forma permanente en la partida
            GameProgression.UnlockAbility(type);

            OnAbilityUnlocked?.Invoke(type);
            if (_abilityUnlockedChannel != null)
            {
                _abilityUnlockedChannel.Raise(type);
            }
            Debug.Log($"<color=#00FF88><b>[PlayerController]</b> ¡Habilidad Desbloqueada y Persistida: {type}!</color>");
        }

        private void Awake()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
            if (_inputReader == null) _inputReader = GetComponent<PlayerInputReader>();
            if (_groundDetector == null) _groundDetector = GetComponent<GroundDetector2D>();
            if (_visualRoot == null) _visualRoot = transform;

            SyncAbilitiesWithProgression();

            ConfigurePhysics();
            InitializeStateMachine();
            _collisionService = new PlayerCollisionService2D(this);

            _activeCheckpointPosition = transform.position;
            _hasDoubleJump = _doubleJumpUnlocked;
            _canAirDash = _dashUnlocked;
        }

        private void SyncAbilitiesWithProgression()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            // Nivel 1-1 es el nivel de despertar/aprendizaje: inicia con el salto bloqueado
            // hasta que Alma recoge la Gema Materna en el altar.
            if (sceneName == "Level_1_1")
            {
                _doubleJumpUnlocked = false;
                _groundPoundUnlocked = false;
                _dashUnlocked = false;
                _roarUnlocked = false;
                return;
            }

            // En Nivel 1-2 en adelante el jugador ya dominó el Aleteo Materno
            GameProgression.EnsureLevelBaseline(sceneName);

            // Si el inspector de este nivel ya activó alguna habilidad, registrarla en la progresión
            if (_doubleJumpUnlocked) GameProgression.UnlockAbility(AbilityType.DoubleJump);
            if (_groundPoundUnlocked) GameProgression.UnlockAbility(AbilityType.GroundPound);
            if (_dashUnlocked) GameProgression.UnlockAbility(AbilityType.Dash);
            if (_roarUnlocked) GameProgression.UnlockAbility(AbilityType.Roar);

            // Sincronizar estado local del jugador con la persistencia
            _doubleJumpUnlocked = GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump);
            _groundPoundUnlocked = GameProgression.IsAbilityUnlocked(AbilityType.GroundPound);
            _dashUnlocked = GameProgression.IsAbilityUnlocked(AbilityType.Dash);
            _roarUnlocked = GameProgression.IsAbilityUnlocked(AbilityType.Roar);
        }

        public void ResetGravityScale()
        {
            _rigidbody.gravityScale = _config != null ? _config.GravityScale : 2.2f;
        }

        private void ConfigurePhysics()
        {
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            _rigidbody.simulated = true;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            ResetGravityScale();
            _rigidbody.linearDamping = 0f;
            _rigidbody.angularDamping = 0.05f;

            // Zero friction prevents player from sticking to walls when pressing towards them
            var frictionless = new PhysicsMaterial2D("PlayerFrictionless") { friction = 0f, bounciness = 0f };
            _rigidbody.sharedMaterial = frictionless;
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.sharedMaterial = frictionless;
            }
        }

        private void InitializeStateMachine()
        {
            _stateMachine = new PlayerStateMachine();
            _stateMachine.RegisterState(new PlayerIdleState(this));
            _stateMachine.RegisterState(new PlayerRunState(this));
            _stateMachine.RegisterState(new PlayerJumpState(this));
            _stateMachine.RegisterState(new PlayerFallState(this));
            _stateMachine.RegisterState(new PlayerDoubleJumpState(this));
            _stateMachine.RegisterState(new PlayerGroundPoundState(this));
            _stateMachine.RegisterState(new PlayerDashState(this));
            _stateMachine.RegisterState(new PlayerRoarState(this));

            _stateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);
            _stateMachine.Initialize(PlayerStateEnum.Idle);
        }

        private void Update()
        {
            HandleTimers(Time.deltaTime);
            CapturePhysicsInput();
            UpdateFacingDirection();
        }

        private void FixedUpdate()
        {
            _physicsInput = _pendingInput;
            _pendingInput.JumpDown = false;
            _pendingInput.JumpUp = false;
            _pendingInput.DashDown = false;
            _pendingInput.GroundPoundDown = false;
            _pendingInput.RoarDown = false;
            _processingPhysics = true;
            _groundDetector.CheckGround(_rigidbody.position);

            if (_groundDetector.IsGrounded && _rigidbody.linearVelocity.y <= 0.1f && !_isBouncing)
            {
                _isBouncing = false;
                _coyoteTimer = _config != null ? _config.CoyoteTime : 0.14f;
                _hasDoubleJump = _doubleJumpUnlocked && (_config == null || _config.CanDoubleJump);
                _canAirDash = _dashUnlocked && (_config == null || _config.CanDash);
            }

            if (_rigidbody.position.y < _fallDeathY)
            {
                KillAndRespawn();
                _processingPhysics = false;
                return;
            }

            _stateMachine.UpdateLogic(Time.fixedDeltaTime);
            _stateMachine.PhysicsUpdate(Time.fixedDeltaTime);
            _processingPhysics = false;
        }

        private void CapturePhysicsInput()
        {
            var input = _inputReader.CurrentInput;
            _pendingInput.MoveVector = input.MoveVector;
            _pendingInput.JumpHeld = input.JumpHeld;
            _pendingInput.JumpDown |= input.JumpDown;
            _pendingInput.JumpUp |= input.JumpUp;
            _pendingInput.DashDown |= input.DashDown;
            _pendingInput.GroundPoundDown |= input.GroundPoundDown;
            _pendingInput.RoarDown |= input.RoarDown;
        }

        private void HandleTimers(float dt)
        {
            if (!_groundDetector.IsGrounded)
            {
                _coyoteTimer = Mathf.Max(0f, _coyoteTimer - dt);
            }

            if (Input.JumpDown)
            {
                _jumpBufferTimer = _config != null ? _config.JumpBufferTime : 0.12f;
            }
            else
            {
                _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - dt);
            }

            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - dt);
            }
        }

        public void ConsumeJumpBuffer()
        {
            _jumpBufferTimer = 0f;
        }

        public void ConsumeCoyoteTime()
        {
            _coyoteTimer = 0f;
        }

        public void ConsumeAirDash()
        {
            _canAirDash = false;
            _dashCooldownTimer = _config != null ? _config.DashCooldown : 0.4f;
        }

        public void RefreshAirDash()
        {
            _canAirDash = _dashUnlocked && (_config == null || _config.CanDash);
            _dashCooldownTimer = 0f;
            _hasDoubleJump = _doubleJumpUnlocked && (_config == null || _config.CanDoubleJump);
        }

        public void SetVelocityX(float vx)
        {
            _rigidbody.linearVelocity = new Vector2(vx, _rigidbody.linearVelocity.y);
        }

        public void SetVelocityY(float vy)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, vy);
        }

        public void SetVelocity(Vector2 velocity)
        {
            _rigidbody.linearVelocity = velocity;
        }

        public void AccelerateHorizontally(float targetSpeed, float rate)
        {
            float speedDifference = targetSpeed - _rigidbody.linearVelocity.x;
            float acceleration = Mathf.Clamp(speedDifference / Time.fixedDeltaTime, -rate, rate);
            _rigidbody.AddForce(Vector2.right * (acceleration * _rigidbody.mass));
        }

        public void ApplyBounce(float verticalVelocity, bool refreshAirAbilities)
        {
            _isBouncing = true;
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;

            // Cambiar a estado Jump primero
            _stateMachine.ChangeState(PlayerStateEnum.Jump);

            // Si el jugador mantiene o presiona el botón de salto al rebotar: SUPER REBOTE (+18%)
            float finalVelocity = verticalVelocity;
            if (Input.JumpHeld || Input.JumpDown)
            {
                finalVelocity *= 1.18f;
                Debug.Log($"<color=#00FFFF><b>[PlayerController]</b> ¡SUPER REBOTE! vy = {finalVelocity:F1} m/s</color>");
            }

            // Asignar velocidad de rebote DESPUÉS de JumpState.Enter()
            SetVelocityY(finalVelocity);

            if (refreshAirAbilities && _doubleJumpUnlocked)
            {
                _hasDoubleJump = true;
            }
        }

        public void ClearBouncing()
        {
            _isBouncing = false;
        }

        private void UpdateFacingDirection()
        {
            float moveX = Input.MoveVector.x;
            if (Mathf.Abs(moveX) > 0.1f)
            {
                FacingDirection2D newDir = moveX > 0 ? FacingDirection2D.Right : FacingDirection2D.Left;
                if (newDir != _facingDirection)
                {
                    _facingDirection = newDir;
                    Vector3 s = _visualRoot.localScale;
                    s.x = (int)_facingDirection * Mathf.Abs(s.x);
                    _visualRoot.localScale = s;
                }
            }
        }

        public void SetCheckpoint(Vector2 checkpointPosition)
        {
            _activeCheckpointPosition = checkpointPosition;
        }

        public void RequestCameraShake(float intensity, float duration)
        {
            _cameraShakeChannel?.Raise(intensity, duration);
        }

        public void RespawnAt(Vector2 position)
        {
            _rigidbody.position = position;
            transform.position = position;
            _rigidbody.linearVelocity = Vector2.zero;
            ResetGravityScale();
            _coyoteTimer = _config != null ? _config.CoyoteTime : 0.15f;
            _jumpBufferTimer = 0f;
            _dashCooldownTimer = 0f;
            _pendingInput = default;
            _physicsInput = default;
            _hasDoubleJump = _doubleJumpUnlocked && (_config == null || _config.CanDoubleJump);
            _canAirDash = _dashUnlocked && (_config == null || _config.CanDash);
            _isBouncing = false;
            _groundDetector?.ResetGroundState();
            _stateMachine.ChangeState(PlayerStateEnum.Idle);
            OnRespawned?.Invoke(position);
        }

        public void KillAndRespawn()
        {
            Debug.Log("[PlayerController] Daño por peligro ambiental detectado. Reapareciendo en checkpoint...");
            RespawnAt(_activeCheckpointPosition);
        }

        private PlayerCollisionService2D _collisionService;
        private void OnTriggerEnter2D(Collider2D other) => _collisionService.HandleTrigger(other);
        private void OnTriggerStay2D(Collider2D other) => _collisionService.HandleTrigger(other);
        private void OnCollisionEnter2D(Collision2D collision) => _collisionService.HandleCollision(collision);
        private void OnCollisionStay2D(Collision2D collision) => _collisionService.HandleCollision(collision);

#if UNITY_EDITOR || DEBUG
        private void OnGUI()
        {
            GUI.color = Color.white;
            GUI.Box(new Rect(10, 10, 360, 120), "<b>Alma Dino - Telemetría en Vivo</b>");
            GUI.Label(new Rect(20, 35, 340, 20), $"Estado: <b><color=#00FF88>{_stateMachine?.CurrentStateType}</color></b> | Suelo: <b>{(_groundDetector != null && _groundDetector.IsGrounded ? "<color=#00FF88>SÍ</color>" : "<color=#FF4444>NO (Aire)</color>")}</b>");
            GUI.Label(new Rect(20, 55, 340, 20), $"Velocidad: <b>{_rigidbody.linearVelocity.x:F1}x, {_rigidbody.linearVelocity.y:F1}y</b>");
            GUI.Label(new Rect(20, 75, 340, 20), $"Input Move: <b>{Input.MoveVector.x:F1}x</b> | Salto: <b>{(Input.JumpDown ? "DOWN" : Input.JumpHeld ? "HELD" : "-")}</b>");
            GUI.Label(new Rect(20, 95, 340, 25), $"<color=#FFD700>Controles: A/D o Joystick (Mover) | Espacio o JUMP (Saltar)</color>");
        }
#endif
    }
}
