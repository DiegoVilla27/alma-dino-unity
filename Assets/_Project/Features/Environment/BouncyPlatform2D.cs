using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Plataforma de hongo o rama elástica que impulsa verticalmente a Alma.
    /// Renueva el Doble Salto e incluye efecto elástico (Squish & Stretch) para máxima respuesta visual.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BouncyPlatform2D : MonoBehaviour
    {
        [Header("Bounce Physics")]
        [Tooltip("Velocidad de rebote normal en m/s (altura alcanzable ~5.5m)")]
        [SerializeField] private float _bounceVelocity = 15.5f;
        [Tooltip("Si el rebote renueva el Doble Salto en el aire")]
        [SerializeField] private bool _refreshDoubleJump = true;

        [Header("Visual Feedback (Squish & Stretch)")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Vector3 _squishScale = new Vector3(1.35f, 0.65f, 1f);
        [SerializeField] private Vector3 _stretchScale = new Vector3(0.85f, 1.25f, 1f);
        [SerializeField] private float _squishDuration = 0.08f;
        [SerializeField] private float _reboundDuration = 0.16f;

        private Vector3 _originalScale;
        private Coroutine _bounceRoutine;
        private float _lastBounceTime;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }
            _originalScale = _visualRoot.localScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryBounce(other.gameObject, other.bounds.center.y, other.attachedRigidbody);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Solo rebotar si el impacto proviene de arriba hacia abajo
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (contact.normal.y < -0.4f)
                {
                    TryBounce(collision.gameObject, collision.transform.position.y, collision.rigidbody);
                    return;
                }
            }
        }

        private void TryBounce(GameObject target, float targetCenterY, Rigidbody2D rb)
        {
            // Cooldown mínimo de 0.15s para evitar multi-triggers
            if (Time.time - _lastBounceTime < 0.15f) return;

            // Verificar que el objetivo esté cayendo o posado sobre el hongo
            if (rb != null && rb.linearVelocity.y > 0.5f) return;

            var bounceable = target.GetComponentInParent<IBounceable2D>();
            if (bounceable != null)
            {
                _lastBounceTime = Time.time;
                bounceable.ApplyBounce(_bounceVelocity, _refreshDoubleJump);
                TriggerVisualBounce();
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
            // 1. Squish (Compresión)
            float t = 0f;
            Vector3 startScale = _visualRoot.localScale;
            Vector3 targetSquish = Vector3.Scale(_originalScale, _squishScale);
            while (t < _squishDuration)
            {
                t += Time.deltaTime;
                _visualRoot.localScale = Vector3.Lerp(startScale, targetSquish, t / _squishDuration);
                yield return null;
            }

            // 2. Stretch (Elongación hacia arriba)
            t = 0f;
            Vector3 targetStretch = Vector3.Scale(_originalScale, _stretchScale);
            float stretchDuration = _reboundDuration * 0.5f;
            while (t < stretchDuration)
            {
                t += Time.deltaTime;
                _visualRoot.localScale = Vector3.Lerp(targetSquish, targetStretch, t / stretchDuration);
                yield return null;
            }

            // 3. Retorno elástico a forma original
            t = 0f;
            float returnDuration = _reboundDuration * 0.5f;
            while (t < returnDuration)
            {
                t += Time.deltaTime;
                _visualRoot.localScale = Vector3.Lerp(targetStretch, _originalScale, t / returnDuration);
                yield return null;
            }

            _visualRoot.localScale = _originalScale;
            _bounceRoutine = null;
        }

        private void OnDisable()
        {
            if (_visualRoot != null)
            {
                _visualRoot.localScale = _originalScale;
            }
            _bounceRoutine = null;
        }
    }
}
