using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Espora luminosa flotante que, al ser tocada en el aire por Alma,
    /// restablece inmediatamente su Dash Aéreo permitiendo encadenar maniobras acrobáticas.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DashRefillPickup2D : MonoBehaviour
    {
        [Header("Respawn & Timing")]
        [Tooltip("Tiempo en segundos para que la espora reaparezca")]
        [SerializeField] private float _respawnDelay = 2.5f;

        [Header("Floating Animation")]
        [SerializeField] private float _bobFrequency = 3f;
        [SerializeField] private float _bobAmplitude = 0.15f;

        [Header("Visual Elements")]
        [SerializeField] private SpriteRenderer _sporeRenderer;
        [SerializeField] private Color _activeColor = new Color(0f, 1f, 0.85f, 1f);
        [SerializeField] private Color _consumedColor = new Color(0f, 1f, 0.85f, 0.15f);

        private Vector3 _startPosition;
        private Collider2D _collider;
        private bool _isAvailable = true;

        public bool IsAvailable => _isAvailable;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            _startPosition = transform.position;

            if (_sporeRenderer == null)
                _sporeRenderer = GetComponentInChildren<SpriteRenderer>();

            UpdateVisual(true);
        }

        private void Update()
        {
            if (_isAvailable)
            {
                // Efecto flotante suave
                float newY = _startPosition.y + Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
                transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isAvailable) return;

            var dashRefillable = other.GetComponentInParent<IDashRefillable2D>();
            if (dashRefillable != null)
            {
                Consume(dashRefillable);
            }
        }

        public void Consume(IDashRefillable2D dashRefillable)
        {
            _isAvailable = false;
            _collider.enabled = false;
            UpdateVisual(false);

            dashRefillable.RefreshAirDash();
            Debug.Log($"<color=#00FFFF><b>[DashRefillPickup2D]</b> ¡Dash Aéreo recargado por espora en {transform.position}!</color>");

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);

            _isAvailable = true;
            _collider.enabled = true;
            UpdateVisual(true);
        }

        private void UpdateVisual(bool available)
        {
            if (_sporeRenderer != null)
            {
                _sporeRenderer.color = available ? _activeColor : _consumedColor;
            }
        }
    }
}
