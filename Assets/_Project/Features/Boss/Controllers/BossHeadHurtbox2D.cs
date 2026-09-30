using System;
using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Boss.Models;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public class BossHeadHurtbox2D : MonoBehaviour
    {
        [SerializeField] private GiantMonkeyBoss2D _boss;
        [SerializeField] private float _stompBounceForce = 13.5f;

        private Collider2D _collider;
        private bool _isActive = false;

        public bool IsActive => _isActive;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;

            if (_boss == null)
            {
                _boss = GetComponentInParent<GiantMonkeyBoss2D>();
            }
        }

        public void SetHurtboxActive(bool active)
        {
            _isActive = active;
            if (_collider != null)
            {
                _collider.enabled = active;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isActive) return;
            if (_boss == null || _boss.CurrentState != BossStateEnum.TiredDescent) return;

            if (collision.TryGetComponent<IBounceable2D>(out var bounceable))
            {
                // Player stepped on the head!
                bounceable.ApplyBounce(_stompBounceForce, true);
                _boss.OnHeadStomped();
            }
        }
    }
}
