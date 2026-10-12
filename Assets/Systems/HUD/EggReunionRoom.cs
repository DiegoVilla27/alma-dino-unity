using AlmaGame.Level;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Root of an egg reunion room prefab (one per boss). HudEggReunion shows it over the whole screen after the
    // boss's defeat, for `Duration` seconds. The prefab holds the backdrop, Alma and the egg; this plays the scene:
    //   1. Letterbox bars slide in and the view starts a slow push-in towards the egg's place. Alma comes running
    //      in from the left edge (animator state "Run", kicking up dust, with her shadow on the ground) and brakes
    //      with a skid to where she is placed in the prefab.
    //   2. She plays "Take" once (hands up, looking up) and holds the last pose.
    //   3. A column of light opens over her and the egg comes down slowly inside it, "item get" style: a halo and
    //      turning rays behind it and sparkles falling, until it rests where it is placed in the prefab (her hands).
    //   4. On landing: a burst of sparkles, a flash and two heartbeats; the light closes and the rays keep shining.
    // While the light is open the backdrop dims (spotlight) and Alma takes a warm tint from it; light motes float
    // in the air all the time. Bars, shadows, dust, motes, light, rays and sparkles are made here at runtime; the
    // light is tinted with `Light Color`. Everything but the bars sits under a runtime "Stage" child (the push-in).
    // The prefab is laid out in HUD units with its origin at the CENTRE of the screen: the screen is 16 units tall
    // and 28.4 wide at 16:9 (37.3 at 21:9 phones; cover the sides with the backdrop). The gizmo shows both frames.
    [DisallowMultipleComponent]
    public sealed class EggReunionRoom : MonoBehaviour
    {
        [Tooltip("Seconds the room stays on screen before going back to the arena.")]
        [SerializeField, Min(1f)] private float _duration = 8.5f;

        [Header("Alma")]
        [Tooltip("Optional: Alma in the room, placed where she stops. Her Animator needs the states \"Run\" and \"Take\".")]
        [SerializeField] private Animator _alma;
        [Tooltip("Where she starts running from (HUD units, off the left edge even on 21:9).")]
        [SerializeField] private float _almaStartX = -21f;
        [SerializeField, Min(0.1f)] private float _runSpeed = 10f;
        [Tooltip("Distance over which she brakes to a stop at the end of the run.")]
        [SerializeField, Min(0f)] private float _brakeDistance = 1.4f;

        [Header("Egg")]
        [Tooltip("Optional: the egg, placed where it ends (in Alma's hands). It comes down from `Egg Drop Height` above.")]
        [SerializeField] private Transform _egg;
        [SerializeField, Min(0f)] private float _eggDropHeight = 9f;
        [Tooltip("When the egg starts coming down and when it lands (seconds after Alma stops; from the start without Alma).")]
        [SerializeField] private Vector2 _eggDropTime = new Vector2(0.5f, 3f);
        [SerializeField] private bool _heartbeat = true;

        [Header("Light")]
        [SerializeField] private Color _lightColor = new Color(1f, 0.93f, 0.6f, 1f);
        [Tooltip("Additive particle material for the sparkles (FX_Fire_Additive). Without it there are no sparkles.")]
        [SerializeField] private Material _sparkleMaterial;
        [Tooltip("Sorting order of the column, halo and rays: behind Alma (and the egg), over the backdrop. The sparkles and the landing flash go 30 above it, in front of everything.")]
        [SerializeField] private int _lightOrder = 5;

        [Header("Staging")]
        [Tooltip("The backdrop sprite: it dims while the light is open.")]
        [SerializeField] private SpriteRenderer _backdrop;
        [Tooltip("Height of the ground Alma stands on (her shadow and the dust go here).")]
        [SerializeField] private float _groundY = -5.9f;
        [Tooltip("Alpha-blended particle material for the dust of the run (FX_Fire_Smoke). Without it there is no dust.")]
        [SerializeField] private Material _dustMaterial;
        [SerializeField] private Color _dustColor = new Color(0.62f, 0.5f, 0.36f, 0.6f);
        [Tooltip("Zoom reached at the end of the room by the slow push-in towards the egg (1 = none).")]
        [SerializeField, Range(1f, 1.3f)] private float _pushIn = 1.1f;
        [Tooltip("Height of each letterbox bar (0 = none).")]
        [SerializeField, Min(0f)] private float _letterbox = 1.3f;
        [Tooltip("Backdrop brightness while the light is open.")]
        [SerializeField, Range(0.3f, 1f)] private float _spotlightDim = 0.68f;

        public float Duration => _duration;

        private const string RunState = "Run", TakeState = "Take";
        private const int RayCount = 8;

        private float _shownAt, _arriveAt, _brakeAt;
        private bool _skidded;
        private Transform _stage;
        private SpriteRenderer _almaRenderer, _almaShadow, _eggShadow, _barTop, _barBottom;
        private ParticleSystem _dust, _motes;
        private Vector3 _almaHome, _eggHome, _eggScale;
        private bool _taking, _burst;
        private SpriteRenderer _beam, _halo, _flash;
        private Transform _rays;
        private SpriteRenderer[] _rayRenderers;
        private ParticleSystem _sparkles;
        private Sprite _streak;

        private void Awake()
        {
            if (_alma != null) { _almaHome = _alma.transform.localPosition; _almaRenderer = _alma.GetComponent<SpriteRenderer>(); }
            if (_egg != null) { _eggHome = _egg.localPosition; _eggScale = _egg.localScale; }
            // Everything in the prefab goes under the Stage (pushed in); the bars stay outside it.
            _stage = new GameObject("Stage").transform;
            _stage.SetParent(transform, false);
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != _stage) child.SetParent(_stage, false);
            }
            BuildLight();
            BuildStaging();
        }

        private void OnEnable()
        {
            _shownAt = Time.time;
            _taking = _burst = _skidded = false;
            float run = _alma != null ? Mathf.Max(0f, _almaHome.x - _almaStartX) : 0f, brake = Mathf.Min(_brakeDistance, run);
            _brakeAt = (run - brake) / _runSpeed;
            _arriveAt = _alma != null ? _brakeAt + 2f * brake / _runSpeed : 0f;   // constant braking from full speed
            if (_alma != null) _alma.Play(RunState, 0, 0f);
            Update();
        }

        private void OnDestroy()
        {
            if (_streak != null) Destroy(_streak);
            HazardFx.DestroyMaterial(_sparkles);
            HazardFx.DestroyMaterial(_dust);
            HazardFx.DestroyMaterial(_motes);
        }

        private void Update()
        {
            float t = Time.time - _shownAt;
            UpdateStage(t);
            UpdateAlma(t);
            if (_egg == null) return;
            float since = t - _arriveAt;   // time since Alma stopped
            float drop0 = _eggDropTime.x, drop1 = Mathf.Max(drop0 + 0.01f, _eggDropTime.y);

            // The egg: hidden until the light opens, then down the column slowly (ease out), swaying a little.
            float d = Mathf.Clamp01((since - drop0) / (drop1 - drop0));
            float e = 1f - (1f - d) * (1f - d);
            _egg.gameObject.SetActive(since >= drop0 - 0.05f);
            _egg.localPosition = _eggHome + Vector3.up * (_eggDropHeight * (1f - e)) + Vector3.right * (Mathf.Sin(t * 2.4f) * 0.2f * (1f - d));
            _egg.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 3.1f) * 9f * (1f - d));
            float beat = 0f;
            if (_heartbeat)
                foreach (float at in new[] { drop1 + 0.25f, drop1 + 0.75f })
                {
                    float b = (since - at) / 0.3f;
                    if (b > 0f && b < 1f) beat = Mathf.Max(beat, Mathf.Sin(b * Mathf.PI));
                }
            _egg.localScale = _eggScale * (1f + 0.15f * beat);

            UpdateLight(t, since, drop0, drop1, beat);
        }

        // Runs in from the left at full speed, brakes evenly over the last stretch (skid dust), then "Take" once.
        private void UpdateAlma(float t)
        {
            if (_alma == null) return;
            float x;
            if (t < _brakeAt) x = _almaStartX + _runSpeed * t;
            else
            {
                float b = Mathf.Min(t, _arriveAt) - _brakeAt, decel = _runSpeed / Mathf.Max(0.001f, _arriveAt - _brakeAt);
                x = _almaStartX + _runSpeed * _brakeAt + _runSpeed * b - 0.5f * decel * b * b;
                if (!_skidded) { _skidded = true; Puff(new Vector3(_almaStartX + _runSpeed * _brakeAt, _groundY, 0f), 22, 3f); }
            }
            x = Mathf.Min(x, _almaHome.x);
            _alma.transform.localPosition = new Vector3(x, _almaHome.y, _almaHome.z);
            if (_almaShadow != null) _almaShadow.transform.localPosition = new Vector3(x + 0.15f, _groundY + 0.05f, 0f);
            if (_dust != null)
            {
                _dust.transform.position = _stage.TransformPoint(new Vector3(x - 0.6f, _groundY + 0.1f, 0f));
                var emission = _dust.emission;
                emission.rateOverTime = t < _arriveAt ? (t < _brakeAt ? 22f : 45f) : 0f;
            }
            if (!_taking && t >= _arriveAt)
            {
                _taking = true;
                _alma.Play(TakeState, 0, 0f);   // plays once and holds its last pose
            }
        }

        // ---- Staging: letterbox, push-in, shadows, dust, motes ----

        private void BuildStaging()
        {
            if (_letterbox > 0f)
            {
                _barTop = NewRenderer("LetterboxTop", transform, HazardFx.Pixel(), 200);
                _barBottom = NewRenderer("LetterboxBottom", transform, HazardFx.Pixel(), 200);
                foreach (var bar in new[] { _barTop, _barBottom })
                {
                    bar.transform.localScale = new Vector3(60f, _letterbox, 1f);
                    bar.color = Color.black;
                }
            }
            var dark = new Color(0f, 0f, 0f, 0f);
            if (_alma != null)
            {
                int order = _almaRenderer != null ? _almaRenderer.sortingOrder : 10;
                _almaShadow = NewRenderer("AlmaShadow", _stage, HazardFx.Glow(), order - 1);
                _almaShadow.transform.localScale = new Vector3(2.8f * _alma.transform.localScale.x / 2.2f, 0.5f, 1f);
                _almaShadow.color = new Color(0.05f, 0.08f, 0f, 0.6f);
                if (_dustMaterial != null)
                {
                    _dust = HazardFx.CreateParticles("RunDust", _stage, _dustMaterial, HazardFx.Puff(), 80, 0, order + 1);
                    var main = _dust.main;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
                    main.startColor = _dustColor;
                    main.gravityModifier = -0.05f;
                    var shape = _dust.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.2f; shape.arc = 180f;
                    HazardFx.SetSizeOverLifetime(_dust, 0.6f, 1.6f);
                    var emission = _dust.emission; emission.enabled = true;
                }
            }
            if (_egg != null)
            {
                _eggShadow = NewRenderer("EggShadow", _stage, HazardFx.Glow(), (_almaRenderer != null ? _almaRenderer.sortingOrder : 10) - 1);
                _eggShadow.color = dark;
            }
            if (_sparkleMaterial != null)
            {
                // Light motes drifting in the air over the whole screen.
                _motes = HazardFx.CreateParticles("Motes", _stage, _sparkleMaterial, HazardFx.Puff(), 60, 0, _lightOrder + 1, false);
                var main = _motes.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
                main.startColor = new Color(_lightColor.r, _lightColor.g, _lightColor.b, 0.7f);
                main.prewarm = true;
                var shape = _motes.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(38f, 13f, 0f);
                _motes.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                var noise = _motes.noise; noise.enabled = true; noise.strength = 0.4f; noise.frequency = 0.3f;
                var emission = _motes.emission; emission.enabled = true; emission.rateOverTime = 9f;
                _motes.Stop(); _motes.Play();
            }
        }

        private void UpdateStage(float t)
        {
            // Letterbox: in over 0.6 s, out over the last 0.5 s.
            float bars = Mathf.SmoothStep(0f, 1f, t / 0.6f) * (1f - Mathf.SmoothStep(0f, 1f, (t - _duration + 0.5f) / 0.5f));
            if (_barTop != null)
            {
                _barTop.transform.localPosition = new Vector3(0f, 8f + _letterbox * (0.5f - bars), 0f);
                _barBottom.transform.localPosition = new Vector3(0f, -8f - _letterbox * (0.5f - bars), 0f);
            }
            // Push-in: slow zoom towards the egg's place (that point stays still on screen).
            float zoom = 1f + (_pushIn - 1f) * Mathf.SmoothStep(0f, 1f, t / _duration);
            Vector3 focus = _egg != null ? _eggHome : (_alma != null ? _almaHome : Vector3.zero);
            _stage.localScale = Vector3.one * zoom;
            _stage.localPosition = new Vector3(-focus.x * (zoom - 1f), -focus.y * (zoom - 1f), 0f);
        }

        // A puff of dust on the ground (skid).
        private void Puff(Vector3 local, int count, float speed)
        {
            if (_dust == null) return;
            for (int i = 0; i < count; i++)
            {
                float a = Random.Range(10f, 170f) * Mathf.Deg2Rad;
                _dust.Emit(new ParticleSystem.EmitParams
                {
                    position = _stage.TransformPoint(local + new Vector3(Random.Range(-0.4f, 0.4f), 0f, 0f)),
                    velocity = new Vector3(Mathf.Cos(a) * 1.6f, Mathf.Sin(a) * 0.6f, 0f) * Random.Range(0.4f, 1f) * speed,
                    startSize = Random.Range(0.9f, 1.7f),
                    startLifetime = Random.Range(0.6f, 1.1f),
                    startColor = _dustColor,
                }, 1);
            }
        }

        // ---- Light: column, halo, rays, sparkles, flash ----

        private void BuildLight()
        {
            if (_egg == null) return;
            var root = new GameObject("EggLight").transform;
            root.SetParent(_stage, false);
            Texture2D streak = HazardFx.Streak();
            _streak = Sprite.Create(streak, new Rect(0f, 0f, streak.width, streak.height), new Vector2(0.5f, 0.5f), streak.height);
            _streak.name = "EggLightStreak";

            // The column comes from above the screen down to the egg's place; the streak is horizontal, so it is turned.
            _beam = NewRenderer("Beam", root, _streak, _lightOrder - 2);
            _beam.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            _halo = NewRenderer("Halo", root, HazardFx.Glow(), _lightOrder);
            _rays = new GameObject("Rays").transform;
            _rays.SetParent(root, false);
            _rayRenderers = new SpriteRenderer[RayCount];
            for (int i = 0; i < RayCount; i++)
            {
                _rayRenderers[i] = NewRenderer("Ray", _rays, _streak, _lightOrder - 1);
                _rayRenderers[i].transform.localRotation = Quaternion.Euler(0f, 0f, i * 180f / RayCount);
            }
            _flash = NewRenderer("Flash", root, HazardFx.Glow(), _lightOrder + 30);

            if (_sparkleMaterial != null)
            {
                _sparkles = HazardFx.CreateParticles("Sparkles", root, _sparkleMaterial, HazardFx.Puff(), 160, 0, _lightOrder + 30);
                var main = _sparkles.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2.2f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.35f);
                main.startColor = new ParticleSystem.MinMaxGradient(_lightColor, Color.white);
                main.gravityModifier = 0.15f;
                var shape = _sparkles.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.6f;
                HazardFx.SetSizeOverLifetime(_sparkles, 1f, 0.2f);
                var emission = _sparkles.emission;
                emission.enabled = true;
            }
        }

        private SpriteRenderer NewRenderer(string name, Transform parent, Sprite sprite, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.color = Color.clear;
            return sr;
        }

        private void UpdateLight(float t, float since, float drop0, float drop1, float beat)
        {
            if (_beam == null) return;
            Vector3 eggAt = _egg.localPosition;
            float size = _eggScale.y * (_egg.TryGetComponent(out SpriteRenderer egg) && egg.sprite != null ? egg.sprite.bounds.size.y : 1f);
            Color c = _lightColor;

            // Column: opens just before the egg appears, closes a second after it lands.
            float open = Mathf.Clamp01((since - (drop0 - 0.4f)) / 0.4f) * (1f - Mathf.Clamp01((since - drop1 - 0.6f) / 1f));
            float top = 9f, bottom = _eggHome.y - size * 0.3f;
            _beam.transform.localPosition = new Vector3(_eggHome.x, (top + bottom) * 0.5f, 0f);
            _beam.transform.localScale = new Vector3(top - bottom, size * 1.6f * (0.6f + 0.4f * open), 1f);
            _beam.color = new Color(c.r, c.g, c.b, 0.55f * open);

            // Halo and rays follow the egg; they stay (smaller) after it lands.
            bool shown = since >= drop0 - 0.05f;
            float glow = shown ? 1f : 0f;
            float pulse = 1f + 0.08f * Mathf.Sin(t * 5f) + 0.35f * beat;
            _halo.transform.localPosition = eggAt;
            _halo.transform.localScale = Vector3.one * (size * 2.6f * pulse);
            _halo.color = new Color(c.r, c.g, c.b, 0.75f * glow);
            _rays.localPosition = eggAt;
            _rays.localRotation = Quaternion.Euler(0f, 0f, t * 25f);
            float rayLength = size * (since < drop1 ? 2.6f : Mathf.Lerp(2.6f, 2f, Mathf.Clamp01(since - drop1))) * pulse;
            for (int i = 0; i < _rayRenderers.Length; i++)
            {
                float flicker = 0.75f + 0.25f * Mathf.Sin(t * 3f + i * 1.7f);
                _rayRenderers[i].transform.localScale = new Vector3(rayLength * (i % 2 == 0 ? 1f : 0.7f), size * 0.5f, 1f);
                _rayRenderers[i].color = new Color(c.r, c.g, c.b, 0.45f * glow * flicker);
            }

            // Sparkles fall from the egg while it comes down; a burst and a flash when it lands.
            if (_sparkles != null)
            {
                _sparkles.transform.localPosition = eggAt;
                var emission = _sparkles.emission;
                emission.rateOverTime = shown && since < drop1 ? 28f : 4f * glow;
                if (!_burst && since >= drop1)
                {
                    _burst = true;
                    for (int i = 0; i < 40; i++)
                    {
                        float a = i / 40f * Mathf.PI * 2f;
                        var p = new ParticleSystem.EmitParams
                        {
                            position = _sparkles.transform.position,
                            velocity = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * Random.Range(3f, 6f),
                            startSize = Random.Range(0.2f, 0.45f),
                            startLifetime = Random.Range(0.6f, 1.1f),
                            startColor = Color.Lerp(c, Color.white, Random.value),
                        };
                        _sparkles.Emit(p, 1);
                    }
                }
            }
            // Spotlight: the backdrop dims while the light is open (half as much once the egg is home) and Alma
            // takes a warm tint from it. The egg's shadow grows on the ground as it comes down.
            float lit = Mathf.Max(open, glow * 0.5f);
            if (_backdrop != null) { float v = Mathf.Lerp(1f, _spotlightDim, lit); _backdrop.color = new Color(v, v, v, 1f); }
            if (_almaRenderer != null) _almaRenderer.color = Color.Lerp(Color.white, Color.Lerp(c, Color.white, 0.45f), 0.6f * lit);
            if (_eggShadow != null)
            {
                float near = 1f - Mathf.Clamp01((eggAt.y - _eggHome.y) / Mathf.Max(0.01f, _eggDropHeight));
                _eggShadow.transform.localPosition = new Vector3(eggAt.x, _groundY + 0.05f, 0f);
                _eggShadow.transform.localScale = new Vector3(size * (0.6f + 0.7f * near), 0.35f * (0.5f + 0.5f * near), 1f);
                _eggShadow.color = new Color(0f, 0f, 0f, 0.3f * near * glow);
            }

            float f = since - drop1;
            float flash = f > 0f && f < 0.6f ? 1f - f / 0.6f : 0f;
            _flash.transform.localPosition = _eggHome;
            _flash.transform.localScale = Vector3.one * (size * (3f + 6f * (1f - flash)));
            _flash.color = new Color(1f, 1f, 1f, 0.8f * flash);
        }

        private void OnDrawGizmos()
        {
            // The screen at 16:9 (white) and at 21:9 (grey), 16 units tall, centred on the prefab's origin.
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(16f * 16f / 9f, 16f, 0f));
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(16f * 21f / 9f, 16f, 0f));
            if (_egg != null)
            {
                Gizmos.color = new Color(1f, 0.9f, 0.4f, 0.8f);
                Vector3 home = Application.isPlaying ? _eggHome : _egg.localPosition;
                Gizmos.DrawLine(home, home + Vector3.up * _eggDropHeight);
            }
        }
    }
}
