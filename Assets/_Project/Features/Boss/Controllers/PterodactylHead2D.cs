using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PterodactylHead2D : MonoBehaviour, IDashStrikeReceiver2D, IConditionalHazard2D
    {
        [SerializeField] private PterodactylBoss2D _boss;
        public float BounceVelocity => _boss.BounceVelocity;
        public bool IsDangerous => _boss != null && _boss.IsDiving;
        public bool TryReceiveAirDash(Vector2 velocity) => _boss != null && _boss.TryStrike(velocity);
        public void OnHazardTouch() { }
    }
}
