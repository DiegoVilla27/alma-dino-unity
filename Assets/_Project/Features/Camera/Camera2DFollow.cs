using UnityEngine;

namespace AlmaDino.Features.Camera
{
    public class Camera2DFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1.5f);

        [Header("Follow Settings")]
        [SerializeField] private float _smoothTime = 0.2f;
        [SerializeField] private Vector2 _deadZone = new Vector2(0.5f, 0.5f);

        [Header("Bounds")]
        [SerializeField] private bool _useBounds = true;
        [SerializeField] private Vector2 _minBounds = new Vector2(-10f, -3f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(40f, 15f);

        private Vector3 _currentVelocity;

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + (Vector3)_offset;

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
