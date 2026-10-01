using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class TimedRuneGate2D : MonoBehaviour
    {
        [SerializeField] private RuneSwitch2D[] _switches;
        [SerializeField] private SpriteRenderer _visual;
        [SerializeField] private Transform _timerBar;
        [SerializeField] private SeesawConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        private IPlayerRespawnable _player;
        private BoxCollider2D _collider;
        private Vector2 _center;
        private Vector2 _size;
        private readonly Collider2D[] _overlaps = new Collider2D[8];
        public bool IsOpen => _collider != null && !_collider.enabled;
        public float OpenDuration => _config != null ? _config.GateOpenDuration : 4f;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _center = _collider.bounds.center;
            _size = _collider.bounds.size;
        }

        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetGate;
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetGate;
        }

        private void FixedUpdate()
        {
            if (_config == null || _switches == null) return;
            float remaining = _config.GateOpenDuration;
            foreach (var rune in _switches) remaining = Mathf.Min(remaining, rune.RemainingTime);
            bool open = _switches.Length > 0 && remaining > 0f;
            if (!open && IsOpen) open = PlayerInsideGate();
            _collider.enabled = !open;
            if (_visual != null)
                _visual.color = open ? new Color(0.3f, 0.9f, 0.55f, 0.15f) : new Color(0.82f, 0.54f, 0.18f);
            if (_timerBar != null) _timerBar.localScale = new Vector3(1f, Mathf.Clamp01(remaining / _config.GateOpenDuration), 1f);
        }

        private bool PlayerInsideGate()
        {
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.OverlapBox(_center, _size, 0f, filter, _overlaps);
            for (int i = 0; i < count; i++)
                if (_overlaps[i].GetComponent<IPlayerRespawnable>() != null) return true;
            return false;
        }

        private void ResetGate(Vector2 position)
        {
            foreach (var rune in _switches) rune.ResetSignal();
            _collider.enabled = true;
        }
    }
}
