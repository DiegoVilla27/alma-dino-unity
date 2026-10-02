using UnityEngine;
namespace AlmaDino.Features.Environment.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Environment/Resonance Bell Config")]
    public sealed class ResonanceBellConfigSO : ScriptableObject
    {
        [SerializeField] private float _suppressionDuration = 5f;
        [SerializeField] private float _roarRange = 8f;
        [SerializeField] private float _warningThreshold = 1f;
        public float SuppressionDuration => _suppressionDuration;
        public float RoarRange => _roarRange;
        public float WarningThreshold => _warningThreshold;
    }
}
