using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Enemies.Services;
using AlmaDino.Features.Enemies.ScriptableObjects;
using UnityEngine;
namespace AlmaDino.Features.Enemies.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class MagmaSalamander2D : MonoBehaviour, IRoarReactive2D, IConditionalHazard2D, ISeismicReactive2D
    {
        [SerializeField] private MagmaSalamanderConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private PoisonBubble2D _fireballPrefab;
        [SerializeField] private SpriteRenderer _visual;
        [SerializeField] private TextMesh _status;
        [SerializeField] private Vector2[] _route;
        private Rigidbody2D _body;
        private PoisonShotCycle _cycle;
        private readonly PoisonBubble2D[] _pool = new PoisonBubble2D[3];
        private IPlayerRespawnable _player;
        private float _stunRemaining;
        private int _waypoint;
        public bool IsStunned => _stunRemaining > 0f;
        public bool IsDangerous => !IsStunned;
        public bool IsWarning => !IsStunned && _cycle != null && _cycle.IsWarning;
        public int ShotsFired { get; private set; }
        public Vector2 LastShotDirection { get; private set; }
        private void Awake() => _body = GetComponent<Rigidbody2D>();
        private void Start()
        {
            _cycle = new PoisonShotCycle(_config.ShotInterval, _config.WarningDuration);
            for (int i = 0; i < _pool.Length; i++) { _pool[i] = Instantiate(_fireballPrefab, transform.parent); _pool[i].Clear(); }
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetEncounter;
        }
        private void OnDestroy() { if (_player != null) _player.OnRespawned -= ResetEncounter; }
        private void FixedUpdate()
        {
            if (_cycle == null) return;
            if (IsStunned)
            {
                _stunRemaining -= Time.fixedDeltaTime;
                if (!IsStunned) RestoreToRoute();
                return;
            }
            Vector2 target = _route[_waypoint];
            _body.MovePosition(Vector2.MoveTowards(_body.position, target, _config.MoveSpeed * Time.fixedDeltaTime));
            if (Vector2.Distance(_body.position, target) < .1f) _waypoint = (_waypoint + 1) % _route.Length;
            Vector2 delta = (Vector2)_playerSource.transform.position - _body.position;
            if (delta.sqrMagnitude > _config.DetectionDistance * _config.DetectionDistance) { _cycle.Reset(); return; }
            if (!_cycle.Tick(Time.fixedDeltaTime)) return;
            foreach (var ball in _pool)
            {
                if (ball.IsDangerous) continue;
                LastShotDirection = delta.normalized;
                ball.Launch(_body.position + LastShotDirection * .8f, LastShotDirection * _config.ProjectileSpeed, 0f, _config.ProjectileLifetime);
                ShotsFired++; break;
            }
        }
        private void Update()
        {
            _visual.color = IsStunned ? Color.cyan : IsWarning ? Color.yellow : new Color(1f, .25f, .04f);
            _visual.flipX = _playerSource.transform.position.x < transform.position.x;
            _status.text = IsStunned ? "ATURDIDA" : IsWarning ? "¡FUEGO!" : "ROAR → ATURDE";
        }
        public void ReceiveRoar(Vector2 direction)
        {
            _stunRemaining = _config.StunDuration; _cycle.Reset();
            foreach (var ball in _pool) ball.Clear();
            _body.bodyType = RigidbodyType2D.Dynamic; _body.gravityScale = 2f;
            _body.linearVelocity = new Vector2(direction.x * _config.KnockbackSpeed, 2f);
        }
        public void ReceiveSeismicShock() => ReceiveRoar(Vector2.right * Mathf.Sign(transform.position.x - _playerSource.transform.position.x));
        private void RestoreToRoute()
        {
            _body.bodyType = RigidbodyType2D.Kinematic; _body.gravityScale = 0f; _body.linearVelocity = Vector2.zero;
            _body.position = _route[0]; _waypoint = 1; _cycle.Reset();
        }
        private void ResetEncounter(Vector2 checkpoint)
        {
            _stunRemaining = 0f; ShotsFired = 0; RestoreToRoute();
            foreach (var ball in _pool) ball.Clear();
        }
        public void OnHazardTouch() { }
    }
}
