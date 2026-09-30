using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Plataforma de hongo o rama elástica que impulsa verticalmente a Alma.
    /// Renueva el Doble Salto e incluye efecto elástico (Squish & Stretch) para máxima respuesta visual.
    /// Funciona de manera robusta tanto con colisiones sólidas como con zonas de activación (Triggers).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BouncyPlatform2D : MonoBehaviour, IBouncySurface2D
    {
        [Header("Bounce Physics")]
        [Tooltip("Velocidad de rebote normal en m/s (altura alcanzable ~6.5m)")]
        [SerializeField] private float _bounceVelocity = 17.0f;
        [Tooltip("Si el rebote renueva el Doble Salto en el aire")]
        [SerializeField] private bool _refreshDoubleJump = true;

        [Header("Visual Feedback (Squish & Stretch)")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Vector3 _squishScale = new Vector3(1.35f, 0.55f, 1f);
        [SerializeField] private Vector3 _stretchScale = new Vector3(0.85f, 1.35f, 1f);
        [SerializeField] private float _squishDuration = 0.06f;
        [SerializeField] private float _reboundDuration = 0.14f;

        private Vector3 _originalScale;
        private Coroutine _bounceRoutine;
        private float _lastBounceTime;
        private const float COOLDOWN = 0.15f;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }
            _originalScale = _visualRoot.localScale;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleCollision(collision);
        }

        private void HandleCollision(Collision2D collision)
        {
            if (Time.time - _lastBounceTime < COOLDOWN) return;

            // Verificar que el impacto provenga de arriba del hongo
            if (collision.transform.position.y >= transform.position.y - 0.25f)
            {
                TryBounce(collision.gameObject, collision.rigidbody);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleTrigger(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            HandleTrigger(other);
        }

        private void HandleTrigger(Collider2D other)
        {
            if (Time.time - _lastBounceTime < COOLDOWN) return;

            if (other.transform.position.y >= transform.position.y - 0.35f)
            {
                TryBounce(other.gameObject, other.attachedRigidbody);
            }
        }

        private void TryBounce(GameObject target, Rigidbody2D rb)
        {
            // Si el objetivo ya fue lanzado y sigue subiendo a alta velocidad, evitar re-triggering prematuro
            if (rb != null && rb.linearVelocity.y > _bounceVelocity * 0.6f) return;

            var bounceable = target.GetComponentInParent<IBounceable2D>();
            if (bounceable != null)
            {
                _lastBounceTime = Time.time;
                bounceable.ApplyBounce(_bounceVelocity, _refreshDoubleJump);
                TriggerVisualBounce();
                Debug.Log($"<color=#FF00FF><b>[BouncyPlatform2D]</b> ¡BOING! Hongo activado en '{gameObject.name}'.</color>");
            }
            else if (rb != null && !rb.isKinematic)
            {
                _lastBounceTime = Time.time;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, _bounceVelocity);
                TriggerVisualBounce();
            }
        }

        public void TriggerVisualBounce()
        {
            if (_visualRoot == null) return;
            if (_bounceRoutine != null)
            {
                StopCoroutine(_bounceRoutine);
            }
            _bounceRoutine = StartCoroutine(SquishAndStretchRoutine());
        }

        private IEnumerator SquishAndStretchRoutine()
        {
            // 1. Squish (Compresión rápida)
            float t = 0f;
            Vector3 startScale = _visualRoot.localScale;
            Vector3 targetSquish = Vector3.Scale(_originalScale, _squishScale);
            while (t < _squishDuration)
            {
                t += Time.deltaTime;
                _visualRoot.localScale = Vector3.Lerp(startScale, targetSquish, t / _squishDuration);
                yield return null;
            }

            // 2. Stretch (Expansión elástica vertical)
            t = 0f;
            Vector3 targetStretch = Vector3.Scale(_originalScale, _stretchScale);
            while (t < _reboundDuration * 0.5f)
            {
                t += Time.deltaTime;
                _visualRoot.localScale = Vector3.Lerp(targetSquish, targetStretch, t / (_reboundDuration * 0.5f));
                yield return null;
            }

            // 3. Regreso elástico a la escala original
            t = 0f;
            while (t < _reboundDuration * 0.5f)
            {
                t += Time.deltaTime;
                _visualRoot.localScale = Vector3.Lerp(targetStretch, _originalScale, t / (_reboundDuration * 0.5f));
                yield return null;
            }

            _visualRoot.localScale = _originalScale;
            _bounceRoutine = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, new Vector3(2f, 0.5f, 0f));
        }
    }
}
