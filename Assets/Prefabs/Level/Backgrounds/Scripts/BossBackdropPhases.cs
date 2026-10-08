using System;
using UnityEngine;

namespace AlmaGame.Level
{
    // Drives a boss backdrop through the fight's phases, so the arena escalates with the boss (the storm rolls
    // in, the cave cracks open...). Each track sets one thing per phase (`Values`, or `Vectors` for positions,
    // scales and lightning intervals); changes blend smoothly over `Transition` seconds.
    // The boss calls SetPhase(0..2); `Preview Phase` lets you try them in Play mode from the Inspector.
    [DisallowMultipleComponent]
    public sealed class BossBackdropPhases : MonoBehaviour
    {
        public enum Kind
        {
            Brightness,         // SpriteRenderer colour (keeps alpha)
            Alpha,              // SpriteRenderer opacity
            RaiseY,             // Transform: units above its starting local position
            Position,           // Transform local position (Vectors)
            Scale,              // Transform local scale (Vectors)
            Drift,              // ParallaxLayer2D self-motion speed
            Emission,           // ParticleSystem particles per second
            LightningInterval,  // LightningFlash2D seconds between strikes (Vectors x..y)
            PulseLevel,         // Pulse2D opacity
            PulseSpeed,         // Pulse2D cycles per second
            SwayAmplitude,      // Sway2D units
            SwaySpeed,          // Sway2D cycles per second
            WindStrength,       // GustWind2D peak push (units/s²): the gale that pushes Alma
            WindCalm,           // GustWind2D seconds of calm between gusts
        }

        [Serializable]
        public class Track
        {
            public string Name;
            public Kind Kind;
            public Component Target;
            public float[] Values = new float[3];
            public Vector3[] Vectors = new Vector3[3];
            [NonSerialized] public Vector3 From, Current, Base;
        }

        [SerializeField, Min(0.01f)] private float _transition = 2.5f;
        [SerializeField] private Track[] _tracks = new Track[0];
        [SerializeField, Range(0, 2)] private int _previewPhase;
        [Tooltip("Mirrored when the wind turns (SetWindDirection): wind streaks, leaves... Drift tracks flip too.")]
        [SerializeField] private Transform[] _windFlip = new Transform[0];
        [SerializeField] private bool _previewWindLeft;

        private float _windSign = 1f;

        private float _blend = 1f;

        public int CurrentPhase { get; private set; }

        private void Awake()
        {
            CurrentPhase = Mathf.Clamp(_previewPhase, 0, 2);
            foreach (var t in _tracks)
            {
                if (t.Target == null) continue;
                t.Base = t.Target.transform.localPosition;
                t.Current = t.From = Target(t, CurrentPhase);
                Apply(t, t.Current);
            }
        }

        // +1 = wind blowing to the right, -1 = to the left (the boss turns it after each hit).
        public void SetWindDirection(float sign)
        {
            _windSign = sign < 0f ? -1f : 1f;
            foreach (var t in _windFlip)
            {
                if (t == null) continue;
                var s = t.localScale;
                s.x = Mathf.Abs(s.x) * _windSign;
                t.localScale = s;
            }
            foreach (var t in _tracks) if (t.Target != null && t.Kind == Kind.Drift) Apply(t, t.Current);
            foreach (var gale in GetComponentsInChildren<GustWind2D>()) gale.Side = _windSign;
        }

        public void SetPhase(int phase)
        {
            phase = Mathf.Clamp(phase, 0, 2);
            if (phase == CurrentPhase && _blend >= 1f) return;
            CurrentPhase = phase;
            foreach (var t in _tracks) t.From = t.Current;
            _blend = 0f;
        }

        private void Update()
        {
            if (_blend >= 1f) return;
            _blend = Mathf.MoveTowards(_blend, 1f, Time.deltaTime / _transition);
            float k = Mathf.SmoothStep(0f, 1f, _blend);
            foreach (var t in _tracks)
            {
                if (t.Target == null) continue;
                t.Current = Vector3.Lerp(t.From, Target(t, CurrentPhase), k);
                Apply(t, t.Current);
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || _tracks == null) return;
            SetPhase(_previewPhase);
            SetWindDirection(_previewWindLeft ? -1f : 1f);
        }

        private static bool UsesVectors(Kind kind) => kind == Kind.Position || kind == Kind.Scale || kind == Kind.LightningInterval;

        private static Vector3 Target(Track t, int phase) =>
            UsesVectors(t.Kind) ? t.Vectors[Mathf.Min(phase, t.Vectors.Length - 1)] : new Vector3(t.Values[Mathf.Min(phase, t.Values.Length - 1)], 0f, 0f);

        private void Apply(Track t, Vector3 v)
        {
            switch (t.Kind)
            {
                case Kind.Brightness when t.Target is SpriteRenderer sr: sr.color = new Color(v.x, v.x, v.x, sr.color.a); break;
                case Kind.Alpha when t.Target is SpriteRenderer sr: { var c = sr.color; c.a = v.x; sr.color = c; break; }
                case Kind.RaiseY: t.Target.transform.localPosition = t.Base + Vector3.up * v.x; break;
                case Kind.Position: t.Target.transform.localPosition = v; break;
                case Kind.Scale: t.Target.transform.localScale = v; break;
                case Kind.Drift when t.Target is ParallaxLayer2D layer: layer.Drift = v.x * _windSign; break;
                case Kind.Emission when t.Target is ParticleSystem ps: { var e = ps.emission; e.rateOverTime = v.x; break; }
                case Kind.LightningInterval when t.Target is LightningFlash2D l: l.Interval = new Vector2(v.x, v.y); break;
                case Kind.PulseLevel when t.Target is Pulse2D p: p.Level = v.x; break;
                case Kind.PulseSpeed when t.Target is Pulse2D p: p.Speed = v.x; break;
                case Kind.SwayAmplitude when t.Target is Sway2D w: w.Amplitude = v.x; break;
                case Kind.SwaySpeed when t.Target is Sway2D w: w.Speed = v.x; break;
                case Kind.WindStrength when t.Target is GustWind2D g: g.Strength = v.x; break;
                case Kind.WindCalm when t.Target is GustWind2D g: g.Calm = v.x; break;
            }
        }
    }
}
