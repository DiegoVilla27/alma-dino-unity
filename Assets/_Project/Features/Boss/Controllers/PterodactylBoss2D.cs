using System.Collections;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.ScriptableObjects;
using AlmaDino.Features.Boss.Services;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PterodactylBoss2D : MonoBehaviour
    {
        [SerializeField] private PterodactylBossConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private GameObject[] _branches;
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _crest;
        [SerializeField] private TextMesh _hint;
        [SerializeField] private GameObject _exit;
        [SerializeField] private CameraShakeEventChannelSO _shake;
        [SerializeField] private ParticleSystem _sparks;
        [SerializeField] private SpriteRenderer[] _windStreaks;
        [SerializeField] private Transform[] _wings;
        [SerializeField] private Transform _attackMarker;
        private readonly PterodactylFight _fight = new PterodactylFight();
        private Rigidbody2D _body;
        private Rigidbody2D _playerBody;
        private IPlayerRespawnable _player;
        private float _timer;
        private float _direction = -1f;
        private Vector2 _target;
        private float _endX;
        private float _previousTimeScale = 1f;
        private bool _hitStopped;
        public PterodactylFight Fight => _fight;
        public bool IsDiving => _fight.Phase == PterodactylPhase.Dive;
        public float DiveDirection => _direction;
        public Vector2 Target => _target;
        public int DestroyedBranchCount => _fight.Hits;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _player = _playerSource as IPlayerRespawnable;
            _playerBody = _playerSource.GetComponent<Rigidbody2D>();
            _timer = _config.WindDuration + 2f;
            _exit.SetActive(false);
        }
        private void OnEnable() { if (_player != null) _player.OnRespawned += ResetAfterRespawn; }
        private void OnDisable()
        {
            if (_player != null) _player.OnRespawned -= ResetAfterRespawn;
            RestoreTimeScale();
        }
        private void Start() => NarrativeBannerEvents.RequestBanner("PTERODÁCTILO ALFA",
            "Resiste el viento. En el aviso: Doble Salto, luego DASH de frente a la cresta cian. Tres impactos.", Color.cyan, 6f);

        private void FixedUpdate()
        {
            if (_fight.IsDefeated) return;
            float dt = Time.fixedDeltaTime;
            _timer -= dt;
            switch (_fight.Phase)
            {
                case PterodactylPhase.Wind:
                    if (!(_playerSource is IWindAffected2D wind && wind.IgnoresWind))
                        _playerBody.AddForce(Vector2.right * (_direction * _config.WindAcceleration * _playerBody.mass));
                    _body.MovePosition(new Vector2(_playerBody.position.x + _direction * -5f, 6f));
                    if (_timer <= 0f) LockDive();
                    break;
                case PterodactylPhase.Warning:
                    if (_timer <= 0f) { _fight.BeginDive(); SetHint("¡DASH " + (_direction < 0f ? "→" : "←") + " A LA CABEZA!", Color.cyan); }
                    break;
                case PterodactylPhase.Dive:
                    float x = _body.position.x + _direction * (_config.DiveSpeed + _fight.Hits * _config.SpeedPerHit) * dt;
                    _body.MovePosition(new Vector2(x, _target.y));
                    if ((_direction > 0f && x >= _endX) || (_direction < 0f && x <= _endX)) ResetAfterRespawn(_playerBody.position);
                    break;
                case PterodactylPhase.Recovery:
                    _body.MovePosition(Vector2.MoveTowards(_body.position, new Vector2(_target.x, _target.y - _config.FlightHeight + .9f), 4f * dt));
                    if (_timer <= 0f) ResetAfterRespawn(_playerBody.position);
                    break;
            }
            if (_fight.Phase == PterodactylPhase.Wind) SetHint("VIENTO: " + (_direction < 0f ? "←" : "→") + "  |  " + _fight.Hits + "/3", Color.white);
        }

        private void Update()
        {
            bool wind = _fight.Phase == PterodactylPhase.Wind;
            for (int i = 0; i < _windStreaks.Length; i++)
            {
                _windStreaks[i].enabled = wind;
                if (wind) _windStreaks[i].transform.position = new Vector2(_playerBody.position.x + Mathf.Repeat(Time.time * _direction * 8f + i * 3f, 20f) - 10f, 1f + i % 4);
            }
            for (int i = 0; i < _wings.Length; i++)
                _wings[i].localRotation = Quaternion.Euler(0f, 0f, (i == 0 ? -1f : 1f) * (30f + (wind ? Mathf.Sin(Time.time * 7f) * 15f : 0f)));
            _attackMarker.gameObject.SetActive(_fight.Phase == PterodactylPhase.Warning || IsDiving);
            _attackMarker.position = _target;
        }

        private void LockDive()
        {
            GameObject branch = _branches[0];
            float best = float.MaxValue;
            foreach (var candidate in _branches)
            {
                if (!candidate.activeSelf) continue;
                float distance = Mathf.Abs(candidate.transform.position.x - _playerBody.position.x);
                if (distance >= best) continue;
                best = distance;
                branch = candidate;
            }
            var bounds = branch.GetComponent<Collider2D>().bounds;
            _target = new Vector2(Mathf.Clamp(_playerBody.position.x, bounds.min.x + .8f, bounds.max.x - .8f), bounds.max.y + _config.FlightHeight);
            _body.position = new Vector2(_target.x - _direction * _config.DiveReach, _target.y);
            _endX = _target.x + _direction * _config.DiveReach;
            _visual.localScale = new Vector3(-_direction, 1f, 1f);
            _fight.BeginWarning();
            _timer = _config.WarningDuration;
            _crest.color = Color.cyan;
            SetHint("PICADO " + (_direction < 0f ? "←" : "→") + "  DOBLE SALTO + DASH " + (_direction < 0f ? "→" : "←"), Color.yellow);
        }

        public bool TryStrike(Vector2 dashVelocity)
        {
            if (!_fight.TryStrike(dashVelocity.x * _direction < 0f)) return false;
            DestroyFarthestBranch();
            _timer = _config.RecoveryDuration;
            _crest.color = Color.yellow;
            SetHint("¡IMPACTO! " + _fight.Hits + "/3", Color.yellow);
            _shake?.Raise(.4f, .25f);
            _sparks.Play();
            StartCoroutine(HitStop());
            if (_fight.IsDefeated)
            {
                GameProgression.CompleteWorld(3);
                _exit.SetActive(true);
                _visual.gameObject.SetActive(false);
                NarrativeBannerEvents.RequestBanner("¡MUNDO 3 COMPLETADO!",
                    "El rey de los cielos huye. La Cima Volcánica y tu último hijo te esperan.", Color.cyan, 7f);
            }
            return true;
        }

        private void DestroyFarthestBranch()
        {
            GameObject branch = null;
            float farthest = -1f;
            for (int i = 1; i < _branches.Length; i++)
            {
                if (!_branches[i].activeSelf) continue;
                float distance = Mathf.Abs(_branches[i].transform.position.x - _playerBody.position.x);
                if (distance <= farthest) continue;
                farthest = distance;
                branch = _branches[i];
            }
            if (branch != null) branch.SetActive(false);
        }
        private void ResetAfterRespawn(Vector2 position)
        {
            if (_fight.IsDefeated) return;
            _fight.ResetCycle();
            _direction = _fight.Hits % 2 == 0 ? -1f : 1f;
            _timer = _config.WindDuration;
            _crest.color = new Color(.2f, .7f, .8f);
        }
        private void SetHint(string text, Color color)
        {
            _hint.text = text;
            _hint.color = color;
            _hint.transform.position = _playerSource.transform.position + Vector3.up * 4f;
        }
        private IEnumerator HitStop()
        {
            _previousTimeScale = Time.timeScale;
            _hitStopped = true;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(_config.HitStopDuration);
            RestoreTimeScale();
        }
        private void RestoreTimeScale()
        {
            if (!_hitStopped) return;
            Time.timeScale = _previousTimeScale;
            _hitStopped = false;
        }
        public float BounceVelocity => _config.BounceVelocity;
    }
}
