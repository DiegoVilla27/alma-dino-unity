using System.Collections;
using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Bottom-right action buttons laid out like a gamepad's face buttons: Jump (bottom), Dash (left), Ground
    // Pound (right) and Roar (top). They are touch buttons (the game ships on mobile first; a mouse click works
    // too, for testing) and also the HUD's record of Alma's powers: a locked ability is an empty stone socket;
    // an unlocked one shows its altar rune with a ring in its colour. Jump always works and gets the Double Jump
    // rune engraved once it is unlocked. Buttons flash when their action happens (any input) and the Dash
    // button dims while the air Dash is spent. When an altar is taken its rune flies from the altar to its
    // button (FlyIn) and is engraved with a pop, a flash, a shockwave and two heartbeats.
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudAbilityButtons : MonoBehaviour
    {
        [System.Serializable]
        public struct ButtonDef
        {
            public string Name;
            public AlmaAbility Ability;
            public bool IsJump;          // the Jump button: always usable; shows the Double Jump rune when unlocked
            public Vector2 Offset;       // from the cluster centre (HUD units)
            [Min(0.1f)] public float Radius;
            public Sprite Rune;
            public Color Color;
        }

        [SerializeField] private ButtonDef[] _buttons = new ButtonDef[0];
        [Tooltip("Cluster centre, measured from the bottom-right corner (HUD units, camera size 8).")]
        [SerializeField] private Vector2 _inset = new Vector2(2.15f, 2.35f);
        [SerializeField] private Sprite _baseSprite;
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private Sprite _jumpIcon;
        [SerializeField] private Sprite _shockwaveSprite;
        [Tooltip("Extra touch area around each button (HUD units), so thumbs don't miss.")]
        [SerializeField, Min(0f)] private float _touchMargin = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _lockedAlpha = 0.6f;
        [SerializeField, Min(0.1f)] private float _flightTime = 1.0f;
        [Tooltip("Seconds the game freezes when the rune leaves the altar.")]
        [SerializeField, Min(0f)] private float _hitStop = 0.12f;

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
            public float StartedAt;
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
            _sparkles = HazardFx.CreateParticles("Sparkles", transform, material, HazardFx.Puff(), 80, 0, order + 6, false);
            var main = _sparkles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            var shape = _sparkles.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.3f;
            HazardFx.SetSizeOverLifetime(_sparkles, 1f, 0.1f);
            _trail = HazardFx.CreateParticles("RuneTrail", null, material, HazardFx.Puff(), 160, 0, order + 6, true);
            var tmain = _trail.main;
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
            if (Time.timeScale == 0f) Time.timeScale = 1f;
        }

        private bool IsUnlocked(Button b)
        {
            AlmaAbility ability = b.Def.IsJump ? AlmaAbility.DoubleJump : b.Def.Ability;
            if (GameProgress.Instance != null) return GameProgress.Instance.IsUnlocked(ability);
            return _alma != null && _alma.IsUnlocked(ability);
        }

        private Button For(AlmaAbility ability) =>
            _all.Find(b => ability == AlmaAbility.DoubleJump ? b.Def.IsJump : (!b.Def.IsJump && b.Def.Ability == ability));

        private SpriteRenderer New(string name, Transform parent, Sprite sprite, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        private Vector2 Position(Button b) => _hud.BottomRight(_inset) + b.Def.Offset;

        // An altar was taken: its rune (at `worldPosition`) bursts free, flies here and is engraved on its button.
        public void FlyIn(AlmaAbility ability, Vector3 worldPosition)
        {
            Button b = For(ability);
            if (b == null) return;
            b.Arriving = true;
            var f = new Flight { Button = b, From = worldPosition, StartedAt = Time.time };
            f.Glow = New("FlyingGlow", null, HazardFx.Glow(), _hud.SortingOrder + 6);
            f.Rune = New("FlyingRune", null, b.Def.Rune, _hud.SortingOrder + 7);
            var main = _trail.main;
            main.startColor = new ParticleSystem.MinMaxGradient(b.Def.Color, Color.white);
            _flights.Add(f);
            if (_hitStop > 0f) StartCoroutine(HitStop());
        }

        private IEnumerator HitStop()
        {
            float before = Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(_hitStop);
            if (Time.timeScale == 0f) Time.timeScale = before > 0f ? before : 1f;
        }

        private void Update()
        {
            if (_hud.Visibility < 0.5f) { ReleaseAll(); return; }   // hidden (cinematic): no touches
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
                if (Vector2.Distance(p, Position(b)) > b.Def.Radius + _touchMargin) continue;
                if (!b.Def.IsJump && !b.Unlocked) return;          // a locked power does nothing
                _fingers[finger] = b;
                b.Held = true;
                b.PressedAt = Time.time;
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

        private void LateUpdate()
        {
            if (_alma == null) _alma = FindAnyObjectByType<AlmaMotor2D>();
            DetectActions();
            float vis = _hud.Visibility;
            foreach (var b in _all) Draw(b, vis);
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
            if (b != null && Time.time - b.PressedAt > 0.15f) b.PressedAt = Time.time;
        }

        private void Draw(Button b, float vis)
        {
            b.Root.localPosition = Position(b);
            float r = b.Def.Radius;
            bool lit = b.Unlocked && !b.Arriving;
            float since = Time.time - b.UnlockedAt;
            float press = Mathf.Clamp01(1f - (Time.time - b.PressedAt) / 0.18f);
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
            float glow = lit ? 0.3f + 0.08f * Mathf.Sin(Time.time * 2f + r) + 0.4f * beat + 0.5f * press + (since < 1f ? 0.7f * (1f - since) : 0f) : 0f;
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

        private void UpdateFlights()
        {
            float hudScale = _hud.Scale;
            for (int i = _flights.Count - 1; i >= 0; i--)
            {
                var f = _flights[i];
                float t = (Time.time - f.StartedAt) / _flightTime;
                Vector3 to = _hud.ToWorld(Position(f.Button));
                Vector3 p; float size, spin;
                float runeH = f.Button.Def.Rune != null ? f.Button.Def.Rune.bounds.size.y : 1f;
                if (t < 0.3f)
                {
                    // Breaks free: rises from the altar, grows and spins like a coin.
                    float k = t / 0.3f;
                    p = f.From + Vector3.up * (0.9f * Mathf.Sin(k * Mathf.PI * 0.5f));
                    size = 1f + 0.5f * k;
                    spin = Mathf.Cos(k * Mathf.PI * 4f);
                }
                else
                {
                    float k = Mathf.Clamp01((t - 0.3f) / 0.7f);
                    float e = k * k * (3f - 2f * k);
                    Vector3 start = f.From + Vector3.up * 0.9f;
                    Vector3 control = Vector3.Lerp(start, to, 0.4f) + Vector3.up * (2f * hudScale);
                    p = Vector3.Lerp(Vector3.Lerp(start, control, e), Vector3.Lerp(control, to, e), e);
                    float end = f.Button.Def.Radius * 1.25f * hudScale / runeH;
                    size = Mathf.Lerp(1.5f, end, e);
                    spin = 1f;
                }
                f.Rune.transform.position = p;
                f.Rune.transform.localScale = new Vector3(size * Mathf.Max(0.15f, Mathf.Abs(spin)), size, 1f);
                f.Glow.transform.position = p;
                f.Glow.transform.localScale = Vector3.one * (size * runeH * 2.2f);
                Color c = f.Button.Def.Color;
                f.Glow.color = new Color(c.r, c.g, c.b, 0.75f);
                _trail.transform.position = p;
                _trail.Emit(t < 0.3f ? 1 : 3);
                if (t >= 1f)
                {
                    Destroy(f.Rune.gameObject); Destroy(f.Glow.gameObject);
                    _flights.RemoveAt(i);
                    f.Button.Arriving = false;
                    f.Button.Unlocked = true;
                    f.Button.UnlockedAt = Time.time;
                    _sparkles.transform.localPosition = Position(f.Button);
                    var main = _sparkles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(c, Color.white);
                    _sparkles.Emit(30);
                }
            }
        }
    }
}
