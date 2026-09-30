using System.Collections;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class CrumblingPlatform2D : MonoBehaviour
    {
        [Header("Timing")]
        [Tooltip("Tiempo desde que se pisa hasta que colapsa (GDD: 1.5s)")]
        [SerializeField] private float _crumbleDelay = 1.5f;
        [Tooltip("Tiempo en reaparecer tras colapsar")]
        [SerializeField] private float _respawnDelay = 3.0f;

        [Header("Juice Feedback")]
        [SerializeField] private float _shakeIntensity = 0.05f;
        [SerializeField] private Color _crackingTint = new Color(0.85f, 0.4f, 0.4f, 1f);

        private Vector3 _originalPosition;
        private Color _originalColor = Color.white;
        private Collider2D _collider;
        private SpriteRenderer _renderer;
        private bool _isCrumbling;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponentInChildren<SpriteRenderer>();
            _originalPosition = transform.position;

            if (_renderer != null)
            {
                _originalColor = _renderer.color;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isCrumbling) return;

            // Comprobar que el jugador aterrizó desde arriba
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                // Si la normal apunta hacia arriba, significa que el jugador está parado encima
                if (contact.normal.y < -0.5f)
                {
                    StartCoroutine(CrumbleSequenceRoutine());
                    break;
                }
            }
        }

        private IEnumerator CrumbleSequenceRoutine()
        {
            _isCrumbling = true;
            float elapsed = 0f;

            // Fase de temblor antes de colapsar
            while (elapsed < _crumbleDelay)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _crumbleDelay;

                // Aumentar temblor a medida que se acerca al colapso
                float currentShake = _shakeIntensity * (0.3f + progress * 0.7f);
                Vector3 randomOffset = (Vector3)(Random.insideUnitCircle * currentShake);
                transform.position = _originalPosition + randomOffset;

                // Tinte rojizo/agrietado progresivo
                if (_renderer != null)
                {
                    _renderer.color = Color.Lerp(_originalColor, _crackingTint, progress);
                }

                yield return null;
            }

            // Colapso
            transform.position = _originalPosition;
            if (_collider != null) _collider.enabled = false;
            if (_renderer != null) _renderer.enabled = false;

            // Esperar tiempo de reaparición
            yield return new WaitForSeconds(_respawnDelay);

            // Reaparición
            if (_renderer != null)
            {
                _renderer.color = _originalColor;
                _renderer.enabled = true;
            }
            if (_collider != null) _collider.enabled = true;

            _isCrumbling = false;
        }
    }
}
