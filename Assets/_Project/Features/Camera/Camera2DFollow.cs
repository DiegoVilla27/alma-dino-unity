using UnityEngine;

namespace AlmaDino.Features.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class Camera2DFollow : MonoBehaviour
    {
        public const float StandardOrthographicSize = 6f;

        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1.5f);

        [Header("Follow Settings")]
        [SerializeField] private float _smoothTime = 0.12f;
        [SerializeField] private Vector2 _deadZone = new Vector2(0.5f, 0.5f);

        [Header("Dynamic Look-Ahead")]
        [Tooltip("Distancia horizontal que la cámara se adelanta en la dirección de avance")]
        [SerializeField] private float _lookAheadDistance = 1.25f;
        [Tooltip("Velocidad de transición del anticipo de cámara")]
        [SerializeField] private float _lookAheadSpeed = 4.0f;

        [Header("Bounds")]
        [SerializeField] private bool _useBounds = true;
        [SerializeField] private Vector2 _minBounds = new Vector2(-10f, -3f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(40f, 15f);

        private Vector3 _currentVelocity;
        private Rigidbody2D _targetRb;
        private float _currentLookAheadX;

        public void SetTarget(Transform target)
        {
            _target = target;
            _targetRb = _target != null ? _target.GetComponent<Rigidbody2D>() : null;
            _currentVelocity = Vector3.zero;
            _currentLookAheadX = 0f;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            _useBounds = true;
            _minBounds = min;
            _maxBounds = max;
        }

        private void Awake()
        {
            var camera = GetComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = StandardOrthographicSize;
        }

        private void Start()
        {
            if (_target != null && _targetRb == null)
            {
                _targetRb = _target.GetComponent<Rigidbody2D>();
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            if (_targetRb == null) _targetRb = _target.GetComponent<Rigidbody2D>();

            // Anticipación dinámica (Look-Ahead): se adelanta al correr, se centra al parar
            float targetLookAheadX = 0f;
            if (_targetRb != null)
            {
                float vx = _targetRb.linearVelocity.x;
                if (vx > 0.4f)
                {
                    targetLookAheadX = _lookAheadDistance;
                }
                else if (vx < -0.4f)
                {
                    targetLookAheadX = -_lookAheadDistance;
                }
            }

            _currentLookAheadX = Mathf.MoveTowards(_currentLookAheadX, targetLookAheadX, _lookAheadSpeed * Time.deltaTime);

            Vector3 desiredPosition = _target.position + (Vector3)_offset;
            desiredPosition.x += _currentLookAheadX;

            // Deadzone para evitar micro-temblores al reposar
            Vector3 diff = desiredPosition - transform.position;
            if (Mathf.Abs(diff.x) < _deadZone.x) desiredPosition.x = transform.position.x;
            if (Mathf.Abs(diff.y) < _deadZone.y) desiredPosition.y = transform.position.y;

            desiredPosition.z = transform.position.z;

            // Smooth follow amortiguado
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, _smoothTime);

            // Clamp a los límites del nivel
            if (_useBounds)
            {
                smoothed.x = Mathf.Clamp(smoothed.x, _minBounds.x, _maxBounds.x);
                smoothed.y = Mathf.Clamp(smoothed.y, _minBounds.y, _maxBounds.y);
            }

            transform.position = smoothed;
        }

        private void OnDrawGizmosSelected()
        {
            if (_useBounds)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = new Vector3((_minBounds.x + _maxBounds.x) * 0.5f, (_minBounds.y + _maxBounds.y) * 0.5f, 0f);
                Vector3 size = new Vector3(_maxBounds.x - _minBounds.x, _maxBounds.y - _minBounds.y, 1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
