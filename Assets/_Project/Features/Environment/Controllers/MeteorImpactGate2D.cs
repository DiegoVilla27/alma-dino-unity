using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment.ScriptableObjects;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class MeteorImpactGate2D : MonoBehaviour
    {
        [SerializeField] private FractureConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private BreakableGround2D _ground;
        [SerializeField] private ReflectableMeteor2D _meteor;
        [SerializeField] private SpriteRenderer _visual;
        [SerializeField] private Transform _warningMarker;
        [SerializeField] private TextMesh _status;
        [SerializeField] private float _chamberLeft;
        private Collider2D _barrier;
        private IPlayerRespawnable _player;
        private float _warningTimer;
        public bool IsOpen { get; private set; }
        public bool IsWarning { get; private set; }
        public int MeteorsLaunched { get; private set; }
        public ReflectableMeteor2D Meteor => _meteor;
        private void Awake() { _barrier = GetComponent<Collider2D>(); _warningTimer = _config.MeteorWarning; }
        private void Start()
        {
            _meteor.Clear(); _warningMarker.gameObject.SetActive(false);
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetForCheckpoint;
        }
        private void OnDestroy() { if (_player != null) _player.OnRespawned -= ResetForCheckpoint; }
        private void FixedUpdate()
        {
            Vector2 p = _playerSource.transform.position;
            bool ready = !IsOpen && _ground.IsBroken && p.x >= _chamberLeft && p.x < transform.position.x - 3.3f && p.y < -.25f;
            IsWarning = ready && !_meteor.IsFlying;
            if (!IsWarning) { _warningTimer = _config.MeteorWarning; return; }
            _warningTimer -= Time.fixedDeltaTime;
            if (_warningTimer > 0f) return;
            // Offset follows the launch position, keeping the meteor inside the ordinary 3m Roar cone.
            _meteor.Launch(p + new Vector2(2.4f, 1.35f), new Vector2(-_config.MeteorSpeed, -.4f),
                _config.MeteorLifetime, _config.ReflectedSpeed, this);
            MeteorsLaunched++; IsWarning = false; _warningTimer = _config.MeteorWarning;
        }
        private void Update()
        {
            _warningMarker.gameObject.SetActive(IsWarning);
            if (IsWarning) _warningMarker.position = _playerSource.transform.position + new Vector3(2.4f, 1.35f);
            _status.text = IsOpen ? "COMPUERTA ABIERTA" : !_ground.IsBroken ? "PISOTÓN ↓ / METEORITO →" : IsWarning ? "¡METEORITO! MIRA A LA DERECHA" : "ROAR → DEVUELVE EL METEORITO";
            if (!IsOpen && _ground.IsBroken && _playerSource.transform.position.x >= transform.position.x - 3.3f) _status.text = "RETROCEDE AL REFUGIO ←";
            _status.color = IsOpen ? Color.cyan : Color.yellow;
        }
        public void OpenByMeteor()
        {
            if (IsOpen) return;
            IsOpen = true; _barrier.enabled = false; _visual.enabled = false;
            _warningMarker.gameObject.SetActive(false);
        }
        private void ResetForCheckpoint(Vector2 checkpoint)
        {
            _meteor.Clear(); IsWarning = false; _warningTimer = _config.MeteorWarning; MeteorsLaunched = 0;
            _warningMarker.gameObject.SetActive(false);
            if (checkpoint.x > transform.position.x) return;
            IsOpen = false; _barrier.enabled = true; _visual.enabled = true; _ground.Restore();
        }
    }
}
