using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Interruptor rúnico activable por contacto o pisotón de Alma,
    /// que envía la señal de apertura a una o más puertas rúnicas temporizadas.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RuneSwitch2D : MonoBehaviour
    {
        [Header("Target Gate")]
        [SerializeField] private TimedRuneGate2D _targetGate;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _crystalRenderer;
        [SerializeField] private Color _idleColor = new Color(0.2f, 0.5f, 0.9f, 1f);
        [SerializeField] private Color _activeColor = new Color(0.1f, 1f, 0.8f, 1f);

        private bool _isPressed;
        private Coroutine _resetRoutine;

        private void Awake()
        {
            if (_crystalRenderer == null)
                _crystalRenderer = GetComponentInChildren<SpriteRenderer>();

            SetVisual(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isPressed) return;

            if (other.GetComponentInParent<IPlayerRespawnable>() != null || other.CompareTag("Player"))
            {
                ActivateSwitch();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isPressed) return;

            if (collision.gameObject.GetComponentInParent<IPlayerRespawnable>() != null || collision.gameObject.CompareTag("Player"))
            {
                ActivateSwitch();
            }
        }

        public void ActivateSwitch()
        {
            _isPressed = true;
            SetVisual(true);

            if (_targetGate != null)
            {
                _targetGate.OpenGate();
                if (_resetRoutine != null) StopCoroutine(_resetRoutine);
                _resetRoutine = StartCoroutine(ResetSwitchRoutine(_targetGate.OpenDuration));
            }
            else
            {
                if (_resetRoutine != null) StopCoroutine(_resetRoutine);
                _resetRoutine = StartCoroutine(ResetSwitchRoutine(4f));
            }
        }

        private IEnumerator ResetSwitchRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isPressed = false;
            SetVisual(false);
            _resetRoutine = null;
        }

        private void SetVisual(bool active)
        {
            if (_crystalRenderer != null)
            {
                _crystalRenderer.color = active ? _activeColor : _idleColor;
            }
        }

        public void SetTargetGate(TimedRuneGate2D gate)
        {
            _targetGate = gate;
        }
    }
}
