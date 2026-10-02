using UnityEngine;
namespace AlmaDino.Features.Environment.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Environment/Geyser Config")]
    public sealed class GeyserConfigSO : ScriptableObject
    {
        [SerializeField] private float _dormantDuration = 2.2f;
        [SerializeField] private float _warningDuration = .8f;
        [SerializeField] private float _eruptionDuration = 1.2f;
        public float DormantDuration => _dormantDuration;
        public float WarningDuration => _warningDuration;
        public float EruptionDuration => _eruptionDuration;
    }
}
