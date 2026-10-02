using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RoarWaveVisual2D : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private float _duration = .25f;
        [SerializeField] private float _radius = 3f;
        [SerializeField] private float _halfAngle = 45f;
        private IRoarEmitter2D _emitter;
        private LineRenderer _line;
        private Vector2 _origin, _direction;
        private float _remaining;
        private void Awake()
        {
            _line = GetComponent<LineRenderer>(); _line.positionCount = 17; _line.useWorldSpace = true; _line.enabled = false;
        }
        private void Start() { _emitter = _playerSource as IRoarEmitter2D; if (_emitter != null) _emitter.OnRoared += Show; }
        private void OnDestroy() { if (_emitter != null) _emitter.OnRoared -= Show; }
        private void Show(Vector2 origin, Vector2 direction) { _origin = origin; _direction = direction; _remaining = _duration; _line.enabled = true; }
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
