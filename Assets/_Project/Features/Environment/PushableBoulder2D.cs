using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Environment.Services;
using UnityEngine;
namespace AlmaDino.Features.Environment
{
    /// <summary>Heavy basalt moved only by Roar. Lava landings become flat, stable supports.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class PushableBoulder2D : MonoBehaviour, IRoarReactive2D
    {
        [SerializeField] private BoulderRoarConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private Collider2D _lava;
        [SerializeField] private SpriteRenderer _boulderRenderer;
        [SerializeField] private BoxCollider2D _bridgeCollider;
        [SerializeField] private GameObject _bridgeVisual;
        private Rigidbody2D _body;
        private BoulderPushMotion _motion;
        private IPlayerRespawnable _player;
        private Vector2 _spawn;
        private bool _landingInLava;
        public bool IsSolidified { get; private set; }
        public bool IsMoving => _motion != null && _motion.IsMoving;
        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.bodyType = RigidbodyType2D.Kinematic;
            _body.useFullKinematicContacts = true;
            _spawn = _body.position;
            if (_config == null) { enabled = false; return; }
            _motion = new BoulderPushMotion(_config.PushDuration, _config.ArcHeight);
            _motion.Reset(_spawn);
            _bridgeCollider.enabled = false;
            _bridgeVisual.SetActive(false);
        }
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetForCheckpoint;
        }
        private void OnDestroy() { if (_player != null) _player.OnRespawned -= ResetForCheckpoint; }
        public void ReceiveRoar(Vector2 direction)
        {
            if (_motion == null || IsMoving || IsSolidified || Mathf.Abs(direction.x) < .5f) return;
            Vector2 destination = _body.position + Vector2.right * (Mathf.Sign(direction.x) * _config.PushDistance);
            _landingInLava = _lava != null && destination.x >= _lava.bounds.min.x && destination.x <= _lava.bounds.max.x;
            if (_landingInLava) destination.y = _config.BridgeTop - _config.Radius;
            _motion.Start(_body.position, destination);
            _boulderRenderer.color = new Color(.95f, .47f, .12f);
        }
        private void FixedUpdate()
        {
            if (!IsMoving) return;
            _motion.Tick(Time.fixedDeltaTime);
            _body.MovePosition(_motion.Position);
            if (IsMoving) return;
            if (_landingInLava) Solidify();
            else _boulderRenderer.color = new Color(.23f, .24f, .26f);
        }
        public void Solidify()
        {
            IsSolidified = true;
            _bridgeCollider.enabled = true;
            _bridgeVisual.SetActive(true);
            _boulderRenderer.color = new Color(.25f, .3f, .34f);
        }
        private void ResetForCheckpoint(Vector2 checkpoint)
        {
            if (IsSolidified && checkpoint.x > _spawn.x) return;
            IsSolidified = false;
            _landingInLava = false;
            _motion?.Reset(_spawn);
            _body.position = _spawn;
            transform.position = _spawn;
            _bridgeCollider.enabled = false;
            _bridgeVisual.SetActive(false);
            _boulderRenderer.color = new Color(.23f, .24f, .26f);
        }
    }
}
