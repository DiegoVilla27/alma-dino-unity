using UnityEngine;

namespace AlmaGame.Systems
{
    // Tension vignette (GDD 8.4): the screen edges close in, darkened in the colour of the danger, when the
    // moment is tense — there is no health bar, so this is how danger is felt. Driven by TensionState.
    // - Low tension: only the corners darken a little.
    // - The more tension, the further the dark edge creeps in, with a layer of slowly turning smoke at its rim.
    // - From 45 % up it beats like a heart (lub-dub), faster the higher it goes, with a flash of its colour.
    // It eases in (0.7 s) and out slowly (1.6 s), covers the whole screen (any aspect) and sits under the rest of
    // the HUD. It ignores cinematic fades (narrative tension also lives in cinematics).
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudTensionVignette : MonoBehaviour
    {
        [SerializeField] private Sprite _vignetteSprite;
        [SerializeField] private Sprite _smokeSprite;
        [SerializeField, Min(0.01f)] private float _riseTime = 0.7f;
        [SerializeField, Min(0.01f)] private float _fallTime = 1.6f;
        [Tooltip("Tension from which the vignette starts to beat.")]
        [SerializeField, Range(0f, 1f)] private float _heartbeatFrom = 0.45f;
        [Tooltip("Beats per minute at the heartbeat threshold and at full tension.")]
        [SerializeField] private Vector2 _beatsPerMinute = new Vector2(62f, 120f);

        [Header("Preview (Play mode)")]
        [Tooltip("Forces a tension level to look at it (0 = off).")]
        [SerializeField, Range(0f, 1f)] private float _preview;
        [SerializeField] private Color _previewTint = new Color(0.42f, 0.07f, 0.02f);

        private Hud2D _hud;
        private SpriteRenderer _base, _smoke, _rim;
        private float _level, _beatPhase;
        private Color _tint = TensionState.Narrative;

        public float Level => _level;

        private void Awake()
        {
            _hud = GetComponent<Hud2D>();
            TensionState.Reset();
            int order = _hud.SortingOrder - 10;     // under the rest of the HUD
            _base = New("Vignette", _vignetteSprite, order);
            _smoke = New("VignetteSmoke", _smokeSprite, order + 1);
            _rim = New("VignetteRim", _vignetteSprite, order + 2);
        }

        private SpriteRenderer New(string name, Sprite sprite, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(transform, false);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = Color.clear;
            return sr;
        }

        private void OnDestroy() => TensionState.Reset();

        private void LateUpdate()
        {
            float target = TensionState.Current(out Color tint);
            if (_preview > 0f) { target = _preview; tint = _previewTint; }
            float dt = Time.unscaledDeltaTime;
            _level = Mathf.MoveTowards(_level, target, dt / (target > _level ? _riseTime : _fallTime));
            if (target > 0f) _tint = Color.Lerp(_tint, tint, 1f - Mathf.Exp(-dt * 3f));

            bool show = _level > 0.002f;
            _base.enabled = _smoke.enabled = _rim.enabled = show;
            if (!show) return;

            // Heartbeat: lub-dub, faster with tension.
            float beat = 0f;
            if (_level > _heartbeatFrom)
            {
                float k = Mathf.InverseLerp(_heartbeatFrom, 1f, _level);
                float bpm = Mathf.Lerp(_beatsPerMinute.x, _beatsPerMinute.y, k);
                _beatPhase = (_beatPhase + dt * bpm / 60f) % 1f;
                beat = (Pulse(_beatPhase, 0f) + 0.6f * Pulse(_beatPhase, 0.17f)) * Mathf.Lerp(0.35f, 1f, k);
            }

            // Cover the whole screen whatever its aspect (the sprites are square; stretch them).
            float halfW = 8f * (_hud.Camera != null ? _hud.Camera.aspect : 16f / 9f), halfH = 8f;
            float w = _vignetteSprite != null ? _vignetteSprite.bounds.size.x : 1f;
            float h = _vignetteSprite != null ? _vignetteSprite.bounds.size.y : 1f;
            // The dark edge creeps inwards as tension grows (smaller scale = more covered) and kicks on each beat.
            // (never smaller than the screen, so no edge is left uncovered).
            float reach = Mathf.Max(1.02f, Mathf.Lerp(1.5f, 1.0f, Ease(_level)) * (1f - 0.05f * beat));
            Vector3 size = new Vector3(halfW * 2f / w, halfH * 2f / h, 1f);
            _base.transform.localScale = Vector3.Scale(size, new Vector3(reach, reach, 1f));
            Color dark = Color.Lerp(_tint * 0.55f, Color.black, 0.35f);
            _base.color = new Color(dark.r, dark.g, dark.b, Mathf.Clamp01(0.3f + 0.65f * Ease(_level) + 0.08f * beat));

            // Smoke rim: turns slowly and breathes; a touch lighter than the edge so it reads as moving haze.
            float t = Time.unscaledTime;
            // Oversized (its own edge never shows) and swaying instead of spinning, so its corners stay off-screen.
            float smokeReach = reach * (1.55f + 0.03f * Mathf.Sin(t * 0.7f));
            _smoke.transform.localScale = Vector3.Scale(size, new Vector3(smokeReach, smokeReach, 1f));
            _smoke.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.35f) * 7f);
            Color haze = Color.Lerp(_tint, Color.black, 0.15f);
            _smoke.color = new Color(haze.r, haze.g, haze.b, 0.55f * Mathf.Clamp01(_level * 1.4f) * (0.85f + 0.15f * Mathf.Sin(t * 1.3f)));

            // Rim flash: on each beat the edge glows in the danger's colour.
            Color glow = Color.Lerp(_tint, Color.white, 0.25f) * 1.6f;
            _rim.transform.localScale = Vector3.Scale(size, new Vector3(reach * 1.04f, reach * 1.04f, 1f));
            _rim.color = new Color(Mathf.Clamp01(glow.r), Mathf.Clamp01(glow.g), Mathf.Clamp01(glow.b), 0.35f * beat);
        }

        // A soft pulse centred at `at` in the beat cycle.
        private static float Pulse(float phase, float at)
        {
            float d = Mathf.Repeat(phase - at + 0.5f, 1f) - 0.5f;
            return Mathf.Exp(-d * d / 0.0018f);
        }

        private static float Ease(float t) => 1f - (1f - t) * (1f - t);
    }
}
