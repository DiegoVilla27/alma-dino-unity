using UnityEngine;

namespace AlmaDino.Features.Player.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AlmaPhysicsConfig", menuName = "AlmaDino/Config/Alma Physics Config")]
    public class AlmaPhysicsConfigSO : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Velocidad horizontal máxima en m/s (Calibrado: 7.0)")]
        [SerializeField] private float _moveSpeed = 7.0f;
        [Tooltip("Tiempo de aceleración hacia velocidad máxima")]
        [SerializeField] private float _accelerationTime = 0.10f;
        [Tooltip("Tiempo de desaceleración hasta detenerse")]
        [SerializeField] private float _decelerationTime = 0.08f;

        [Header("Jump Physics")]
        [Tooltip("Escala de gravedad base para físicas 2D ágiles (Calibrado: 2.2)")]
        [SerializeField] private float _gravityScale = 2.2f;
        [Tooltip("Fuerza del salto inicial en m/s (Calibrado: 8.2, altura ~1.56m)")]
        [SerializeField] private float _jumpForce = 8.2f;
        [Tooltip("Fuerza del doble salto en m/s (Calibrado: 7.6, altura extra ~1.34m)")]
        [SerializeField] private float _doubleJumpForce = 7.6f;
        [Tooltip("Multiplicador de gravedad al caer (Calibrado: 1.8)")]
        [SerializeField] private float _fallGravityMultiplier = 1.8f;
        [Tooltip("Multiplicador de gravedad al soltar el botón de salto antes de la cima (Jump Cut, Calibrado: 2.4)")]
        [SerializeField] private float _jumpCutGravityMultiplier = 2.4f;
        [Tooltip("Velocidad terminal de caída máxima")]
        [SerializeField] private float _maxFallSpeed = 20.0f;

        [Header("Mobile Latency Compensation")]
        [Tooltip("Tiempo tras dejar una plataforma en el que aún se permite saltar (0.14s)")]
        [SerializeField] private float _coyoteTime = 0.14f;
        [Tooltip("Tiempo en que se almacena una pulsación de salto previa al aterrizaje (0.12s)")]
        [SerializeField] private float _jumpBufferTime = 0.12f;

        [Header("Air Dash (Mundo 3)")]
        [Tooltip("Distancia del dash en metros (GDD: 6.0)")]
        [SerializeField] private float _dashDistance = 6.0f;
        [Tooltip("Duración del dash en segundos (GDD: 0.2)")]
        [SerializeField] private float _dashDuration = 0.2f;
        [Tooltip("Cooldown entre dashes")]
        [SerializeField] private float _dashCooldown = 0.4f;

        [Header("Ground Pound / Pisotón Sísmico (Mundo 2)")]
        [Tooltip("Pausa antes de descender en seco (GDD: 0.1s)")]
        [SerializeField] private float _groundPoundWindup = 0.1f;
        [Tooltip("Velocidad vertical descendente en m/s (GDD: 22.0)")]
        [SerializeField] private float _groundPoundSpeed = 22.0f;
        [Tooltip("Radio de la onda sísmica al impactar desde arriba") ]
        [SerializeField] private float _groundPoundShockRadius = 2f;

        [Header("Shock Roar / Rugido (Mundo 4)")]
        [Tooltip("Duración de la animación y efecto del rugido")]
        [SerializeField] private float _roarDuration = 0.25f;
        [Tooltip("Radio del área de choque")]
        [SerializeField] private float _roarRadius = 3f;

        [SerializeField] private float _roarHalfAngle = 45f;
        [SerializeField] private float _roarResonanceRange = 8f;

        [Header("Abilities Unlocked (Para pruebas o progresión)")]
        [SerializeField] private bool _canDoubleJump = true;
        [SerializeField] private bool _canGroundPound = true;
        [SerializeField] private bool _canDash = true;
        [SerializeField] private bool _canRoar = true;

        public float MoveSpeed => _moveSpeed;
        public float AccelerationTime => _accelerationTime;
        public float DecelerationTime => _decelerationTime;
        public float GravityScale => _gravityScale;
        public float JumpForce => _jumpForce;
        public float DoubleJumpForce => _doubleJumpForce;
        public float FallGravityMultiplier => _fallGravityMultiplier;
        public float JumpCutGravityMultiplier => _jumpCutGravityMultiplier;
        public float MaxFallSpeed => _maxFallSpeed;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;
        public float DashDistance => _dashDistance;
        public float DashDuration => _dashDuration;
        public float DashCooldown => _dashCooldown;
        public float GroundPoundShockRadius => _groundPoundShockRadius;

        public float GroundPoundWindup => _groundPoundWindup;
        public float GroundPoundSpeed => _groundPoundSpeed;
        public float RoarDuration => _roarDuration;
        public float RoarResonanceRange => _roarResonanceRange;
        public float RoarHalfAngle => _roarHalfAngle;
        public float RoarRadius => _roarRadius;

        public bool CanDoubleJump { get => _canDoubleJump; set => _canDoubleJump = value; }
        public bool CanGroundPound { get => _canGroundPound; set => _canGroundPound = value; }
        public bool CanDash { get => _canDash; set => _canDash = value; }
        public bool CanRoar { get => _canRoar; set => _canRoar = value; }
    }
}
