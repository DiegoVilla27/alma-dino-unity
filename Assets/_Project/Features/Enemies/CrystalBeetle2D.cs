using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class CrystalBeetle2D : MonoBehaviour, IConditionalHazard2D, ISeismicReactive2D
    {
        [SerializeField] private CrystalEnemyConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private Vector2 _patrolBounds;
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _shell;
        [SerializeField] private Transform _timerBar;
        private Rigidbody2D _body;
        private IPlayerRespawnable _player;
        private Vector2 _spawn;
        private float _flippedUntil;
        private float _groundedUntil;
        private float _direction = 1f;
        public bool IsFlipped => Time.time < _flippedUntil;
        public bool IsDangerous => !IsFlipped;
        public float FlipTimeRemaining => Mathf.Max(0f, _flippedUntil - Time.time);

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _spawn = _body.position;
        }

        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetEnemy;
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetEnemy;
        }

        private void FixedUpdate()
        {
            if (_config == null) return;
            if (_body.position.x >= _patrolBounds.y) _direction = -1f;
            if (_body.position.x <= _patrolBounds.x) _direction = 1f;
            float speed = !IsFlipped && Time.time < _groundedUntil ? _direction * _config.BeetleSpeed : 0f;
            _body.linearVelocity = new Vector2(speed, _body.linearVelocity.y);
        }

        private void Update()
        {
            if (_visual != null) _visual.localRotation = Quaternion.Euler(0f, 0f, IsFlipped ? 180f : 0f);
            if (_shell != null) _shell.color = IsFlipped ? new Color(0.97f, 0.48f, 0.05f) : new Color(0.88f, 0.67f, 1f);
            if (_timerBar != null) _timerBar.localScale = new Vector3(FlipTimeRemaining / _config.FlipDuration, 1f, 1f);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<IPlayerRespawnable>() != null) return;
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y > 0.5f) _groundedUntil = Time.time + 0.08f;
        }

        public void ReceiveSeismicShock()
        {
            _flippedUntil = Time.time + _config.FlipDuration;
            _body.linearVelocity = new Vector2(0f, Mathf.Max(_body.linearVelocity.y, _config.FlipHopVelocity));
        }

        public void OnHazardTouch() { }

        private void ResetEnemy(Vector2 position)
        {
            _flippedUntil = _groundedUntil = 0f;
            _direction = 1f;
            _body.position = _spawn;
            _body.linearVelocity = Vector2.zero;
        }
    }
}
