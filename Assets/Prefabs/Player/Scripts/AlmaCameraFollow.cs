using UnityEngine;

namespace AlmaGame.Player
{
    [RequireComponent(typeof(Camera))]
    public sealed class AlmaCameraFollow : MonoBehaviour
    {
        public AlmaMotor2D Target;
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);
        [SerializeField, Min(0.01f)] private float _smoothTime = 0.15f;
        [SerializeField, Min(0f)] private float _lookAhead = 1.25f;
        private Vector3 _velocity;
        private Vector3 _followPosition;
        private float _depth;
        private float _shakeAmplitude;
        private float _shakeDuration;
        private float _shakeEndsAt;

        private void Awake()
        {
            var cameraComponent = GetComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 8f;
            _depth = transform.position.z;
            _followPosition = transform.position;
        }

        // Short shake that fades linearly to zero; a new call replaces the current one.
        public void Shake(float amplitude, float duration)
        {
            _shakeAmplitude = amplitude;
            _shakeDuration = duration;
            _shakeEndsAt = Time.time + duration;
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            float ahead = Mathf.Clamp(Target.Velocity.x / Target.Settings.MoveSpeed, -1f, 1f) * _lookAhead;
            Vector3 destination = Target.transform.position + (Vector3)_offset + Vector3.right * ahead;
            destination.z = _depth;
            // Follow is smoothed on its own position so the shake offset never feeds back into it.
            _followPosition = Vector3.SmoothDamp(_followPosition, destination, ref _velocity, _smoothTime);
            transform.position = _followPosition + ShakeOffset();
        }

        private Vector3 ShakeOffset()
        {
            float remaining = _shakeEndsAt - Time.time;
            if (remaining <= 0f || _shakeDuration <= 0f) return Vector3.zero;
            return (Vector3)(Random.insideUnitCircle * (_shakeAmplitude * remaining / _shakeDuration));
        }
    }
}
