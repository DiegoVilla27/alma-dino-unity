using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ThiefKingHazard2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private ThiefKingBoss2D _boss;
        [SerializeField] private bool _meteor;
        public bool IsDangerous => !_boss.Fight.IsDefeated && (!_meteor || !_boss.MeteorReflected);
        public void OnHazardTouch() { Debug.Log("Final boss hazard: " + name); }
    }
}
