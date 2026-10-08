using UnityEngine;

namespace AlmaGame.Level
{
    // Breathing glow: the sprite's opacity is `Level` (0..1), dimmed by up to `Depth` in a slow sine wave of
    // `Speed` cycles per second. Boss backdrops drive Level and Speed per phase (geodes, cracks, light beams).
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class Pulse2D : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _level = 1f;
        [SerializeField, Min(0f)] private float _speed = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _depth = 0.4f;

        private SpriteRenderer _renderer;
        private float _phase;

        public float Level { get => _level; set => _level = value; }
        public float Speed { get => _speed; set => _speed = value; }

        private void Awake() => _renderer = GetComponent<SpriteRenderer>();

        private void Update()
        {
            _phase += _speed * Time.deltaTime * Mathf.PI * 2f;
            float wave = 0.5f + 0.5f * Mathf.Sin(_phase);
            Color c = _renderer.color;
            c.a = _level * Mathf.Lerp(1f - _depth, 1f, wave);
            _renderer.color = c;
        }
    }
}
