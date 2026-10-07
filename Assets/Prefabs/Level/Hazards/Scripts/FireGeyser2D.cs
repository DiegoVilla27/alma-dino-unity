using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Volcanic geyser: a vent that spits a fireball straight up on a cycle.
    // Rest → Warning (vent glows, smoke and sparks rise) → Erupt (fireball flies up and falls back
    // into the vent, splashing lava) → Rest. Only the fireball is lethal; the vent is safe to walk on.
    // Visuals are generated in code: a cartoon flame ball over a heat glow, a cooling trail, sparks and puffs.
    // In the editor, resizing the vent sprite with the Rect tool updates `Vent Size`, so Play keeps it.
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
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
        private ParticleSystem _sparks;
        private ParticleSystem _smoke;
        private ParticleSystem _splash;
        private State _state = State.Rest;
        private float _stateStartedAt;
        private float _gravity;
        private float _launchSpeed;

        private Vector3 VentTop => transform.position + new Vector3(0f, _ventSize.y * 0.5f, 0f);

        private void Awake()
        {
            if (!Application.isPlaying) return;
            _vent = GetComponent<SpriteRenderer>();
            _vent.size = _ventSize;
            _ventColor = _vent.color;
            int layer = _vent.sortingLayerID;
            int order = _vent.sortingOrder;
            Material material = _vent.sharedMaterial;

            // Fireball: a cartoon flame ball (outlined, hot core) over a soft heat glow. It spins and
            // flickers while it flies; a trail cools from yellow to red to smoke and sparks fly off it.
            _fireball = new GameObject("Fireball").transform;
            _fireball.SetParent(transform, false);
            Color glow = _fireballColor;
            glow.a = 0.45f;
            AddSprite(_fireball, "Glow", HazardFx.Glow(), glow, _fireballRadius * 3.4f, layer, order + 3);
            _core = AddSprite(_fireball, "Flame", HazardFx.Fireball(), Color.white, _fireballRadius * 2.8f, layer, order + 4);
            _fireball.gameObject.SetActive(false);

            _trail = HazardFx.CreateParticles("FireballTrail", _fireball, material, HazardFx.Puff(), 36, layer, order + 2);
            var trailMain = _trail.main;
            trailMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            trailMain.startSpeed = 0f;
            trailMain.startSize = new ParticleSystem.MinMaxCurve(_fireballRadius * 0.8f, _fireballRadius * 1.3f);
            trailMain.startColor = Color.white;
            var cooling = new Gradient();
            cooling.SetKeys(
                new[] { new GradientColorKey(_coreColor, 0f), new GradientColorKey(_fireballColor, 0.35f),
                    new GradientColorKey(new Color(0.75f, 0.15f, 0.05f), 0.7f), new GradientColorKey(new Color(0.25f, 0.18f, 0.16f), 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0.45f, 0.75f), new GradientAlphaKey(0f, 1f) });
            var trailColor = _trail.colorOverLifetime;
            trailColor.color = cooling;
            HazardFx.SetSizeOverLifetime(_trail, 1f, 0.25f);
            var trailEmission = _trail.emission;
            trailEmission.rateOverDistance = 10f;
            trailEmission.enabled = true;

            _sparks = HazardFx.CreateParticles("FireballSparks", _fireball, material, HazardFx.Puff(), 24, layer, order + 5);
            var sparksMain = _sparks.main;
            sparksMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            sparksMain.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
            sparksMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            sparksMain.startColor = new ParticleSystem.MinMaxGradient(_coreColor, _fireballColor);
            sparksMain.gravityModifier = 0.6f;
            var sparksShape = _sparks.shape;
            sparksShape.shapeType = ParticleSystemShapeType.Circle;
            sparksShape.radius = _fireballRadius * 0.8f;
            HazardFx.SetSizeOverLifetime(_sparks, 1f, 0.2f);
            var sparksEmission = _sparks.emission;
            sparksEmission.rateOverDistance = 5f;
            sparksEmission.enabled = true;

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
            HazardFx.DestroyMaterial(_sparks);
            HazardFx.DestroyMaterial(_smoke);
            HazardFx.DestroyMaterial(_splash);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SyncVentSizeInEditor();
                return;
            }
#endif
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
            if (!Application.isPlaying || _state != State.Erupt) return;
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
            _sparks.Clear();
            _fireball.gameObject.SetActive(true);
            Enter(State.Erupt);
        }

        // Rises and falls on a parabola; stretches along its speed, its flame tongues trail behind it
        // (turning over at the top of the arc) and it flickers.
        private void Fly(float t)
        {
            float y = _launchSpeed * t - 0.5f * _gravity * t * t;
            _fireball.position = VentTop + new Vector3(0f, Mathf.Max(0f, y), 0f);
            float speed = Mathf.Abs(_launchSpeed - _gravity * t) / _launchSpeed;
            float stretch = 1f + 0.35f * speed;
            _fireball.localScale = new Vector3(1f / Mathf.Sqrt(stretch), stretch, 1f);
            float size = _fireballRadius * 2.8f;
            _core.transform.localScale = new Vector3(size * (1f + 0.05f * Mathf.Sin(Time.time * 29f)),
                size * (1f + 0.09f * Mathf.Sin(Time.time * 37f)), 1f);
            float rising = Mathf.Clamp((_launchSpeed - _gravity * t) / (_launchSpeed * 0.3f), -1f, 1f);
            _core.transform.localRotation = Quaternion.Euler(0f, 0f, (1f - rising) * 90f);

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

        private static SpriteRenderer AddSprite(Transform parent, string name, Sprite sprite, Color color, float size, int layer, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.localScale = Vector3.one * size;
            renderer.sprite = sprite;
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

#if UNITY_EDITOR
        private Vector2? _appliedVentSize;

        // Edit mode only. Whichever side changed since the last sync wins: a vent drawn with the Rect tool
        // updates `Vent Size`; a new `Vent Size` typed in the Inspector resizes the vent sprite.
        private void SyncVentSizeInEditor()
        {
            var vent = GetComponent<SpriteRenderer>();
            if (vent == null || vent.drawMode == SpriteDrawMode.Simple) return;
            _appliedVentSize ??= vent.size;
            if (vent.size != _appliedVentSize.Value)
            {
                UnityEditor.Undo.RecordObject(this, "Resize Geyser Vent");
                _ventSize = vent.size;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
            else if (_ventSize == _appliedVentSize.Value) return;
            vent.size = _ventSize;
            _appliedVentSize = _ventSize;
        }
#endif

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
