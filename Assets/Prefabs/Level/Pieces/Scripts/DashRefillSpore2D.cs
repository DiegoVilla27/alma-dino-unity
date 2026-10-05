using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Floating spore that restores Alma's air Dash (and optionally her double jump) on touch, so Dashes
    // can be chained in the air. It's only used up if it actually restored something; then it pops,
    // hides for `Respawn Time` and grows back. Bobs and pulses while available. Not a platform.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class DashRefillSpore2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(0.7f, 0.7f);
        [SerializeField, Min(0.1f)] private float _respawnTime = 2.5f;
        [SerializeField, Min(0f)] private float _floatFrequency = 3f;
        [SerializeField, Min(0f)] private float _floatAmplitude = 0.15f;
        [SerializeField] private bool _refillDoubleJump = true;
        [SerializeField, Min(0.05f)] private float _regrowTime = 0.2f;
        [SerializeField] private Color _burstColor = new Color(1f, 0.95f, 0.5f, 0.9f);
        [SerializeField] private string _label = "Espora Dash";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private AlmaMotor2D _player;
        private ParticleSystem _burst;
        private Vector3 _home;
        private bool _available = true;
        private float _usedAt;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = true;
            _home = transform.position;
            _player = FindAnyObjectByType<AlmaMotor2D>();

            _burst = HazardFx.CreateParticles("Burst", transform, _renderer.sharedMaterial, HazardFx.Puff(), 12,
                _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            var main = _burst.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startColor = _burstColor;
            var shape = _burst.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.2f);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += MakeAvailable;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= MakeAvailable;
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_burst);

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        private void OnTriggerEnter2D(Collider2D other) => TryUse(other);

        private void OnTriggerStay2D(Collider2D other) => TryUse(other);

        private void TryUse(Collider2D other)
        {
            if (!_available || !LevelPieceUtility.IsAlma(other, out var alma) || alma.IsDead) return;
            if (!alma.RefillAirAbilities(true, _refillDoubleJump)) return;
            _available = false;
            _usedAt = Time.time;
            _burst.Emit(10);
            _renderer.enabled = false;
            _collider.enabled = false;
        }

        private void Update()
        {
            if (_available)
            {
                // Bob up and down and pulse gently.
                transform.position = _home + Vector3.up * (Mathf.Sin(Time.time * _floatFrequency) * _floatAmplitude);
                float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 6f);
                transform.localScale = new Vector3(pulse, pulse, 1f);
                return;
            }

            float since = Time.time - _usedAt;
            if (since < _respawnTime) return;
            // Grow back from nothing.
            _renderer.enabled = true;
            float t = Mathf.Clamp01((since - _respawnTime) / _regrowTime);
            transform.localScale = Vector3.one * t;
            if (t >= 1f) MakeAvailable();
        }

        private void MakeAvailable()
        {
            _available = true;
            _renderer.enabled = true;
            _collider.enabled = true;
            transform.localScale = Vector3.one;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
    }
}
