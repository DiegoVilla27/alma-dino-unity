using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Balancín o balancín-catapulta de piedra que oscila sobre un punto de apoyo central.
    /// Responde a la masa y posición de aterrizaje de Alma, limitando su inclinación máxima
    /// y regresando a la horizontal cuando no hay peso sobre él.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SeesawPlatform2D : MonoBehaviour
    {
        [Header("Tilt Settings")]
        [Tooltip("Ángulo máximo de inclinación en grados hacia cada lado")]
        [SerializeField] private float _maxAngle = 25f;
        [Tooltip("Sensibilidad de inclinación según la distancia al fulcro")]
        [SerializeField] private float _tiltSensitivity = 18f;
        [Tooltip("Velocidad de retorno a la horizontal cuando queda libre")]
        [SerializeField] private float _returnSpeed = 4f;

        [Header("Pivot Reference")]
        [SerializeField] private Transform _plankTransform;

        private float _currentAngle;
        private float _targetAngle;
        private bool _isOccupied;
        private float _lastOccupiedTime;

        private void Awake()
        {
            if (_plankTransform == null)
                _plankTransform = transform;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Determinar si el objeto está encima de la tabla
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (contact.point.y >= _plankTransform.position.y - 0.2f)
                {
                    _isOccupied = true;
                    _lastOccupiedTime = Time.time;

                    // Calcular desviación horizontal respecto al centro del fulcro
                    float deltaX = contact.point.x - _plankTransform.position.x;
                    // Si deltaX > 0 (derecha), inclinación hacia la derecha (ángulo negativo en Z)
                    _targetAngle = Mathf.Clamp(-deltaX * _tiltSensitivity, -_maxAngle, _maxAngle);
                    return;
                }
            }
        }

        private void Update()
        {
            if (Time.time - _lastOccupiedTime > 0.1f)
            {
                _isOccupied = false;
                _targetAngle = 0f;
            }

            float speed = _isOccupied ? _tiltSensitivity * 1.5f : _returnSpeed;
            _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, speed * Time.deltaTime * 20f);
            _plankTransform.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
        }

        public float CurrentAngle => _currentAngle;
        public bool IsOccupied => _isOccupied;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
