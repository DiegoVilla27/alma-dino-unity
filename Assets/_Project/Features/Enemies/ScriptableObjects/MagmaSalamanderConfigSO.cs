using UnityEngine;
namespace AlmaDino.Features.Enemies.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Enemies/Magma Salamander Config")]
    public sealed class MagmaSalamanderConfigSO : ScriptableObject
    {
        [SerializeField] private float _moveSpeed = 2.2f;
        [SerializeField] private float _shotInterval = 2.2f;
        [SerializeField] private float _warningDuration = .65f;
        [SerializeField] private float _detectionDistance = 8f;
        [SerializeField] private float _projectileSpeed = 7f;
        [SerializeField] private float _projectileLifetime = 2f;
        [SerializeField] private float _stunDuration = 3f;
        [SerializeField] private float _knockbackSpeed = 6f;
        public float MoveSpeed => _moveSpeed;
        public float ShotInterval => _shotInterval;
        public float WarningDuration => _warningDuration;
        public float DetectionDistance => _detectionDistance;
        public float ProjectileSpeed => _projectileSpeed;
        public float ProjectileLifetime => _projectileLifetime;
        public float StunDuration => _stunDuration;
        public float KnockbackSpeed => _knockbackSpeed;
    }
}
