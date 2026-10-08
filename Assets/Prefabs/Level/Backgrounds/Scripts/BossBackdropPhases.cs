using System;
using UnityEngine;

namespace AlmaGame.Level
{
    // Drives a boss backdrop through the fight's phases: the storm rolls in as the boss escalates. Each phase
    // sets how lit the scene is, how far the storm cloud has come down, how fast the clouds below pass, where
    // and how often lightning strikes and how hard it rains; changes blend over `Transition` seconds.
    // The boss calls SetPhase(0..2); `Preview Phase` lets you try them in Play mode from the Inspector.
    [DisallowMultipleComponent]
    public sealed class BossBackdropPhases : MonoBehaviour
    {
        [Serializable]
        public struct Phase
        {
            [Tooltip("Brightness of the sky, clouds and lair (1 = as painted).")]
            [Range(0f, 1f)] public float Light;
            [Range(0f, 1f)] public float StormAlpha;
            [Tooltip("How far above its final place the storm cloud is (units).")]
            public float StormRaise;
            [Tooltip("Speed of the cloud sea below the arena (units/s, negative = left).")]
            public float CloudDrift;
            public Vector2 LightningInterval;
            public Vector3 LightningPosition;
            public Vector3 LightningScale;
            [Tooltip("Rain drops per second.")]
            [Min(0f)] public float Rain;
        }

        [SerializeField] private SpriteRenderer _sky;
        [SerializeField] private SpriteRenderer _storm;
        [SerializeField] private ParallaxLayer2D _clouds;
        [SerializeField] private SpriteRenderer _lair;
        [SerializeField] private LightningFlash2D _lightning;
        [SerializeField] private ParticleSystem _rain;
        [Tooltip("The lair is closer and keeps more of its light than the sky.")]
        [SerializeField, Range(0f, 1f)] private float _lairLightShare = 0.6f;
        [SerializeField, Min(0.01f)] private float _transition = 2.5f;
        [SerializeField] private Phase[] _phases = new Phase[3];
        [SerializeField, Range(0, 2)] private int _previewPhase;

        private Phase _current;
        private Phase _from;
        private Phase _to;
        private float _blend = 1f;
        private Vector3 _stormBase;
        private SpriteRenderer _cloudRenderer;

        public int CurrentPhase { get; private set; }

        private void Awake()
        {
            if (_storm != null) _stormBase = _storm.transform.localPosition;
            if (_clouds != null) _cloudRenderer = _clouds.GetComponent<SpriteRenderer>();
            CurrentPhase = Mathf.Clamp(_previewPhase, 0, _phases.Length - 1);
            _current = _from = _to = _phases[CurrentPhase];
            Apply(_current);
        }

        public void SetPhase(int phase)
        {
            phase = Mathf.Clamp(phase, 0, _phases.Length - 1);
            if (phase == CurrentPhase && _blend >= 1f) return;
            CurrentPhase = phase;
            _from = _current;
            _to = _phases[phase];
            _blend = 0f;
        }

        private void Update()
        {
            if (_blend >= 1f) return;
            _blend = Mathf.MoveTowards(_blend, 1f, Time.deltaTime / _transition);
            _current = Lerp(_from, _to, Mathf.SmoothStep(0f, 1f, _blend));
            Apply(_current);
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _phases != null && _phases.Length > 0) SetPhase(_previewPhase);
        }

        private void Apply(Phase p)
        {
            Color lit = new Color(p.Light, p.Light, p.Light, 1f);
            if (_sky != null) _sky.color = lit;
            if (_cloudRenderer != null) _cloudRenderer.color = lit;
            if (_lair != null)
            {
                float l = Mathf.Lerp(1f, p.Light, _lairLightShare);
                _lair.color = new Color(l, l, l, 1f);
            }
            if (_storm != null)
            {
                _storm.color = new Color(p.Light, p.Light, p.Light, p.StormAlpha);
                _storm.transform.localPosition = _stormBase + Vector3.up * p.StormRaise;
            }
            if (_clouds != null) _clouds.Drift = p.CloudDrift;
            if (_lightning != null)
            {
                _lightning.Interval = p.LightningInterval;
                _lightning.transform.localPosition = p.LightningPosition;
                _lightning.transform.localScale = p.LightningScale;
            }
            if (_rain != null)
            {
                var emission = _rain.emission;
                emission.rateOverTime = p.Rain;
            }
        }

        private static Phase Lerp(Phase a, Phase b, float t) => new Phase
        {
            Light = Mathf.Lerp(a.Light, b.Light, t),
            StormAlpha = Mathf.Lerp(a.StormAlpha, b.StormAlpha, t),
            StormRaise = Mathf.Lerp(a.StormRaise, b.StormRaise, t),
            CloudDrift = Mathf.Lerp(a.CloudDrift, b.CloudDrift, t),
            LightningInterval = Vector2.Lerp(a.LightningInterval, b.LightningInterval, t),
            LightningPosition = Vector3.Lerp(a.LightningPosition, b.LightningPosition, t),
            LightningScale = Vector3.Lerp(a.LightningScale, b.LightningScale, t),
            Rain = Mathf.Lerp(a.Rain, b.Rain, t),
        };
    }
}
