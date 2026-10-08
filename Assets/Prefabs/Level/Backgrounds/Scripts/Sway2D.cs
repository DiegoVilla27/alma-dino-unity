using UnityEngine;

namespace AlmaGame.Level
{
    // Gusty horizontal sway (a canopy in the wind): two out-of-step sine waves around the start position, so it
    // never looks mechanical. Boss backdrops drive `Amplitude` (units) and `Speed` (cycles per second) per phase.
    [DisallowMultipleComponent]
    public sealed class Sway2D : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _amplitude = 0.1f;
        [SerializeField, Min(0f)] private float _speed = 0.4f;

        private Vector3 _start;
        private float _phase;

        public float Amplitude { get => _amplitude; set => _amplitude = value; }
        public float Speed { get => _speed; set => _speed = value; }

        private void Awake() => _start = transform.localPosition;

        private void Update()
        {
            _phase += _speed * Time.deltaTime * Mathf.PI * 2f;
            float gust = Mathf.Sin(_phase) + 0.45f * Mathf.Sin(_phase * 2.3f + 1.1f);
            transform.localPosition = _start + Vector3.right * (_amplitude * gust / 1.45f);
        }
    }
}
