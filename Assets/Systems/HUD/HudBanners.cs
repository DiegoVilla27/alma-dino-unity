using System.Collections.Generic;
using AlmaGame.Level;
using TMPro;
using UnityEngine;

namespace AlmaGame.Systems
{
    public enum BannerKind
    {
        Title,   // a centred moment: world completed, victory (kicker + headline + ornament + short text)
        Boss,    // boss presentation: cinema bars slide in, the name sits in the bottom bar, a hint in the top one
        Line,    // a spoken / thought line: egg rescue, a taunt (optional icon + text, like a subtitle)
        Shout,   // a short huge cry: «¡RUGE, ALMA!»
        Story,   // narration: cinema bars + dark veil, the lines fade in one after another (prologue, epilogue)
    }

    [System.Serializable]
    public struct Banner
    {
        public BannerKind Kind;
        public string Kicker;      // small spaced line above the headline (Title, Boss)
        public string Headline;    // the big words (Title, Boss, Shout)
        [TextArea] public string Text;
        public Color Accent;
        public Sprite Icon;        // optional (Line): e.g. the egg
        [Tooltip("Seconds on screen once shown; 0 = from the text length.")]
        public float Hold;
    }

    // The HUD's single banner layer: every on-screen text (eggs, bosses, prologue…) goes through here with one
    // shared style (HudText: TextMeshPro, two fonts, outline and shadow) — it appears, is read and goes away.
    // Banners queue, one at a time, on unscaled time, and stay visible during cinematics. Responsive: everything
    // is anchored to the safe area and scaled with Hud2D.TextScale (never under a legible real size); long texts wrap
    // or shrink, never leave the screen.
    // New abilities have their own paused presentation (HudAbilityButtons.Present), built from the same style.
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudBanners : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset _headlineFont;
        [SerializeField] private TMP_FontAsset _textFont;
        [SerializeField] private Sprite _veilSprite;
        [SerializeField] private Sprite _dividerSprite;
        [Tooltip("Reading time: seconds per character, on top of 1.6 s.")]
        [SerializeField, Min(0f)] private float _readTime = 0.045f;
        [SerializeField, Min(0.05f)] private float _fadeIn = 0.4f, _fadeOut = 0.5f;
        [Tooltip("Height of each cinema bar, as a fraction of the screen height.")]
        [SerializeField, Range(0.05f, 0.25f)] private float _barHeight = 0.14f;

        private class Active
        {
            public Banner B;
            public float StartedAt, Duration;
            public Transform Root;          // anchored + scaled content
            public Transform Bars;          // full-screen cinema bars (not scaled)
            public SpriteRenderer TopBar, BottomBar, Veil, Backdrop, Divider, Icon, Glow;
            public TextMeshPro Kicker, Headline;
            public readonly List<TextMeshPro> Lines = new List<TextMeshPro>();
            public readonly List<float> LineAt = new List<float>();
            public Vector3 HeadlineHome;
            public float BarH;
            public float HalfHeight;      // Line: half the height of its text block (design units)
            public bool Cinematic;
            public float DividerWidth;
        }

        public static HudBanners Instance { get; private set; }

        private Hud2D _hud;
        private readonly Queue<Banner> _queue = new Queue<Banner>();
        private Active _current;
        private float _now;

        public bool IsShowing => _current != null || _queue.Count > 0;
        public Sprite VeilSprite => _veilSprite;
        public Sprite DividerSprite => _dividerSprite;

        private void Awake()
        {
            Instance = this;
            _hud = GetComponent<Hud2D>();
            HudText.Configure(_headlineFont, _textFont);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_current != null && _current.Cinematic) CinematicState.End();
        }

        // Queues a banner. Returns false if it repeats the one showing or waiting.
        public bool Show(Banner banner)
        {
            if (_current != null && Same(_current.B, banner)) return false;
            foreach (var b in _queue) if (Same(b, banner)) return false;
            if (banner.Accent.a <= 0f) banner.Accent = Color.white;
            _queue.Enqueue(banner);
            return true;
        }

        // Shortcut; returns false when the scene has no HUD (callers then keep their own fallback text).
        public static bool Show(BannerKind kind, string headline, string text, Color accent, string kicker = null, Sprite icon = null)
        {
            if (Instance == null) return false;
            Instance.Show(new Banner { Kind = kind, Headline = headline, Text = text, Accent = accent, Kicker = kicker, Icon = icon });
            return true;
        }

        public void Clear()
        {
            _queue.Clear();
            if (_current != null) Finish(_current);
            _current = null;
        }

        private static bool Same(Banner a, Banner b) => a.Kind == b.Kind && a.Headline == b.Headline && a.Text == b.Text;

        private void LateUpdate()
        {
            _now = Time.unscaledTime;
            bool presenting = HudAbilityButtons.Instance != null && HudAbilityButtons.Instance.IsPresenting;
            if (_current == null && _queue.Count > 0 && !presenting) _current = Build(_queue.Dequeue());
            if (_current == null) return;
            if (!Animate(_current)) { Finish(_current); _current = null; }
        }

        private void Finish(Active a)
        {
            if (a.Cinematic) { a.Cinematic = false; CinematicState.End(); }
            if (a.Root != null) Destroy(a.Root.gameObject);
            if (a.Bars != null) Destroy(a.Bars.gameObject);
        }

        // ---------- building ----------

        private int Order => _hud.SortingOrder + 30;

        private Active Build(Banner b)
        {
            var a = new Active { B = b, StartedAt = _now };
            a.Root = new GameObject("Banner_" + b.Kind).transform;
            a.Root.SetParent(transform, false);
            float k = _hud.TextScale;
            a.BarH = Mathf.Min(16f * 0.22f, 16f * _barHeight * Mathf.Max(1f, k));   // bars grow with the text
            a.Root.localScale = Vector3.one * k;
            Rect safe = _hud.Safe;
            float availW = safe.width / k;          // design units available inside the safe area
            int o = Order;
            int chars = (b.Text?.Length ?? 0) + (b.Headline?.Length ?? 0);

            switch (b.Kind)
            {
                case BannerKind.Title:
                {
                    a.Root.localPosition = _hud.Anchor(new Vector2(0.5f, 0.6f), Vector2.zero);
                    a.Backdrop = NewSprite("Backdrop", a.Root, HazardFx.Glow(), o + 2, Color.clear);
                    a.Backdrop.transform.localScale = new Vector3(Mathf.Min(availW, 28f) / HazardFx.Glow().bounds.size.x * 1.3f, 9.5f / HazardFx.Glow().bounds.size.y, 1f);
                    float y = 0f;
                    if (!string.IsNullOrEmpty(b.Kicker))
                        a.Kicker = HudText.Create(a.Root, "Kicker", HudTextStyle.Kicker, b.Kicker.ToUpperInvariant(), 0.44f, Light(b.Accent), o + 5, Mathf.Min(availW - 2f, 20f), true);
                    a.Headline = HudText.Create(a.Root, "Headline", HudTextStyle.Headline, (b.Headline ?? "").ToUpperInvariant(), 1.25f, b.Accent, o + 6, Mathf.Min(availW - 2f, 24f), true);
                    HudText.Gradient(a.Headline, b.Accent);
                    if (a.Kicker != null) a.Kicker.transform.localPosition = new Vector3(0f, 1.85f, 0f);   // room for Á É Ó over the headline
                    a.HeadlineHome = new Vector3(0f, y, 0f);
                    a.Divider = NewSprite("Divider", a.Root, _dividerSprite, o + 4, b.Accent);
                    a.DividerWidth = Mathf.Min(availW - 4f, 11f);
                    a.Divider.transform.localPosition = new Vector3(0f, -1.15f, 0f);
                    if (!string.IsNullOrEmpty(b.Text))
                    {
                        var t = HudText.Create(a.Root, "Text", HudTextStyle.Body, b.Text, Hud2D.BodyCap * 0.95f, Color.white, o + 5, Mathf.Min(availW - 3f, 20f));
                        Below(t, -1.7f);
                        a.Lines.Add(t); a.LineAt.Add(0.55f);
                    }
                    a.Duration = Hold(b, chars);
                    break;
                }
                case BannerKind.Boss:
                {
                    BuildBars(a, false);
                    float barH = a.BarH;
                    // Name in the bottom bar (full width, not the safe area: the bars are part of the image).
                    a.Root.localPosition = new Vector3(safe.center.x, -8f + barH * 0.5f, 0f);
                    a.Headline = HudText.Create(a.Root, "Headline", HudTextStyle.Headline, (b.Headline ?? "").ToUpperInvariant(), Mathf.Min(1.1f, barH * 0.32f / k), b.Accent, o + 6, Mathf.Min(availW - 2f, 26f), true);
                    HudText.Gradient(a.Headline, b.Accent);
                    a.HeadlineHome = new Vector3(0f, -barH * 0.16f / k, 0f);
                    if (!string.IsNullOrEmpty(b.Kicker))
                    {
                        a.Kicker = HudText.Create(a.Root, "Kicker", HudTextStyle.Kicker, b.Kicker.ToUpperInvariant(), Mathf.Min(0.4f, barH * 0.13f / k), Light(b.Accent), o + 5, Mathf.Min(availW - 2f, 20f), true);
                        a.Kicker.transform.localPosition = new Vector3(0f, barH * 0.33f / k, 0f);
                    }
                    if (!string.IsNullOrEmpty(b.Text))
                    {
                        // The hint sits in the top bar.
                        var t = HudText.Create(a.Root, "Text", HudTextStyle.Body, b.Text, Mathf.Min(Hud2D.BodyCap * 0.9f, barH * 0.18f / k), Color.white, o + 5, Mathf.Min(availW - 2f, 26f), true);
                        t.transform.localPosition = new Vector3(0f, (16f - barH) / k, 0f);
                        a.Lines.Add(t); a.LineAt.Add(0.9f);
                    }
                    a.Duration = Hold(b, chars) + 0.6f;
                    break;
                }
                case BannerKind.Line:
                {
                    a.Root.localPosition = LineHome(a, 0f);
                    float iconW = b.Icon != null ? 1.6f : 0f;
                    var t = HudText.Create(a.Root, "Text", HudTextStyle.Body, b.Text ?? "", Hud2D.BodyCap, Color.white, o + 5, Mathf.Min(availW - 4f - iconW, 19f));
                    float w = HudText.Width(t), h = t.textBounds.size.y;
                    a.HalfHeight = Mathf.Max(h, b.Icon != null ? 1.2f : 0f) * 0.5f;
                    t.transform.localPosition = new Vector3(iconW * 0.5f, 0f, 0f);
                    a.Lines.Add(t); a.LineAt.Add(0f);
                    if (b.Icon != null)
                    {
                        a.Icon = NewSprite("Icon", a.Root, b.Icon, o + 5, Color.white);
                        a.Icon.transform.localScale = Vector3.one * (1.2f / b.Icon.bounds.size.y);
                        a.Icon.transform.localPosition = new Vector3(-w * 0.5f - 0.1f, 0f, 0f);
                        a.Glow = NewSprite("IconGlow", a.Root, HazardFx.Glow(), o + 4, Color.clear);
                        a.Glow.transform.localScale = Vector3.one * (2.4f / HazardFx.Glow().bounds.size.y);
                        a.Glow.transform.localPosition = a.Icon.transform.localPosition;
                    }
                    a.Backdrop = NewSprite("Backdrop", a.Root, HazardFx.Glow(), o + 2, Color.clear);
                    a.Backdrop.transform.localScale = new Vector3((w + iconW + 4f) / HazardFx.Glow().bounds.size.x * 1.25f, (h + 2.2f) / HazardFx.Glow().bounds.size.y * 1.25f, 1f);
                    a.Duration = Hold(b, chars);
                    break;
                }
                case BannerKind.Shout:
                {
                    a.Root.localPosition = _hud.Anchor(new Vector2(0.5f, 0.58f), Vector2.zero);
                    a.Glow = NewSprite("Glow", a.Root, HazardFx.Glow(), o + 3, Color.clear);
                    a.Headline = HudText.Create(a.Root, "Headline", HudTextStyle.Shout, (b.Headline ?? b.Text ?? "").ToUpperInvariant(), 2.0f, b.Accent, o + 6, Mathf.Min(availW - 6f, 20f), true);
                    HudText.Gradient(a.Headline, b.Accent);
                    a.Duration = b.Hold > 0f ? b.Hold : 1.6f;
                    break;
                }
                case BannerKind.Story:
                {
                    BuildBars(a, true);
                    a.Root.localPosition = new Vector3(safe.center.x, 0f, 0f);
                    float width = Mathf.Min(availW - 4f, 22f), at = _fadeIn + 0.3f, gap = 0.7f;
                    var made = new List<TextMeshPro>();
                    foreach (string para in (b.Text ?? "").Replace("\r", "").Split('\n'))
                    {
                        if (para.Trim().Length == 0) continue;
                        var t = HudText.Create(a.Root, "Line", HudTextStyle.Body, para.Trim(), Hud2D.BodyCap * 1.05f, new Color(1f, 0.95f, 0.85f), o + 5, width);
                        made.Add(t);
                        a.Lines.Add(t); a.LineAt.Add(at);
                        at += 0.9f + para.Length * _readTime * 0.6f;
                    }
                    // Stack the paragraphs centred on the screen; shrink them if they don't fit between the bars.
                    float total = -gap;
                    foreach (var t in made) { t.ForceMeshUpdate(); total += t.textBounds.size.y + gap; }
                    float room = (16f - 2f * a.BarH - 1f) / k;
                    float shrink = total > room ? room / total : 1f;
                    a.Root.localScale = Vector3.one * k * shrink;
                    float y = total * 0.5f;
                    foreach (var t in made)
                    {
                        float h = t.textBounds.size.y;
                        t.transform.localPosition = new Vector3(0f, y - h * 0.5f, 0f);
                        y -= h + gap;
                    }
                    a.Duration = at + 1.2f + (b.Hold > 0f ? b.Hold : 0f);
                    break;
                }
            }
            foreach (var t in a.Lines) HudText.Alpha(t, 0f);
            if (a.Kicker != null) HudText.Alpha(a.Kicker, 0f);
            if (a.Headline != null) HudText.Alpha(a.Headline, 0f);
            Animate(a);
            return a;
        }

        // Banners with cinema bars are cinematics: the rest of the HUD (eggs, buttons) fades out meanwhile.
        private void BuildBars(Active a, bool veil)
        {
            a.Cinematic = true;
            CinematicState.Begin();
            a.Bars = new GameObject("CinemaBars").transform;
            a.Bars.SetParent(transform, false);
            float halfW = 8f * (_hud.Camera != null ? _hud.Camera.aspect : 16f / 9f) + 1f;
            float barH = a.BarH;
            a.TopBar = NewSprite("Top", a.Bars, _veilSprite, Order + 1, Color.black);
            a.BottomBar = NewSprite("Bottom", a.Bars, _veilSprite, Order + 1, Color.black);
            foreach (var bar in new[] { a.TopBar, a.BottomBar })
                bar.transform.localScale = new Vector3(halfW * 2f / _veilSprite.bounds.size.x, barH / _veilSprite.bounds.size.y, 1f);
            if (veil)
            {
                a.Veil = NewSprite("Veil", a.Bars, _veilSprite, Order, new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0f));
                a.Veil.transform.localScale = new Vector3(halfW * 2f / _veilSprite.bounds.size.x, 18f / _veilSprite.bounds.size.y, 1f);
            }
        }

        // The line sits under the egg indicator (which uses the buttons' scale), centred in the safe area.
        // Its top edge stays 0.5 units under the eggs, however many lines it has.
        private Vector2 LineHome(Active a, float offset)
        {
            float eggs = HudAbilityButtons.Instance != null ? HudAbilityButtons.Instance.Scale : _hud.UiScale;
            float k = _hud.TextScale;
            Rect safe = _hud.Safe;
            return new Vector2(safe.center.x, safe.yMax - 1.75f * eggs - (0.5f + a.HalfHeight) * k + offset * k);
        }

        private static void Below(TextMeshPro t, float top)
        {
            t.ForceMeshUpdate();
            t.transform.localPosition = new Vector3(0f, top - t.textBounds.size.y * 0.5f, 0f);
        }

        private float Hold(Banner b, int chars) => b.Hold > 0f ? b.Hold : 1.6f + chars * _readTime;

        // ---------- animation ----------

        // Returns false when the banner has finished.
        private bool Animate(Active a)
        {
            float t = _now - a.StartedAt;
            float inT = Mathf.Clamp01(t / _fadeIn);
            float outT = Mathf.Clamp01((t - _fadeIn - a.Duration) / _fadeOut);
            float vis = Ease(inT) * (1f - outT);
            Color acc = a.B.Accent;

            // Cinema bars slide in from the edges and back out.
            if (a.Bars != null)
            {
                float barH = a.BarH;
                float slide = Ease(Mathf.Clamp01(t / 0.5f)) * (1f - Ease(outT));
                a.TopBar.transform.localPosition = new Vector3(0f, 8f + barH * 0.5f - barH * slide, 0f);
                a.BottomBar.transform.localPosition = new Vector3(0f, -8f - barH * 0.5f + barH * slide, 0f);
                if (a.Veil != null) a.Veil.color = new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0.85f * vis);
            }

            switch (a.B.Kind)
            {
                case BannerKind.Title:
                case BannerKind.Boss:
                {
                    float h = Mathf.Clamp01((t - (a.Bars != null ? 0.35f : 0.1f)) / 0.45f);
                    a.Headline.transform.localPosition = a.HeadlineHome;
                    a.Headline.transform.localScale = Vector3.one * Mathf.LerpUnclamped(1.25f, 1f, Back(h));
                    HudText.Alpha(a.Headline, Ease(h) * (1f - outT));
                    if (a.Kicker != null)
                    {
                        float kk = Ease(Mathf.Clamp01((t - 0.2f) / 0.4f));
                        HudText.Alpha(a.Kicker, kk * (1f - outT));
                        a.Kicker.characterSpacing = Mathf.Lerp(60f, 28f, kk);
                    }
                    if (a.Divider != null)
                    {
                        float d = Ease(Mathf.Clamp01((t - 0.45f) / 0.5f));
                        a.Divider.transform.localScale = new Vector3(Mathf.Max(0.001f, a.DividerWidth * d) / _dividerSprite.bounds.size.x, 0.55f / _dividerSprite.bounds.size.y, 1f);
                        a.Divider.color = new Color(acc.r, acc.g, acc.b, d * (1f - outT));
                    }
                    if (a.Backdrop != null) a.Backdrop.color = new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0.6f * vis);
                    FadeLines(a, t, outT);
                    if (a.B.Kind == BannerKind.Title)
                        a.Root.localPosition = _hud.Anchor(new Vector2(0.5f, 0.6f), new Vector2(0f, 0.3f * Ease(outT)));
                    break;
                }
                case BannerKind.Line:
                {
                    a.Root.localPosition = LineHome(a, -0.25f * (1f - Ease(inT)) + 0.2f * Ease(outT));
                    a.Backdrop.color = new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0.55f * vis);
                    foreach (var l in a.Lines) HudText.Alpha(l, vis);
                    if (a.Icon != null)
                    {
                        a.Icon.color = new Color(1f, 1f, 1f, vis);
                        float pulse = 0.5f + 0.15f * Mathf.Sin(_now * 3f);
                        a.Glow.color = new Color(acc.r, acc.g, acc.b, pulse * vis);
                    }
                    break;
                }
                case BannerKind.Shout:
                {
                    float p = Mathf.Clamp01(t / 0.25f);
                    float s = Mathf.LerpUnclamped(2.2f, 1f, Back(p)) * (1f + 0.03f * Mathf.Sin(_now * 9f)) * (1f + 0.12f * outT);
                    float shake = t > 0.2f ? Mathf.Clamp01(1f - (t - 0.2f) / 0.5f) * 0.1f : 0f;
                    a.Headline.transform.localScale = Vector3.one * s;
                    a.Headline.transform.localPosition = new Vector3(Random.Range(-shake, shake), Random.Range(-shake, shake), 0f);
                    HudText.Alpha(a.Headline, Mathf.Clamp01(p * 3f) * (1f - outT));
                    float flash = Mathf.Clamp01(1f - (t - 0.15f) / 0.5f);
                    float gw = Mathf.Min(HudText.Width(a.Headline) + 6f, 30f);
                    a.Glow.transform.localScale = new Vector3(gw / HazardFx.Glow().bounds.size.x * 1.2f, 6f / HazardFx.Glow().bounds.size.y, 1f) * (1f + 0.3f * flash);
                    a.Glow.color = new Color(acc.r, acc.g, acc.b, (0.35f + 0.5f * flash) * (1f - outT));
                    break;
                }
                case BannerKind.Story:
                    FadeLines(a, t, outT);
                    break;
            }
            return outT < 1f;
        }

        // Each line fades in (and rises a little) at its own time; all fade out together.
        private void FadeLines(Active a, float t, float outT)
        {
            for (int i = 0; i < a.Lines.Count; i++)
            {
                float l = Ease(Mathf.Clamp01((t - a.LineAt[i]) / 0.6f));
                HudText.Alpha(a.Lines[i], l * (1f - outT));
            }
        }

        // ---------- helpers ----------

        private static float Ease(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
        private static float Back(float t) { const float c = 1.7f; t -= 1f; return 1f + (c + 1f) * t * t * t + c * t * t; }
        private static Color Light(Color c) => Color.Lerp(c, Color.white, 0.45f);

        private static SpriteRenderer NewSprite(string name, Transform parent, Sprite sprite, int order, Color color)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = color;
            return sr;
        }
    }
}
