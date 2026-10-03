using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Boss.Controllers
{
    public enum KingMechanism { HeatSeal, WeakPlate, Meteor, SteamSeal, Anchor }
    [RequireComponent(typeof(Collider2D))]
    public sealed class ThiefKingMechanism2D : MonoBehaviour, IDashBreakable2D, IGroundPoundReceiver2D, IRoarReactive2D
    {
        [SerializeField] private ThiefKingBoss2D _boss;
        [SerializeField] private KingMechanism _kind;
        public void BreakWithDash() { if(_kind == KingMechanism.HeatSeal) _boss.CounterCharge(); }
        public void ReceiveGroundPound(Vector2 impact)
        {
            if(_kind == KingMechanism.WeakPlate) _boss.Strike();
            else if(_kind == KingMechanism.SteamSeal) _boss.OpenSteam();
            else if(_kind == KingMechanism.Anchor) _boss.Finish();
        }
        public void ReceiveRoar(Vector2 direction)
        {
            if(_kind == KingMechanism.Meteor) _boss.ReflectMeteor(direction);
            else if(_kind == KingMechanism.Anchor) _boss.CrackAnchor();
        }
    }
}
