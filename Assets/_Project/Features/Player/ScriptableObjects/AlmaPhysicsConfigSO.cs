using UnityEngine;

namespace AlmaDino.Features.Player.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AlmaPhysicsConfig", menuName = "AlmaDino/Config/Alma Physics Config")]
    public class AlmaPhysicsConfigSO : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Velocidad horizontal máxima en m/s (GDD: 8.5)")]
        [SerializeField] private float _moveSpeed = 8.5f;
        [Tooltip("Tiempo de aceleración hacia velocidad máxima")]
        [SerializeField] private float _accelerationTime = 0.05f;
        [Tooltip("Tiempo de desaceleración hasta detenerse")]
        [SerializeField] private float _decelerationTime = 0.04f;

        [Header("Jump Physics")]
        [Tooltip("Fuerza del salto inicial en m/s (GDD: 14.0)")]
        [SerializeField] private float _jumpForce = 14.0f;
        [Tooltip("Fuerza del doble salto en m/s (GDD: 12.0)")]
        [SerializeField] private float _doubleJumpForce = 12.0f;
        [Tooltip("Multiplicador de gravedad al caer")]
        [SerializeField] private float _fallGravityMultiplier = 1.9f;
        [Tooltip("Multiplicador de gravedad al soltar el botón de salto antes de la cima (Jump Cut)")]
        [SerializeField] private float _jumpCutGravityMultiplier = 2.6f;
        [Tooltip("Velocidad terminal de caída máxima")]
        [SerializeField] private float _maxFallSpeed = 20.0f;

        [Header("Mobile Latency Compensation")]
        [Tooltip("Tiempo tras dejar una plataforma en el que aún se permite saltar (GDD: 0.12s, ajustado para mobile: 0.14s)")]
        [SerializeField] private float _coyoteTime = 0.14f;
        [Tooltip("Tiempo en que se almacena una pulsación de salto previa al aterrizaje (GDD: 0.10s, ajustado para mobile: 0.12s)")]
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

        [Header("Shock Roar / Rugido (Mundo 4)")]
        [Tooltip("Duración de la animación y efecto del rugido")]
        [SerializeField] private float _roarDuration = 0.25f;
        [Tooltip("Radio del área de choque")]
        [SerializeField] private float _roarRadius = 2.5f;

        [Header("Abilities Unlocked (Para pruebas o progresión)")]
        [SerializeField] private bool _canDoubleJump = true;
        [SerializeField] private bool _canGroundPound = true;
        [SerializeField] private bool _canDash = true;
        [SerializeField] private bool _canRoar = true;

        public float MoveSpeed => _moveSpeed;
        public float AccelerationTime => _accelerationTime;
        public float DecelerationTime => _decelerationTime;
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
        public float GroundPoundWindup => _groundPoundWindup;
        public float GroundPoundSpeed => _groundPoundSpeed;
        public float RoarDuration => _roarDuration;
        public float RoarRadius => _roarRadius;

        public bool CanDoubleJump { get => _canDoubleJump; set => _canDoubleJump = value; }
        public bool CanGroundPound { get => _canGroundPound; set => _canGroundPound = value; }
        public bool CanDash { get => _canDash; set => _canDash = value; }
        public bool CanRoar { get => _canRoar; set => _canRoar = value; }
    }
}
