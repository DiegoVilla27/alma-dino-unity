using UnityEngine;
namespace AlmaDino.Features.Boss.ScriptableObjects
{
    [CreateAssetMenu(menuName="Alma/Boss/Thief King")]
    public sealed class ThiefKingConfigSO : ScriptableObject
    {
        public float Telegraph = 1.3f;
        public float SweepDuration = 1.8f;
        public float Recovery = 4f;
        public float MeteorSpeed = 3f;
        public float ReflectedSpeed = 12f;
        public float RainWarning = 1.3f;
        public float RockSpeed = 8f;
        public float LavaDelay = 7f;
        public float LavaSpeed = .38f;
        public float SteamVelocity = 22f;
    }
}
