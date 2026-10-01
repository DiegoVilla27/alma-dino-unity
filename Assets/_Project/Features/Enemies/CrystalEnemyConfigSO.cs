using UnityEngine;

namespace AlmaDino.Features.Enemies
{
    [CreateAssetMenu(menuName = "Alma/Enemies/Crystal Enemy Config")]
    public class CrystalEnemyConfigSO : ScriptableObject
    {
        [SerializeField] private float _beetleSpeed = 0.7f;
        [SerializeField] private float _flipDuration = 3.5f;
        [SerializeField] private float _flipHopVelocity = 1.5f;
        [SerializeField] private float _batDetectionDistance = 4f;
        [SerializeField] private float _batWarningDuration = 0.65f;
        [SerializeField] private float _batFlightDuration = 1.6f;
        [SerializeField] private float _batRestDuration = 2f;
        [SerializeField] private float _batArcWidth = 2.2f;
        public float BeetleSpeed => _beetleSpeed;
        public float FlipDuration => _flipDuration;
        public float FlipHopVelocity => _flipHopVelocity;
        public float BatDetectionDistance => _batDetectionDistance;
        public float BatWarningDuration => _batWarningDuration;
        public float BatFlightDuration => _batFlightDuration;
        public float BatRestDuration => _batRestDuration;
        public float BatArcWidth => _batArcWidth;
    }
}
