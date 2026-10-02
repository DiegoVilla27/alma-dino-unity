using UnityEngine;
namespace AlmaDino.Features.Environment.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Environment/Boulder Roar Config")]
    public sealed class BoulderRoarConfigSO : ScriptableObject
    {
        [SerializeField] private float _pushDistance = 5f;
        [SerializeField] private float _pushDuration = .8f;
        [SerializeField] private float _arcHeight = .6f;
        [SerializeField] private float _radius = 1.8f;
        [SerializeField] private float _bridgeWidth = 4.2f;
        [SerializeField] private float _bridgeTop = .2f;
        public float PushDistance => _pushDistance;
        public float PushDuration => _pushDuration;
        public float ArcHeight => _arcHeight;
        public float Radius => _radius;
        public float BridgeWidth => _bridgeWidth;
        public float BridgeTop => _bridgeTop;
    }
}
