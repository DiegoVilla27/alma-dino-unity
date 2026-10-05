using UnityEngine;

namespace AlmaGame.Player
{
    [CreateAssetMenu(menuName = "Alma/Movement settings")]
    public sealed class AlmaMovementSettings : ScriptableObject
    {
        [Header("Movement")]
        [Min(0.1f)] public float MoveSpeed = 7f;
        [Min(0.01f)] public float AccelerationTime = 0.10f;
        [Min(0.01f)] public float BrakingTime = 0.08f;
        [Min(0.01f)] public float AirAccelerationTime = 0.13f;

        [Header("Jump")]
        [Min(0.1f)] public float JumpSpeed = 8.2f;
        [Min(0.1f)] public float DoubleJumpSpeed = 7.6f;
        [Min(0.1f)] public float GravityScale = 2.2f;
        [Min(1f)] public float FallGravityMultiplier = 1.8f;
        [Min(1f)] public float ReleasedJumpGravityMultiplier = 2.4f;
        [Min(1f)] public float MaxFallSpeed = 20f;
        [Min(0f)] public float CoyoteTime = 0.14f;
        [Min(0f)] public float JumpBufferTime = 0.12f;

        [Header("Ground pound")]
        [Min(0f)] public float GroundPoundWindupTime = 0.1f;
        [Min(1f)] public float GroundPoundSpeed = 22f;

        [Header("Dash")]
        [Min(0.1f)] public float DashDistance = 6f;
        [Min(0.01f)] public float DashDuration = 0.3f;
        [Min(0f)] public float DashCooldown = 0.4f;

        [Header("Roar")]
        // Matches Player_Roar_Animation (8 frames at 12 fps): Alma can't walk until the roar ends.
        [Min(0.01f)] public float RoarDuration = 0.6666667f;
        [Min(0.1f)] public float RoarRange = 3f;
        [Range(0f, 90f)] public float RoarHalfAngle = 45f;
        [Min(0.1f)] public float RoarResonanceRange = 8f;

        [Header("Ground detection")]
        public LayerMask GroundLayers = ~0;
        [Range(0.01f, 0.15f)] public float GroundProbeDistance = 0.04f;
        [Range(0.1f, 1f)] public float MinimumGroundNormal = 0.65f;
    }
}
