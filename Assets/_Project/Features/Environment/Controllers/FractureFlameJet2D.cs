using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Environment.Services;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class FractureFlameJet2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private FractureConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private GameObject _flame;
        [SerializeField] private TextMesh _status;
        [SerializeField] private Vector2 _landingBounds;
        private Collider2D _collider;
        private IPlayerRespawnable _player;
        private GeyserCycle _cycle;
        public bool IsDangerous => _cycle != null && _cycle.Phase == GeyserPhase.Erupting;
        public bool IsWarning => _cycle != null && _cycle.Phase == GeyserPhase.Warning;
        private void Awake() { _collider = GetComponent<Collider2D>(); _collider.isTrigger = true; ResetCycle(); }
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += OnRespawn;
        }
        private void OnDestroy() { if (_player != null) _player.OnRespawned -= OnRespawn; }
        private void FixedUpdate()
        {
            Vector2 p = _playerSource.transform.position;
            if (p.x >= _landingBounds.x && p.x <= _landingBounds.y) _cycle.Tick(Time.fixedDeltaTime);
            else _cycle.Reset();
            _collider.enabled = IsDangerous;
        }
        private void Update()
        {
            _flame.SetActive(IsDangerous);
            _status.text = IsDangerous ? "¡FUEGO! BAJA CON PISOTÓN" : IsWarning ? "¡AVISO! PISOTÓN ↓" : "PISOTÓN ↓ AL REFUGIO";
            _status.color = IsDangerous || IsWarning ? Color.yellow : Color.cyan;
        }
        private void ResetCycle() { _cycle = new GeyserCycle(_config.JetRecovery, _config.JetWarning, _config.JetDuration); _collider.enabled = false; }
        private void OnRespawn(Vector2 checkpoint) { ResetCycle(); _flame.SetActive(false); }
        public void OnHazardTouch() { }
    }
}
