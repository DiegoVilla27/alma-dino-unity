using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Muro o barrera vegetal densa que solo puede ser destruida cuando Alma
    /// la embiste a toda velocidad utilizando el Dash Aéreo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DashBreakableBarrier2D : MonoBehaviour, IDashBreakable2D
    {
        [Header("Break Settings")]
        [Tooltip("Si la barrera reaparece tras un tiempo o queda permanentemente abierta")]
        [SerializeField] private bool _canRespawn = false;
        [SerializeField] private float _respawnDelay = 5.0f;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _barrierRenderer;
        [SerializeField] private float _fadeDuration = 0.2f;

        private Collider2D _collider;
        private bool _isBroken;

        public bool IsBroken => _isBroken;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_barrierRenderer == null)
                _barrierRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void BreakWithDash()
        {
            if (_isBroken) return;
            StartCoroutine(BreakSequenceRoutine());
        }

        private IEnumerator BreakSequenceRoutine()
        {
            _isBroken = true;
            _collider.enabled = false;
            Debug.Log($"<color=#00FFAA><b>[DashBreakableBarrier2D]</b> ¡Barrera vegetal destruida por Dash en {gameObject.name}!</color>");

            // Desvanecimiento rápido
            if (_barrierRenderer != null)
            {
                Color startColor = _barrierRenderer.color;
                float t = 0f;
                while (t < _fadeDuration)
                {
                    t += Time.deltaTime;
                    _barrierRenderer.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t / _fadeDuration);
                    yield return null;
                }
                _barrierRenderer.enabled = false;
            }

            if (_canRespawn)
            {
                yield return new WaitForSeconds(_respawnDelay);
                Respawn();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void Respawn()
        {
            _isBroken = false;
            if (_collider != null) _collider.enabled = true;
            if (_barrierRenderer != null)
            {
                Color c = _barrierRenderer.color;
                _barrierRenderer.color = new Color(c.r, c.g, c.b, 1f);
                _barrierRenderer.enabled = true;
            }
            gameObject.SetActive(true);
        }
    }
}
