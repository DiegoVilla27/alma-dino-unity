using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [CreateAssetMenu(menuName = "Alma/Environment/Seesaw Config")]
    public class SeesawConfigSO : ScriptableObject
    {
        [SerializeField] private float _maxAngle = 18f;
        [SerializeField] private float _tiltSpeed = 35f;
        [SerializeField] private float _returnSpeed = 25f;
        [SerializeField] private float _impactTiltSpeed = 150f;
        [SerializeField] private float _minimumImpactLever = 0.5f;
        [SerializeField] private float _launchWindow = 2.8f;
        [SerializeField] private float _playerLaunchVelocity = 15f;
        [SerializeField] private float _weightLaunchVelocity = 14f;
        [SerializeField] private float _weightResetDelay = 4.7f;
        [SerializeField] private float _gateOpenDuration = 4f;

        public float MaxAngle => _maxAngle;
        public float TiltSpeed => _tiltSpeed;
        public float ReturnSpeed => _returnSpeed;
        public float ImpactTiltSpeed => _impactTiltSpeed;
        public float MinimumImpactLever => _minimumImpactLever;
        public float LaunchWindow => _launchWindow;
        public float PlayerLaunchVelocity => _playerLaunchVelocity;
        public float WeightLaunchVelocity => _weightLaunchVelocity;
        public float WeightResetDelay => _weightResetDelay;
        public float GateOpenDuration => _gateOpenDuration;
    }
}
