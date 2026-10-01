using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class SeesawPlatform2D : MonoBehaviour, IGroundPoundReceiver2D
    {
        [SerializeField] private SeesawConfigSO _config;
        [SerializeField] private CatapultWeight2D _counterweight;
        [SerializeField] private MonoBehaviour _playerSource;
        private Rigidbody2D _body;
        private IPlayerRespawnable _player;
        private float _halfLength;
        private float _targetAngle;
        private float _occupiedUntil;
        private float _armedUntil;
        private float _impactSide;
        private bool _playerLaunched;

        public float CurrentAngle => _body != null ? Mathf.DeltaAngle(0f, _body.rotation) : 0f;
        public bool IsOccupied => Time.time < _occupiedUntil;
        public bool IsArmed => Time.time < _armedUntil;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _halfLength = GetComponent<BoxCollider2D>().size.x * transform.lossyScale.x * 0.5f;
        }

        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetMechanism;
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetMechanism;
        }

        private void FixedUpdate()
        {
            if (_config == null) return;
            float target = IsArmed ? -_impactSide * _config.MaxAngle : IsOccupied ? _targetAngle : 0f;
            float speed = IsArmed ? _config.ImpactTiltSpeed : IsOccupied ? _config.TiltSpeed : _config.ReturnSpeed;
            _body.MoveRotation(Mathf.MoveTowards(CurrentAngle, target, speed * Time.fixedDeltaTime));
        }

        private void OnCollisionEnter2D(Collision2D collision) => HandleContact(collision);
        private void OnCollisionStay2D(Collision2D collision) => HandleContact(collision);

        private void HandleContact(Collision2D collision)
        {
            if (_config == null || collision.gameObject.GetComponent<IPlayerRespawnable>() == null) return;
            bool above = false;
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y < -0.5f) above = true;
            if (!above) return;
            float lever = Mathf.Clamp((collision.transform.position.x - _body.position.x) / _halfLength, -1f, 1f);
            _occupiedUntil = Time.time + 0.08f;
            _targetAngle = -lever * _config.MaxAngle;
            if (!IsArmed || _playerLaunched || lever * _impactSide > -0.65f) return;
            var bounceable = collision.gameObject.GetComponent<IBounceable2D>();
            if (bounceable == null) return;
            _playerLaunched = true;
            bounceable.ApplyBounce(_config.PlayerLaunchVelocity, true);
        }

        public void ReceiveGroundPound(Vector2 impactPosition)
        {
            if (_config == null) return;
            float lever = (impactPosition.x - _body.position.x) / _halfLength;
            if (Mathf.Abs(lever) < _config.MinimumImpactLever) return;
            _impactSide = Mathf.Sign(lever);
            _armedUntil = Time.time + _config.LaunchWindow;
            _playerLaunched = false;
            if (_counterweight != null && (_counterweight.transform.position.x - _body.position.x) * _impactSide < 0f)
                _counterweight.Launch(_config.WeightLaunchVelocity);
        }

        private void ResetMechanism(Vector2 position)
        {
            _armedUntil = _occupiedUntil = 0f;
            _targetAngle = 0f;
            _playerLaunched = false;
            _body.rotation = 0f;
            if (_counterweight != null) _counterweight.ResetWeight();
        }
    }
}
