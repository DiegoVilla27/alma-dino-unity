using UnityEngine;

namespace AlmaDino.Features.Boss.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Boss/Armadillo Configuration")]
    public sealed class ArmadilloBossConfigSO : ScriptableObject
    {
        [Min(1f)] public float RollSpeed = 7f;
        [Min(0f)] public float SpeedPerHit = 1.5f;
        [Min(.1f)] public float WarningDuration = 1.2f;
        [Min(.1f)] public float StunDuration = 4.5f;
        [Min(.1f)] public float RecoveryDuration = 1f;
        [Min(0f)] public float JumpHeight = 2.7f;
        [Min(.1f)] public float RockWarningDuration = .9f;
        [Min(.1f)] public float RockFallSpeed = 10f;
        [Min(.1f)] public float RockInterval = 2f;
    }
}
