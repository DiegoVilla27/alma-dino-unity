using System.Collections;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Puerta o barrera de cristal rúnico que se abre temporalmente durante una ventana de tiempo
    /// y luego se reactiva automáticamente.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TimedRuneGate2D : MonoBehaviour
    {
        [Header("Timing")]
        [Tooltip("Duración en segundos que permanece abierta la barrera")]
        [SerializeField] private float _openDuration = 4.0f;

        [Header("Visual Elements")]
        [SerializeField] private SpriteRenderer _gateRenderer;
        [SerializeField] private Color _closedColor = new Color(0.3f, 0.7f, 1f, 1f);
        [SerializeField] private Color _openColor = new Color(0.3f, 0.7f, 1f, 0.2f);

        private Collider2D _gateCollider;
        private Coroutine _openRoutine;
        private bool _isOpen;

        public bool IsOpen => _isOpen;
        public float OpenDuration => _openDuration;

        private void Awake()
        {
            _gateCollider = GetComponent<Collider2D>();
            if (_gateRenderer == null)
                _gateRenderer = GetComponentInChildren<SpriteRenderer>();

            UpdateVisuals(false);
        }

        public void OpenGate()
        {
            if (_openRoutine != null)
                StopCoroutine(_openRoutine);

            _openRoutine = StartCoroutine(OpenGateRoutine());
        }

        private IEnumerator OpenGateRoutine()
        {
            _isOpen = true;
            _gateCollider.enabled = false;
            UpdateVisuals(true);

            float remaining = _openDuration;
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                // Si faltan menos de 1.2 segundos, parpadeo de advertencia
                if (remaining <= 1.2f && _gateRenderer != null)
                {
                    float flash = Mathf.PingPong(remaining * 6f, 1f);
                    _gateRenderer.color = Color.Lerp(_openColor, _closedColor, flash * 0.7f);
                }
                yield return null;
            }

            _isOpen = false;
            _gateCollider.enabled = true;
            UpdateVisuals(false);
            _openRoutine = null;
        }

        private void UpdateVisuals(bool open)
        {
            if (_gateRenderer != null)
            {
                _gateRenderer.color = open ? _openColor : _closedColor;
            }
        }
    }
}
