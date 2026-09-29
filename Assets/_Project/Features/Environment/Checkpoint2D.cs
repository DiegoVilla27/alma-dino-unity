using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint2D : MonoBehaviour
    {
        [SerializeField] private Color _inactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _activeColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private SpriteRenderer _indicatorRenderer;

        private bool _isActivated;

        private void Awake()
        {
            if (_indicatorRenderer == null)
                _indicatorRenderer = GetComponent<SpriteRenderer>();

            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isActivated) return;

            if (other.TryGetComponent<IPlayerRespawnable>(out var playerRespawnable))
            {
                _isActivated = true;
                playerRespawnable.SetCheckpoint(transform.position);
                UpdateVisuals();
                Debug.Log($"[Checkpoint2D] Checkpoint alcanzado y guardado en {transform.position}");
            }
        }

        private void UpdateVisuals()
        {
            if (_indicatorRenderer != null)
            {
                _indicatorRenderer.color = _isActivated ? _activeColor : _inactiveColor;
            }
        }
    }
}
