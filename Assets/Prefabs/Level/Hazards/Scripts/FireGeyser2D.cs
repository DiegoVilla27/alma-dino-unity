using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Volcanic geyser: a vent that spits a fireball straight up on a cycle.
    // Rest → Warning (vent glows, smoke and sparks rise) → Erupt (fireball flies up and falls back
    // into the vent, splashing lava) → Rest. Only the fireball is lethal; the vent is safe to walk on.
    // Visuals are generated in code: glowing ball with a hot core, a flame trail and puffs.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class FireGeyser2D : MonoBehaviour
    {
        [Header("Cycle (seconds)")]
        [SerializeField, Min(0f)] private float _restTime = 2.2f;
        [SerializeField, Min(0f)] private float _warningTime = 0.8f;
        [SerializeField, Min(0.2f)] private float _eruptTime = 1.2f;
        [SerializeField, Min(0f)] private float _startDelay;

        [Header("Fireball")]
        [SerializeField, Min(0.5f)] private float _height = 4f;
        [SerializeField, Min(0.05f)] private float _fireballRadius = 0.35f;
        [SerializeField] private Color _fireballColor = new Color(1f, 0.45f, 0.1f, 1f);
        [SerializeField] private Color _coreColor = new Color(1f, 0.9f, 0.4f, 1f);

        [Header("Vent")]
        [SerializeField] private Vector2 _ventSize = new Vector2(1.2f, 0.4f);
        [SerializeField] private Color _warningGlow = new Color(1f, 0.55f, 0.2f, 1f);
        [SerializeField] private string _label = "Géiser";
        [SerializeField] private bool _showLabel = true;

        private enum State { Rest, Warning, Erupt }

        private readonly Collider2D[] _hits = new Collider2D[4];
        private SpriteRenderer _vent;
        private AlmaMotor2D _player;
        private Color _ventColor;
        private Transform _fireball;
        private SpriteRenderer _core;
        private ParticleSystem _trail;
        private ParticleSystem _smoke;
        private ParticleSystem _splash;
        private State _state = State.Rest;
        private float _stateStartedAt;
        private float _gravity;
        private float _launchSpeed;

        private Vector3 VentTop => transform.position + new Vector3(0f, _ventSize.y * 0.5f, 0f);

        private void Awake()
        {
            _vent = GetComponent<SpriteRenderer>();
            _vent.size = _ventSize;
            _ventColor = _vent.color;
            int layer = _vent.sortingLayerID;
            int order = _vent.sortingOrder;
            Material material = _vent.sharedMaterial;

            // Fireball: orange glow with a smaller hot core, lit from the trail behind it.
            _fireball = new GameObject("Fireball").transform;
            _fireball.SetParent(transform, false);
            AddGlow(_fireball, "Glow", _fireballColor, _fireballRadius * 2.6f, layer, order + 3);
            _core = AddGlow(_fireball, "Core", _coreColor, _fireballRadius * 1.3f, layer, order + 4);
            _fireball.gameObject.SetActive(false);

            _trail = HazardFx.CreateParticles("FireballTrail", _fireball, material, HazardFx.Puff(), 30, layer, order + 2);
            var trailMain = _trail.main;
            trailMain.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            trailMain.startSpeed = 0f;
            trailMain.startSize = new ParticleSystem.MinMaxCurve(_fireballRadius * 1.6f, _fireballRadius * 2.4f);
            trailMain.startColor = new ParticleSystem.MinMaxGradient(_coreColor, _fireballColor);
            HazardFx.SetSizeOverLifetime(_trail, 1f, 0.1f);
            var trailEmission = _trail.emission;
            trailEmission.rateOverDistance = 8f;
            trailEmission.enabled = true;

            // Warning: dark smoke and orange sparks rising from the vent.
            _smoke = HazardFx.CreateParticles("VentSmoke", transform, material, HazardFx.Puff(), 20, layer, order + 1);
            _smoke.transform.localPosition = new Vector3(0f, _ventSize.y * 0.5f, 0f);
            var smokeMain = _smoke.main;
            smokeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            smokeMain.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            smokeMain.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            smokeMain.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 0.25f, 0.25f, 0.7f), _warningGlow);
            var smokeShape = _smoke.shape;
            smokeShape.shapeType = ParticleSystemShapeType.Cone;
            smokeShape.angle = 12f;
            smokeShape.radius = _ventSize.x * 0.25f;
            smokeShape.rotation = new Vector3(-90f, 0f, 0f);
            HazardFx.SetSizeOverLifetime(_smoke, 0.8f, 1.5f);

            // Landing: lava droplets thrown up and sideways, falling back.
            _splash = HazardFx.CreateParticles("LavaSplash", transform, material, HazardFx.Puff(), 12, layer, order + 2);
            _splash.transform.localPosition = new Vector3(0f, _ventSize.y * 0.5f, 0f);
            var splashMain = _splash.main;
            splashMain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.55f);
            splashMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 3.5f);
            splashMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            splashMain.startColor = new ParticleSystem.MinMaxGradient(_fireballColor, _coreColor);
            splashMain.gravityModifier = 1.2f;
            var splashShape = _splash.shape;
            splashShape.shapeType = ParticleSystemShapeType.Circle;
            splashShape.radius = 0.1f;
            splashShape.arc = 180f;

            // Ballistic flight: up and back down in _eruptTime, peaking at _height above the vent.
            float half = _eruptTime * 0.5f;
            _gravity = 2f * _height / (half * half);
            _launchSpeed = _gravity * half;

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _ventSize.y * 0.5f + 0.3f, 0f), _vent);
            _stateStartedAt = Time.time - _restTime + _startDelay;
            _player = FindAnyObjectByType<AlmaMotor2D>();
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += ResetToStart;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= ResetToStart;
        }

        // Alma reappeared: no fireball in the air; the cycle restarts as on level start.
        private void ResetToStart()
        {
            _fireball.gameObject.SetActive(false);
            _vent.color = _ventColor;
            SetSmoke(0f);
            Enter(State.Rest);
            _stateStartedAt = Time.time - _restTime + _startDelay;
        }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_trail);
            HazardFx.DestroyMaterial(_smoke);
            HazardFx.DestroyMaterial(_splash);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Rest:
                    if (elapsed >= _restTime) Enter(State.Warning);
                    break;

                case State.Warning:
                    float pulse = Mathf.PingPong(elapsed * 6f, 1f);
                    _vent.color = Color.Lerp(_ventColor, _warningGlow, pulse);
                    SetSmoke(18f);
                    if (elapsed >= _warningTime)
                    {
                        _vent.color = _ventColor;
                        SetSmoke(0f);
                        Launch();
                    }
                    break;

                case State.Erupt:
                    Fly(elapsed);
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_state != State.Erupt) return;
            int count = Physics2D.OverlapCircle(_fireball.position, _fireballRadius, ContactFilter2D.noFilter, _hits);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = _hits[i].attachedRigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
            }
        }

        private void Launch()
        {
            _fireball.position = VentTop;
            _trail.Clear();
            _fireball.gameObject.SetActive(true);
            Enter(State.Erupt);
        }

        // Rises and falls on a parabola; stretches along its speed and pulses its core.
        private void Fly(float t)
        {
            float y = _launchSpeed * t - 0.5f * _gravity * t * t;
            _fireball.position = VentTop + new Vector3(0f, Mathf.Max(0f, y), 0f);
            float speed = Mathf.Abs(_launchSpeed - _gravity * t) / _launchSpeed;
            float stretch = 1f + 0.35f * speed;
            _fireball.localScale = new Vector3(1f / Mathf.Sqrt(stretch), stretch, 1f);
            _core.transform.localScale = Vector3.one * _fireballRadius * 1.3f * (1f + 0.15f * Mathf.Sin(Time.time * 30f));

            if (t < _eruptTime) return;
            _fireball.gameObject.SetActive(false);
            _splash.Emit(8);
            Enter(State.Rest);
        }

        private void SetSmoke(float rate)
        {
            var emission = _smoke.emission;
            emission.enabled = rate > 0f;
            emission.rateOverTime = rate;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private static SpriteRenderer AddGlow(Transform parent, string name, Color color, float size, int layer, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.localScale = Vector3.one * size;
            renderer.sprite = HazardFx.Glow();
            renderer.color = color;
            renderer.sortingLayerID = layer;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void OnValidate() => HazardFx.DeferInEditor(this, () =>
        {
            var vent = GetComponent<SpriteRenderer>();
            if (vent != null) vent.size = _ventSize;
        });

        private void OnDrawGizmosSelected()
        {
            // Fireball path and lethal size at the top.
            Vector3 top = transform.position + new Vector3(0f, _ventSize.y * 0.5f, 0f);
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
            Gizmos.DrawLine(top, top + Vector3.up * _height);
            Gizmos.DrawWireSphere(top + Vector3.up * _height, _fireballRadius);
            Gizmos.DrawWireSphere(top, _fireballRadius);
        }
    }
}
