using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Environment
{
    public enum CeilingPhase { Open, Warning, Descending, Closed, Retracting }

    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class CrushingCeiling2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private CrushingCeilingConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private Vector2 _closedPosition;
        [SerializeField] private SpriteRenderer _indicator;
        [SerializeField] private TextMesh _warningLabel;
        private Rigidbody2D _body;
        private IPlayerRespawnable _player;
        private Vector2 _openPosition;
        private float _elapsed;
        private float _activationDistance;
        private bool _activated;
        private bool _resetPending;
        public CeilingPhase Phase { get; private set; }
        public bool IsDangerous => true;
        public void OnHazardTouch() { }
        public float CycleTime => _elapsed;
        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _openPosition = _body.position;
            _activationDistance = GetComponent<BoxCollider2D>().size.x * 0.5f + 3f;
            UpdateFeedback();
        }
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetCycle;
        }
        private void OnDestroy() { if (_player != null) _player.OnRespawned -= ResetCycle; }
        private void FixedUpdate()
        {
            if (_resetPending)
            {
                _resetPending = false;
                _body.position = _openPosition;
                _body.linearVelocity = Vector2.zero;
                UpdateFeedback();
            }
            if (_config == null || _playerSource == null) return;
            if (!_activated)
            {
                if (Mathf.Abs(_playerSource.transform.position.x - _openPosition.x) > _activationDistance) return;
                _activated = true;
            }
            _elapsed = (_elapsed + Time.fixedDeltaTime) % _config.CycleDuration;
            float t = _elapsed;
            Vector2 target = _openPosition;
            if (t < _config.OpenDuration)
                Phase = t >= _config.OpenDuration - _config.WarningDuration ? CeilingPhase.Warning : CeilingPhase.Open;
            else if ((t -= _config.OpenDuration) < _config.DescentDuration)
            {
                Phase = CeilingPhase.Descending;
                target = Vector2.Lerp(_openPosition, _closedPosition, t / _config.DescentDuration);
            }
            else if ((t -= _config.DescentDuration) < _config.ClosedDuration)
            { Phase = CeilingPhase.Closed; target = _closedPosition; }
            else
            {
                Phase = CeilingPhase.Retracting;
                target = Vector2.Lerp(_closedPosition, _openPosition, (t - _config.ClosedDuration) / _config.RetractDuration);
            }
            _body.MovePosition(target);
            UpdateFeedback();
        }

        private void UpdateFeedback()
        {
            bool warning = Phase == CeilingPhase.Warning || Phase == CeilingPhase.Descending || Phase == CeilingPhase.Closed;
            if (_indicator != null) _indicator.color = warning ? new Color(1f, 0.35f, 0.15f) : new Color(0.56f, 0.88f, 0.94f);
            if (_warningLabel != null) _warningLabel.text = warning ? "¡BAJA! ↓" : "ESPERA / AVANZA →";
        }
        private void ResetCycle(Vector2 position)
        {
            _activated = false; _elapsed = 0f; Phase = CeilingPhase.Open;
            _resetPending = true;
        }
    }
}
