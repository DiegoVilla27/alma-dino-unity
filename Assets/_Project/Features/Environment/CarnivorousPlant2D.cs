using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Planta carnívora estática de la selva con ciclo rítmico de advertencia y mordisco.
    /// Es inofensiva cuando está abierta y letal (IHazard2D) cuando cierra sus fauces con espinas.
    /// </summary>
    public class CarnivorousPlant2D : MonoBehaviour, IHazard2D
    {
        [Header("Cycle Timings")]
        [Tooltip("Tiempo en reposo abierta (segura)")]
        [SerializeField] private float _openDuration = 2.0f;
        [Tooltip("Tiempo de temblor / advertencia previa")]
        [SerializeField] private float _warningDuration = 0.5f;
        [Tooltip("Tiempo en que las fauces permanecen cerradas (letal)")]
        [SerializeField] private float _biteDuration = 1.2f;

        [Header("References & Visuals")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _hazardCollider;
        [SerializeField] private Color _openColor = new Color(0.25f, 0.85f, 0.45f, 1f);
        [SerializeField] private Color _warningColor = new Color(1.0f, 0.75f, 0.15f, 1f);
        [SerializeField] private Color _biteColor = new Color(0.95f, 0.2f, 0.2f, 1f);

        private Vector3 _initialLocalPos;
        private bool _isSnapping = false;

        private void Awake()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_hazardCollider == null) _hazardCollider = GetComponent<Collider2D>();

            _initialLocalPos = transform.localPosition;
            if (_hazardCollider != null)
            {
                _hazardCollider.isTrigger = true;
                _hazardCollider.enabled = false; // Inofensiva por defecto
            }
        }

        private void OnEnable()
        {
            StartCoroutine(PlantCycleRoutine());
        }

        private IEnumerator PlantCycleRoutine()
        {
            while (true)
            {
                // 1. Estado Abierto (Inofensivo)
                _isSnapping = false;
                if (_hazardCollider != null) _hazardCollider.enabled = false;
                if (_spriteRenderer != null) _spriteRenderer.color = _openColor;
                transform.localPosition = _initialLocalPos;
                yield return new WaitForSeconds(_openDuration);

                // 2. Estado Advertencia (Temblor anticipatorio)
                if (_spriteRenderer != null) _spriteRenderer.color = _warningColor;
                float elapsed = 0f;
                while (elapsed < _warningDuration)
                {
                    elapsed += Time.deltaTime;
                    float shake = Mathf.Sin(elapsed * 45f) * 0.08f;
                    transform.localPosition = _initialLocalPos + new Vector3(shake, 0f, 0f);
                    yield return null;
                }

                // 3. Estado Mordisco (Cerrada y Letal)
                _isSnapping = true;
                transform.localPosition = _initialLocalPos;
                if (_hazardCollider != null) _hazardCollider.enabled = true;
                if (_spriteRenderer != null) _spriteRenderer.color = _biteColor;
                yield return new WaitForSeconds(_biteDuration);

                // 4. Reapertura rápida
                if (_hazardCollider != null) _hazardCollider.enabled = false;
                _isSnapping = false;
                yield return new WaitForSeconds(0.2f);
            }
        }

        public void OnHazardTouch()
        {
            Debug.Log($"[CarnivorousPlant2D] Alma fue atrapada por {gameObject.name}.");
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            transform.localPosition = _initialLocalPos;
            if (_hazardCollider != null) _hazardCollider.enabled = false;
        }
    }
}
