using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    public sealed class PterodactylBody2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private PterodactylBoss2D _boss;
        public bool IsDangerous => _boss != null && _boss.IsDiving;
        public void OnHazardTouch() { }
    }
}
