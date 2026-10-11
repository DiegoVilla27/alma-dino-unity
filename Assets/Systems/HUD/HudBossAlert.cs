using UnityEngine;

namespace AlmaGame.Systems
{
    // "The boss is vulnerable now!" — shared by every boss. When a boss opens a window to hit him (the Monkey
    // exhausted, the Armadillo dizzy…) it calls HudBossAlert.Show and, when the window closes, Hide:
    // - a Shout banner (e.g. «¡MAREADO!») so the player notices wherever he is;
    // - while the boss is off screen, a badge on the screen edge pointing at him: a disc with dizzy stars,
    //   an arrow towards him and a ring of dots that empties with the time left. On screen it hides (the
    //   boss's own stars show it). Responsive: kept inside the safe area, scaled with UiScale; fades with
    //   the HUD in cinematics. Added to the HUD by itself the first time it is used (nothing to set up).
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudBossAlert : MonoBehaviour
    {
        private const int RingDots = 20;
        private const float Radius = 0.75f;      // badge radius (design units)
        private const float EdgeInset = 0.35f;   // gap between the badge and the safe-area edge

        private static HudBossAlert s_instance;

        private Hud2D _hud;
        private Transform _root, _arrowPivot;
        private SpriteRenderer _disc, _rim, _arrow;
        private SpriteRenderer[] _stars, _dots;
        private System.Func<Vector2> _target;
        private float _startAt, _duration, _shown;
        private Color _accent;
        private bool _active;

        // Opens the alert: `target` returns the point to point at (world), `duration` the seconds of the window.
        public static void Show(System.Func<Vector2> target, float duration, string shout, Color accent)
        {
            var hud = Hud2D.Instance;
            if (hud == null) return;
            if (s_instance == null)
            {
                s_instance = hud.GetComponent<HudBossAlert>();
                if (s_instance == null) s_instance = hud.gameObject.AddComponent<HudBossAlert>();
            }
            s_instance.Open(target, duration, accent);
            if (!string.IsNullOrEmpty(shout)) HudBanners.Show(BannerKind.Shout, shout, null, accent);
        }

        // Closes it (the boss was hit, woke up, or the fight was reset).
        public static void Hide()
        {
            if (s_instance != null) s_instance._active = false;
        }

        private void Awake()
        {
            s_instance = this;
            _hud = GetComponent<Hud2D>();
            int o = _hud.SortingOrder + 30;
            _root = new GameObject("BossAlert").transform;
            _root.SetParent(transform, false);
            _arrowPivot = new GameObject("ArrowPivot").transform;
            _arrowPivot.SetParent(_root, false);
            _arrow = New("Arrow", _arrowPivot, Arrow(), o, new Vector3(Radius + 0.32f, 0f, 0f), 0.55f);
            _rim = New("Rim", _root, Disc(), o + 1, Vector3.zero, (Radius + 0.12f) * 2f);
            _disc = New("Disc", _root, Disc(), o + 2, Vector3.zero, Radius * 2f);
            _stars = new SpriteRenderer[3];
            for (int i = 0; i < 3; i++) _stars[i] = New("Star", _root, Star(), o + 4, Vector3.zero, 0.42f);
            _dots = new SpriteRenderer[RingDots];
            for (int i = 0; i < RingDots; i++)
            {
                float a = Mathf.PI / 2f - i * Mathf.PI * 2f / RingDots;   // clockwise from the top
                _dots[i] = New("Dot", _root, Disc(), o + 3, new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * (Radius - 0.14f), 0.13f);
            }
            SetAlpha(0f);
        }

        private void OnDestroy() { if (s_instance == this) s_instance = null; }

        private void Open(System.Func<Vector2> target, float duration, Color accent)
        {
            _target = target; _duration = Mathf.Max(0.1f, duration); _accent = accent;
            _startAt = Time.time; _active = true;
        }

        private SpriteRenderer New(string name, Transform parent, Sprite sprite, int order, Vector3 pos, float size)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.transform.localPosition = pos;
            sr.transform.localScale = Vector3.one * size / sprite.bounds.size.x;
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        private void LateUpdate()
        {
            float left = _active ? 1f - (Time.time - _startAt) / _duration : 0f;
            if (left <= 0f) _active = false;
            Camera cam = _hud.Camera;
            bool offScreen = false;
            Vector2 local = Vector2.zero, dir = Vector2.right;
            if (_active && _target != null && cam != null)
            {
                // Boss position in HUD units (the HUD follows the camera view and scales with it).
                Vector2 p = ((Vector3)_target() - transform.position) / Mathf.Max(0.0001f, transform.localScale.x);
                float k = _hud.UiScale, r = (Radius + 0.45f) * k + EdgeInset;
                Rect safe = _hud.Safe;
                Rect inner = Rect.MinMaxRect(safe.xMin + r, safe.yMin + r, safe.xMax - r, safe.yMax - r);
                offScreen = !Rect.MinMaxRect(safe.xMin - 0.5f, safe.yMin - 0.5f, safe.xMax + 0.5f, safe.yMax + 0.5f).Contains(p);
                if (offScreen)
                {
                    // Where the line from the screen centre to the boss meets the inner rectangle.
                    Vector2 c = inner.center, d = p - c;
                    float sx = d.x != 0f ? (inner.width * 0.5f) / Mathf.Abs(d.x) : float.MaxValue;
                    float sy = d.y != 0f ? (inner.height * 0.5f) / Mathf.Abs(d.y) : float.MaxValue;
                    local = c + d * Mathf.Min(sx, sy, 1f);
                    dir = d.normalized;
                }
            }
            _shown = Mathf.MoveTowards(_shown, offScreen ? 1f : 0f, Time.deltaTime / 0.2f);
            if (_shown <= 0f) { SetAlpha(0f); return; }

            float pulse = 1f + 0.06f * Mathf.Sin(Time.time * 8f) + (left < 0.3f ? 0.06f * Mathf.Sin(Time.time * 22f) : 0f);
            if (offScreen) _root.localPosition = local;
            _root.localScale = Vector3.one * _hud.UiScale * pulse * Mathf.Lerp(0.6f, 1f, _shown);
            _arrowPivot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            for (int i = 0; i < 3; i++)
            {
                float a = Time.time * 3f + i * Mathf.PI * 2f / 3f;
                _stars[i].transform.localPosition = new Vector3(Mathf.Cos(a) * 0.3f, Mathf.Sin(a) * 0.18f, 0f);
                _stars[i].transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 180f + i * 50f);
            }
            SetAlpha(_shown * _hud.Visibility, left);
        }

        private void SetAlpha(float a, float left = 0f)
        {
            _disc.color = new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0.85f * a);
            _rim.color = new Color(_accent.r, _accent.g, _accent.b, a);
            _arrow.color = new Color(_accent.r, _accent.g, _accent.b, a);
            foreach (var s in _stars) s.color = new Color(1f, 0.9f, 0.35f, a);
            for (int i = 0; i < _dots.Length; i++)
            {
                bool on = i < Mathf.CeilToInt(left * RingDots);   // the ring empties clockwise
                _dots[i].color = on ? new Color(1f, 1f, 1f, a) : new Color(1f, 1f, 1f, 0.15f * a);
            }
        }

        // ---------- sprites generated once ----------

        private static Sprite s_disc, s_arrow, s_star;

        private static Sprite Disc()
        {
            if (s_disc != null) return s_disc;
            const int n = 64; var tex = NewTex(n);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(n / 2f - d)));
            }
            tex.Apply();
            return s_disc = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        // Triangle pointing right.
        private static Sprite Arrow()
        {
            if (s_arrow != null) return s_arrow;
            const int n = 64; var tex = NewTex(n);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float fx = (x + 0.5f) / n, fy = Mathf.Abs((y + 0.5f) / n - 0.5f) * 2f;
                float edge = (1f - fx) - fy;   // inside when the half-height left at this x is bigger than |y|
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(edge * n * 0.5f)));
            }
            tex.Apply();
            return s_arrow = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        private static Sprite Star()
        {
            if (s_star != null) return s_star;
            const int n = 64; var tex = NewTex(n);
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++) { float r = i % 2 == 0 ? 30f : 13f, a = Mathf.PI / 2f + i * Mathf.PI / 5f; pts[i] = new Vector2(32f + Mathf.Cos(a) * r, 32f + Mathf.Sin(a) * r); }
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f); bool inside = false;
                for (int i = 0, j = 9; i < 10; j = i++)
                    if ((pts[i].y > p.y) != (pts[j].y > p.y) && p.x < (pts[j].x - pts[i].x) * (p.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x) inside = !inside;
                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
            tex.Apply();
            return s_star = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        private static Texture2D NewTex(int n) => new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
    }
}
