using UnityEngine;
namespace AlmaDino.Features.Environment
{
    [CreateAssetMenu(menuName = "Alma/Environment/Crushing Ceiling Config")]
    public class CrushingCeilingConfigSO : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _openDuration = 3f;
        [SerializeField, Min(0.3f)] private float _warningDuration = 0.8f;
        [SerializeField, Min(0.2f)] private float _descentDuration = 0.6f;
        [SerializeField, Min(0.1f)] private float _closedDuration = 0.4f;
        [SerializeField, Min(0.5f)] private float _retractDuration = 2f;
        public float OpenDuration => _openDuration;
        public float WarningDuration => _warningDuration;
        public float DescentDuration => _descentDuration;
        public float ClosedDuration => _closedDuration;
        public float RetractDuration => _retractDuration;
        public float CycleDuration => _openDuration + _descentDuration + _closedDuration + _retractDuration;
    }
}
