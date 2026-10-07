using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Stone block resting on one end of a seesaw. While resting it rides on the plank (kinematic).
    // When the other end is slammed it becomes a dynamic body launched straight up (14 u/s); while it
    // is launched and still rising it can activate rune switches. After 4.7 s (or when Alma respawns)
    // it reappears at its resting spot. Its seesaw and side are taken from where it is placed.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class CatapultWeight2D : MonoBehaviour
    {
        [SerializeField] private SeesawPlatform2D _seesaw;
        [SerializeField] private Vector2 _size = new Vector2(0.9f, 0.9f);
        [SerializeField, Min(0.5f)] private float _resetTime = 4.7f;
        [SerializeField, Min(0.05f)] private float _reappearTime = 0.25f;
        [SerializeField] private string _label = "Contrapeso";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private Rigidbody2D _body;
        private AlmaMotor2D _player;
        private float _restLocalX;
        private bool _launched;
        private float _launchedAt;
        private float _reappearStartedAt = float.NegativeInfinity;
        private Color _baseColor;

        public int Side { get; private set; }
        public bool IsLaunched => _launched;
        public bool IsRising => _launched && _body.linearVelocity.y > 0.1f;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _baseColor = _renderer.color;
            if (!TryGetComponent(out _body)) _body = gameObject.AddComponent<Rigidbody2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _body.gravityScale = 2.2f;
            _player = FindAnyObjectByType<AlmaMotor2D>();
            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        // After every Awake: find the seesaw (nearest if not set) and remember where on it we rest.
        private void Start()
        {
            if (_seesaw == null) _seesaw = FindNearestSeesaw();
            if (_seesaw == null)
            {
                Debug.LogWarning("Counterweight has no seesaw to rest on.", this);
                return;
            }
            Vector3 local = _seesaw.transform.InverseTransformPoint(transform.position);
            _restLocalX = Mathf.Clamp(local.x, -_seesaw.HalfLength + _size.x * 0.5f, _seesaw.HalfLength - _size.x * 0.5f);
            Side = _restLocalX >= 0f ? 1 : -1;
            _seesaw.Register(this);
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += ReturnToRest;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= ReturnToRest;
            if (_seesaw != null) _seesaw.Unregister(this);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        public void Launch(float speed)
        {
            if (_launched || _seesaw == null) return;
            _launched = true;
            _launchedAt = Time.time;
            _body.bodyType = RigidbodyType2D.Dynamic;
            _body.linearVelocity = new Vector2(0f, speed);
            _body.angularVelocity = Random.Range(-90f, 90f);
        }

        private void FixedUpdate()
        {
            if (_seesaw == null) return;
            if (_launched)
            {
                if (Time.time - _launchedAt >= _resetTime) ReturnToRest();
                return;
            }
            // Ride on the plank.
            Vector3 rest = _seesaw.transform.TransformPoint(new Vector3(_restLocalX, _seesaw.TopOffset + _size.y * 0.5f, 0f));
            _body.MovePosition(rest);
            _body.MoveRotation(_seesaw.transform.eulerAngles.z);
        }

        private void Update()
        {
            float t = (Time.time - _reappearStartedAt) / _reappearTime;
            if (t < 0f || t > 1f) return;
            Color color = _baseColor;
            color.a *= Mathf.Clamp01(t);
            _renderer.color = color;
        }

        private void ReturnToRest()
        {
            if (_seesaw == null) return;
            _launched = false;
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            Vector3 rest = _seesaw.transform.TransformPoint(new Vector3(_restLocalX, _seesaw.TopOffset + _size.y * 0.5f, 0f));
            _body.position = rest;
            transform.SetPositionAndRotation(rest, _seesaw.transform.rotation);
            _reappearStartedAt = Time.time;
        }

        private SeesawPlatform2D FindNearestSeesaw()
        {
            SeesawPlatform2D nearest = null;
            float best = float.MaxValue;
            foreach (var seesaw in FindObjectsByType<SeesawPlatform2D>(FindObjectsSortMode.None))
            {
                float distance = Vector2.Distance(seesaw.transform.position, transform.position);
                if (distance < best)
                {
                    best = distance;
                    nearest = seesaw;
                }
            }
            return nearest;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
    }
}
