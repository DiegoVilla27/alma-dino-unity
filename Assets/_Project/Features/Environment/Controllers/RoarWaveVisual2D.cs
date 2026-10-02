using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(LineRenderer), typeof(AudioSource))]
    public sealed class RoarWaveVisual2D : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private float _duration = .25f;
        [SerializeField] private float _radius = 3f;
        [SerializeField] private float _halfAngle = 45f;
        private IRoarEmitter2D _emitter;
        private LineRenderer _line;
        private AudioSource _audio;
        private AudioClip _clip;
        private Vector2 _origin, _direction;
        private float _remaining;
        private void Awake()
        {
            _line = GetComponent<LineRenderer>(); _line.positionCount = 17; _line.useWorldSpace = true; _line.enabled = false;
            _audio = GetComponent<AudioSource>();
            _clip = AudioClip.Create("Roar_Prototype", 11025, 1, 44100, false);
            var samples = new float[11025];
            for (int i = 0; i < samples.Length; i++) samples[i] = (Mathf.Sin(i * .014f) + .3f * Mathf.Sin(i * .039f)) * (1f - i / 11025f) * .2f;
            _clip.SetData(samples, 0);
        }
        private void Start() { _emitter = _playerSource as IRoarEmitter2D; if (_emitter != null) _emitter.OnRoared += Show; }
        private void OnDestroy() { if (_emitter != null) _emitter.OnRoared -= Show; if (_clip != null) Destroy(_clip); }
        private void Show(Vector2 origin, Vector2 direction) { _origin = origin; _direction = direction; _remaining = _duration; _line.enabled = true; _audio.PlayOneShot(_clip); }
        private void Update()
        {
            if (_remaining <= 0f) { _line.enabled = false; return; }
            _remaining -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(_remaining / _duration);
            float radius = Mathf.Lerp(.4f, _radius, progress);
            for (int i = 0; i < 17; i++)
            {
                float angle = Mathf.Lerp(-_halfAngle, _halfAngle, i / 16f) * Mathf.Deg2Rad;
                _line.SetPosition(i, _origin + new Vector2(_direction.x * Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            var color = new Color(1f, .72f, .03f, 1f - progress);
            _line.startColor = color; _line.endColor = color;
        }
    }
}
