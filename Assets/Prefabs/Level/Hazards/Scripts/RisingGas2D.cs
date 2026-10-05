using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Rising toxic gas: a lethal volume (HazardZone2D) whose bottom stays fixed and whose top rises.
    // Idle → Warning (puffs churn, body pulses) → Rising at constant speed → Full at its max height.
    // Starts on level start or when Alma passes an X position; resets to its initial height when Alma
    // respawns. Visual: translucent tinted body plus soft puffs bubbling along the top edge.
    [DisallowMultipleComponent, RequireComponent(typeof(HazardZone2D), typeof(SpriteRenderer))]
    public sealed class RisingGas2D : MonoBehaviour
    {
        private enum Activation { OnStart, WhenAlmaPassesX }

        [Header("Activation")]
        [SerializeField] private Activation _activation = Activation.WhenAlmaPassesX;
        [SerializeField] private float _activationOffsetX = 0f;
        [SerializeField] private bool _resetOnRespawn = true;

        [Header("Rise")]
        [SerializeField, Min(0f)] private float _warningTime = 3f;
        [SerializeField, Min(0.05f)] private float _riseSpeed = 0.9f;
        [SerializeField, Min(0.5f)] private float _maxHeight = 10f;

        [Header("Look")]
        [SerializeField] private Color _puffColorA = new Color(0.55f, 0.95f, 0.35f, 0.75f);
        [SerializeField] private Color _puffColorB = new Color(0.35f, 0.75f, 0.3f, 0.75f);
        [SerializeField, Min(0f)] private float _puffsPerUnit = 3f;

        private enum State { Idle, Warning, Rising, Full }

        private HazardZone2D _zone;
        private SpriteRenderer _renderer;
        private AlmaMotor2D _player;
        private ParticleSystem _topPuffs;
        private State _state = State.Idle;
        private float _stateStartedAt;
        private float _bottomY;
        private float _initialHeight;
        private float _height;
        private float _width;
        private Color _baseColor;

        public bool IsRising => _state == State.Rising;

        private void Awake()
        {
            _zone = GetComponent<HazardZone2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _player = FindAnyObjectByType<AlmaMotor2D>();
            _width = _zone.Size.x;
            _initialHeight = _zone.Size.y;
            _bottomY = transform.position.y - _initialHeight * 0.5f;
            _baseColor = _renderer.color;

            _topPuffs = HazardFx.CreateParticles("GasTopPuffs", transform, _renderer.sharedMaterial, HazardFx.Puff(),
                Mathf.CeilToInt(_width * _puffsPerUnit * 1.6f) + 4, _renderer.sortingLayerID, _renderer.sortingOrder + 1, false);
            var main = _topPuffs.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 1.6f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startColor = new ParticleSystem.MinMaxGradient(_puffColorA, _puffColorB);
            var shape = _topPuffs.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_width, 0.15f, 0f);
            var velocity = _topPuffs.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            HazardFx.SetSizeOverLifetime(_topPuffs, 0.7f, 1.2f);
            var emission = _topPuffs.emission;
            emission.enabled = true;

            ResetGas();
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += OnAlmaRespawned;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= OnAlmaRespawned;
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_topPuffs);

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Idle:
                    if (_activation == Activation.OnStart || AlmaPassedActivation()) Enter(State.Warning);
                    break;

                case State.Warning:
                    // Body pulses and the top churns faster before it starts to rise.
                    float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * Mathf.PI * 4f);
                    Color warning = _baseColor;
                    warning.a = Mathf.Lerp(_baseColor.a, Mathf.Min(1f, _baseColor.a + 0.25f), pulse);
                    _renderer.color = warning;
                    SetPuffRate(2.5f);
                    if (elapsed >= _warningTime)
                    {
                        _renderer.color = _baseColor;
                        SetPuffRate(1.5f);
                        Enter(State.Rising);
                    }
                    break;

                case State.Rising:
                    SetHeight(Mathf.Min(_maxHeight, _height + _riseSpeed * Time.deltaTime));
                    if (_height >= _maxHeight)
                    {
                        SetPuffRate(1f);
                        Enter(State.Full);
                    }
                    break;
            }
        }

        public void Activate()
        {
            if (_state == State.Idle) Enter(State.Warning);
        }

        // Stops wherever it is (e.g. an encounter or rescue ends).
        public void Stop()
        {
            _renderer.color = _baseColor;
            SetPuffRate(1f);
            Enter(State.Full);
        }

        private bool AlmaPassedActivation()
        {
            return _player != null && !_player.IsDead
                && _player.transform.position.x >= transform.position.x + _activationOffsetX;
        }

        private void OnAlmaRespawned()
        {
            if (_resetOnRespawn) ResetGas();
        }

        private void ResetGas()
        {
            _renderer.color = _baseColor;
            SetHeight(_initialHeight);
            SetPuffRate(1f);
            _topPuffs.Clear();
            Enter(State.Idle);
        }

        // Keeps the bottom fixed: the object is re-centred and the zone resized.
        private void SetHeight(float height)
        {
            _height = height;
            Vector3 position = transform.position;
            position.y = _bottomY + height * 0.5f;
            transform.position = position;
            _zone.SetSize(new Vector2(_width, height));
            _topPuffs.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        }

        private void SetPuffRate(float multiplier)
        {
            var emission = _topPuffs.emission;
            emission.rateOverTime = _width * _puffsPerUnit * multiplier;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            // Maximum height and activation line.
            var zone = GetComponent<HazardZone2D>();
            if (zone == null) return;
            float width = zone.Size.x;
            float bottom = Application.isPlaying ? _bottomY : transform.position.y - zone.Size.y * 0.5f;
            Gizmos.color = new Color(0.4f, 1f, 0.3f, 0.8f);
            Gizmos.DrawWireCube(new Vector3(transform.position.x, bottom + _maxHeight * 0.5f, 0f), new Vector3(width, _maxHeight, 0f));
            if (_activation == Activation.WhenAlmaPassesX)
            {
                float x = transform.position.x + _activationOffsetX;
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
                Gizmos.DrawLine(new Vector3(x, bottom, 0f), new Vector3(x, bottom + _maxHeight, 0f));
            }
        }
    }
}
