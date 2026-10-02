using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Enemies.Services;
using AlmaDino.Features.Enemies.ScriptableObjects;
using UnityEngine;
namespace AlmaDino.Features.Enemies.Controllers
{
    public sealed class PoisonToad2D : MonoBehaviour, IHazard2D
    {
        [SerializeField] private PoisonToadConfigSO _config;
        [SerializeField] private PoisonBubble2D _projectilePrefab;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private SpriteRenderer _visual;
        [SerializeField] private GameObject _warningSign;
        private float _shotDirection = -1f;
        private PoisonBubble2D[] _pool;
        private PoisonShotCycle _cycle;
        private IPlayerRespawnable _player;
        public bool IsWarning => _cycle != null && _cycle.IsWarning;
        public int ShotsFired { get; private set; }
        public int ActiveProjectileCount
        {
            get
            {
                int count = 0;
                if (_pool != null) foreach (var bubble in _pool) if (bubble.IsDangerous) count++;
                return count;
            }
        }
        private void Start()
        {
            if (_config == null || _projectilePrefab == null) { enabled = false; return; }
            _cycle = new PoisonShotCycle(_config.ShotInterval, _config.WarningDuration);
            _pool = new PoisonBubble2D[_config.PoolSize];
            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i] = Instantiate(_projectilePrefab, transform.parent);
                _pool[i].Clear();
            }
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetEncounter;
        }
        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetEncounter;
        }
        private void FixedUpdate()
        {
            if (_cycle == null || _playerSource == null) return;
            if (Mathf.Abs(_playerSource.transform.position.x - transform.position.x) > _config.DetectionDistance)
            { _cycle.Reset(); return; }
            float horizontalDistance = _playerSource.transform.position.x - transform.position.x;
            if (Mathf.Abs(horizontalDistance) > 0.01f) _shotDirection = Mathf.Sign(horizontalDistance);
            if (!_cycle.Tick(Time.fixedDeltaTime)) return;
            foreach (var bubble in _pool)
            {
                if (bubble.IsDangerous) continue;
                float direction = Mathf.Sign(_shotDirection);
                bubble.Launch((Vector2)transform.position + new Vector2(direction * 0.8f, 0.3f),
                    new Vector2(direction * _config.HorizontalSpeed, _config.VerticalSpeed),
                    _config.GravityScale, _config.ProjectileLifetime);
                ShotsFired++;
                break;
            }
        }
        private void Update()
        {
            if (_warningSign != null) _warningSign.SetActive(IsWarning);
            if (_visual != null) _visual.color = IsWarning ? new Color(1f, 0.8f, 0.2f) : new Color(0.35f, 0.09f, 0.6f);
        }
        private void ResetEncounter(Vector2 position)
        {
            _cycle?.Reset();
            ShotsFired = 0;
            if (_pool != null) foreach (var bubble in _pool) bubble.Clear();
        }
        public void OnHazardTouch() { }
    }
}
