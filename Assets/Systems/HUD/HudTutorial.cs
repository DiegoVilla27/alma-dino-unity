using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using TMPro;
using UnityEngine;

namespace AlmaGame.Systems
{
    // What a tutorial prompt teaches and where (TutorialPrompt2D builds it).
    public struct TutorialRequest
    {
        public AlmaAbility Action;      // DoubleJump = jump in the air (Jump button)
        public string Title;            // «¡SALTA!»
        public string Text;             // «Toca SALTAR para cruzar el hueco.»
        public Color Accent;
        public Vector3 ArcFrom, ArcTo;  // world: the move to show (from Alma's feet to the landing)
        public float ArcHeight;         // apex above the higher end
        public bool ShowArc;
        public Vector2 Focus;           // world point the camera frames
        public float Zoom;              // orthographic size of the shot
        public bool AssistMove;         // after the press, keep Alma moving towards ArcTo until she lands
    }

    // Epic in-game tutorial (HUD): time slows to a stop, the camera closes in on the obstacle, the screen darkens
    // around it, a dotted arc shows the move, the button to press lights up with a pointing arrow and the title
    // and hint appear. Pressing that button (touch, keyboard or pad) resumes the game instantly and the move
    // happens right then. Runs on unscaled time.
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudTutorial : MonoBehaviour
    {
        [Tooltip("HUD_Spotlight: dark with a soft clear circle in the middle.")]
        [SerializeField] private Sprite _vignetteSprite;
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private Sprite _shockwaveSprite;
        [SerializeField] private Sprite _arrowSprite;
        [SerializeField, Min(0.05f)] private float _slowTime = 0.35f;
        [SerializeField, Min(0.05f)] private float _cameraTime = 0.6f;
        [SerializeField, Min(0.05f)] private float _leaveTime = 0.45f;
        [Tooltip("Seconds before a press is accepted (so a held button doesn't skip it).")]
        [SerializeField, Min(0f)] private float _minShowTime = 0.4f;

        public static HudTutorial Instance { get; private set; }
        public bool IsShowing => _active;
        public event System.Action Finished;

        private Hud2D _hud;
        private AlmaCameraFollow _camera;
        private AlmaMotor2D _alma;
        private bool _active, _leaving;
        private float _startedAt, _leftAt, _prevTimeScale = 1f;
        private TutorialRequest _r;
        private Transform _root;
        private SpriteRenderer _vignette, _ring, _wave, _arrow, _landing;
        private TextMeshPro _title, _text;
        private readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>();
        private SpriteRenderer _comet;
        private float _assistUntil;
        private float _assistDir;

        private void Awake()
        {
            Instance = this;
            _hud = GetComponent<Hud2D>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_active) { Time.timeScale = _prevTimeScale > 0f ? _prevTimeScale : 1f; AlmaTouchControls.InputLocked = false; }
            Cleanup();
        }

        public bool Show(TutorialRequest r)
        {
            if (_active) return false;
            var buttons = HudAbilityButtons.Instance;
            if (buttons == null || !buttons.IsUsable(r.Action)) return false;
            _camera = FindAnyObjectByType<AlmaCameraFollow>();
            _alma = FindAnyObjectByType<AlmaMotor2D>();
            _r = r;
            if (_r.Accent.a <= 0f) _r.Accent = new Color(0.55f, 1f, 0.4f);
            _active = true; _leaving = false;
            _startedAt = Time.unscaledTime;
            _prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            AlmaTouchControls.Clear();
            AlmaTouchControls.InputLocked = true;
            if (HudBanners.Instance != null) HudBanners.Instance.Clear();   // nothing else on screen while teaching
            Build();
            buttons.Spotlight(r.Action);
            return true;
        }

        // ---------- building ----------

        private SpriteRenderer New(string name, Transform parent, Sprite sprite, int order, Color color)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.sprite = sprite; sr.sortingOrder = order; sr.color = color;
            return sr;
        }

        private int Order => _hud.SortingOrder;

        private void Build()
        {
            Cleanup();
            _root = new GameObject("Tutorial").transform;
            _root.SetParent(transform, false);
            _vignette = New("Vignette", _root, _vignetteSprite, Order + 35, Color.clear);
            _ring = New("ButtonRing", _root, _ringSprite, Order + HudAbilityButtons.SpotlightBoost + 8, Color.clear);
            _wave = New("ButtonWave", _root, _shockwaveSprite, Order + HudAbilityButtons.SpotlightBoost + 7, Color.clear);
            _arrow = New("Arrow", _root, _arrowSprite, Order + HudAbilityButtons.SpotlightBoost + 12, Color.clear);
            if (HudText.Ready)
            {
                _title = HudText.Create(_root, "Title", HudTextStyle.Headline, (_r.Title ?? "").ToUpperInvariant(), 1.35f, _r.Accent, Order + 50, 20f, true);
                HudText.Gradient(_title, _r.Accent);
                _text = HudText.Create(_root, "Text", HudTextStyle.Body, _r.Text ?? "", Hud2D.BodyCap, Color.white, Order + 50, 15f);
                foreach (var t in new[] { _title, _text }) HudText.Alpha(t, 0f);
            }
            if (_r.ShowArc)
            {
                // World-space dotted arc + landing marker, drawn over the level but under the HUD.
                for (int i = 0; i < 22; i++) _dots.Add(New("ArcDot", null, HazardFx.Glow(), 45, Color.clear));
                _comet = New("ArcComet", null, HazardFx.Glow(), 46, Color.clear);
                _landing = New("Landing", null, _ringSprite, 44, Color.clear);
            }
        }

        private void Cleanup()
        {
            if (_root != null) Destroy(_root.gameObject);
            foreach (var d in _dots) if (d != null) Destroy(d.gameObject);
            _dots.Clear();
            if (_comet != null) Destroy(_comet.gameObject);
            if (_landing != null) Destroy(_landing.gameObject);
        }

        // ---------- input ----------

        private bool Pressed(out bool fromKeyboard)
        {
            fromKeyboard = false;
            switch (_r.Action)
            {
                case AlmaAbility.Dash:
                    if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) { fromKeyboard = true; return true; }
                    return AlmaTouchControls.DashPending;
                case AlmaAbility.GroundPound:
                    if (Input.GetAxisRaw("Vertical") < -0.5f || Input.GetKeyDown(KeyCode.C)) { fromKeyboard = true; return true; }
                    return AlmaTouchControls.PoundPending;
                case AlmaAbility.Roar:
                    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F)) { fromKeyboard = true; return true; }
                    return AlmaTouchControls.RoarPending;
                default:
                    if (Input.GetButtonDown("Jump")) { fromKeyboard = true; return true; }
                    return AlmaTouchControls.JumpPending;
            }
        }

        private void Update()
        {
            // Movement assist after a taught jump (touch movement doesn't exist yet).
            if (!_active && _assistUntil > 0f)
            {
                bool done = Time.unscaledTime > _assistUntil || (_alma != null && _alma.IsGrounded && Time.unscaledTime > _assistUntil - 1.1f);
                if (done || Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.15f) { AlmaTouchControls.Move = 0f; _assistUntil = 0f; }
                else AlmaTouchControls.Move = _assistDir;
            }
            if (!_active || _leaving) return;
            float t = Time.unscaledTime - _startedAt;
            // Bullet time: slow to a stop.
            float slow = Mathf.Clamp01(t / _slowTime);
            Time.timeScale = Mathf.Lerp(_prevTimeScale, 0f, 1f - (1f - slow) * (1f - slow));
            if (t < _minShowTime) { AlmaTouchControls.Clear(); AlmaTouchControls.InputLocked = true; return; }
            if (!Pressed(out bool keyboard)) return;

            // Keep only the taught press; resume now so the move happens on this frame's input.
            AlmaTouchControls.Clear();
            switch (_r.Action)
            {
                case AlmaAbility.Dash: AlmaTouchControls.PressDash(); break;
                case AlmaAbility.GroundPound: AlmaTouchControls.PressPound(); break;
                case AlmaAbility.Roar: AlmaTouchControls.PressRoar(); break;
                default:
                    AlmaTouchControls.PressJump();
                    if (keyboard) AlmaTouchControls.ReleaseJump();   // the key itself holds the jump
                    break;
            }
            AlmaTouchControls.InputLocked = false;
            Time.timeScale = _prevTimeScale;
            if (_r.AssistMove)
            {
                _assistDir = Mathf.Sign(_r.ArcTo.x - _r.ArcFrom.x);
                _assistUntil = Time.unscaledTime + 1.6f;
                AlmaTouchControls.Move = _assistDir;
            }
            _leaving = true; _leftAt = Time.unscaledTime;
            if (HudAbilityButtons.Instance != null) HudAbilityButtons.Instance.Spotlight(null);
        }

        // ---------- drawing ----------

        private static float Ease(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
        private static float Back(float t) { const float c = 1.7f; t -= 1f; return 1f + (c + 1f) * t * t * t + c * t * t; }

        private void LateUpdate()
        {
            if (!_active) return;
            float now = Time.unscaledTime, t = now - _startedAt;
            float lv = _leaving ? Mathf.Clamp01((now - _leftAt) / _leaveTime) : 0f;
            float vis = Ease(Mathf.Clamp01(t / 0.35f)) * (1f - Ease(lv));
            Color c = _r.Accent;

            // Camera shot.
            if (_camera != null)
            {
                float w = _leaving ? 1f - Ease(Mathf.Clamp01((now - _leftAt) / _cameraTime)) : Ease(Mathf.Clamp01(t / _cameraTime));
                _camera.SetShot(_r.Focus, _r.Zoom > 0f ? _r.Zoom : 6.5f, w);
            }

            // Vignette spotlight centred on the obstacle (HUD-local, follows the camera shot).
            Vector3 focusLocal = transform.InverseTransformPoint(new Vector3(_r.Focus.x, _r.Focus.y, transform.position.z));
            float halfH = 8f, halfW = 8f * (_hud.Camera != null ? _hud.Camera.aspect : 16f / 9f);
            // The spotlight texture is dark everywhere except a soft circle (r ≈ 0.17 of its half size): drawn
            // big enough that its dark rim always covers the screen, with the hole closing in on the obstacle.
            float hole = Mathf.Lerp(9f, 5.2f, Ease(Mathf.Clamp01(t / 0.7f)));
            float span = hole / 0.17f * 2f;
            _vignette.transform.localPosition = new Vector3(focusLocal.x, focusLocal.y, 0f);
            _vignette.transform.localScale = Vector3.one * (Mathf.Max(span, (halfW + halfH) * 2.4f) / _vignetteSprite.bounds.size.x);
            _vignette.color = new Color(HudText.Ink.r, HudText.Ink.g, HudText.Ink.b, 0.78f * vis);

            // Button: pulsing ring, repeating shockwave and a bobbing arrow pointing at it.
            var buttons = HudAbilityButtons.Instance;
            if (buttons != null && buttons.TryGetButton(_r.Action, out Vector2 bp, out float br))
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(now * 6f);
                float ringUnit = 1f / _ringSprite.bounds.extents.x;
                _ring.transform.localPosition = bp;
                _ring.transform.localScale = Vector3.one * (br * ringUnit * (1.18f + 0.08f * pulse));
                _ring.color = new Color(Mathf.Lerp(c.r, 1f, 0.4f), Mathf.Lerp(c.g, 1f, 0.4f), Mathf.Lerp(c.b, 1f, 0.4f), vis);
                float wt = (now * 0.9f) % 1f;
                _wave.transform.localPosition = bp;
                _wave.transform.localScale = Vector3.one * (br * (1f / _shockwaveSprite.bounds.extents.x) * (1.1f + 1.6f * wt));
                _wave.color = new Color(c.r, c.g, c.b, (1f - wt) * vis);
                // Arrow from the upper-left, pointing down-right at the button.
                Vector2 dir = new Vector2(1f, -0.75f).normalized;
                float bob = 0.35f * Mathf.Abs(Mathf.Sin(now * 4.5f));
                float arrowLen = Mathf.Max(br * 3.2f, 2f);
                Vector2 tip = bp - dir * (br * 1.25f + bob);
                _arrow.transform.localPosition = tip - dir * arrowLen * 0.5f;
                _arrow.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                _arrow.transform.localScale = Vector3.one * (arrowLen / _arrowSprite.bounds.size.x);
                _arrow.color = new Color(1f, 1f, 1f, vis);
            }

            // Texts, upper centre of the safe area (responsive).
            if (_title != null)
            {
                float k = _hud.TextScale;
                // Keep both texts inside the safe area whatever the device: the body wraps to the room left,
                // the title shrinks if it doesn't fit on one line.
                float room = _hud.Safe.width / k - 3f;
                _text.rectTransform.sizeDelta = new Vector2(Mathf.Min(16f, room), _text.rectTransform.sizeDelta.y);
                _title.rectTransform.sizeDelta = new Vector2(Mathf.Min(20f, room), _title.rectTransform.sizeDelta.y);
                Vector2 top = _hud.Anchor(new Vector2(0.5f, 0.8f), Vector2.zero);
                float pop = Mathf.Clamp01((t - 0.2f) / 0.45f);
                _title.transform.localPosition = top;
                _title.transform.localScale = Vector3.one * k * (_leaving ? 1f + 0.25f * lv : Mathf.LerpUnclamped(1.6f, 1f, Back(pop)));
                HudText.Alpha(_title, Ease(pop) * (1f - Ease(lv)));
                // Body under the title, hint under the body, each placed by its real height (wrapping).
                float bodyH = _text.textBounds.size.y;
                _text.transform.localPosition = top + Vector2.down * ((1.15f + bodyH * 0.5f) * k);
                _text.transform.localScale = Vector3.one * k;
                HudText.Alpha(_text, Ease(Mathf.Clamp01((t - 0.45f) / 0.4f)) * (1f - Ease(lv)));
            }

            // Dotted arc drawn progressively, a comet running along it, a pulsing landing ring.
            if (_r.ShowArc)
            {
                float grow = Ease(Mathf.Clamp01((t - 0.3f) / 0.6f));
                float apex = Mathf.Max(_r.ArcFrom.y, _r.ArcTo.y) + Mathf.Max(0.3f, _r.ArcHeight);
                for (int i = 0; i < _dots.Count; i++)
                {
                    float u = (i + 0.5f) / _dots.Count;
                    _dots[i].transform.position = Arc(u, apex) + Vector3.up * 0.7f;
                    _dots[i].transform.localScale = Vector3.one * 0.32f / HazardFx.Glow().bounds.size.x;
                    float on = Mathf.Clamp01((grow - u) * 8f);
                    _dots[i].color = new Color(1f, 1f, 1f, 0.95f * on * vis);
                }
                float cu = (now * 0.7f) % 1f;
                _comet.transform.position = Arc(cu, apex) + Vector3.up * 0.7f;
                _comet.transform.localScale = Vector3.one * 0.8f / HazardFx.Glow().bounds.size.x;
                _comet.color = new Color(c.r, c.g, c.b, (grow >= 1f ? 0.9f : 0f) * vis);
                _landing.transform.position = _r.ArcTo + Vector3.up * 0.15f;
                float lp = (now * 1.2f) % 1f;
                _landing.transform.localScale = new Vector3(1f, 0.35f, 1f) * (0.6f + 0.6f * lp) / _ringSprite.bounds.extents.x;
                _landing.color = new Color(c.r, c.g, c.b, (1f - lp) * grow * vis);
            }

            if (_leaving && lv >= 1f && (_camera == null || now - _leftAt >= _cameraTime))
            {
                if (_camera != null) _camera.ClearShot();
                Cleanup();
                _active = false; _leaving = false;
                Finished?.Invoke();
            }
        }

        private Vector3 Arc(float u, float apex)
        {
            // Quadratic Bézier whose peak is `apex`.
            Vector3 a = _r.ArcFrom, b = _r.ArcTo;
            Vector3 mid = new Vector3((a.x + b.x) * 0.5f, 2f * apex - (a.y + b.y) * 0.5f, 0f);
            return (1 - u) * (1 - u) * a + 2 * (1 - u) * u * mid + u * u * b;
        }
    }
}
