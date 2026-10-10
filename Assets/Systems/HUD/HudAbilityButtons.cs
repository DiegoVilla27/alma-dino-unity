using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using TMPro;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Bottom-right action buttons laid out like a gamepad's face buttons: Jump (bottom), Dash (left), Ground
    // Pound (right) and Roar (top). They are touch buttons (the game ships on mobile first; a mouse click works
    // too, for testing) and also the HUD's record of Alma's powers: a locked ability is an empty stone socket;
    // an unlocked one shows its altar rune with a ring in its colour. Jump always works and gets the Double Jump
    // rune engraved once it is unlocked. Buttons flash when their action happens (any input) and the Dash
    // button dims while the air Dash is spent.
    // Responsive: anchored to the safe area's bottom-right corner, scaled with Hud2D.UiScale but never smaller
    // than a comfortable thumb size (_minButtonMillimetres) on devices that report their DPI.
    // New ability (Present): the game pauses, the screen darkens, the altar's rune flies to the centre and shines
    // over rotating rays, its name and a one-line hint appear (with the button it lives on); a tap continues,
    // and the rune flies to its button and is engraved there (pop, flash, shockwave, two heartbeats).
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudAbilityButtons : MonoBehaviour
    {
        [System.Serializable]
        public struct ButtonDef
        {
            public string Name;
            public AlmaAbility Ability;
            public bool IsJump;          // the Jump button: always usable; shows the Double Jump rune when unlocked
            public Vector2 Offset;       // from the cluster centre (design units)
            [Min(0.1f)] public float Radius;
            public Sprite Rune;
            public Color Color;
        }

        [SerializeField] private ButtonDef[] _buttons = new ButtonDef[0];
        [Tooltip("Cluster centre, from the safe area's bottom-right corner (design units, camera size 8).")]
        [SerializeField] private Vector2 _inset = new Vector2(2.15f, 2.35f);
        [SerializeField] private Sprite _baseSprite;
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private Sprite _jumpIcon;
        [SerializeField] private Sprite _shockwaveSprite;
        [Tooltip("Extra touch area around each button (design units), so thumbs don't miss.")]
        [SerializeField, Min(0f)] private float _touchMargin = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _lockedAlpha = 0.6f;
        [Tooltip("Smallest real size of the smaller buttons (diameter, mm) on devices that report their DPI.")]
        [SerializeField, Min(0f)] private float _minButtonMillimetres = 3f;

        [Header("New ability presentation")]
        [SerializeField] private Sprite _raysSprite;
        [Tooltip("Seconds before a tap can continue (so it isn't skipped by accident).")]
        [SerializeField, Min(0f)] private float _minPresentTime = 1.2f;
        [SerializeField, Min(0.1f)] private float _flightTime = 0.75f;

        private class Button
        {
            public ButtonDef Def;
            public Transform Root;
            public SpriteRenderer Base, Ring, Icon, Rune, Glow, Flash, Wave;
            public bool Unlocked;
            public float UnlockedAt = -100f, PressedAt = -100f;
            public bool Held;
            public bool Arriving;
        }

        private class Flight
        {
            public Button Button;
            public SpriteRenderer Rune, Glow;
            public Vector3 From;
            public float FromSize;
            public float StartedAt;
        }

        // The paused presentation of a new ability.
        private class Reveal
        {
            public Button Button;
            public Transform Root;
            public SpriteRenderer Veil, Rays, Glow, Rune, Flash, Divider;
            public SpriteRenderer MiniBase, MiniRing, MiniRune;
            public TextMeshPro Kicker, Name, Hint, Prompt;
            public Vector3 From;          // world position of the altar's rune
            public float FromSize;        // its world height
            public float StartedAt, LeavingAt = -1f;
            public float PreviousTimeScale = 1f;
        }

        public static HudAbilityButtons Instance { get; private set; }

        private Hud2D _hud;
        private AlmaMotor2D _alma;
        private readonly List<Button> _all = new List<Button>();
        private readonly List<Flight> _flights = new List<Flight>();
        private readonly Dictionary<int, Button> _fingers = new Dictionary<int, Button>();
        private ParticleSystem _sparkles, _trail;
        private bool _wasDashing, _wasPounding, _wasRoaring;
        private float _baseUnit = 1f;
        private float _scale = 1f;         // responsive scale of the cluster
        private Reveal _reveal;

        public bool IsPresenting => _reveal != null;
        // Responsive scale of the buttons (the egg indicator uses it too, so both read at the same size).
        public float Scale => _scale;
        private static float Now => Time.unscaledTime;

        private void Awake()
        {
            Instance = this;
            _hud = GetComponent<Hud2D>();
            int order = _hud.SortingOrder;
            if (_baseSprite != null) _baseUnit = 1f / _baseSprite.bounds.extents.x;   // scale for radius 1
            foreach (var def in _buttons)
            {
                var b = new Button { Def = def };
                b.Root = new GameObject("Button_" + def.Name).transform;
                b.Root.SetParent(transform, false);
                b.Glow = New("Glow", b.Root, HazardFx.Glow(), order);
                b.Base = New("Base", b.Root, _baseSprite, order + 1);
                b.Ring = New("Ring", b.Root, _ringSprite, order + 2);
                b.Icon = New("Icon", b.Root, _jumpIcon, order + 3);
                b.Rune = New("Rune", b.Root, def.Rune, order + 3);
                b.Flash = New("Flash", b.Root, HazardFx.Glow(), order + 4);
                b.Wave = New("Wave", b.Root, _shockwaveSprite, order + 5);
                _all.Add(b);
            }
            var material = new Material(Shader.Find("Sprites/Default"));
            _sparkles = HazardFx.CreateParticles("Sparkles", transform, material, HazardFx.Puff(), 120, 0, order + 50, false);
            var main = _sparkles.main;
            main.useUnscaledTime = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            var shape = _sparkles.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.3f;
            HazardFx.SetSizeOverLifetime(_sparkles, 1f, 0.1f);
            _trail = HazardFx.CreateParticles("RuneTrail", null, material, HazardFx.Puff(), 200, 0, order + 49, true);
            var tmain = _trail.main;
            tmain.useUnscaledTime = true;
            tmain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
            tmain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.6f);
            tmain.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.26f);
            HazardFx.SetSizeOverLifetime(_trail, 1f, 0.1f);
        }

        private void Start()
        {
            _alma = FindAnyObjectByType<AlmaMotor2D>();
            AlmaTouchControls.Clear();
            foreach (var b in _all) b.Unlocked = IsUnlocked(b);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            AlmaTouchControls.Clear();
            HazardFx.DestroyMaterial(_sparkles);
            HazardFx.DestroyMaterial(_trail);
            if (_trail != null) Destroy(_trail.gameObject);
            if (_reveal != null) Time.timeScale = _reveal.PreviousTimeScale > 0f ? _reveal.PreviousTimeScale : 1f;
        }

        private bool IsUnlocked(Button b)
        {
            AlmaAbility ability = b.Def.IsJump ? AlmaAbility.DoubleJump : b.Def.Ability;
            if (GameProgress.Instance != null) return GameProgress.Instance.IsUnlocked(ability);
            return _alma != null && _alma.IsUnlocked(ability);
        }

        private Button For(AlmaAbility ability) =>
            _all.Find(b => ability == AlmaAbility.DoubleJump ? b.Def.IsJump : (!b.Def.IsJump && b.Def.Ability == ability));

        private static SpriteRenderer New(string name, Transform parent, Sprite sprite, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        // ---------- responsive layout ----------

        private void UpdateScale()
        {
            float smallest = float.MaxValue;
            foreach (var b in _all) smallest = Mathf.Min(smallest, b.Def.Radius * 2f);
            float physical = _hud.ScaleForPhysicalSize(smallest, _minButtonMillimetres);
            // Never wider than 45 % or taller than 50 % of the safe area (narrow / portrait screens).
            Rect safe = _hud.Safe;
            float clusterW = 0f, clusterH = 0f;
            foreach (var b in _all)
            {
                clusterW = Mathf.Max(clusterW, Mathf.Abs(b.Def.Offset.x) + b.Def.Radius);
                clusterH = Mathf.Max(clusterH, Mathf.Abs(b.Def.Offset.y) + b.Def.Radius);
            }
            float fit = Mathf.Min(safe.width * 0.45f / Mathf.Max(0.1f, clusterW * 2f), safe.height * 0.5f / Mathf.Max(0.1f, clusterH * 2f));
            _scale = Mathf.Clamp(Mathf.Min(Mathf.Max(_hud.UiScale, physical), fit), 0.3f, 1.6f);
        }

        private Vector2 Position(Button b)
        {
            // The cluster keeps its inset in scaled units, so a bigger cluster moves inwards and never leaves the screen.
            Rect safe = _hud.Safe;
            return new Vector2(safe.xMax - _inset.x * _scale, safe.yMin + _inset.y * _scale) + b.Def.Offset * _scale;
        }

        private float Radius(Button b) => b.Def.Radius * _scale;

        // ---------- tutorials ----------

        // HUD-local position and radius of the button for `ability` (DoubleJump = the Jump button).
        public bool TryGetButton(AlmaAbility ability, out Vector2 localPosition, out float radius)
        {
            Button b = For(ability);
            localPosition = b != null ? Position(b) : Vector2.zero;
            radius = b != null ? Radius(b) : 0f;
            return b != null;
        }

        // Is the button usable now (unlocked; Jump always)?
        public bool IsUsable(AlmaAbility ability)
        {
            Button b = For(ability);
            return b != null && (b.Def.IsJump || b.Unlocked);
        }

        private Button _spotlit;
        public const int SpotlightBoost = 60;

        // Draws the button above a tutorial's dark veil (or back in place with `null`).
        public void Spotlight(AlmaAbility? ability)
        {
            Button b = ability.HasValue ? For(ability.Value) : null;
            if (b == _spotlit) return;
            if (_spotlit != null) foreach (var r in _spotlit.Root.GetComponentsInChildren<SpriteRenderer>(true)) r.sortingOrder -= SpotlightBoost;
            _spotlit = b;
            if (_spotlit != null) foreach (var r in _spotlit.Root.GetComponentsInChildren<SpriteRenderer>(true)) r.sortingOrder += SpotlightBoost;
        }

        // ---------- new ability ----------

        // An altar was taken: pause and present the ability, then engrave its rune on its button.
        // `worldHeight` is the rune's height on the altar (world units).
        public void Present(AlmaAbility ability, Vector3 worldPosition, float worldHeight, string title, string hint)
        {
            Button b = For(ability);
            if (b == null) return;
            if (_reveal != null) EndReveal(true);
            b.Arriving = true;
            var r = new Reveal { Button = b, From = worldPosition, FromSize = worldHeight, StartedAt = Now, PreviousTimeScale = Time.timeScale };
            Time.timeScale = 0f;
            AlmaTouchControls.Clear();
            AlmaTouchControls.InputLocked = true;
            ReleaseAll();

            int o = _hud.SortingOrder + 40;
            Color c = b.Def.Color;
            var banners = HudBanners.Instance;
            r.Root = new GameObject("AbilityReveal").transform;
            r.Root.SetParent(transform, false);
            Sprite veil = banners != null ? banners.VeilSprite : null;
            r.Veil = New("Veil", transform, veil, o);
            float halfW = 8f * (_hud.Camera != null ? _hud.Camera.aspect : 16f / 9f) + 1f;
            if (veil != null) r.Veil.transform.localScale = new Vector3(halfW * 2f / veil.bounds.size.x, 18f / veil.bounds.size.y, 1f);
            r.Veil.color = Color.clear;
            r.Veil.transform.SetParent(r.Root, true);

            var content = new GameObject("Content").transform;
            content.SetParent(r.Root, false);
            r.Rays = New("Rays", content, _raysSprite, o + 2);
            r.Glow = New("Glow", content, HazardFx.Glow(), o + 3);
            r.Rune = New("Rune", null, b.Def.Rune, o + 5);          // flies in world space
            r.Flash = New("Flash", content, HazardFx.Glow(), o + 6);
            if (HudText.Ready)
            {
                r.Kicker = HudText.Create(content, "Kicker", HudTextStyle.Kicker, "¡HABILIDAD DESPERTADA!", 0.44f, Color.Lerp(c, Color.white, 0.5f), o + 7, 20f, true);
                r.Name = HudText.Create(content, "Name", HudTextStyle.Headline, (title ?? "").ToUpperInvariant(), 1.3f, c, o + 8, 22f, true);
                HudText.Gradient(r.Name, c);
                r.Hint = HudText.Create(content, "Hint", HudTextStyle.Body, hint ?? "", Hud2D.BodyCap, Color.white, o + 7, 17f);
                r.Prompt = HudText.Create(content, "Prompt", HudTextStyle.Prompt, "Toca para continuar", 0.38f, new Color(1f, 1f, 1f, 0.85f), o + 7, 12f, true);
                foreach (var t in new[] { r.Kicker, r.Name, r.Hint, r.Prompt }) HudText.Alpha(t, 0f);
            }
            if (banners != null && banners.DividerSprite != null) r.Divider = New("Divider", content, banners.DividerSprite, o + 6);
            // The button it lives on, next to the hint.
            r.MiniBase = New("MiniBase", content, _baseSprite, o + 6);
            r.MiniRing = New("MiniRing", content, _ringSprite, o + 7);
            r.MiniRune = New("MiniRune", content, b.Def.Rune, o + 8);
            var main = _trail.main;
            main.startColor = new ParticleSystem.MinMaxGradient(c, Color.white);
            _reveal = r;
            LayoutReveal(r);
        }

        // Design units inside the reveal's content (scaled by Hud2D.TextScale, centred in the safe area).
        private const float RuneY = 2.9f, RuneHeight = 3.0f, KickerY = 0.85f, NameY = -0.75f, DividerY = -1.9f, HintY = -2.75f;

        private void LayoutReveal(Reveal r)
        {
            float k = _hud.TextScale;
            Transform content = r.Root.Find("Content");
            content.localPosition = _hud.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0f, 0.4f));
            content.localScale = Vector3.one * k;
            float availW = _hud.Safe.width / k;
            if (r.Name != null)
            {
                r.Kicker.rectTransform.sizeDelta = new Vector2(Mathf.Min(availW - 2f, 20f), r.Kicker.rectTransform.sizeDelta.y);
                r.Name.rectTransform.sizeDelta = new Vector2(Mathf.Min(availW - 2f, 22f), r.Name.rectTransform.sizeDelta.y);
                r.Hint.rectTransform.sizeDelta = new Vector2(Mathf.Min(availW - 5f, 17f), r.Hint.rectTransform.sizeDelta.y);
                r.Kicker.transform.localPosition = new Vector3(0f, KickerY, 0f);
                r.Name.transform.localPosition = new Vector3(0f, NameY, 0f);
                r.Hint.ForceMeshUpdate();
                float hintW = r.Hint.textBounds.size.x, hintH = r.Hint.textBounds.size.y;
                r.Hint.transform.localPosition = new Vector3(0.75f, HintY - hintH * 0.5f + 0.25f, 0f);
                Vector3 mini = new Vector3(0.75f - hintW * 0.5f - 0.95f, HintY - hintH * 0.5f + 0.25f, 0f);
                r.MiniBase.transform.localPosition = r.MiniRing.transform.localPosition = r.MiniRune.transform.localPosition = mini;
                float promptY = (_hud.Safe.yMin - content.localPosition.y) / k + 0.8f;
                r.Prompt.transform.localPosition = new Vector3(0f, promptY, 0f);
            }
            if (r.Divider != null) r.Divider.transform.localPosition = new Vector3(0f, DividerY, 0f);
            r.Rays.transform.localPosition = r.Glow.transform.localPosition = r.Flash.transform.localPosition = new Vector3(0f, RuneY, 0f);
        }

        // Tap to continue (also called by tests and, later, by a gamepad button).
        public void ContinueReveal()
        {
            if (_reveal == null || _reveal.LeavingAt >= 0f) return;
            if (Now - _reveal.StartedAt < _minPresentTime) return;
            _reveal.LeavingAt = Now;
            // The rune leaves the centre for its button.
            Vector3 centre = _reveal.Rune.transform.position;
            float size = _reveal.Rune.transform.localScale.y;
            var f = new Flight { Button = _reveal.Button, From = centre, FromSize = size, StartedAt = Now };
            f.Glow = New("FlyingGlow", null, HazardFx.Glow(), _hud.SortingOrder + 48);
            f.Rune = _reveal.Rune;
            _reveal.Rune = null;
            _flights.Add(f);
        }

        private void EndReveal(bool immediate)
        {
            var r = _reveal;
            _reveal = null;
            if (r.Rune != null) Destroy(r.Rune.gameObject);
            if (r.Root != null) Destroy(r.Root.gameObject);
            Time.timeScale = r.PreviousTimeScale > 0f ? r.PreviousTimeScale : 1f;
            AlmaTouchControls.Clear();
            if (immediate) { r.Button.Arriving = false; r.Button.Unlocked = true; r.Button.UnlockedAt = Now; }
        }

        private void AnimateReveal(Reveal r)
        {
            LayoutReveal(r);
            float t = Now - r.StartedAt;
            float leave = r.LeavingAt < 0f ? 0f : Mathf.Clamp01((Now - r.LeavingAt) / 0.45f);
            float k = _hud.TextScale;
            Color c = r.Button.Def.Color;
            Transform content = r.Root.Find("Content");
            Vector3 centreWorld = content.TransformPoint(new Vector3(0f, RuneY, 0f));
            float runeH = r.Button.Def.Rune != null ? r.Button.Def.Rune.bounds.size.y : 1f;
            float bigSize = RuneHeight * k * _hud.Scale / runeH;    // world scale of the rune at the centre

            // Veil.
            float veil = Ease(Mathf.Clamp01(t / 0.4f)) * (1f - Ease(Mathf.Clamp01((Now - (r.LeavingAt < 0f ? float.MaxValue : r.LeavingAt + 0.25f)) / 0.5f)));
            r.Veil.color = new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0.8f * veil);

            // Rune: breaks free from the altar and arcs to the centre (0.6 s), then floats and shines.
            if (r.Rune != null)
            {
                float a = Mathf.Clamp01(t / 0.6f), e = a * a * (3f - 2f * a);
                Vector3 control = Vector3.Lerp(r.From, centreWorld, 0.5f) + Vector3.up * (1.5f * _hud.Scale);
                Vector3 p = Vector3.Lerp(Vector3.Lerp(r.From, control, e), Vector3.Lerp(control, centreWorld, e), e);
                if (a >= 1f) p = centreWorld + Vector3.up * (Mathf.Sin((t - 0.6f) * 2f) * 0.12f * k * _hud.Scale);
                float fromScale = r.FromSize / runeH;
                float size = Mathf.Lerp(fromScale, bigSize, e);
                float spin = a < 1f ? Mathf.Cos(e * Mathf.PI * 4f) : 1f;
                r.Rune.transform.position = p;
                r.Rune.transform.localScale = new Vector3(size * Mathf.Max(0.12f, Mathf.Abs(spin)), size, 1f);
                if (a < 1f) { _trail.transform.position = p; _trail.Emit(3); }
                if (a >= 1f && t - Time.unscaledDeltaTime < 0.6f)
                {
                    _sparkles.transform.position = p;
                    var main = _sparkles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(c, Color.white);
                    _sparkles.Emit(40);
                }
            }

            float arrived = Mathf.Clamp01((t - 0.6f) / 0.5f);
            float fade = 1f - leave;
            float flash = t > 0.6f ? Mathf.Clamp01(1f - (t - 0.6f) / 0.6f) : 0f;
            float raysUnit = _raysSprite != null ? 1f / _raysSprite.bounds.size.y : 1f;
            r.Rays.transform.localScale = Vector3.one * (8.5f * raysUnit * (0.6f + 0.4f * Ease(arrived)));
            r.Rays.transform.localRotation = Quaternion.Euler(0f, 0f, Now * 12f);
            r.Rays.color = new Color(c.r, c.g, c.b, 0.55f * Ease(arrived) * fade);
            float glowUnit = 1f / HazardFx.Glow().bounds.size.y;
            r.Glow.transform.localScale = Vector3.one * (5.5f * glowUnit * (1f + 0.06f * Mathf.Sin(Now * 3f)));
            r.Glow.color = new Color(c.r, c.g, c.b, 0.6f * Ease(arrived) * fade);
            r.Flash.transform.localScale = Vector3.one * ((3f + 6f * (1f - flash)) * glowUnit);
            r.Flash.color = new Color(1f, 1f, 1f, 0.9f * flash);

            if (r.Name != null)
            {
                float kk = Ease(Mathf.Clamp01((t - 0.75f) / 0.4f));
                HudText.Alpha(r.Kicker, kk * fade);
                r.Kicker.characterSpacing = Mathf.Lerp(70f, 28f, kk);
                float n = Mathf.Clamp01((t - 0.85f) / 0.45f);
                r.Name.transform.localScale = Vector3.one * Mathf.LerpUnclamped(1.35f, 1f, Back(n));
                HudText.Alpha(r.Name, Ease(n) * fade);
                float h = Ease(Mathf.Clamp01((t - 1.25f) / 0.45f));
                HudText.Alpha(r.Hint, h * fade);
                float ready = Mathf.Clamp01((t - _minPresentTime) / 0.4f);
                HudText.Alpha(r.Prompt, ready * (0.55f + 0.45f * Mathf.Sin(Now * 3.5f)) * fade);
                float miniUnit = _baseSprite != null ? 0.55f / _baseSprite.bounds.extents.x : 1f;
                r.MiniBase.transform.localScale = r.MiniRing.transform.localScale = Vector3.one * miniUnit;
                r.MiniBase.color = new Color(1f, 1f, 1f, h * fade);
                r.MiniRing.color = new Color(c.r, c.g, c.b, h * fade);
                float miniRune = r.Button.Def.Rune != null ? 0.72f / runeH : 1f;
                r.MiniRune.transform.localScale = Vector3.one * miniRune;
                r.MiniRune.color = new Color(1f, 1f, 1f, h * fade);
            }
            if (r.Divider != null)
            {
                float d = Ease(Mathf.Clamp01((t - 1.05f) / 0.5f));
                float width = Mathf.Min(_hud.Safe.width / k - 4f, 11f) * d;
                r.Divider.transform.localScale = new Vector3(Mathf.Max(0.001f, width) / r.Divider.sprite.bounds.size.x, 0.55f / r.Divider.sprite.bounds.size.y, 1f);
                r.Divider.color = new Color(c.r, c.g, c.b, d * fade);
            }
        }

        // ---------- input ----------

        private void Update()
        {
            if (_reveal != null)
            {
                bool tap = Input.GetMouseButtonDown(0) || Input.anyKeyDown;
                for (int i = 0; i < Input.touchCount; i++) tap |= Input.GetTouch(i).phase == TouchPhase.Began;
                if (tap) ContinueReveal();
                return;
            }
            if (_hud.Visibility < 0.5f) { ReleaseAll(); return; }   // hidden (cinematic): no touches
            UpdateScale();
            var seen = new HashSet<int>();
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                seen.Add(t.fingerId);
                if (t.phase == TouchPhase.Began) Press(t.fingerId, t.position);
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) Release(t.fingerId);
            }
            // Mouse stands in for a finger when there is no touch screen (editor, PC).
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0)) Press(-1, Input.mousePosition);
                if (Input.GetMouseButtonUp(0)) Release(-1);
                if (Input.GetMouseButton(0)) seen.Add(-1);
            }
            var stale = new List<int>();
            foreach (var id in _fingers.Keys) if (!seen.Contains(id)) stale.Add(id);
            foreach (var id in stale) Release(id);
        }

        private void Press(int finger, Vector2 screen)
        {
            Vector2 p = _hud.ScreenToLocal(screen);
            foreach (var b in _all)
            {
                if (Vector2.Distance(p, Position(b)) > Radius(b) + _touchMargin * _scale) continue;
                if (!b.Def.IsJump && !b.Unlocked) return;          // a locked power does nothing
                _fingers[finger] = b;
                b.Held = true;
                b.PressedAt = Now;
                if (b.Def.IsJump) AlmaTouchControls.PressJump();
                else if (b.Def.Ability == AlmaAbility.GroundPound) AlmaTouchControls.PressPound();
                else if (b.Def.Ability == AlmaAbility.Dash) AlmaTouchControls.PressDash();
                else if (b.Def.Ability == AlmaAbility.Roar) AlmaTouchControls.PressRoar();
                return;
            }
        }

        private void Release(int finger)
        {
            if (!_fingers.TryGetValue(finger, out Button b)) return;
            _fingers.Remove(finger);
            b.Held = _fingers.ContainsValue(b);
            if (b.Def.IsJump && !b.Held) AlmaTouchControls.ReleaseJump();
        }

        private void ReleaseAll()
        {
            if (_fingers.Count == 0) return;
            _fingers.Clear();
            foreach (var b in _all) b.Held = false;
            AlmaTouchControls.ReleaseJump();
        }

        // ---------- drawing ----------

        private void LateUpdate()
        {
            if (_alma == null) _alma = FindAnyObjectByType<AlmaMotor2D>();
            UpdateScale();
            DetectActions();
            float vis = _hud.Visibility;
            foreach (var b in _all) Draw(b, vis);
            if (_reveal != null) AnimateReveal(_reveal);
            UpdateFlights();
        }

        // Flash the matching button whenever an action happens, whatever the input (keyboard, pad, touch).
        private void DetectActions()
        {
            if (_alma == null) return;
            if (_alma.IsDashing && !_wasDashing) Flash(AlmaAbility.Dash);
            if (_alma.IsGroundPounding && !_wasPounding) Flash(AlmaAbility.GroundPound);
            if (_alma.IsRoaring && !_wasRoaring) Flash(AlmaAbility.Roar);
            _wasDashing = _alma.IsDashing; _wasPounding = _alma.IsGroundPounding; _wasRoaring = _alma.IsRoaring;
        }

        private void Flash(AlmaAbility ability)
        {
            Button b = For(ability);
            if (b != null && Now - b.PressedAt > 0.15f) b.PressedAt = Now;
        }

        private void Draw(Button b, float vis)
        {
            b.Root.localPosition = Position(b);
            float r = Radius(b);
            bool lit = b.Unlocked && !b.Arriving;
            float since = Now - b.UnlockedAt;
            float press = Mathf.Clamp01(1f - (Now - b.PressedAt) / 0.18f);
            float pop = lit && since < 0.5f ? 1f + 0.5f * Mathf.Sin(since / 0.5f * Mathf.PI) * (1f - since / 0.5f) : 1f;
            float beat = 0f;
            if (lit)
                for (int k = 0; k < 2; k++)
                {
                    float t = since - 0.65f - k * 0.55f;
                    if (t > 0f && t < 0.3f) beat = Mathf.Max(beat, Mathf.Sin(t / 0.3f * Mathf.PI));
                }
            float scale = r * pop * (1f + 0.1f * beat) * (b.Held ? 0.9f : 1f - 0.08f * press);

            bool usable = b.Def.IsJump || lit;
            bool dim = !b.Def.IsJump && b.Def.Ability == AlmaAbility.Dash && lit && _alma != null && !_alma.DashCharged;
            float alpha = (usable ? (dim ? 0.55f : 0.92f) : _lockedAlpha) * vis;

            b.Base.transform.localScale = Vector3.one * (_baseUnit * scale);
            b.Base.color = new Color(1f, 1f, 1f, alpha);
            Color c = b.Def.Color;
            b.Ring.transform.localScale = b.Base.transform.localScale;
            b.Ring.color = new Color(c.r, c.g, c.b, lit ? alpha : 0f);

            // Jump: plain arrow until the Double Jump rune is engraved over it.
            b.Icon.enabled = b.Def.IsJump && !lit;
            if (b.Icon.enabled) { b.Icon.transform.localScale = Vector3.one * (_baseUnit * scale * 0.55f); b.Icon.color = new Color(1f, 1f, 1f, alpha); }

            b.Rune.enabled = lit;
            if (lit && b.Def.Rune != null)
            {
                float runeH = b.Def.Rune.bounds.size.y;
                b.Rune.transform.localScale = Vector3.one * (scale * 1.25f / runeH);
                b.Rune.color = new Color(1f, 1f, 1f, (dim ? 0.55f : 1f) * vis);
            }
            float glow = lit ? 0.3f + 0.08f * Mathf.Sin(Now * 2f + r) + 0.4f * beat + 0.5f * press + (since < 1f ? 0.7f * (1f - since) : 0f) : 0f;
            if (dim) glow *= 0.3f;
            b.Glow.transform.localScale = Vector3.one * (scale * 3.2f);
            b.Glow.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(glow) * 0.55f * vis);

            float flash = lit && since < 0.5f ? 1f - since / 0.5f : 0f;
            b.Flash.transform.localScale = Vector3.one * (scale * (2f + 2.5f * (1f - flash)));
            b.Flash.color = new Color(1f, 1f, 1f, flash * 0.9f * vis);

            float wave = lit && since < 0.7f ? since / 0.7f : 1f;
            b.Wave.enabled = wave < 1f;
            if (b.Wave.enabled)
            {
                b.Wave.transform.localScale = Vector3.one * (_baseUnit * r * (1f + 2.2f * wave));
                b.Wave.color = new Color(c.r, c.g, c.b, (1f - wave) * vis);
            }
        }

        // Rune flying from the centre of the screen to its button; on arrival it is engraved and the game resumes.
        private void UpdateFlights()
        {
            float hudScale = _hud.Scale;
            for (int i = _flights.Count - 1; i >= 0; i--)
            {
                var f = _flights[i];
                float t = Mathf.Clamp01((Now - f.StartedAt) / _flightTime);
                float e = t * t * (3f - 2f * t);
                Vector3 to = _hud.ToWorld(Position(f.Button));
                Vector3 control = Vector3.Lerp(f.From, to, 0.35f) + Vector3.up * (1.6f * hudScale);
                Vector3 p = Vector3.Lerp(Vector3.Lerp(f.From, control, e), Vector3.Lerp(control, to, e), e);
                float runeH = f.Button.Def.Rune != null ? f.Button.Def.Rune.bounds.size.y : 1f;
                float end = Radius(f.Button) * 1.25f * hudScale / runeH;
                float size = Mathf.Lerp(f.FromSize, end, e);
                f.Rune.transform.position = p;
                f.Rune.transform.localScale = Vector3.one * size;
                f.Glow.transform.position = p;
                f.Glow.transform.localScale = Vector3.one * (size * runeH * 2.2f);
                Color c = f.Button.Def.Color;
                f.Glow.color = new Color(c.r, c.g, c.b, 0.75f);
                _trail.transform.position = p;
                _trail.Emit(3);
                if (t >= 1f)
                {
                    Destroy(f.Rune.gameObject); Destroy(f.Glow.gameObject);
                    _flights.RemoveAt(i);
                    f.Button.Arriving = false;
                    f.Button.Unlocked = true;
                    f.Button.UnlockedAt = Now;
                    _sparkles.transform.position = to;
                    var main = _sparkles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(c, Color.white);
                    _sparkles.Emit(30);
                    if (_reveal != null && _reveal.Button == f.Button) EndReveal(false);
                }
            }
        }

        private static float Ease(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
        private static float Back(float t) { const float c = 1.7f; t -= 1f; return 1f + (c + 1f) * t * t * t + c * t * t; }
    }
}
