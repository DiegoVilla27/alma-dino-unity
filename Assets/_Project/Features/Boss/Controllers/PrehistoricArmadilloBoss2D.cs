using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.ScriptableObjects;
using AlmaDino.Features.Boss.Services;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PrehistoricArmadilloBoss2D : MonoBehaviour, IConditionalHazard2D, IGroundPoundReceiver2D
    {
        [SerializeField] private ArmadilloBossConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private SpriteRenderer _shell;
        [SerializeField] private GameObject _weakPoint;
        [SerializeField] private GameObject _exit;
        [SerializeField] private BossFallingCrystal2D _rockPrefab;
        [SerializeField] private CameraShakeEventChannelSO _shake;
        [SerializeField] private Vector2 _limits = new Vector2(-8f, 8f);
        [SerializeField] private float _floorY = 1f;
        private readonly ArmadilloFight _fight = new ArmadilloFight();
        private Rigidbody2D _body;
        private IPlayerRespawnable _player;
        private float _timer;
        private float _rockTimer;
        private float _direction = 1f;
        private float _legStart;
        private Transform _rocks;
        public ArmadilloFight Fight => _fight;
        public bool IsDangerous => _fight.IsRolling;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _player = _playerSource as IPlayerRespawnable;
            var rocks = new GameObject("Boss_Active_Crystals");
            rocks.transform.SetParent(transform.parent, false);
            _rocks = rocks.transform;
            _weakPoint.SetActive(false);
            if (_exit != null) _exit.SetActive(false);
            _timer = _config.WarningDuration + 2f;
            _legStart = _body.position.x;
        }

        private void OnEnable()
        {
            if (_player != null) _player.OnRespawned += ResetAfterRespawn;
        }

        private void OnDisable()
        {
            if (_player != null) _player.OnRespawned -= ResetAfterRespawn;
            ClearRocks();
        }

        private void Start()
        {
            NarrativeBannerEvents.RequestBanner("ARMADILLO PREHISTÓRICO",
                "Esquiva tres rebotes. Cuando brille su fisura, salta y pulsa POUND sobre él.", Color.cyan, 5f);
        }

        private void FixedUpdate()
        {
            if (_fight.IsDefeated) return;
            if (!_fight.IsRolling)
            {
                _timer -= Time.fixedDeltaTime;
                if (_timer <= 0f) BeginRoll();
                return;
            }
            float speed = _config.RollSpeed + _fight.Hits * _config.SpeedPerHit;
            float x = Mathf.Clamp(_body.position.x + _direction * speed * Time.fixedDeltaTime, _limits.x, _limits.y);
            float target = _direction > 0f ? _limits.y : _limits.x;
            float progress = Mathf.InverseLerp(_legStart, target, x);
            float height = _fight.Hits == 2 && _fight.Bounces == 2
                ? Mathf.Sin(progress * Mathf.PI) * _config.JumpHeight : 0f;
            _body.MovePosition(new Vector2(x, _floorY + height));
            if (Mathf.Abs(x - target) < .01f)
            {
                _fight.HitPillar();
                _shake?.Raise(.25f, .2f);
                _direction *= -1f;
                _legStart = x;
                if (_fight.IsStunned)
                {
                    ClearRocks();
                    _weakPoint.SetActive(true);
                    _shell.color = new Color(.3f, .8f, .9f);
                    _timer = _config.StunDuration;
                }
            }
            if (_fight.Hits > 0 && _fight.IsRolling) UpdateRocks();
        }

        private void BeginRoll()
        {
            _weakPoint.SetActive(false);
            _shell.color = new Color(.22f, .25f, .35f);
            _fight.BeginRoll();
            _legStart = _body.position.x;
            _rockTimer = _config.RockInterval;
        }

        private void UpdateRocks()
        {
            _rockTimer -= Time.fixedDeltaTime;
            if (_rockTimer > 0f || _rockPrefab == null || _playerSource == null) return;
            _rockTimer = _config.RockInterval;
            float x = Mathf.Clamp(_playerSource.transform.position.x, -8f, 8f);
            var rock = Instantiate(_rockPrefab, new Vector3(x, 8f, 0f), Quaternion.identity, _rocks);
            rock.Initialize(_config.RockWarningDuration, _config.RockFallSpeed);
        }

        public void ReceiveGroundPound(Vector2 impactPosition)
        {
            if (Mathf.Abs(impactPosition.x - _body.position.x) > .85f || !_fight.TryDamage()) return;
            _weakPoint.SetActive(false);
            ClearRocks();
            _shake?.Raise(.45f, .3f);
            _timer = _config.RecoveryDuration + _config.WarningDuration;
            _shell.color = new Color(1f, .55f, .25f);
            if (!_fight.IsDefeated) return;
            GetComponent<Collider2D>().enabled = false;
            GameProgression.CompleteWorld(2);
            if (_exit != null) _exit.SetActive(true);
            _shell.color = new Color(.35f, .4f, .45f, .45f);
            NarrativeBannerEvents.RequestBanner("¡MUNDO 2 COMPLETADO!",
                "El acorazado cede el paso. Con dos huevos a salvo, Alma busca el camino al Pantano de Viento.", Color.cyan, 7f);
        }

        public void OnHazardTouch() { }

        private void ResetAfterRespawn(Vector2 position)
        {
            if (_fight.IsDefeated) return;
            _fight.ResetCycle();
            ClearRocks();
            _weakPoint.SetActive(false);
            _body.position = new Vector2(0f, _floorY);
            _direction = 1f;
            _shell.color = new Color(1f, .55f, .25f);
            _timer = _config.WarningDuration + 1f;
        }

        private void ClearRocks()
        {
            if (_rocks == null) return;
            foreach (Transform rock in _rocks)
            {
                rock.gameObject.SetActive(false);
                Destroy(rock.gameObject);
            }
        }
    }
}
