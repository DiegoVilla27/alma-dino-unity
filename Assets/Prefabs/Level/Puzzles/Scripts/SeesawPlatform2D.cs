using System;
using System.Collections.Generic;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Stone seesaw on a central pivot (Level 2-2). Walking on it tilts it gently towards Alma's side.
    // A Pisotón on one end slams that end down (up to 18°): counterweights resting on the other end are
    // launched (14 u/s) and, for 2.8 s, if Alma runs onto the raised end she is catapulted up (15 u/s,
    // double jump restored). Afterwards it levels out again. Rotates through a kinematic Rigidbody2D
    // so whatever stands on it moves with it.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class SeesawPlatform2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(6f, 0.4f);
        [SerializeField, Range(1f, 45f)] private float _maxAngle = 18f;
        [SerializeField, Range(0f, 1f)] private float _walkTiltFraction = 0.4f;
        [SerializeField, Min(1f)] private float _walkTiltSpeed = 25f;
        [SerializeField, Min(0.02f)] private float _slamTime = 0.12f;
        [SerializeField, Min(0.1f)] private float _launchWindow = 2.8f;
        [SerializeField, Min(1f)] private float _almaLaunchSpeed = 15f;
        [SerializeField, Min(1f)] private float _weightLaunchSpeed = 14f;
        [SerializeField, Min(1f)] private float _returnSpeed = 20f;
        [Header("Art")]
        [Tooltip("Fixed fulcrum drawn under the plank (it doesn't rotate). Empty = a small grey block.")]
        [SerializeField] private Sprite _pivotSprite;
        [Tooltip("Distance from the plank's centre down to the pivot sprite's centre (units).")]
        [SerializeField] private float _pivotDrop = 0.5f;
        [SerializeField] private string _label = "Balancín";
        [SerializeField] private bool _showLabel = true;

        private readonly List<CatapultWeight2D> _weights = new List<CatapultWeight2D>();
        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private Rigidbody2D _body;
        private AlmaMotor2D _player;
        private Collider2D _playerCollider;
        private AlmaCameraFollow _camera;
        private SpriteRenderer _pivot;
        private ParticleSystem _dust;
        private float _angle;
        private int _slamSide;
        private float _slamStartedAt = float.NegativeInfinity;
        private bool _almaLaunched;

        public float HalfLength => _size.x * 0.5f;
        public float TopOffset => _size.y * 0.5f;
        public float WeightLaunchSpeed => _weightLaunchSpeed;
        public bool InLaunchWindow => Time.time < _slamStartedAt + _slamTime + _launchWindow;
        public event Action<int> Slammed;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            if (!TryGetComponent(out _body)) _body = gameObject.AddComponent<Rigidbody2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _angle = transform.eulerAngles.z > 180f ? transform.eulerAngles.z - 360f : transform.eulerAngles.z;

            _player = FindAnyObjectByType<AlmaMotor2D>();
            if (_player != null) _playerCollider = _player.GetComponent<Collider2D>();

            // Fixed pivot under the plank (doesn't rotate with it).
            _pivot = new GameObject("SeesawPivot").AddComponent<SpriteRenderer>();
            _pivot.transform.SetParent(transform.parent, false);
            _pivot.sortingLayerID = _renderer.sortingLayerID;
            if (_pivotSprite != null)
            {
                // Art fulcrum: its axle disc sits on the plank's centre, drawn in front like a real pin.
                _pivot.transform.position = transform.position + Vector3.down * _pivotDrop;
                _pivot.sprite = _pivotSprite;
                _pivot.sortingOrder = _renderer.sortingOrder + 1;
            }
            else
            {
                _pivot.transform.position = transform.position + Vector3.down * (_size.y * 0.5f + 0.3f);
                _pivot.sprite = _renderer.sprite;
                _pivot.drawMode = SpriteDrawMode.Tiled;
                _pivot.size = new Vector2(0.5f, 0.6f);
                _pivot.color = _renderer.color * 0.75f;
                _pivot.sortingOrder = _renderer.sortingOrder - 1;
            }

            _dust = HazardFx.CreateParticles("SlamDust", transform, _renderer.sharedMaterial, HazardFx.Puff(), 16,
                _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            var main = _dust.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startColor = new Color(0.7f, 0.68f, 0.65f, 0.75f);
            var shape = _dust.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;
            shape.arc = 180f;
            HazardFx.SetSizeOverLifetime(_dust, 0.8f, 1.5f);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void Start() => _camera = FindAnyObjectByType<AlmaCameraFollow>();

        private void OnEnable()
        {
            if (_player != null) _player.GroundPoundLanded += OnGroundPound;
        }

        private void OnDisable()
        {
            if (_player != null) _player.GroundPoundLanded -= OnGroundPound;
        }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_dust);
            if (_pivot != null) Destroy(_pivot.gameObject);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        public void Register(CatapultWeight2D weight)
        {
            if (!_weights.Contains(weight)) _weights.Add(weight);
        }

        public void Unregister(CatapultWeight2D weight) => _weights.Remove(weight);

        // Pisotón landing on one half of the plank (not right over the pivot).
        private void OnGroundPound()
        {
            if (!AlmaOnPlank(out float localX) || Mathf.Abs(localX) < HalfLength * 0.15f) return;
            Slam(localX > 0f ? 1 : -1);
        }

        public void Slam(int sideDown)
        {
            _slamSide = sideDown;
            _slamStartedAt = Time.time;
            _almaLaunched = false;
            _dust.transform.position = transform.TransformPoint(new Vector3(sideDown * HalfLength, -_size.y * 0.5f, 0f));
            _dust.Emit(12);
            if (_camera != null) _camera.Shake(0.1f, 0.15f);
            foreach (var weight in _weights)
                if (weight.Side == -sideDown) weight.Launch(_weightLaunchSpeed);
            Slammed?.Invoke(sideDown);
        }

        private void FixedUpdate()
        {
            bool onPlank = AlmaOnPlank(out float localX);
            float target;
            float speed;
            if (InLaunchWindow)
            {
                target = -_slamSide * _maxAngle;
                speed = _maxAngle / _slamTime;
                // Catapult: Alma reaches the raised end during the window.
                if (!_almaLaunched && onPlank && Mathf.Sign(localX) == -_slamSide && Mathf.Abs(localX) > HalfLength * 0.4f)
                {
                    _almaLaunched = true;
                    _player.Bounce(_almaLaunchSpeed, 1f, true);
                }
            }
            else if (onPlank)
            {
                target = -Mathf.Sign(localX) * _maxAngle * _walkTiltFraction * Mathf.Clamp01(Mathf.Abs(localX) / HalfLength);
                speed = _walkTiltSpeed;
            }
            else
            {
                target = 0f;
                speed = _returnSpeed;
            }
            _angle = Mathf.MoveTowards(_angle, target, speed * Time.fixedDeltaTime);
            _body.MoveRotation(_angle);
        }

        // Alma standing on the plank; localX is her feet's position along it (0 = pivot).
        private bool AlmaOnPlank(out float localX)
        {
            localX = 0f;
            if (_player == null || _playerCollider == null || _player.IsDead || !_player.IsGrounded) return false;
            Bounds bounds = _playerCollider.bounds;
            Vector3 local = transform.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0f));
            localX = local.x;
            return Mathf.Abs(local.x) <= HalfLength + 0.1f && local.y >= TopOffset - 0.2f && local.y <= TopOffset + 0.35f;
        }

        private void OnDrawGizmosSelected()
        {
            // Tilt limits.
            Vector3 center = transform.position;
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            foreach (float sign in new[] { -1f, 1f })
            {
                Vector3 end = Quaternion.Euler(0f, 0f, sign * _maxAngle) * Vector3.right * HalfLength;
                Gizmos.DrawLine(center - end, center + end);
            }
#if UNITY_EDITOR
            UnityEditor.Handles.Label(center + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
