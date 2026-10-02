using UnityEngine;

namespace AlmaDino.Features.Environment.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Environment/Rising Gas Config")]
    public class RisingGasConfigSO : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _warningDuration = 3f;
        [SerializeField, Min(0f)] private float _riseSpeed = 0.9f;
        [SerializeField, Min(1f)] private float _checkpointClearance = 4f;
        public float WarningDuration => _warningDuration;
        public float RiseSpeed => _riseSpeed;
        public float CheckpointClearance => _checkpointClearance;
    }
}
