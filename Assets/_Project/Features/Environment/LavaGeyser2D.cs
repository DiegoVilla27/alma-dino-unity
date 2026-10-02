using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Environment.Services;
using UnityEngine;
namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LavaGeyser2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private GeyserConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private Transform _flamePillarRoot;
        [SerializeField] private SpriteRenderer _ventBaseRenderer;
        [SerializeField] private TextMesh _warningLabel;
        private Collider2D _collider;
        private GeyserCycle _cycle;
        private IPlayerRespawnable _player;
        public GeyserPhase Phase => _cycle != null ? _cycle.Phase : GeyserPhase.Dormant;
        public bool IsErupting => Phase == GeyserPhase.Erupting;
        public bool IsDangerous => IsErupting;
        private void Awake()
        {
            _collider = GetComponent<Collider2D>(); _collider.isTrigger = true; _collider.enabled = false;
            if (_config == null) { enabled = false; return; }
            _cycle = new GeyserCycle(_config.DormantDuration, _config.WarningDuration, _config.EruptionDuration);
            _flamePillarRoot.gameObject.SetActive(false);
        }
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetForCheckpoint;
        }
        private void OnDestroy() { if (_player != null) _player.OnRespawned -= ResetForCheckpoint; }
        private void FixedUpdate() { _cycle?.Tick(Time.fixedDeltaTime); _collider.enabled = IsErupting; }
        private void Update()
        {
            _flamePillarRoot.gameObject.SetActive(IsErupting);
            _ventBaseRenderer.color = Phase == GeyserPhase.Dormant ? new Color(.25f, .23f, .2f) : Color.yellow;
            _warningLabel.text = Phase == GeyserPhase.Warning ? "¡VAPOR!" : IsErupting ? "¡ESPERA!" : "PASA";
            _warningLabel.color = Phase == GeyserPhase.Dormant ? Color.cyan : Color.yellow;
        }
        private void ResetForCheckpoint(Vector2 checkpoint) { _cycle?.Reset(); _collider.enabled = false; _flamePillarRoot.gameObject.SetActive(false); }
        public void OnHazardTouch() { }
    }
}
