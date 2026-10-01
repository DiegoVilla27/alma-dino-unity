using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Enemies
{
    public enum CaveBatPhase { Sleeping, Warning, Flying, Resting }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class CaveBat2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private CrystalEnemyConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private float _diveDepth = 2.6f;
        [SerializeField] private SpriteRenderer _visual;
        [SerializeField] private GameObject _warningSign;
        private Rigidbody2D _body;
        private IPlayerRespawnable _player;
        private Transform _playerTransform;
        private Vector2 _roost;
        private float _phaseTime;
        private float _direction;
        public CaveBatPhase Phase { get; private set; }
        public bool IsDangerous => Phase == CaveBatPhase.Flying;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _roost = _body.position;
        }

        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_playerSource != null) _playerTransform = _playerSource.transform;
            if (_player != null) _player.OnRespawned += ResetEnemy;
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetEnemy;
        }

        private void FixedUpdate()
        {
            if (_config == null || _playerTransform == null) return;
            _phaseTime += Time.fixedDeltaTime;
            switch (Phase)
            {
                case CaveBatPhase.Sleeping:
                    if (Mathf.Abs(_playerTransform.position.x - _roost.x) < _config.BatDetectionDistance
                        && _playerTransform.position.y < _roost.y && _roost.y - _playerTransform.position.y < 8f)
                    {
                        _direction = Mathf.Sign(_playerTransform.position.x - _roost.x);
                        if (_direction == 0f) _direction = 1f;
                        ChangePhase(CaveBatPhase.Warning);
                    }
                    break;
                case CaveBatPhase.Warning:
                    if (_phaseTime >= _config.BatWarningDuration) ChangePhase(CaveBatPhase.Flying);
                    break;
                case CaveBatPhase.Flying:
                    float arc = Mathf.Sin(Mathf.PI * Mathf.Clamp01(_phaseTime / _config.BatFlightDuration));
                    _body.MovePosition(_roost + new Vector2(_direction * _config.BatArcWidth * arc, -_diveDepth * arc));
                    if (_phaseTime >= _config.BatFlightDuration)
                    {
                        _body.MovePosition(_roost);
                        ChangePhase(CaveBatPhase.Resting);
                    }
                    break;
                case CaveBatPhase.Resting:
                    if (_phaseTime >= _config.BatRestDuration) ChangePhase(CaveBatPhase.Sleeping);
                    break;
            }
        }

        private void Update()
        {
            if (_warningSign != null) _warningSign.SetActive(Phase == CaveBatPhase.Warning);
            if (_visual == null) return;
            _visual.color = Phase == CaveBatPhase.Warning ? new Color(1f, 0.75f, 0.2f)
                : IsDangerous ? new Color(0.85f, 0.3f, 0.5f) : new Color(0.55f, 0.48f, 0.75f);
        }

        private void ChangePhase(CaveBatPhase phase)
        {
            Phase = phase;
            _phaseTime = 0f;
        }

        public void OnHazardTouch() { }

        private void ResetEnemy(Vector2 position)
        {
            ChangePhase(CaveBatPhase.Sleeping);
            _body.position = _roost;
            _body.linearVelocity = Vector2.zero;
        }
    }
}
