using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public class BossBodyHazard2D : MonoBehaviour, IHazard2D
    {
        [SerializeField] private bool _isActive = true;

        private Collider2D _collider;

        public bool IsActive => _isActive;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        public void SetHazardActive(bool active)
        {
            _isActive = active;
            if (_collider != null)
            {
                _collider.enabled = active;
            }
        }

        public void OnHazardTouch()
        {
            Debug.Log("[BossBodyHazard2D] Alma impactó con el cuerpo del simio gigante.");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isActive) return;

            if (collision.collider.TryGetComponent<IPlayerRespawnable>(out var respawnable) ||
                collision.gameObject.TryGetComponent<IPlayerRespawnable>(out respawnable))
            {
                OnHazardTouch();
                respawnable.KillAndRespawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isActive) return;

            if (collision.TryGetComponent<IPlayerRespawnable>(out var respawnable) ||
                collision.gameObject.TryGetComponent<IPlayerRespawnable>(out respawnable))
            {
                OnHazardTouch();
                respawnable.KillAndRespawn();
            }
        }
    }
}
