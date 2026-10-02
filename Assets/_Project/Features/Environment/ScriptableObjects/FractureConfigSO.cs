using UnityEngine;
namespace AlmaDino.Features.Environment.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Environment/Fracture Config")]
    public sealed class FractureConfigSO : ScriptableObject
    {
        [SerializeField] private float _meteorWarning = 1.1f;
        [SerializeField] private float _meteorSpeed = 1.8f;
        [SerializeField] private float _reflectedSpeed = 12f;
        [SerializeField] private float _meteorLifetime = 4f;
        [SerializeField] private float _jetWarning = 1.2f;
        [SerializeField] private float _jetDuration = 1.1f;
        [SerializeField] private float _jetRecovery = 2f;
        [SerializeField] private float _eruptionWarning = 1.4f;
        [SerializeField] private float _eruptionRiseSpeed = .9f;
        [SerializeField] private float _eruptionInitialHeight = -3.2f;
        [SerializeField] private float _eruptionCeiling = 3.4f;
        public float EruptionWarning => _eruptionWarning;
        public float EruptionRiseSpeed => _eruptionRiseSpeed;
        public float EruptionInitialHeight => _eruptionInitialHeight;
        public float EruptionCeiling => _eruptionCeiling;
        public float MeteorWarning => _meteorWarning;
        public float MeteorSpeed => _meteorSpeed;
        public float ReflectedSpeed => _reflectedSpeed;
        public float MeteorLifetime => _meteorLifetime;
        public float JetWarning => _jetWarning;
        public float JetDuration => _jetDuration;
        public float JetRecovery => _jetRecovery;
    }
}
