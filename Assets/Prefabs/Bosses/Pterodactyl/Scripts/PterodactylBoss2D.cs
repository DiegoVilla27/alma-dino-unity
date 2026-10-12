using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Bosses
{
    // Boss 3 — the Alpha Pterodactyl (Docs/Levels/World_3_Swamp/Boss_3.md). Same structure as the Monkey and
    // the Armadillo: intro with lines and title card, one life per phase, red flash on a hit, defeat with lines
    // and the exit portal. He owns the sky: he hovers out of reach beating up a gale that pushes Alma towards
    // the abyss (the arena's GustWind2D), then screeches, leaves the screen and dives across it along a
    // marked line. The only way to hurt him is an air Dash against his flight, into his cyan crest. Each hit
    // makes him tumble onto a side branch and break it; the next phase is faster.
    [DisallowMultipleComponent]
    public sealed class PterodactylBoss2D : MonoBehaviour
    {
        [System.Serializable]
        public struct Phase
        {
            public float GaleTime;     // seconds of gale before a dive
            public float Wind;         // gale push (units/s²)
            public bool WindTurns;     // the wind changes side halfway through the gale
            public float DiveSpeed;    // units/s
            public int Dives;          // dives in a row (from alternating sides)
            public float LineTime;     // seconds the dive line shows before he comes in
        }

        [Header("Arena")]
        [Tooltip("Branch he lands on at the end and where Alma respawns.")]
        [SerializeField] private SolidPlatform2D _centerBranch;
        [Tooltip("Side branches: one breaks after each hit (the one farthest from Alma).")]
        [SerializeField] private SolidPlatform2D[] _sideBranches = new SolidPlatform2D[0];
        [Tooltip("Where he hovers during the gale (world).")]
        [SerializeField] private Vector2 _hoverPoint = new Vector2(0f, 2.5f);
        [SerializeField] private Vector2 _hoverSway = new Vector2(3f, 0.4f);

        [Header("Fight")]
        [SerializeField] private Phase[] _phases =
        {
            new Phase { GaleTime = 4f, Wind = 10f, WindTurns = false, DiveSpeed = 9f, Dives = 1, LineTime = 1.3f },
            new Phase { GaleTime = 4f, Wind = 13f, WindTurns = true, DiveSpeed = 10.35f, Dives = 1, LineTime = 1.15f },
            new Phase { GaleTime = 4f, Wind = 16f, WindTurns = true, DiveSpeed = 11.9f, Dives = 2, LineTime = 1f },
        };
        [Tooltip("Height of the dive (his centre) above the top of the branch nearest Alma: standing there she gets hit.")]
        [SerializeField] private float _diveHeight = 2.4f;
        [Tooltip("Cyan crest hitbox: offset from his centre (facing right) and radius.")]
        [SerializeField] private Vector2 _headOffset = new Vector2(1.6f, 0.2f);
        [SerializeField] private float _headRadius = 0.85f;
        [Tooltip("Lethal body box while diving (centre offset facing right, size).")]
        [SerializeField] private Vector2 _bodyOffset = new Vector2(-0.3f, -0.1f);
        [SerializeField] private Vector2 _bodySize = new Vector2(3f, 0.9f);
        [SerializeField] private float _hitBounce = 11f;

        [Header("Art")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [SerializeField] private string _tauntState = "Boss_Pterodactyl_Taunt";
        [SerializeField] private string _flyState = "Boss_Pterodactyl_Fly";
        [SerializeField] private string _diveState = "Boss_Pterodactyl_Dive";
        [SerializeField, Min(0.1f)] private float _scale = 1f;
        [SerializeField] private int _sortingOrder = 4;
        [SerializeField] private string _diveShout = "¡Picado!";

        [Header("Story")]
        [SerializeField, TextArea(1, 2)] private string[] _introLines = { "¡Kraaa! Este cielo es mío, lagartija sin alas.", "¡El viento te devolverá al pantano!" };
        [SerializeField, TextArea(1, 2)] private string[] _defeatLines = { "¡Kraa...! Mi cresta...", "Vuela si puedes, madre... el volcán te espera." };
        [SerializeField] private Sprite _bubbleSprite, _bubbleTailSprite;
        [Header("Egg (reunion after the defeat)")]
        [SerializeField] private string _eggId = "Purple";
        [Tooltip("The reunion room shown after the defeat (prefab with EggReunionRoom on its root).")]
        [SerializeField] private GameObject _reunionRoom;
        [SerializeField] private Color _accent = new Color(0.8f, 0.5f, 1f);   // the Dash colour: the ability that beats him
        [Tooltip("Height of the speech bubble above his feet when perched (the sprite frame is taller than his head).")]
        [SerializeField, Min(0f)] private float _bubbleHeight = 3.8f;
        [SerializeField] private string _bossName = "Pterodáctilo Alfa";
        [SerializeField] private string _bossEpithet = "El Señor de las Ráfagas";
        [SerializeField] private string _bossKicker = "Jefe del Mundo 3";
        [SerializeField, Range(3f, 8f)] private float _introZoom = 5.5f;
        [Tooltip("Exit portal (inactive) shown after the defeat.")]
        [SerializeField] private GameObject _exitPortal;
        [SerializeField] private Vector2 _portalPosition = new Vector2(0f, -3.41f);
        [Tooltip("After the defeat Alma reappears on the portal's left, with this gap between her and the portal.")]
        [SerializeField, Min(0f)] private float _almaPortalGap = 0.35f;
        [Tooltip("Perched, his feet sink this much into the branch (the claws have some empty space under them).")]
        [SerializeField, Min(0f)] private float _perchSink = 0.12f;

        private enum State { Intro, TakeOff, Gale, Warn, Dive, Return, Tumble, Defeat, Done }

        private State _state;
        private float _stateAt;
        private int _phase, _divesLeft, _diveDir = 1;
        private float _diveY, _diveFromX, _diveToX, _windSide = 1f;
        private bool _windTurned;
        private Vector2 _pos, _from;                 // flying: his centre; perched: his feet
        private AlmaMotor2D _alma;
        private Collider2D _almaCol;
        private BossBackdropPhases _backdrop;
        private GustWind2D _gale;
        private SpeechBubble2D _bubble;
        private AlmaCameraFollow _cam;
        private SpriteRenderer _sprite, _line;
        private Animator _animator;
        private string _playing;
        private SolidPlatform2D _breaking;
        private readonly List<SolidPlatform2D> _broken = new List<SolidPlatform2D>();
        private float _hitFlashAt = -10f;

        public int Hits { get; private set; }
        public bool IsDiving => _state == State.Dive;
        public bool IsDefeated => _state == State.Defeat || _state == State.Done;
        public Vector2 Position => _pos;
        public Vector2 Head => _pos + new Vector2(_headOffset.x * Facing, _headOffset.y) * _scale;
        public event System.Action<int> Hit;
        public event System.Action Defeated;

        private float Facing => _sprite != null && _sprite.flipX ? -1f : 1f;
        private Phase P => _phases[Mathf.Clamp(_phase, 0, _phases.Length - 1)];
        private float T => Time.time - _stateAt;

        private void Enter(State s)
        {
            if ((_state == State.Warn || _state == State.Dive) && s != State.Warn && s != State.Dive) HudBossAlert.Hide();
            _state = s; _stateAt = Time.time;
        }

        // ---------- setup ----------

        private void Start()
        {
            _alma = FindAnyObjectByType<AlmaMotor2D>();
            _almaCol = _alma != null ? _alma.GetComponent<Collider2D>() : null;
            _backdrop = FindAnyObjectByType<BossBackdropPhases>();
            _gale = FindAnyObjectByType<GustWind2D>();
            _cam = FindAnyObjectByType<AlmaCameraFollow>();
            if (_alma != null)
            {
                _alma.Respawned += OnAlmaRespawned;
                _alma.SetUnlocked(AlmaAbility.DoubleJump, true);
                _alma.SetUnlocked(AlmaAbility.Dash, true);
            }
            _sprite = new GameObject("Sprite").AddComponent<SpriteRenderer>();
            _sprite.transform.SetParent(transform, false);
            _sprite.sortingOrder = _sortingOrder;
            _animator = _sprite.gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = _animatorController;
            _line = new GameObject("DiveLine").AddComponent<SpriteRenderer>();
            _line.transform.SetParent(transform, false);
            _line.sprite = HazardFx.Pixel();
            _line.sortingOrder = _sortingOrder + 2;
            _line.enabled = false;

            if (_introLines != null && _introLines.Length > 0 && HudText.Ready)
            {
                // Anchored to an empty point over his head (no renderer → the bubble sits on its position).
                var anchor = new GameObject("BubbleAnchor").transform;
                anchor.SetParent(_sprite.transform, false);
                anchor.localPosition = new Vector3(0f, _bubbleHeight, 0f);
                _bubble = SpeechBubble2D.Create(anchor, _bubbleSprite, _bubbleTailSprite, _bossName, _accent);
            }
            if (_backdrop != null) { _backdrop.SetPhase(0); _backdrop.SetWindDirection(_windSide); }

            // Perched on the side branch farthest to the right, facing Alma (empty slots are ignored).
            _sideBranches = System.Array.FindAll(_sideBranches ?? new SolidPlatform2D[0], b => b != null);
            SolidPlatform2D perch = _centerBranch;
            foreach (var b in _sideBranches) if (perch == null || perch == _centerBranch || b.transform.position.x > perch.transform.position.x) perch = b;
            _pos = perch != null ? new Vector2(perch.transform.position.x, Top(perch) - _perchSink) : _hoverPoint;
            _sprite.flipX = _alma != null && _alma.transform.position.x < _pos.x;
            Place(_pos, 0f);
            LevelFadeIn.Focus = _pos + Vector2.up * 1.8f;
            AlmaTouchControls.InputLocked = true;
            CinematicState.Begin();
            if (Hud2D.Instance != null) Hud2D.Instance.HideNow();
            if (HudLevelTitle.Instance != null) HudLevelTitle.Instance.enabled = false;
            Enter(State.Intro);
        }

        private void OnDestroy()
        {
            HudBossAlert.Track(this, null, default);
            if (_alma != null) _alma.Respawned -= OnAlmaRespawned;
            AlmaTouchControls.InputLocked = false;
        }

        private static float Top(SolidPlatform2D b)
        {
            var col = b.GetComponent<Collider2D>();
            return col != null ? col.bounds.max.y : b.transform.position.y;
        }

        private void Place(Vector2 p, float angle)
        {
            _pos = p;
            _sprite.transform.position = p;
            _sprite.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            _sprite.transform.localScale = Vector3.one * _scale;
        }

        // Plays an animator state at `speed`, or shows the frame at `frame` (0..1) when given.
        private void Play(string state, float speed = 1f, float frame = -1f)
        {
            if (_playing != state) { _animator.Play(state, 0, 0f); _playing = state; }
            if (frame < 0f) { _animator.speed = speed; return; }
            _animator.speed = 0f;
            _animator.Play(state, 0, Mathf.Clamp(frame, 0f, 0.999f));
            _animator.Update(0f);
        }

        // ---------- state machine ----------

        // Off-screen locator: the HUD edge badge (HudBossAlert tracker, no text) points at him whenever he is off
        // screen, during the fight only (not in the intro, cinematics or the defeat) and while he is visible.
        private System.Func<Vector2> _bodyCentre;
        private void UpdateTracker()
        {
            bool on = _sprite != null && _sprite.enabled && !CinematicState.IsPlaying && _state != State.Intro && _state != State.Defeat && _state != State.Done;
            if (on && _bodyCentre == null) _bodyCentre = () => _sprite.bounds.center;
            HudBossAlert.Track(this, on ? _bodyCentre : null, _accent);
        }

        private void Update()
        {
            UpdateTracker();
            switch (_state)
            {
                case State.Intro: UpdateIntro(); break;
                case State.TakeOff: UpdateFlyTo(HoverNow(), 1f, State.Gale); break;
                case State.Gale: UpdateGale(); break;
                case State.Warn: UpdateWarn(); break;
                case State.Dive: UpdateDive(); break;
                case State.Return: UpdateFlyTo(HoverNow(), 1.1f, State.Gale); break;
                case State.Tumble: UpdateTumble(); break;
                case State.Defeat: UpdateDefeat(); break;
            }
            float flash = Mathf.Clamp01(1f - (Time.time - _hitFlashAt) / 0.5f);
            if (_state != State.Warn) _sprite.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), flash * (0.5f + 0.5f * Mathf.Cos(Time.time * 40f)));
            UpdateBreaking();
        }

        // The gale only blows while he beats it up; the boss drives the arena's GustWind2D.
        private void LateUpdate()
        {
            if (_gale == null) return;
            bool blowing = _state == State.Gale && _alma != null && !_alma.IsDead;
            _gale.Strength = blowing ? P.Wind : 0f;
            _gale.Calm = 0.6f;
        }

        // ---------- intro ----------

        private const float LinesAt = 1.1f, LineTime = 3f, TauntTime = 0.9f, ShotOut = 0.9f;
        private float _tauntAt = -1f, _cardAt, _endAt = -1f;
        private bool _cardShown;

        private void UpdateIntro()
        {
            if (_tauntAt < 0f) { _tauntAt = LinesAt + (_introLines?.Length ?? 0) * LineTime + 0.2f; _cardAt = _tauntAt + TauntTime + 0.4f; }
            float t = T;
            AlmaTouchControls.InputLocked = true;
            float w = _endAt > 0f ? 1f - Mathf.Clamp01((t - _endAt) / ShotOut) : 1f;
            if (_cam != null) _cam.SetShot(_pos + Vector2.up * 1.8f, _introZoom, w);
            if (t < _tauntAt) Play(_tauntState, 1f, 0f);
            else
            {
                float u = Mathf.Clamp01((t - _tauntAt) / TauntTime);
                Play(_tauntState, 1f, u);
            }
            if (_bubble != null)
            {
                for (int i = 0; i < (_introLines?.Length ?? 0); i++)
                {
                    float at = LinesAt + i * LineTime;
                    if (t >= at && t - Time.deltaTime < at) _bubble.Say(_introLines[i]);
                }
                if (t >= _tauntAt - 0.2f && t - Time.deltaTime < _tauntAt - 0.2f) _bubble.Hide();
            }
            if (!_cardShown && t >= _cardAt)
            {
                _cardShown = true;
                if (HudBanners.Instance != null)
                    HudBanners.Instance.Show(new Banner { Kind = BannerKind.BossIntro, Kicker = _bossKicker, Headline = _bossName, Text = _bossEpithet, Accent = _accent });
            }
            if (_cardShown && _endAt < 0f && t >= _cardAt + 0.3f && (HudBanners.Instance == null || !HudBanners.Instance.IsShowing)) _endAt = t;
            if (_endAt > 0f && t >= _endAt + ShotOut)
            {
                if (_cam != null) _cam.ClearShot();
                AlmaTouchControls.InputLocked = false;
                CinematicState.End();
                // Takes off: from perched (feet) to flying (centre).
                _pos += Vector2.up * 1.6f * _scale;
                _from = _pos;
                Enter(State.TakeOff);
            }
        }

        // ---------- gale, warning, dive ----------

        private Vector2 HoverNow()
        {
            float t = Time.time;
            return _hoverPoint + new Vector2(Mathf.Sin(t * 0.9f) * _hoverSway.x, Mathf.Sin(t * 2.1f) * _hoverSway.y);
        }

        private void FaceAlma()
        {
            if (_alma != null) _sprite.flipX = _alma.transform.position.x < _pos.x;
        }

        // Flies (eased) from where he was to `to`, then enters `next`.
        private void UpdateFlyTo(Vector2 to, float time, State next)
        {
            if (T < Time.deltaTime * 1.5f) _from = _pos;
            float u = Mathf.Clamp01(T / time), e = u * u * (3f - 2f * u);
            Play(_flyState, 1.2f);
            FaceAlma();
            Place(Vector2.Lerp(_from, to, e), 0f);
            if (u >= 1f) StartGale(next);
        }

        private void StartGale(State next)
        {
            if (next != State.Gale) { Enter(next); return; }
            _windTurned = false;
            if (_backdrop != null) _backdrop.SetWindDirection(_windSide);
            Enter(State.Gale);
        }

        // Hovers beating his wings hard: the gale blows (LateUpdate). Halfway, the wind may turn.
        private void UpdateGale()
        {
            Play(_flyState, 1.7f);
            FaceAlma();
            Place(HoverNow(), 0f);
            if (P.WindTurns && !_windTurned && T >= P.GaleTime * 0.5f)
            {
                _windTurned = true;
                _windSide = -_windSide;
                if (_backdrop != null) _backdrop.SetWindDirection(_windSide);
            }
            if (T < P.GaleTime) return;
            _divesLeft = Mathf.Max(1, P.Dives);
            _diveDir = Random.value < 0.5f ? 1 : -1;
            BeginWarn(0.6f);
        }

        private float _warnScreech;

        private void BeginWarn(float screech)
        {
            _warnScreech = screech;
            // The dive goes along a line over the branch nearest Alma: standing there she would be hit.
            float top = _centerBranch != null ? Top(_centerBranch) : 0f, best = float.MaxValue;
            float ax = _alma != null ? _alma.transform.position.x : 0f;
            foreach (var b in Branches())
            {
                var col = b.GetComponent<Collider2D>();
                float d = col != null ? Mathf.Abs(Mathf.Clamp(ax, col.bounds.min.x, col.bounds.max.x) - ax) : Mathf.Abs(b.transform.position.x - ax);
                if (d < best) { best = d; top = Top(b); }
            }
            _diveY = top + _diveHeight;
            float camX = _cam != null ? _cam.transform.position.x : 0f;
            float half = (Camera.main != null ? Camera.main.orthographicSize * Camera.main.aspect : 14f) + 5f;
            _diveFromX = camX - _diveDir * half;
            _diveToX = camX + _diveDir * half;
            _from = _pos;
            HudBossAlert.Show(() => _state == State.Warn ? new Vector2(_diveFromX, _diveY) : _pos, screech + P.LineTime + 2f * half / P.DiveSpeed, _diveShout, _accent);
            Enter(State.Warn);
        }

        private IEnumerable<SolidPlatform2D> Branches()
        {
            if (_centerBranch != null) yield return _centerBranch;
            foreach (var b in _sideBranches) if (b != null && !_broken.Contains(b)) yield return b;
        }

        // Screeches with the crest glowing, leaves the screen; the cyan line shows where he will cross.
        private void UpdateWarn()
        {
            float t = T;
            if (t < _warnScreech)
            {
                Play(_flyState, 2f);
                Place(HoverNow(), 0f);
                float pulse = 0.5f + 0.5f * Mathf.Sin(t * 30f);
                _sprite.color = Color.Lerp(Color.white, _accent, pulse * 0.7f);
            }
            else
            {
                // Climbs out of the top of the screen towards the side he will come in from.
                _sprite.color = Color.white;
                float u = Mathf.Clamp01((t - _warnScreech) / 0.5f);
                if (_from == default || u <= 0f) _from = _pos;
                Play(_flyState, 2f);
                _sprite.flipX = _diveDir > 0;
                Place(Vector2.Lerp(_from, new Vector2(_diveFromX, _hoverPoint.y + 10f), u * u), 0f);
            }
            ShowLine(t >= _warnScreech * 0.5f, t);
            if (t >= _warnScreech + P.LineTime) { _line.enabled = false; Enter(State.Dive); }
        }

        private void ShowLine(bool on, float t)
        {
            _line.enabled = on;
            if (!on) return;
            float camX = _cam != null ? _cam.transform.position.x : 0f;
            float width = (Camera.main != null ? Camera.main.orthographicSize * Camera.main.aspect * 2f : 30f) + 4f;
            _line.transform.position = new Vector3(camX, _diveY, 0f);
            _line.transform.localScale = new Vector3(width / _line.sprite.bounds.size.x, 0.12f / _line.sprite.bounds.size.y, 1f);
            float blink = 0.55f + 0.45f * Mathf.Sin(t * 14f);
            _line.color = new Color(_accent.r, _accent.g, _accent.b, blink);
        }

        private void UpdateDive()
        {
            if (T < Time.deltaTime * 1.5f) _pos = new Vector2(_diveFromX, _diveY);
            Play(_diveState);
            _sprite.flipX = _diveDir < 0;   // the art faces right
            float x = _pos.x + _diveDir * P.DiveSpeed * Time.deltaTime;
            Place(new Vector2(x, _diveY), 0f);
            if (DiveContact()) return;
            if ((x - _diveToX) * _diveDir < 0f) return;
            // Crossed without being hit.
            HudBossAlert.Hide();
            if (--_divesLeft > 0) { _diveDir = -_diveDir; _pos = new Vector2(_diveToX, _hoverPoint.y + 10f); BeginWarn(0f); return; }
            _pos = new Vector2(_diveToX, _hoverPoint.y + 4f);
            Enter(State.Return);
        }

        // Air Dash against his flight into the crest: hit. Any other touch of his body: Alma dies.
        private bool DiveContact()
        {
            if (_alma == null || _alma.IsDead || _almaCol == null) return false;
            Vector2 head = Head;
            Vector2 closest = _almaCol.ClosestPoint(head);
            bool onHead = (closest - head).sqrMagnitude < _headRadius * _headRadius * _scale * _scale;
            if (onHead && _alma.IsDashing && _alma.FacingDirection == -_diveDir) { TakeHit(); return true; }
            Vector2 c = _pos + new Vector2(_bodyOffset.x * Facing, _bodyOffset.y) * _scale;
            var body = new Rect(c - _bodySize * _scale * 0.5f, _bodySize * _scale);
            if (onHead || body.Contains(_almaCol.ClosestPoint(c))) _alma.Die();
            return false;
        }

        // ---------- hit, branches ----------

        private void TakeHit()
        {
            Hits = _phase + 1;   // one life per phase
            HudBossAlert.Hide();
            _alma.Bounce(_hitBounce, 1f, true);
            _alma.RefillAirAbilities(true, true);
            _hitFlashAt = Time.time;
            Shake(0.5f, 0.45f);
            NinjaSmoke2D.Burst(Head, 0f);
            Hit?.Invoke(Hits);
            // Tumbles onto a branch: on the last hit the centre one (he gives up), otherwise the side branch
            // farthest from Alma, which he breaks.
            bool last = _phase >= _phases.Length - 1;
            _breaking = last ? null : FarthestSideBranch();
            var target = last || _breaking == null ? _centerBranch : _breaking;
            _from = _pos;
            _tumbleTo = target != null ? new Vector2(target.transform.position.x, Top(target) - _perchSink) : new Vector2(_pos.x, _pos.y - 4f);
            Enter(State.Tumble);
        }

        private Vector2 _tumbleTo;

        private SolidPlatform2D FarthestSideBranch()
        {
            float ax = _alma != null ? _alma.transform.position.x : 0f, best = -1f;
            SolidPlatform2D pick = null;
            foreach (var b in _sideBranches)
            {
                if (b == null || _broken.Contains(b)) continue;
                float d = Mathf.Abs(b.transform.position.x - ax);
                if (d > best) { best = d; pick = b; }
            }
            return pick;
        }

        // Spirals down onto the branch; lands on it (breaking it), then flies back up for the next phase.
        private void UpdateTumble()
        {
            const float fall = 0.9f;
            float u = Mathf.Clamp01(T / fall);
            Vector2 landing = _tumbleTo + Vector2.up * 1.1f * _scale;   // his centre just above the branch
            Vector2 p = Vector2.Lerp(_from, landing, u) + Vector2.up * Mathf.Sin(u * Mathf.PI) * 1.2f;
            Play(_flyState, 0f, 0.6f);
            Place(p, -u * 720f * _diveDir);
            if (u < 1f) return;
            Shake(0.4f, 0.3f);
            NinjaSmoke2D.Burst(_tumbleTo + Vector2.up * 0.4f, 0f);
            if (_phase >= _phases.Length - 1) { BeginDefeat(); return; }
            if (_breaking != null) { _broken.Add(_breaking); BreakBranch(_breaking); }
            _phase = Mathf.Min(Hits, _phases.Length - 1);
            if (_backdrop != null) _backdrop.SetPhase(_phase);
            _windSide = -_windSide;   // the wind alternates after each hit
            Place(landing, 0f);
            Enter(State.Return);
        }

        // A broken branch loses its collider and falls into the mist, fading.
        private readonly List<(Transform t, SpriteRenderer sr, float at, Vector3 from)> _falling = new List<(Transform, SpriteRenderer, float, Vector3)>();

        private void BreakBranch(SolidPlatform2D b)
        {
            foreach (var col in b.GetComponentsInChildren<Collider2D>()) col.enabled = false;
            _falling.Add((b.transform, b.GetComponent<SpriteRenderer>(), Time.time, b.transform.position));
        }

        private void UpdateBreaking()
        {
            for (int i = _falling.Count - 1; i >= 0; i--)
            {
                var f = _falling[i];
                float t = Time.time - f.at;
                f.t.position = f.from + new Vector3(Mathf.Sin(t * 25f) * 0.05f * Mathf.Clamp01(1f - t * 3f), -0.5f * 25f * t * t, 0f);
                f.t.rotation = Quaternion.Euler(0f, 0f, -t * 25f);
                if (f.sr != null) { var c = f.sr.color; c.a = Mathf.Clamp01(1f - (t - 0.6f) / 0.8f); f.sr.color = c; }
                if (t > 1.6f) { f.t.gameObject.SetActive(false); _falling.RemoveAt(i); }
            }
        }

        // ---------- defeat ----------

        private const float DefeatLineTime = 3f;
        private bool _defeatSmoked, _portalShown;

        private void BeginDefeat()
        {
            AlmaTouchControls.Clear();
            CinematicState.Begin();
            if (_backdrop != null) TensionState.Clear(_backdrop);
            _pos = _tumbleTo;   // perched (feet) on the centre branch
            FaceAlma();
            Place(_pos, 0f);
            _defeatSmoked = _portalShown = false;
            HideAlma(true);
            FreezeAlma(true);   // hidden and still: she can't fall into the abyss (and respawn visible) mid-speech
            Enter(State.Defeat);
        }

        // Perched facing Alma (taunt frame 1), says his defeat lines and flies off in a burst of feathers.
        // «El Reencuentro» (HudEggReunion): right after he leaves, the reunion room shows Alma getting back the egg
        // he stole, before the way out opens. The defeat's clock stops while the room plays.
        private bool _reunionStarted, _reunionDone;
        private float _reunionAt, _reunionShift;
        // Back from the room (still behind its white flash): he is gone, so the portal and Alma beside it appear
        // right then and the view jumps straight to her instead of showing the empty spot where he was.
        private void BackToAlma()
        {
            if (!_portalShown) ShowPortal();
            if (_cam != null) { _cam.ClearShot(); _cam.SnapToTarget(); }
        }

        private void ShowPortal()
        {
            _portalShown = true;
            if (_exitPortal != null)
            {
                _exitPortal.transform.SetParent(null, true);
                _exitPortal.transform.position = _portalPosition;
                _exitPortal.SetActive(true);
                NinjaSmoke2D.Burst(_portalPosition);
            }
            ShowAlmaBesidePortal();
        }

        private bool Reunion(float t, float at)
        {
            if (!_reunionStarted && t >= at)
            {
                _reunionStarted = true; _reunionAt = Time.time;
                HudEggReunion.Play(_eggId, _reunionRoom, () => { _reunionDone = true; _reunionShift += Time.time - _reunionAt; BackToAlma(); });
            }
            return _reunionStarted && !_reunionDone;   // true while the room plays
        }

        private void UpdateDefeat()
        {
            AlmaTouchControls.InputLocked = true;
            float t = T - _reunionShift, lines = (_defeatLines?.Length ?? 0) * DefeatLineTime;
            float w = 1f - Mathf.Clamp01((t - (lines + 2.6f)) / ShotOut);
            if (_cam != null && !_reunionDone) _cam.SetShot(_pos + Vector2.up * 1.8f, _introZoom, Mathf.Clamp01(t / 0.9f) * w);   // after the room the view stays on Alma
            if (!_defeatSmoked)
            {
                Play(_tauntState, 1f, 0f);
                if (_bubble != null)
                    for (int i = 0; i < (_defeatLines?.Length ?? 0); i++)
                    {
                        float at = 0.8f + i * DefeatLineTime;
                        if (t >= at && t - Time.deltaTime < at) _bubble.Say(_defeatLines[i]);
                    }
            }
            if (!_defeatSmoked && t >= 0.8f + lines)
            {
                _defeatSmoked = true;
                if (_bubble != null) _bubble.Hide();
                NinjaSmoke2D.Burst(_pos + Vector2.up * 1.6f, 0.2f);
            }
            if (_defeatSmoked && t >= 0.8f + lines + 0.08f) _sprite.enabled = false;
            if (_defeatSmoked && Reunion(t, 0.8f + lines + 0.4f)) return;
            if (!_portalShown && t >= 0.8f + lines + 0.9f) ShowPortal();
            if (t >= lines + 2.6f + ShotOut + 0.8f)
            {
                if (_cam != null) _cam.ClearShot();
                AlmaTouchControls.InputLocked = false;
                CinematicState.End();
                Defeated?.Invoke();
                Enter(State.Done);
            }
        }

        private void FreezeAlma(bool freeze)
        {
            var body = _alma != null ? _alma.GetComponent<Rigidbody2D>() : null;
            if (body == null) return;
            body.linearVelocity = Vector2.zero;
            body.simulated = !freeze;
        }

        // After the speech Alma appears on the left of the portal, on the branch, with an invisible wall
        // behind her: the only way left is right, into the portal.
        private void ShowAlmaBesidePortal()
        {
            if (_alma == null) return;
            float floor = _centerBranch != null ? Top(_centerBranch) : _portalPosition.y - 1.8f;
            var portalCol = _exitPortal != null ? _exitPortal.GetComponent<Collider2D>() : null;
            float portalLeft = portalCol != null ? portalCol.bounds.min.x : _portalPosition.x - 1.1f;
            // Her collider is not centred on her pivot: place it by its edges.
            Bounds b = _almaCol != null ? _almaCol.bounds : new Bounds(_alma.transform.position, Vector3.one);
            Vector2 pivot = _alma.transform.position;
            var at = new Vector2(portalLeft - _almaPortalGap - (b.max.x - pivot.x), floor + (pivot.y - b.min.y) + 0.02f);
            _alma.transform.position = at;
            var body = _alma.GetComponent<Rigidbody2D>();
            if (body != null) body.position = at;
            _alma.SetRespawnPosition(at);
            FreezeAlma(false);
            HideAlma(false);
            NinjaSmoke2D.Burst(at + Vector2.up * 0.6f, 0f);

            var wall = new GameObject("ExitWall").AddComponent<BoxCollider2D>();
            float almaLeft = at.x + (b.min.x - pivot.x);
            wall.transform.position = new Vector3(almaLeft - 0.25f, floor + 5f, 0f);
            wall.size = new Vector2(0.4f, 10f);
        }

        private readonly List<Renderer> _almaHidden = new List<Renderer>();
        private void HideAlma(bool hide)
        {
            if (hide)
            {
                _almaHidden.Clear();
                if (_alma == null) return;
                foreach (var r in _alma.GetComponentsInChildren<Renderer>())
                    if (r.enabled) { r.enabled = false; _almaHidden.Add(r); }
                return;
            }
            foreach (var r in _almaHidden) if (r != null) r.enabled = true;
            _almaHidden.Clear();
        }

        // ---------- respawn ----------

        // Alma fell or was hit: no dive in the air; he flies back to the gale of the same phase (hits and
        // broken branches are kept).
        private void OnAlmaRespawned()
        {
            if (_state == State.Intro || IsDefeated || _state == State.Tumble) return;
            _line.enabled = false;
            _sprite.color = Color.white;
            if (_state == State.Warn || _state == State.Dive) _pos = new Vector2(_pos.x, _hoverPoint.y + 6f);
            Enter(State.Return);
        }

        private void Shake(float amp, float time)
        {
            if (_cam != null) _cam.Shake(amp, time);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.8f, 0.5f, 1f, 0.9f);
            Gizmos.DrawWireSphere(_hoverPoint, 0.4f);
            Gizmos.DrawLine(_hoverPoint - new Vector2(_hoverSway.x, 0f), _hoverPoint + new Vector2(_hoverSway.x, 0f));
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(_portalPosition, 0.3f);
        }
    }
}
