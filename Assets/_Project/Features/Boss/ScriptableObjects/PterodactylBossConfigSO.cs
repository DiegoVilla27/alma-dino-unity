using UnityEngine;

namespace AlmaDino.Features.Boss.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Boss/Pterodactyl Config")]
    public sealed class PterodactylBossConfigSO : ScriptableObject
    {
        [SerializeField] private float _windDuration = 3f;
        [SerializeField] private float _windAcceleration = 12f;
        [SerializeField] private float _warningDuration = 1.4f;
        [SerializeField] private float _diveSpeed = 7f;
        [SerializeField] private float _speedPerHit = 1f;
        [SerializeField] private float _recoveryDuration = 2.5f;
        [SerializeField] private float _flightHeight = 3.5f;
        [SerializeField] private float _diveReach = 8f;
        [SerializeField] private float _bounceVelocity = 8.2f;
        [SerializeField] private float _hitStopDuration = 0.05f;
        public float WindDuration => _windDuration;
        public float WindAcceleration => _windAcceleration;
        public float WarningDuration => _warningDuration;
        public float DiveSpeed => _diveSpeed;
        public float SpeedPerHit => _speedPerHit;
        public float RecoveryDuration => _recoveryDuration;
        public float FlightHeight => _flightHeight;
        public float DiveReach => _diveReach;
        public float BounceVelocity => _bounceVelocity;
        public float HitStopDuration => _hitStopDuration;
    }
}
