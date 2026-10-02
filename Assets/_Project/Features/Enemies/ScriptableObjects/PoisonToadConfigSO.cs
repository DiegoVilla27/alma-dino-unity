using UnityEngine;
namespace AlmaDino.Features.Enemies.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Alma/Enemies/Poison Toad Configuration")]
    public sealed class PoisonToadConfigSO : ScriptableObject
    {
        [Min(0.1f)] public float ShotInterval = 2f;
        [Min(0.1f)] public float WarningDuration = 0.6f;
        [Min(1f)] public float DetectionDistance = 18f;
        [Min(0.1f)] public float HorizontalSpeed = 8f;
        public float VerticalSpeed = 3f;
        [Min(0.1f)] public float GravityScale = 0.6f;
        [Min(0.1f)] public float ProjectileLifetime = 3f;
        [Min(1)] public int PoolSize = 4;
    }
}
