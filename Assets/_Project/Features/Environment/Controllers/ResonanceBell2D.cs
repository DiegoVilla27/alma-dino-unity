using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Environment.Services;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Collider2D), typeof(AudioSource))]
    public sealed class ResonanceBell2D : MonoBehaviour, IRangedRoarReactive2D
    {
        [SerializeField] private ResonanceBellConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _rim;
        [SerializeField] private TextMesh _countdown;
        private BellSuppressionWindow _window;
        private IPlayerRespawnable _player;
        private AudioSource _audio;
        private AudioClip _tone;
        public bool IsOpen => _window != null && _window.IsOpen;
        public float Remaining => _window != null ? _window.Remaining : 0f;
        public float RoarRange => _config != null ? _config.RoarRange : 0f;
        public int RingCount { get; private set; }
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            _window = new BellSuppressionWindow(_config.SuppressionDuration);
            _audio = GetComponent<AudioSource>();
            _tone = AudioClip.Create("Basalt_Bell_Prototype", 22050, 1, 44100, false);
            var samples = new float[22050];
            for (int i = 0; i < samples.Length; i++) samples[i] = (Mathf.Sin(i * .024f) + .25f * Mathf.Sin(i * .057f)) * (1f - i / 22050f) * .18f;
            _tone.SetData(samples, 0);
        }
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetForCheckpoint;
        }
        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetForCheckpoint;
            if (_tone != null) Destroy(_tone);
        }
        public void ReceiveRoar(Vector2 direction)
        {
            _window.Ring(); RingCount++;
            _audio.PlayOneShot(_tone);
        }
        private void FixedUpdate() => _window.Tick(Time.fixedDeltaTime);
        private void Update()
        {
            _visual.localRotation = Quaternion.Euler(0f, 0f, IsOpen ? Mathf.Sin(Time.time * 18f) * 8f : 0f);
            _rim.color = IsOpen ? Color.cyan : new Color(.87f, .63f, .37f);
            _countdown.text = IsOpen ? Remaining.ToString("0.0") + " s" : "ROAR → CAMPANA";
            _countdown.color = IsOpen && Remaining <= _config.WarningThreshold ? Color.yellow : IsOpen ? Color.cyan : Color.white;
        }
        private void ResetForCheckpoint(Vector2 checkpoint) { _window.Reset(); RingCount = 0; }
    }
}
