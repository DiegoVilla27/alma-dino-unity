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
        private float _depth;

        private void Awake()
        {
            var cameraComponent = GetComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 8f;
            _depth = transform.position.z;
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            float ahead = Mathf.Clamp(Target.Velocity.x / Target.Settings.MoveSpeed, -1f, 1f) * _lookAhead;
            Vector3 destination = Target.transform.position + (Vector3)_offset + Vector3.right * ahead;
            destination.z = _depth;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, _smoothTime);
        }
    }
}
