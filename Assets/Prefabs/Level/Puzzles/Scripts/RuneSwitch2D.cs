using System;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Rune switch (usually on a ceiling). Only a launched counterweight that is still rising activates
    // it — Alma touching it does nothing. While active (4 s) it glows green and a bar under it shows the
    // time left; another hit restarts the time. Timed rune gates read IsActive. Resets when Alma respawns.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class RuneSwitch2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(1f, 1f);
        [SerializeField, Min(0.1f)] private float _signalTime = 4f;
        [SerializeField] private Color _activeColor = new Color(0.45f, 1f, 0.6f, 1f);
        [Tooltip("Sprite tint while waiting (dimmed rune) and while active (full brightness).")]
        [SerializeField] private Color _idleTint = new Color(0.55f, 0.6f, 0.7f, 1f);
        [SerializeField] private Color _activeTint = Color.white;
        [SerializeField] private string _label = "Runa";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _trigger;
        private SpriteRenderer _glow;
        private SpriteRenderer _bar;
        private ParticleSystem _burst;
        private AlmaMotor2D _player;
        private float _activeUntil = float.NegativeInfinity;

        public bool IsActive => Time.time < _activeUntil;
        public float Remaining => Mathf.Max(0f, _activeUntil - Time.time);
        public event Action Activated;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _trigger, _size);
            _trigger.isTrigger = true;
            _player = FindAnyObjectByType<AlmaMotor2D>();
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;

            _glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            _glow.transform.SetParent(transform, false);
            _glow.transform.localScale = Vector3.one * Mathf.Max(_size.x, _size.y) * 2.2f;
            _glow.sprite = HazardFx.Glow();
            _glow.color = new Color(_activeColor.r, _activeColor.g, _activeColor.b, 0.5f);
            _glow.sortingLayerID = layer;
            _glow.sortingOrder = order - 1;
            _glow.enabled = false;

            // Time-left bar under the rune.
            _bar = new GameObject("TimeBar").AddComponent<SpriteRenderer>();
            _bar.transform.SetParent(transform, false);
            _bar.transform.localPosition = new Vector3(0f, -_size.y * 0.5f - 0.2f, 0f);
            _bar.sprite = HazardFx.Pixel();
            _bar.color = _activeColor;
            _bar.sortingLayerID = layer;
            _bar.sortingOrder = order + 1;
            _bar.enabled = false;

            _burst = HazardFx.CreateParticles("Burst", transform, _renderer.sharedMaterial, HazardFx.Puff(), 16, layer, order + 2);
            var main = _burst.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = _activeColor;
            var shape = _burst.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.1f);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, -_size.y * 0.5f - 0.55f, 0f), _renderer);
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += Deactivate;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= Deactivate;
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_burst);

        private void OnValidate() => HazardFx.DeferInEditor(this, () =>
        {
            LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size);
            var trigger = GetComponent<BoxCollider2D>();
            if (trigger != null) trigger.isTrigger = true;
        });

        private void OnTriggerEnter2D(Collider2D other) => TryActivate(other);

        private void OnTriggerStay2D(Collider2D other) => TryActivate(other);

        // Only a counterweight that was launched and is still going up.
        private void TryActivate(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out CatapultWeight2D weight) || !weight.IsRising) return;
            bool wasActive = IsActive;
            _activeUntil = Time.time + _signalTime;
            if (!wasActive)
            {
                _burst.Emit(12);
                Activated?.Invoke();
            }
        }

        private void Update()
        {
            bool active = IsActive;
            _renderer.color = active ? _activeTint : _idleTint;
            _glow.enabled = active;
            _bar.enabled = active;
            if (!active) return;
            float left = Remaining / _signalTime;
            _bar.transform.localScale = new Vector3(_size.x * left, 0.12f, 1f);
            // Blink in the last second.
            if (Remaining < 1f) _glow.enabled = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f;
        }

        public void Deactivate() => _activeUntil = float.NegativeInfinity;

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, -_size.y * 0.5f - 0.8f, 0f), _label);
#endif
    }
}
