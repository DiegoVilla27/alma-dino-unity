using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CatapultWeight2D : MonoBehaviour
    {
        [SerializeField] private Transform _plank;
        [SerializeField] private Vector2 _restLocalPosition = new Vector2(2.3f, 0.65f);
        [SerializeField] private SeesawConfigSO _config;
        private Rigidbody2D _body;
        private float _launchedAt;
        public bool IsLaunched { get; private set; }

        private void Awake() => _body = GetComponent<Rigidbody2D>();

        private void FixedUpdate()
        {
            if (IsLaunched)
            {
                if (_config != null && Time.time - _launchedAt >= _config.WeightResetDelay) ResetWeight();
                return;
            }
            if (_plank == null) return;
            _body.MovePosition(_plank.TransformPoint(_restLocalPosition));
            _body.MoveRotation(_plank.eulerAngles.z);
        }

        public void Launch(float velocity)
        {
            if (IsLaunched) return;
            IsLaunched = true;
            _launchedAt = Time.time;
            _body.bodyType = RigidbodyType2D.Dynamic;
            _body.linearVelocity = new Vector2(0f, velocity);
            _body.angularVelocity = 75f;
        }

        public void ResetWeight()
        {
            IsLaunched = false;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.bodyType = RigidbodyType2D.Kinematic;
            if (_plank == null) return;
            _body.position = _plank.TransformPoint(_restLocalPosition);
            _body.rotation = _plank.eulerAngles.z;
        }
    }
}
