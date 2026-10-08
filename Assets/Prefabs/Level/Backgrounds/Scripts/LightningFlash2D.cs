using System;
using UnityEngine;

namespace AlmaGame.Level
{
    // Distant lightning for a backdrop: every few seconds this sprite (a soft glow over the storm) flashes
    // twice, quickly, then fades. `Flashed` fires on each strike so other things (a boss silhouette) can
    // light up with it.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class LightningFlash2D : MonoBehaviour
    {
        [Tooltip("Seconds between strikes, picked at random in this range.")]
        [SerializeField] private Vector2 _interval = new Vector2(4f, 9f);
        [Tooltip("Strongest opacity of the flash.")]
        [SerializeField, Range(0f, 1f)] private float _peakAlpha = 0.5f;
        [Tooltip("Length of the whole double flash (seconds).")]
        [SerializeField, Min(0.05f)] private float _duration = 0.45f;

        public event Action Flashed;

        // Seconds between strikes (min, max); boss backdrops shorten it as the storm arrives.
        public Vector2 Interval
        {
            get => _interval;
            set
            {
                _interval = value;
                _nextStrike = Mathf.Min(_nextStrike, Time.time + value.y);
            }
        }

        private SpriteRenderer _renderer;
        private Color _color;
        private float _nextStrike;
        private float _strikeStart = -100f;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _color = _renderer.color;
            SetAlpha(0f);
            _nextStrike = Time.time + UnityEngine.Random.Range(1f, _interval.x);
        }

        private void Update()
        {
            if (Time.time >= _nextStrike)
            {
                _strikeStart = Time.time;
                _nextStrike = Time.time + UnityEngine.Random.Range(_interval.x, _interval.y);
                Flashed?.Invoke();
            }
            float t = (Time.time - _strikeStart) / _duration;
            SetAlpha(t >= 0f && t <= 1f ? _peakAlpha * Shape(t) : 0f);
        }

        // Two sharp peaks (a strong one, a short gap, a weaker one) and a fade.
        private static float Shape(float t)
        {
            if (t < 0.12f) return t / 0.12f;
            if (t < 0.22f) return Mathf.Lerp(1f, 0.15f, (t - 0.12f) / 0.1f);
            if (t < 0.3f) return Mathf.Lerp(0.15f, 0.75f, (t - 0.22f) / 0.08f);
            return Mathf.Lerp(0.75f, 0f, (t - 0.3f) / 0.7f);
        }

        private void SetAlpha(float alpha) => _renderer.color = new Color(_color.r, _color.g, _color.b, alpha);
    }
}
