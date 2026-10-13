using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Bosses
{
    // Final boss — the Thief King, Ancestral Tyrant (Docs/Levels/World_4_Volcano/Boss_Final.md). Same structure
    // as the other bosses (intro with lines and title card, one life per phase, red flash on a hit, HudBossAlert
    // on every window, defeat with lines). The exam of the four abilities:
    // - Phase 1: Stomp (a curtain of fire runs along the floor: only a Dash goes through it) and Charge (jump
    //   over his lowered head). The charge ends against a side wall: stuck there, a Ground Pound on his back
    //   hurts him.
    // - Phase 2: crashing no longer stuns him; he adds a meteor shower (marked on the floor) and kicks a burning
    //   boulder at Alma. Roared back, it hits his jaws and he lies stunned: Ground Pound on him.
    // - Phase 3: the sides of the floor sink into lava; he holds the centre under the colossal stalactite. When
    //   the returned boulder stuns him, Alma climbs to the ledge, roars the stalactite (it cracks) and pounds
    //   the ledge: the stalactite falls on him.
    [DisallowMultipleComponent]
    public sealed class TRexBoss2D : MonoBehaviour
    {
        public enum Move { Stomp, Charge, Meteors, Kick }

        [System.Serializable]
        public struct Phase
        {
            public Move[] Cycle;          // his attacks, in order, repeated
            public float ChargeSpeed;     // units/s
            public float CurtainSpeed;    // fire curtain, units/s
            public bool CurtainBothWays;  // phase 3: one curtain each way
            public int Meteors;
            public float BoulderSpeed;    // kicked boulder, units/s (roared back it goes twice as fast)
            public bool StuckOnCrash;     // the charge against the wall leaves him stuck (the window)
            public float Window;          // seconds stuck / stunned
            public float Rest;            // pause between two attacks
        }

        [Header("Arena (world)")]
        [Tooltip("Inner faces of the side walls.")]
        [SerializeField] private float _wallLeftX = -19f;
        [SerializeField] private float _wallRightX = 19f;
        [SerializeField] private float _floorY = -5.41f;
        [Tooltip("Height the meteors fall from.")]
        [SerializeField] private float _skyY = 11f;
        [Tooltip("Where he stands in phase 3 (under the stalactite).")]
        [SerializeField] private float _centerX = 1f;
        [Tooltip("Floor pieces that sink into the lava in phase 3, and the lava that appears under them.")]
        [SerializeField] private GameObject[] _sideFloors = new GameObject[0];
        [SerializeField] private GameObject[] _sideLava = new GameObject[0];
        [Tooltip("The ledge next to the stalactite: a Ground Pound on it drops the cracked stalactite.")]
        [SerializeField] private Collider2D _ledge;
        [Tooltip("The colossal stalactite hanging over the centre (needs a Collider2D for the roar).")]
        [SerializeField] private Transform _stalactite;

        [Header("Fight")]
        [SerializeField]
        private Phase[] _phases =
        {
            new Phase { Cycle = new[] { Move.Stomp, Move.Charge }, ChargeSpeed = 11f, CurtainSpeed = 7f, Meteors = 0, BoulderSpeed = 6f, StuckOnCrash = true, Window = 4f, Rest = 0.7f },
            new Phase { Cycle = new[] { Move.Stomp, Move.Charge, Move.Meteors, Move.Kick }, ChargeSpeed = 12.5f, CurtainSpeed = 8f, Meteors = 4, BoulderSpeed = 6.5f, StuckOnCrash = false, Window = 4f, Rest = 0.6f },
            new Phase { Cycle = new[] { Move.Meteors, Move.Kick }, ChargeSpeed = 12.5f, CurtainSpeed = 9f, CurtainBothWays = true, Meteors = 6, BoulderSpeed = 7f, StuckOnCrash = false, Window = 8f, Rest = 0.5f },
        };
        [SerializeField] private float _stompBounce = 13.5f;
        [Tooltip("Bounce when Alma lands on him without a Ground Pound (no damage).")]
        [SerializeField] private float _softBounce = 9f;
        [Tooltip("Height of the fire curtain: more than a double jump, so only a Dash goes through.")]
        [SerializeField] private float _curtainHeight = 3.4f;
        [SerializeField] private float _boulderRadius = 0.9f;
        [SerializeField] private float _meteorWarning = 1f;
        [SerializeField] private string _stuckShout = "¡Atascado!";
        [SerializeField] private string _stunnedShout = "¡Aturdido!";
        [Tooltip("Phase 3 hints, one per step of the finale (shown while he is stunned).")]
        [SerializeField] private string _hintClimb = "¡Sube a la cornisa!";
        [SerializeField] private string _hintRoar = "¡Ruge a la estalactita!";
        [SerializeField] private string _hintPound = "¡Pisotón en la cornisa!";

        [Header("Hitboxes (scale 1, facing right, from the feet)")]
        [Tooltip("Lethal box while standing / stomping: x from–to, height.")]
        [SerializeField] private Vector3 _standBox = new Vector3(-1.2f, 2.2f, 3f);
        [Tooltip("Lethal box while charging (head lowered): x from–to, height.")]
        [SerializeField] private Vector3 _chargeBox = new Vector3(-1.8f, 2.7f, 2.3f);
        [Tooltip("His back when stunned (where the Ground Pound lands): x from–to, top.")]
        [SerializeField] private Vector3 _backBox = new Vector3(-0.8f, 1.6f, 3.4f);
        [Tooltip("Snout, from the feet (x).")]
        [SerializeField] private float _snoutX = 2.7f;

        [Header("Art")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [SerializeField] private string _tauntState = "Boss_TRex_Taunt";
        [SerializeField] private string _chargeState = "Boss_TRex_Charge";
        [SerializeField] private string _stompState = "Boss_TRex_Stomp";
        [SerializeField] private string _stunnedState = "Boss_TRex_Stunned";
        [Tooltip("Size of the art (1 = 45 px per unit: 5.7 u per frame).")]
        [SerializeField, Min(0.1f)] private float _scale = 1f;
        [SerializeField] private int _sortingOrder = 4;
        [Tooltip("The feet sink this much into the floor (the claws have empty space under them).")]
        [SerializeField, Min(0f)] private float _groundSink = 0.18f;
        [Tooltip("After the last hit the stalactite rests on him at this height above his feet (base taunt frame).")]
        [SerializeField, Min(0f)] private float _stalactiteRestHeight = 3.8f;
        [Tooltip("After the explosion the stalactite hits the floor and sinks until its position reaches this Y.")]
        [SerializeField] private float _stalactiteSinkY = -8.69f;
        [Tooltip("Height of the speech bubble above his feet.")]
        [SerializeField, Min(0f)] private float _bubbleHeight = 4.1f;
        [SerializeField] private Sprite _boulderSprite;
        [Tooltip("Fire curtain: additive material for the flames and embers, alpha-blended one for the smoke.")]
        [SerializeField] private Material _fireAdditive;
        [SerializeField] private Material _fireSmoke;

        [Header("Story")]
        [SerializeField, TextArea(1, 2)] private string[] _introLines = { "¿La madre ha llegado hasta mi volcán?", "¡Tus crías ahora son mías!" };
        [SerializeField, TextArea(1, 2)] private string[] _defeatLines = { "¡Imposible...! Tu valor arde más que mi fuego...", "El volcán... me reclama..." };
        [SerializeField] private Sprite _bubbleSprite, _bubbleTailSprite;
        [Header("Egg (reunion after the defeat)")]
        [SerializeField] private string _eggId = "Red";
        [Tooltip("The reunion room shown after the defeat (prefab with EggReunionRoom on its root).")]
        [SerializeField] private GameObject _reunionRoom;
        [Tooltip("The colour of the Roar: the ability that beats him.")]
        [SerializeField] private Color _accent = new Color(1f, 0.75f, 0.25f);
        [SerializeField] private string _bossName = "El Rey Ladrón";
        [SerializeField] private string _bossEpithet = "Tirano Ancestral";
        [SerializeField] private string _bossKicker = "Jefe Final";
        [SerializeField, Range(3f, 8f)] private float _introZoom = 6f;
        [SerializeField] private float _introX = 7f;

        private enum State { Intro, Rest, Stomp, ChargeWarn, Charge, Crash, Roar, Kick, Window, Hurt, Collapse, Walk, Finale, Defeat, Done }

        private State _state;
        private float _stateAt;
        private int _phase, _step;
        private AlmaMotor2D _alma;
        private Collider2D _almaCol;
        private BossBackdropPhases _backdrop;
        private SpeechBubble2D _bubble;
        private AlmaCameraFollow _cam;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private string _playing;
        private Vector2 _feet;
        private int _dir = 1;                      // where he faces / charges
        private bool _fired, _cracked, _collapsed, _boulderBack;
        private float _hitFlashAt = -10f;

        public int Hits { get; private set; }
        public bool IsVulnerable => _state == State.Window;
        public bool IsDefeated => _state == State.Defeat || _state == State.Done;
        public bool StalactiteCracked => _cracked;
        public Vector2 Feet => _feet;
        public event System.Action<int> Hit;
        public event System.Action Defeated;

        private float U => _scale;
        private Phase P => _phases[Mathf.Clamp(_phase, 0, _phases.Length - 1)];
        private float T => Time.time - _stateAt;

        private void Enter(State s)
        {
            if (_state == State.Window && s != State.Window) { HudBossAlert.Hide(); HideHints(); }
            _state = s; _stateAt = Time.time; _fired = false;
        }

        // ---------- setup ----------

        private void Start()
        {
            _alma = FindAnyObjectByType<AlmaMotor2D>();
            _almaCol = _alma != null ? _alma.GetComponent<Collider2D>() : null;
            _backdrop = FindAnyObjectByType<BossBackdropPhases>();
            _cam = FindAnyObjectByType<AlmaCameraFollow>();
            if (_alma != null)
            {
                _alma.Respawned += OnAlmaRespawned;
                _alma.GroundPoundLanded += OnGroundPoundLanded;
            }
            _sprite = new GameObject("Sprite").AddComponent<SpriteRenderer>();
            _sprite.transform.SetParent(transform, false);
            _sprite.sortingOrder = _sortingOrder;
            _animator = _sprite.gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = _animatorController;
            if (_introLines != null && _introLines.Length > 0 && HudText.Ready)
            {
                // Anchored to an empty point over his head (the sprite frame is taller than him).
                var anchor = new GameObject("BubbleAnchor").transform;
                anchor.SetParent(_sprite.transform, false);
                anchor.localPosition = new Vector3(0f, _bubbleHeight, 0f);
                _bubble = SpeechBubble2D.Create(anchor, _bubbleSprite, _bubbleTailSprite, _bossName, _accent);
            }
            if (_stalactite != null)
            {
                _stalactiteHome = _stalactite.position;
                var target = _stalactite.GetComponent<TRexRoarTarget2D>();
                if (target == null) target = _stalactite.gameObject.AddComponent<TRexRoarTarget2D>();
                target.Roared += OnStalactiteRoared;
            }
            foreach (var lava in _sideLava) if (lava != null) lava.SetActive(false);
            if (_backdrop != null) _backdrop.SetPhase(0);

            _feet = new Vector2(_introX, _floorY);
            FaceAlma();
            Place();
            LevelFadeIn.Focus = _feet + Vector2.up * 2f;
            AlmaTouchControls.InputLocked = true;
            CinematicState.Begin();
            if (Hud2D.Instance != null) Hud2D.Instance.HideNow();
            if (HudLevelTitle.Instance != null) HudLevelTitle.Instance.enabled = false;
            Enter(State.Intro);
        }

        private void OnDestroy()
        {
            HudBossAlert.Track(this, null, default);
            if (_alma != null) { _alma.Respawned -= OnAlmaRespawned; _alma.GroundPoundLanded -= OnGroundPoundLanded; }
            if (_cam != null) _cam.ClearFocus();
            AlmaTouchControls.InputLocked = false;
        }

        private void FaceAlma()
        {
            if (_alma != null) _dir = _alma.transform.position.x < _feet.x ? -1 : 1;
            if (_sprite != null) _sprite.flipX = _dir < 0;   // the art faces right
        }

        private void Place(float shake = 0f)
        {
            _sprite.transform.position = _feet + Vector2.down * _groundSink + (shake > 0f ? new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake) * 0.5f) : Vector2.zero);
            _sprite.transform.localScale = Vector3.one * U;
        }

        // Plays a state normally, or freezes it at `frame` (0..1).
        private void Play(string state, float frame = -1f)
        {
            if (_playing != state) { _animator.Play(state, 0, 0f); _playing = state; }
            if (frame < 0f) { _animator.speed = 1f; return; }
            _animator.speed = 0f;
            _animator.Play(state, 0, Mathf.Clamp(frame, 0f, 0.999f));
            _animator.Update(0f);
        }

        // A box from the feet, given for scale 1 facing right.
        private Rect Box(Vector3 b)
        {
            float a = b.x * U * _dir, c = b.y * U * _dir;
            return new Rect(_feet.x + Mathf.Min(a, c), _feet.y, Mathf.Abs(c - a), b.z * U);
        }

        // ---------- state machine ----------

        private bool _puffPending;

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
            if (_puffPending) { _puffPending = false; NinjaSmoke2D.Burst(_feet + Vector2.up * 1.5f, 0f); }
            switch (_state)
            {
                case State.Intro: UpdateIntro(); break;
                case State.Rest: UpdateRest(); break;
                case State.Stomp: UpdateStomp(); break;
                case State.ChargeWarn: UpdateChargeWarn(); break;
                case State.Charge: UpdateCharge(); break;
                case State.Crash: UpdateCrash(); break;
                case State.Roar: UpdateRoar(); break;
                case State.Kick: UpdateKick(); break;
                case State.Window: UpdateWindow(); break;
                case State.Hurt: UpdateHurt(); break;
                case State.Collapse: UpdateCollapse(); break;
                case State.Walk: UpdateWalk(); break;
                case State.Finale: UpdateFinale(); break;
                case State.Defeat: UpdateDefeat(); break;
            }
            UpdateCurtains();
            UpdateMeteors();
            UpdateBoulder();
            UpdateStars(_state == State.Window);
            UpdateLedgeGlow();
            CheckContact();
        }

        // ---------- intro ----------

        private const float LinesAt = 1.1f, LineTime = 3f, TauntTime = 1.1f, ShotOut = 0.9f;
        private float _tauntAt = -1f, _cardAt, _endAt = -1f;
        private bool _cardShown;

        private void UpdateIntro()
        {
            if (LevelFadeIn.IsOpening) _stateAt = Time.time;   // the intro clock starts once the opening iris is done
            if (_tauntAt < 0f) { _tauntAt = LinesAt + (_introLines?.Length ?? 0) * LineTime + 0.2f; _cardAt = _tauntAt + TauntTime + 0.4f; }
            float t = T;
            AlmaTouchControls.InputLocked = true;
            float w = _endAt > 0f ? 1f - Mathf.Clamp01((t - _endAt) / ShotOut) : 1f;
            if (_cam != null) _cam.SetShot(_feet + Vector2.up * 2f, _introZoom, w);
            if (t < _tauntAt) Play(_tauntState, 0f);
            else RoarOnce(t - _tauntAt);
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
                if (_cam != null) _cam.ClearShot();   // the fight uses the normal camera (ahead of where Alma looks)
                AlmaTouchControls.InputLocked = false;
                CinematicState.End();
                _step = 0;
                Enter(State.Rest);
            }
        }

        // The Taunt once: a roar with the camera shaking at its peak.
        private bool _roared;
        private void RoarOnce(float t)
        {
            float u = Mathf.Clamp01(t / TauntTime);
            Play(_tauntState, u);
            if (u < 0.5f) _roared = false;
            else if (!_roared) { _roared = true; Shake(0.3f, 0.5f); }
        }

        // ---------- the cycle ----------

        private void UpdateRest()
        {
            Play(_tauntState, 0f);
            if (T < P.Rest || _meteors.Count > 0) return;   // one attack at a time: the meteors land first
            FaceAlma();
            var cycle = P.Cycle;
            Move m = cycle[_step % cycle.Length];
            _step++;
            switch (m)
            {
                case Move.Stomp: Enter(State.Stomp); break;
                case Move.Charge: Enter(State.ChargeWarn); break;
                case Move.Meteors: Enter(State.Roar); break;
                case Move.Kick: Enter(State.Kick); break;
            }
        }

        // Stomp: raises the leg and slams it (frame 5): a curtain of fire runs along the floor.
        private const float StompTime = 0.67f, StompImpact = 4f / 8f;
        private void UpdateStomp()
        {
            float u = T / StompTime;
            Play(_stompState, u);
            if (!_fired && u >= StompImpact)
            {
                _fired = true;
                Shake(0.35f, 0.35f);
                NinjaSmoke2D.Burst(_feet + new Vector2(_dir * 2f * U, 0.2f), 0f);
                SpawnCurtain(_dir);
                if (P.CurtainBothWays) SpawnCurtain(-_dir);
            }
            if (u >= 1f) Enter(State.Rest);
        }

        // Charge: scrapes the floor with his eyes flashing, then runs to the wall in front of him.
        private const float ChargeWarnTime = 0.8f;
        private void UpdateChargeWarn()
        {
            Play(_chargeState, 0f);
            float pulse = 0.5f + 0.5f * Mathf.Sin(T * 30f);
            _sprite.color = Color.Lerp(Color.white, new Color(1f, 0.45f, 0.35f), pulse * 0.7f);
            Place(0.04f);
            if (T < ChargeWarnTime) return;
            _sprite.color = Color.white;
            Place();
            Enter(State.Charge);
        }

        private void UpdateCharge()
        {
            Play(_chargeState);
            _feet.x += _dir * P.ChargeSpeed * Time.deltaTime;
            float wall = _dir > 0 ? _wallRightX : _wallLeftX;
            bool hit = (_feet.x + _dir * _snoutX * U - wall) * _dir >= 0f;
            if (hit) _feet.x = wall - _dir * _snoutX * U;
            Place();
            if (!hit) return;
            Shake(0.55f, 0.45f);
            NinjaSmoke2D.Burst(new Vector2(wall, _feet.y + 1.8f * U), 0f);
            if (P.StuckOnCrash) OpenWindow(_stuckShout);
            else Enter(State.Crash);
        }

        // Shakes the crash off (phases 2 and 3).
        private void UpdateCrash()
        {
            Play(_stunnedState);
            if (T >= 0.7f) Enter(State.Rest);
        }

        // Roar: the meteors fall, each one marked on the floor first.
        private void UpdateRoar()
        {
            RoarOnce(T);
            if (!_fired && T >= TauntTime * 0.5f)
            {
                _fired = true;
                float ax = _alma != null ? _alma.transform.position.x : 0f;
                for (int i = 0; i < P.Meteors; i++)
                {
                    float x = i == 0 ? ax : Random.Range(_wallLeftX + 1f, _wallRightX - 1f);
                    SpawnMeteor(x, i * 0.3f);
                }
            }
            if (T >= TauntTime + 0.2f) Enter(State.Rest);
        }

        // Kick: the stomp animation sends a burning boulder rolling at Alma.
        private void UpdateKick()
        {
            float u = T / StompTime;
            Play(_stompState, u);
            if (!_fired && u >= StompImpact)
            {
                _fired = true;
                Shake(0.25f, 0.25f);
                SpawnBoulder();
            }
            if (u >= 1f && _boulder == null) Enter(State.Rest);
            else if (u >= 1f) Play(_tauntState, 0f);
        }

        // ---------- windows ----------

        private void OpenWindow(string shout)
        {
            float time = P.Window;
            bool finale = _phase >= _phases.Length - 1;
            _hintStep = -1;
            if (finale) HudBossAlert.Show(HintTarget, time, null, _accent);   // the edge badge points at the current step
            else HudBossAlert.Show(() => (Vector2)BackTop(), time, shout, _accent);
            Enter(State.Window);
        }

        private Vector2 BackTop()
        {
            Rect b = Box(_backBox);
            return new Vector2(b.center.x, _feet.y + _backBox.z * U);
        }

        private void UpdateWindow()
        {
            Play(_stunnedState);
            bool finale = _phase >= _phases.Length - 1;
            if (StompCheck(!finale)) return;
            if (finale) UpdateHints();
            if (T >= P.Window) { _sprite.color = Color.white; Enter(State.Rest); }
        }

        // ---------- phase 3 hints ----------
        // While he is stunned under the stalactite, one step at a time: climb to the ledge → roar at the
        // stalactite → Ground Pound on the ledge. Each step shouts once and a bouncing arrow marks where to go.

        private int _hintStep = -1;
        private SpriteRenderer _hintArrow;

        private int HintStep()
        {
            bool onLedge = _ledge != null && _almaCol != null && _alma != null && !_alma.IsDead && LevelPieceUtility.IsOnTop(_almaCol, _ledge, 0.3f);
            if (!onLedge) return 0;
            return _cracked ? 2 : 1;
        }

        private Vector2 HintTarget()
        {
            if (_ledge == null) return BackTop();
            Bounds l = _ledge.bounds;
            if (_hintStep == 1 && _stalactite != null)
            {
                // Beside the stalactite, above Alma's head on the ledge.
                float y = l.max.y + 2.9f;
                return new Vector2(StalactiteLeft() - 0.6f, y);
            }
            return new Vector2(l.center.x, l.max.y + 1.3f);
        }

        private float StalactiteLeft()
        {
            var r = _stalactite.GetComponentInChildren<SpriteRenderer>();
            return r != null ? r.bounds.min.x : _stalactite.position.x;
        }

        private void UpdateHints()
        {
            int step = HintStep();
            if (step != _hintStep)
            {
                _hintStep = step;
                string text = step == 0 ? _hintClimb : step == 1 ? _hintRoar : _hintPound;
                if (!string.IsNullOrEmpty(text)) HudBanners.Show(BannerKind.Shout, text, null, _accent);
            }
            if (_hintArrow == null)
            {
                _hintArrow = new GameObject("HintArrow").AddComponent<SpriteRenderer>();
                _hintArrow.sprite = MakeArrow();
                _hintArrow.sortingOrder = _sortingOrder + 10;
            }
            _hintArrow.enabled = true;
            // Points down at the ledge (climb, pound) or right at the stalactite (roar), bouncing.
            float bob = Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.35f;
            Vector2 at = HintTarget();
            if (_hintStep == 1)
            {
                _hintArrow.transform.position = at + new Vector2(-bob, 0f);
                _hintArrow.transform.rotation = Quaternion.Euler(0f, 0f, 90f);   // pointing right
            }
            else
            {
                _hintArrow.transform.position = at + new Vector2(0f, 0.6f + bob);
                _hintArrow.transform.rotation = Quaternion.identity;               // pointing down
            }
            _hintArrow.transform.localScale = Vector3.one * (0.9f + 0.08f * Mathf.Sin(Time.time * 12f)) / _hintArrow.sprite.bounds.size.x;
            _hintArrow.color = Color.Lerp(Color.white, _accent, 0.3f + 0.3f * Mathf.Sin(Time.time * 10f));
        }

        private void HideHints()
        {
            if (_hintArrow != null) _hintArrow.enabled = false;
        }

        // Arrow pointing down: accent fill with a dark outline, like the rest of the HUD marks.
        private static Sprite MakeArrow()
        {
            const int n = 64; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            Vector2[] poly = { new Vector2(32, 4), new Vector2(60, 34), new Vector2(42, 34), new Vector2(42, 60), new Vector2(22, 60), new Vector2(22, 34), new Vector2(4, 34) };
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f); bool inside = false;
                for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                    if ((poly[i].y > p.y) != (poly[j].y > p.y) && p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x) inside = !inside;
                float d = float.MaxValue;
                for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                {
                    Vector2 e = poly[i] - poly[j]; float t = Mathf.Clamp01(Vector2.Dot(p - poly[j], e) / e.sqrMagnitude);
                    d = Mathf.Min(d, Vector2.Distance(p, poly[j] + e * t));
                }
                Color c = !inside ? Color.clear : d < 4f ? new Color(0.2f, 0.08f, 0.03f) : Color.white;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        // A Ground Pound on his back hurts him (not in phase 3: only the stalactite does); a plain landing
        // just bounces Alma off.
        private bool StompCheck(bool canHurt)
        {
            if (_alma == null || _alma.IsDead || _almaCol == null || _alma.Velocity.y > 0.5f) return false;
            Bounds a = _almaCol.bounds;
            Rect back = Box(_backBox);
            float top = _feet.y + _backBox.z * U;
            bool over = a.max.x > back.xMin && a.min.x < back.xMax;
            if (!over || a.min.y > top + 0.4f || a.min.y < top - 1.6f) return false;
            if (canHurt && _alma.IsGroundPounding)
            {
                _alma.Bounce(_stompBounce, 1f, true);
                TakeHit(new Vector2(a.center.x, top));
                return true;
            }
            if (a.min.y >= top - 0.6f) _alma.Bounce(_softBounce, 1f, false);
            return false;
        }

        private void TakeHit(Vector2 at)
        {
            Hits = _phase + 1;   // one life per phase
            _hitFlashAt = Time.time;
            Shake(0.55f, 0.5f);
            NinjaSmoke2D.Burst(at, 0f);
            Hit?.Invoke(Hits);
            ClearSpawned();
            Enter(State.Hurt);
        }

        private void UpdateHurt()
        {
            float u = Mathf.Clamp01(T / 0.7f);
            float pulse = 0.5f + 0.5f * Mathf.Cos(T * 40f);
            _sprite.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), (1f - u) * pulse);
            bool last = _phase >= _phases.Length - 1;
            if (last)
            {
                // Crushed by the stalactite: he straightens up (base frame, he is about to talk) and the
                // stalactite settles on his back.
                Play(_tauntState, 0f);
                if (_stalactite != null) _stalactite.position += Vector3.up * (StalactiteRestBottom() - StalactiteBottom()) * Mathf.Clamp01(Time.deltaTime * 10f);
            }
            else Play(_stunnedState);
            Place((1f - u) * 0.12f);
            if (u < 1f) return;
            _sprite.color = Color.white;
            Place();
            if (_phase >= _phases.Length - 1) { BeginDefeat(); return; }
            _phase = Mathf.Min(Hits, _phases.Length - 1);
            if (_backdrop != null) _backdrop.SetPhase(_phase);
            _step = 0;
            if (_phase == _phases.Length - 1) Enter(State.Collapse);
            else Enter(State.Rest);
        }

        // ---------- phase 3 ----------

        // Roars, the floor sides sink into the lava, then he walks to the centre, under the stalactite.
        private void UpdateCollapse()
        {
            RoarOnce(Mathf.Min(T, TauntTime));
            if (!_fired && T >= TauntTime * 0.5f) { _fired = true; Shake(0.6f, 1.6f); Collapse(true); }
            if (T >= TauntTime + 0.6f) Enter(State.Walk);
        }

        private readonly List<(Transform t, Vector3 home)> _sunk = new List<(Transform, Vector3)>();
        private float _sinkAt = -1f;

        private void Collapse(bool on)
        {
            _collapsed = on;
            if (on)
            {
                _sunk.Clear();
                foreach (var f in _sideFloors)
                    if (f != null) { _sunk.Add((f.transform, f.transform.position)); foreach (var c in f.GetComponentsInChildren<Collider2D>()) c.enabled = false; NinjaSmoke2D.Burst((Vector2)f.transform.position + Vector2.up * 0.5f, 0f); }
                _sinkAt = Time.time;
            }
            foreach (var lava in _sideLava) if (lava != null) lava.SetActive(on);
        }

        private void UpdateSinking()
        {
            if (_sinkAt < 0f) return;
            float u = Mathf.Clamp01((Time.time - _sinkAt) / 1.4f);
            foreach (var (t, home) in _sunk)
            {
                if (t == null) continue;
                t.position = home + Vector3.down * (2.5f * u * u);
                foreach (var r in t.GetComponentsInChildren<SpriteRenderer>()) { var c = r.color; c.a = 1f - u; r.color = c; }
                if (u >= 1f) t.gameObject.SetActive(false);
            }
            if (u >= 1f) _sinkAt = -1f;
        }

        private void UpdateWalk()
        {
            float target = _centerX;
            _dir = target > _feet.x ? 1 : -1;
            _sprite.flipX = _dir < 0;
            Play(_chargeState);
            _animator.speed = 0.6f;
            _feet.x = Mathf.MoveTowards(_feet.x, target, 4.5f * Time.deltaTime);
            Place();
            if (Mathf.Abs(_feet.x - target) > 0.01f) return;
            _animator.speed = 1f;
            Enter(State.Rest);
        }

        // Roared during the finale window, the stalactite cracks.
        private void OnStalactiteRoared(Vector2 origin, int direction)
        {
            if (_state != State.Window || _phase < _phases.Length - 1 || _cracked) return;
            _cracked = true;
            Shake(0.3f, 0.4f);
            NinjaSmoke2D.Burst(_stalactiteHome + Vector3.down * 1f, 0f);
            var r = _stalactite.GetComponentInChildren<SpriteRenderer>();
            if (r != null) r.color = Color.Lerp(r.color, _accent, 0.45f);
        }

        // A Ground Pound on the ledge drops the cracked stalactite on him.
        private void OnGroundPoundLanded()
        {
            if (_state != State.Window || _phase < _phases.Length - 1 || !_cracked || _ledge == null || _almaCol == null) return;
            if (!LevelPieceUtility.IsOnTop(_almaCol, _ledge, 0.25f)) return;
            Shake(0.5f, 0.5f);
            NinjaSmoke2D.Burst(_ledge.bounds.center + Vector3.up * _ledge.bounds.extents.y, 0f);
            Enter(State.Finale);
        }

        private Vector3 _stalactiteHome;

        private void UpdateFinale()
        {
            Play(_stunnedState);
            if (_stalactite == null) { TakeHit(BackTop()); return; }
            // Free fall until its tip reaches his back.
            float fall = 0.5f * 40f * T * T;
            _stalactite.position = new Vector3(_stalactiteHome.x, _stalactiteHome.y - fall, _stalactiteHome.z);
            float top = _feet.y + _backBox.z * U;
            if (StalactiteBottom() > top) return;
            _stalactite.position += Vector3.up * (top - StalactiteBottom());
            TakeHit(new Vector2(_stalactiteHome.x, top));
        }

        private float StalactiteBottom()
        {
            var r = _stalactite.GetComponentInChildren<SpriteRenderer>();
            return r != null ? r.bounds.min.y : _stalactite.position.y;
        }

        private void UpdateLedgeGlow()
        {
            if (_ledge == null) return;
            var r = _ledge.GetComponentInChildren<SpriteRenderer>();
            if (r == null) return;
            bool ready = _state == State.Window && _phase >= _phases.Length - 1;
            float glow = ready ? (_cracked ? 0.6f : 0.25f) * (0.5f + 0.5f * Mathf.Sin(Time.time * 8f)) : 0f;
            r.color = Color.Lerp(Color.white, _accent, glow);
        }

        // ---------- defeat ----------

        private const float DefeatLineTime = 3f, ChargeUp = 1.1f;
        private bool _exploded, _stalactiteLanded;
        private float _stalactiteVy, _stalactiteSinkBrake;
        private ParticleSystem _stalactiteTrail;
        private int _pops;

        private float StalactiteRestBottom() => _feet.y + _stalactiteRestHeight * U;

        private void BeginDefeat()
        {
            AlmaTouchControls.Clear();
            ClearSpawned();
            CinematicState.Begin();
            if (_cam != null) _cam.ClearFocus();
            if (_backdrop != null) TensionState.Clear(_backdrop);
            FaceAlma();
            Place();
            _exploded = _stalactiteLanded = false;
            _stalactiteVy = 0f; _pops = 0;
            if (_stalactite != null) _stalactite.position += Vector3.up * (StalactiteRestBottom() - StalactiteBottom());
            HideAlma(true);
            FreezeAlma(true);
            Enter(State.Defeat);
        }

        // Pinned under the stalactite he says his last lines (base frame), swells with magma flashing through
        // his cracks and explodes; the stalactite drops to the floor and stays there.
        // «El Reencuentro» (HudEggReunion): right after he leaves, the reunion room shows Alma getting back the egg
        // he stole, before the way out opens. The defeat's clock stops while the room plays.
        private bool _reunionStarted, _reunionDone;
        private float _reunionAt, _reunionShift;
        // Back from the room (still behind its white flash): he is gone, so the view jumps straight to Alma
        // instead of showing the empty spot where he was and gliding over.
        private void BackToAlma()
        {
            HideAlma(false);
            if (_cam != null) { _cam.ClearShot(); _cam.SnapToTarget(); }
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
            float boomAt = 0.8f + lines + ChargeUp, endAt = boomAt + 3.2f;
            float w = 1f - Mathf.Clamp01((t - endAt) / ShotOut);
            if (_cam != null && !_reunionDone) _cam.SetShot(_feet + Vector2.up * 2f, _introZoom, Mathf.Clamp01(t / 0.9f) * w);   // after the room the view stays on Alma
            if (t < 0.8f + lines)
            {
                Play(_tauntState, 0f);
                if (_bubble != null)
                    for (int i = 0; i < (_defeatLines?.Length ?? 0); i++)
                    {
                        float at = 0.8f + i * DefeatLineTime;
                        if (t >= at && t - Time.deltaTime < at) _bubble.Say(_defeatLines[i]);
                    }
                return;
            }
            if (t < boomAt)
            {
                // Charge-up: shaking harder and harder, flashing between red and white, swelling; small
                // bursts pop out of his body.
                if (_bubble != null) _bubble.Hide();
                float u = (t - (0.8f + lines)) / ChargeUp;
                Play(_tauntState, 0f);
                float flash = 0.5f + 0.5f * Mathf.Sin(t * Mathf.Lerp(18f, 60f, u));
                _sprite.color = Color.Lerp(new Color(1f, 0.35f, 0.2f), Color.white, flash);
                Place(0.05f + 0.15f * u);
                _sprite.transform.localScale = Vector3.one * U * (1f + 0.08f * u * u);
                if (_pops < 3 && u >= _pops * 0.3f)
                {
                    _pops++;
                    Shake(0.2f + 0.1f * _pops, 0.3f);
                    Vector2 at = _feet + new Vector2(Random.Range(-1.5f, 1.5f) * U, Random.Range(1f, 3.2f) * U);
                    Explosion(at, 0.35f);
                }
                return;
            }
            if (!_exploded)
            {
                _exploded = true;
                Shake(0.9f, 1.2f);
                Explosion(_feet + Vector2.up * 2f * U, 1f);
                NinjaSmoke2D.Burst(_feet + Vector2.up * 1.5f, 0f);
                _sprite.enabled = false;
                _sprite.transform.localScale = Vector3.one * U;
            }
            // The stalactite drops hard (already moving, heavy gravity) with a trail of dust, smashes into the
            // floor (dust cloud, obsidian debris, sparks, big shake) and drives itself in, braking, until its
            // position reaches `Stalactite Sink Y`.
            if (_stalactite != null && _stalactiteLanded && _stalactite.position.y > _stalactiteSinkY)
            {
                _stalactiteVy = Mathf.Max(0.5f, _stalactiteVy - _stalactiteSinkBrake * Time.deltaTime);
                float y = Mathf.MoveTowards(_stalactite.position.y, _stalactiteSinkY, _stalactiteVy * Time.deltaTime);
                _stalactite.position = new Vector3(_stalactite.position.x, y, _stalactite.position.z);
            }
            if (_stalactite != null && !_stalactiteLanded)
            {
                if (_stalactiteVy == 0f) { _stalactiteVy = -12f; StartStalactiteTrail(); }
                _stalactiteVy -= 120f * Time.deltaTime;
                _stalactite.position += Vector3.up * _stalactiteVy * Time.deltaTime;
                if (_stalactiteTrail != null) _stalactiteTrail.transform.position = _stalactite.position;
                if (StalactiteBottom() <= _floorY)
                {
                    _stalactiteLanded = true;
                    if (_stalactiteTrail != null)
                    {
                        _stalactiteTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        StartCoroutine(CleanUp(_stalactiteTrail.gameObject, new List<ParticleSystem> { _stalactiteTrail }, 1.5f));
                        _stalactiteTrail = null;
                    }
                    // From here _stalactiteVy is the (positive) speed it drives into the floor with; it brakes
                    // evenly so it stops right at Sink Y.
                    float depth = Mathf.Max(0.1f, _stalactite.position.y - _stalactiteSinkY);
                    _stalactiteVy = Mathf.Clamp(-_stalactiteVy * 0.6f, 8f, 25f);
                    _stalactiteSinkBrake = _stalactiteVy * _stalactiteVy / (2f * depth);
                    Shake(0.8f, 0.6f);
                    StalactiteImpact(new Vector2(_stalactite.position.x, _floorY));
                }
            }
            if (_stalactiteLanded && Reunion(t, boomAt + 1.6f)) return;
            if (t < endAt + ShotOut + 0.3f) return;
            HideAlma(false);
            FreezeAlma(false);
            if (_cam != null) _cam.ClearShot();
            AlmaTouchControls.InputLocked = false;
            CinematicState.End();
            Defeated?.Invoke();
            Enter(State.Done);
        }

        // An explosion made of particles: a white-hot flash, a fireball of additive flames, obsidian debris
        // with gravity, a spray of stretched embers and a slow cloud of black smoke. `size` 1 = the big one.
        private void Explosion(Vector2 at, float size)
        {
            var root = new GameObject("Explosion").transform;
            root.position = at;
            int o = _sortingOrder + 6;
            var systems = new List<ParticleSystem>();
            ParticleSystem Burst(string name, Material mat, Texture2D tex, int count, int order, float lifeMin, float lifeMax, float sizeMin, float sizeMax, float speedMin, float speedMax)
                => OneShot(root, systems, name, mat, tex, count, order, lifeMin, lifeMax, sizeMin * size, sizeMax * size, speedMin * size, speedMax * size, 0.6f * size, 360f);

            var flash = Burst("Flash", _fireAdditive, HazardFx.Puff(), 2, o + 3, 0.25f, 0.35f, 7f, 9f, 0f, 0f);
            var fl = flash.colorOverLifetime; fl.color = Ramp(new[] { Color.white, new Color(1f, 0.8f, 0.4f), new Color(1f, 0.45f, 0.1f) }, new[] { 1f, 0.6f, 0f });
            HazardFx.SetSizeOverLifetime(flash, 0.4f, 1.2f);

            var fire = Burst("Fireball", _fireAdditive, HazardFx.Puff(), Mathf.RoundToInt(90 * size) + 10, o + 2, 0.6f, 1.2f, 1.2f, 2.4f, 2f, 9f);
            var fc = fire.colorOverLifetime;
            fc.color = Ramp(new[] { new Color(1f, 0.95f, 0.75f), new Color(1f, 0.65f, 0.18f), new Color(1f, 0.3f, 0.05f), new Color(0.45f, 0.06f, 0.02f) }, new[] { 1f, 0.9f, 0.5f, 0f });
            HazardFx.SetSizeOverLifetime(fire, 0.6f, 1.3f);
            var fld = fire.limitVelocityOverLifetime; fld.enabled = true; fld.limit = 0.5f; fld.dampen = 0.15f;
            Noise(fire, 1f, 1.2f);

            var debris = Burst("Debris", _fireSmoke, HazardFx.Chunk(), Mathf.RoundToInt(28 * size) + 4, o + 4, 1.2f, 2f, 0.25f, 0.6f, 6f, 14f);
            var dm = debris.main; dm.gravityModifier = 2.4f;
            dm.startColor = new ParticleSystem.MinMaxGradient(new Color(0.16f, 0.12f, 0.14f), new Color(0.9f, 0.35f, 0.1f));
            var dr = debris.rotationOverLifetime; dr.enabled = true; dr.z = new ParticleSystem.MinMaxCurve(-8f, 8f);
            var dc = debris.colorOverLifetime; dc.color = Ramp(new[] { Color.white, Color.white }, new[] { 1f, 1f, 0f });

            var embers = Burst("Embers", _fireAdditive, HazardFx.Puff(), Mathf.RoundToInt(140 * size) + 10, o + 5, 0.6f, 1.6f, 0.06f, 0.14f, 5f, 16f);
            var em = embers.main; em.gravityModifier = 0.7f;
            em.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.5f), new Color(1f, 0.45f, 0.1f));
            var er = embers.GetComponent<ParticleSystemRenderer>(); er.renderMode = ParticleSystemRenderMode.Stretch; er.velocityScale = 0.05f; er.lengthScale = 1.5f;
            HazardFx.SetSizeOverLifetime(embers, 1f, 0.2f);
            var eld = embers.limitVelocityOverLifetime; eld.enabled = true; eld.limit = 2f; eld.dampen = 0.08f;

            var smoke = Burst("Smoke", _fireSmoke, HazardFx.Puff(), Mathf.RoundToInt(30 * size) + 4, o + 1, 2f, 3.2f, 1.6f, 3f, 0.8f, 3f);
            var sm = smoke.main; sm.gravityModifier = -0.08f;
            var sc = smoke.colorOverLifetime;
            sc.color = Ramp(new[] { new Color(0.4f, 0.2f, 0.1f), new Color(0.12f, 0.1f, 0.1f), new Color(0.07f, 0.06f, 0.06f) }, new[] { 0.7f, 0.6f, 0f });
            HazardFx.SetSizeOverLifetime(smoke, 0.6f, 1.8f);
            var sld = smoke.limitVelocityOverLifetime; sld.enabled = true; sld.limit = 0.3f; sld.dampen = 0.1f;
            Noise(smoke, 0.5f, 0.6f);

            StartCoroutine(CleanUp(root.gameObject, systems, 4f));
        }

        // One burst of particles under `root` (added to `systems` for CleanUp), from a circle of `radius`;
        // `arc` 180 = only upwards (from a floor).
        private ParticleSystem OneShot(Transform root, List<ParticleSystem> systems, string name, Material mat, Texture2D tex, int count, int order,
            float lifeMin, float lifeMax, float sizeMin, float sizeMax, float speedMin, float speedMax, float radius, float arc)
        {
            var ps = HazardFx.CreateParticles(name, root, mat != null ? mat : _sprite.sharedMaterial, tex, count, _sprite.sortingLayerID, order);
            var m = ps.main;
            m.loop = false;
            m.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            m.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            m.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var sh = ps.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = radius; sh.arc = arc;
            var e = ps.emission; e.enabled = true; e.rateOverTime = 0f; e.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            ps.Clear(); ps.Play();
            systems.Add(ps);
            return ps;
        }

        // The stalactite smashing into the floor at `at`: a heavy dust cloud rolling out to both sides, obsidian
        // debris thrown up with gravity, a spray of sparks and a quick flash at the point of impact.
        private void StalactiteImpact(Vector2 at)
        {
            var root = new GameObject("StalactiteImpact").transform;
            root.position = at;
            int o = _sortingOrder + 6;
            var systems = new List<ParticleSystem>();

            var flash = OneShot(root, systems, "Flash", _fireAdditive, HazardFx.Puff(), 1, o + 3, 0.15f, 0.2f, 4f, 5f, 0f, 0f, 0.1f, 360f);
            var fl = flash.colorOverLifetime; fl.color = Ramp(new[] { new Color(1f, 0.85f, 0.6f), new Color(1f, 0.5f, 0.2f) }, new[] { 0.8f, 0f });

            var dust = OneShot(root, systems, "Dust", _fireSmoke, HazardFx.Puff(), 34, o + 1, 1.2f, 2.2f, 1.2f, 2.6f, 4f, 11f, 1.2f, 180f);
            var dm = dust.main; dm.gravityModifier = -0.04f;
            var dc = dust.colorOverLifetime; dc.color = Ramp(new[] { new Color(0.35f, 0.26f, 0.22f), new Color(0.18f, 0.15f, 0.15f), new Color(0.1f, 0.09f, 0.09f) }, new[] { 0.85f, 0.6f, 0f });
            HazardFx.SetSizeOverLifetime(dust, 0.5f, 1.7f);
            var dl = dust.limitVelocityOverLifetime; dl.enabled = true; dl.limit = 0.4f; dl.dampen = 0.12f;
            Noise(dust, 0.4f, 0.5f);

            var debris = OneShot(root, systems, "Debris", _fireSmoke, HazardFx.Chunk(), 36, o + 4, 1f, 1.8f, 0.2f, 0.55f, 6f, 15f, 0.8f, 180f);
            var bm = debris.main; bm.gravityModifier = 2.6f;
            bm.startColor = new ParticleSystem.MinMaxGradient(new Color(0.12f, 0.1f, 0.12f), new Color(0.35f, 0.3f, 0.32f));
            var br = debris.rotationOverLifetime; br.enabled = true; br.z = new ParticleSystem.MinMaxCurve(-10f, 10f);
            var bc = debris.colorOverLifetime; bc.color = Ramp(new[] { Color.white, Color.white }, new[] { 1f, 1f, 0f });

            var sparks = OneShot(root, systems, "Sparks", _fireAdditive, HazardFx.Puff(), 60, o + 5, 0.4f, 0.9f, 0.05f, 0.12f, 7f, 17f, 0.5f, 180f);
            var sm = sparks.main; sm.gravityModifier = 1.2f;
            sm.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.55f), new Color(1f, 0.5f, 0.15f));
            var sr = sparks.GetComponent<ParticleSystemRenderer>(); sr.renderMode = ParticleSystemRenderMode.Stretch; sr.velocityScale = 0.05f; sr.lengthScale = 1.5f;
            HazardFx.SetSizeOverLifetime(sparks, 1f, 0.2f);

            StartCoroutine(CleanUp(root.gameObject, systems, 4f));
        }

        // Dust and grit streaming off the stalactite while it drops (follows it in world space, so it stays behind as
        // a trail); stopped and cleaned up on impact.
        private void StartStalactiteTrail()
        {
            if (_stalactiteTrail != null) return;
            _stalactiteTrail = HazardFx.CreateParticles("StalactiteTrail", null, _fireSmoke != null ? _fireSmoke : _sprite.sharedMaterial,
                HazardFx.Puff(), 200, _sprite.sortingLayerID, _sortingOrder + 5);
            var m = _stalactiteTrail.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1f);
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 0.25f, 0.24f, 0.8f), new Color(0.15f, 0.13f, 0.13f, 0.6f));
            var sh = _stalactiteTrail.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box;
            var r = _stalactite.GetComponentInChildren<SpriteRenderer>();
            Vector2 size = r != null ? (Vector2)r.bounds.size : new Vector2(1f, 6f);
            sh.scale = new Vector3(size.x * 0.6f, size.y * 0.8f, 0f);
            HazardFx.SetSizeOverLifetime(_stalactiteTrail, 1f, 1.8f);
            var e = _stalactiteTrail.emission; e.enabled = true; e.rateOverTime = 160f;
            _stalactiteTrail.transform.position = _stalactite.position;
        }

        private System.Collections.IEnumerator CleanUp(GameObject go, List<ParticleSystem> systems, float after)
        {
            yield return new WaitForSeconds(after);
            foreach (var ps in systems) HazardFx.DestroyMaterial(ps);
            if (go != null) Destroy(go);
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

        private void FreezeAlma(bool freeze)
        {
            var body = _alma != null ? _alma.GetComponent<Rigidbody2D>() : null;
            if (body == null) return;
            body.linearVelocity = Vector2.zero;
            body.simulated = !freeze;
        }

        // ---------- fire curtain ----------
        // A wall of fire that runs along the floor, made of particles: additive flames (soft puffs boiling up
        // with noise, cooling from white-yellow to orange and deep red), a hot core at the base, stretched
        // embers, black smoke rolling off the top, a pulsing heat glow and glowing scorch marks left behind.

        private sealed class Curtain
        {
            public Transform Root; public SpriteRenderer Glow; public ParticleSystem[] Systems;
            public int Dir; public float Speed, DyingAt = -1f;
        }
        private readonly List<Curtain> _curtains = new List<Curtain>();
        private const float CurtainWidth = 0.9f;

        private ParticleSystem FireSystem(string name, Transform parent, Material mat, int max, int order, bool world, float lifeMin, float lifeMax, float sizeMin, float sizeMax)
        {
            var ps = HazardFx.CreateParticles(name, parent, mat != null ? mat : _sprite.sharedMaterial, HazardFx.Puff(), max, _sprite.sortingLayerID, order, world);
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            m.startSpeed = 0f;
            m.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            m.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            m.startColor = Color.white;
            return ps;
        }

        private static Gradient Ramp(Color[] colors, float[] alphas)
        {
            var g = new Gradient();
            var ck = new GradientColorKey[colors.Length]; var ak = new GradientAlphaKey[alphas.Length];
            for (int i = 0; i < colors.Length; i++) ck[i] = new GradientColorKey(colors[i], i / (float)(colors.Length - 1));
            for (int i = 0; i < alphas.Length; i++) ak[i] = new GradientAlphaKey(alphas[i], i / (float)(alphas.Length - 1));
            g.SetKeys(ck, ak);
            return g;
        }

        private static void Noise(ParticleSystem ps, float strength, float frequency)
        {
            var n = ps.noise; n.enabled = true; n.strength = strength; n.frequency = frequency; n.scrollSpeed = 1.5f; n.damping = true; n.quality = ParticleSystemNoiseQuality.Medium;
        }

        private void SpawnCurtain(int dir)
        {
            var root = new GameObject("FireCurtain").transform;
            root.position = new Vector3(_feet.x + dir * (_snoutX * U - 0.5f), _floorY, 0f);
            int o = _sortingOrder + 2;
            float h = _curtainHeight;

            // Heat glow behind (additive when there is a material for it).
            var glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            glow.transform.SetParent(root, false);
            glow.sprite = HazardFx.Glow(); glow.sortingOrder = o - 1; glow.sortingLayerID = _sprite.sortingLayerID;
            if (_fireAdditive != null) glow.sharedMaterial = _fireAdditive;
            glow.transform.localPosition = new Vector3(0f, h * 0.45f, 0f);
            glow.transform.localScale = new Vector3(3.2f / glow.sprite.bounds.size.x, (h + 2f) / glow.sprite.bounds.size.y, 1f);

            // Flames: many soft puffs boiling up from a strip at the floor; they stay with the wall (local)
            // and drift back against the run.
            var flames = FireSystem("Flames", root, _fireAdditive, 220, o + 2, false, 0.45f, 0.85f, 0.8f, 1.5f);
            var fs = flames.shape; fs.enabled = true; fs.shapeType = ParticleSystemShapeType.Box; fs.scale = new Vector3(0.9f, 0.15f, 0f); fs.position = new Vector3(0f, 0.25f, 0f);
            var fv = flames.velocityOverLifetime; fv.enabled = true; fv.space = ParticleSystemSimulationSpace.Local;
            fv.x = new ParticleSystem.MinMaxCurve(-dir * 1.4f, -dir * 0.2f); fv.y = new ParticleSystem.MinMaxCurve(3.6f, 5.4f); fv.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var fc = flames.colorOverLifetime;
            fc.color = Ramp(new[] { new Color(1f, 0.95f, 0.75f), new Color(1f, 0.7f, 0.2f), new Color(1f, 0.38f, 0.06f), new Color(0.6f, 0.08f, 0.02f) }, new[] { 0.9f, 0.85f, 0.55f, 0f });
            HazardFx.SetSizeOverLifetime(flames, 1f, 0.3f);
            Noise(flames, 0.9f, 1.6f);
            var fe = flames.emission; fe.enabled = true; fe.rateOverTime = 160f;

            // Hot core: small white-yellow puffs low down.
            var core = FireSystem("Core", root, _fireAdditive, 80, o + 3, false, 0.25f, 0.45f, 0.5f, 0.9f);
            var cs = core.shape; cs.enabled = true; cs.shapeType = ParticleSystemShapeType.Box; cs.scale = new Vector3(0.7f, 0.1f, 0f); cs.position = new Vector3(0f, 0.2f, 0f);
            var cv = core.velocityOverLifetime; cv.enabled = true; cv.space = ParticleSystemSimulationSpace.Local;
            cv.x = new ParticleSystem.MinMaxCurve(-dir * 0.6f, 0f); cv.y = new ParticleSystem.MinMaxCurve(2.4f, 3.6f); cv.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var cc = core.colorOverLifetime; cc.color = Ramp(new[] { Color.white, new Color(1f, 0.85f, 0.4f), new Color(1f, 0.5f, 0.1f) }, new[] { 0.8f, 0.6f, 0f });
            HazardFx.SetSizeOverLifetime(core, 1f, 0.4f);
            var ce = core.emission; ce.enabled = true; ce.rateOverTime = 70f;

            // Embers: stretched sparks shooting up and back (world: they stay in the air behind it).
            var embers = FireSystem("Embers", root, _fireAdditive, 90, o + 4, true, 0.5f, 1.2f, 0.05f, 0.11f);
            var em = embers.main; em.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f); em.gravityModifier = -0.1f;
            em.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.5f), new Color(1f, 0.5f, 0.1f));
            var es = embers.shape; es.enabled = true; es.shapeType = ParticleSystemShapeType.Cone; es.angle = 22f; es.radius = 0.4f;
            es.rotation = new Vector3(-90f, 0f, 0f); es.position = new Vector3(0f, h * 0.35f, 0f);
            Noise(embers, 1.2f, 2f);
            var er = embers.GetComponent<ParticleSystemRenderer>(); er.renderMode = ParticleSystemRenderMode.Stretch; er.velocityScale = 0.06f; er.lengthScale = 1.5f;
            HazardFx.SetSizeOverLifetime(embers, 1f, 0.2f);
            var ee = embers.emission; ee.enabled = true; ee.rateOverTime = 40f;

            // Black smoke rolling off the top (alpha blended, behind the flames).
            var smoke = FireSystem("Smoke", root, _fireSmoke, 70, o + 1, true, 0.9f, 1.6f, 0.9f, 1.5f);
            var sm = smoke.main; sm.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            var ss = smoke.shape; ss.enabled = true; ss.shapeType = ParticleSystemShapeType.Cone; ss.angle = 18f; ss.radius = 0.35f;
            ss.rotation = new Vector3(-90f, 0f, 0f); ss.position = new Vector3(0f, h * 0.8f, 0f);
            var sc = smoke.colorOverLifetime;
            sc.color = Ramp(new[] { new Color(0.35f, 0.18f, 0.1f), new Color(0.12f, 0.1f, 0.1f), new Color(0.08f, 0.07f, 0.07f) }, new[] { 0f, 0.55f, 0f });
            HazardFx.SetSizeOverLifetime(smoke, 0.7f, 2.2f);
            Noise(smoke, 0.6f, 0.8f);
            var se = smoke.emission; se.enabled = true; se.rateOverTime = 22f;

            // Scorch: glowing marks on the floor that cool to black (world, flat).
            var scorch = FireSystem("Scorch", root, _fireAdditive, 60, o - 2, true, 0.8f, 1.2f, 0.8f, 1.2f);
            var shs = scorch.shape; shs.enabled = true; shs.shapeType = ParticleSystemShapeType.Box; shs.scale = new Vector3(0.6f, 0.04f, 0f); shs.position = new Vector3(0f, 0.06f, 0f);
            var scc = scorch.colorOverLifetime; scc.color = Ramp(new[] { new Color(1f, 0.55f, 0.15f), new Color(0.7f, 0.12f, 0.03f), new Color(0.1f, 0.03f, 0.02f) }, new[] { 0.7f, 0.35f, 0f });
            var sce = scorch.emission; sce.enabled = true; sce.rateOverDistance = 6f;
            scorch.transform.localScale = new Vector3(1f, 0.3f, 1f);

            _curtains.Add(new Curtain { Root = root, Glow = glow, Systems = new[] { flames, core, embers, smoke, scorch }, Dir = dir, Speed = P.CurtainSpeed });
        }

        private void UpdateCurtains()
        {
            for (int i = _curtains.Count - 1; i >= 0; i--)
            {
                var c = _curtains[i];
                if (c.Root == null) { _curtains.RemoveAt(i); continue; }
                float t = Time.time;
                bool dying = c.DyingAt >= 0f;
                float fade = dying ? 1f - Mathf.Clamp01((t - c.DyingAt) / 0.5f) : 1f;
                Vector3 p = c.Root.position;
                if (!dying) { p.x += c.Dir * c.Speed * Time.deltaTime; c.Root.position = p; }
                float flicker = 0.5f + 0.25f * Mathf.Sin(t * 23f) + 0.15f * Mathf.Sin(t * 37f + 1.3f);
                c.Glow.color = new Color(1f, 0.42f, 0.1f, flicker * 0.7f * fade);
                if (!dying && _alma != null && !_alma.IsDead && !_alma.IsDashing && _almaCol != null)
                {
                    var box = new Rect(p.x - CurtainWidth * 0.5f, _floorY, CurtainWidth, _curtainHeight);
                    if (box.Contains(_almaCol.ClosestPoint(box.center))) _alma.Die();
                }
                if (!dying && (p.x <= _wallLeftX || p.x >= _wallRightX))
                {
                    // Dies against the wall: stops emitting, the flames burn out.
                    c.DyingAt = t;
                    foreach (var ps in c.Systems) { var e = ps.emission; e.enabled = false; }
                    NinjaSmoke2D.Burst(new Vector2(p.x, _floorY + 1.2f), 0f);
                }
                if (dying && t - c.DyingAt > 1.8f) { foreach (var ps in c.Systems) HazardFx.DestroyMaterial(ps); Destroy(c.Root.gameObject); _curtains.RemoveAt(i); }
            }
        }

        // ---------- meteors ----------

        private sealed class Meteor { public Transform Rock; public SpriteRenderer Mark; public float X, At, Ground; }
        private readonly List<Meteor> _meteors = new List<Meteor>();
        private const float MeteorRadius = 0.55f, MeteorFall = 26f;

        // Top of the first solid thing under the sky at x (a platform, the ledge or the floor): meteors
        // never go through platforms, so standing under one is safe.
        private float LandingY(float x)
        {
            float best = _floorY;
            int n = Physics2D.RaycastNonAlloc(new Vector2(x, _skyY), Vector2.down, _rayHits, _skyY - _floorY + 1f);
            float bestDist = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                var c = _rayHits[i].collider;
                if (c == null || c.isTrigger || c == _almaCol || c.attachedRigidbody != null) continue;
                if (_rayHits[i].distance < bestDist) { bestDist = _rayHits[i].distance; best = _rayHits[i].point.y; }
            }
            return Mathf.Max(best, _floorY);
        }
        private readonly RaycastHit2D[] _rayHits = new RaycastHit2D[16];

        private void SpawnMeteor(float x, float delay)
        {
            float ground = LandingY(x);
            var mark = new GameObject("MeteorMark").AddComponent<SpriteRenderer>();
            mark.sprite = HazardFx.Glow();
            mark.sortingOrder = _sortingOrder + 2;
            mark.transform.position = new Vector3(x, ground + 0.05f, 0f);
            mark.transform.localScale = new Vector3(1.8f / mark.sprite.bounds.size.x, 0.5f / mark.sprite.bounds.size.y, 1f);
            mark.color = new Color(1f, 0.9f, 0.2f, 0f);
            // Placeholder rock: a white square until the meteor has its own art.
            var rock = new GameObject("Meteor").AddComponent<SpriteRenderer>();
            rock.sprite = HazardFx.Pixel();
            rock.color = Color.white;
            rock.sortingOrder = _sortingOrder + 3;
            rock.transform.localScale = Vector3.one * MeteorRadius * 2f / rock.sprite.bounds.size.x;
            rock.transform.position = new Vector3(x, _skyY, 0f);
            rock.enabled = false;
            _meteors.Add(new Meteor { Rock = rock.transform, Mark = mark, X = x, At = Time.time + delay, Ground = ground });
        }

        private void UpdateMeteors()
        {
            for (int i = _meteors.Count - 1; i >= 0; i--)
            {
                var m = _meteors[i];
                if (m.Rock == null) { _meteors.RemoveAt(i); continue; }
                float t = Time.time - m.At;
                if (t < 0f) continue;
                // The yellow mark blinks on the floor, then the meteor falls on it.
                float blink = t < _meteorWarning ? 0.45f + 0.45f * Mathf.Sin(t * 20f) : 0.9f;
                m.Mark.color = new Color(1f, 0.9f, 0.2f, blink);
                if (t < _meteorWarning) continue;
                var r = m.Rock.GetComponent<SpriteRenderer>(); r.enabled = true;
                float y = _skyY - 0.5f * MeteorFall * 2f * (t - _meteorWarning) * (t - _meteorWarning) - 6f * (t - _meteorWarning);
                m.Rock.position = new Vector3(m.X, Mathf.Max(y, m.Ground + MeteorRadius), 0f);
                m.Rock.localRotation = Quaternion.Euler(0f, 0f, Time.time * 240f);
                if (_alma != null && !_alma.IsDead && _almaCol != null)
                {
                    Vector2 c = m.Rock.position;
                    if (((Vector2)_almaCol.ClosestPoint(c) - c).sqrMagnitude < MeteorRadius * MeteorRadius) _alma.Die();
                }
                if (y > m.Ground + MeteorRadius) continue;
                Shake(0.15f, 0.15f);
                NinjaSmoke2D.Burst(new Vector2(m.X, m.Ground + 0.3f), 0f);
                Destroy(m.Rock.gameObject); Destroy(m.Mark.gameObject);
                _meteors.RemoveAt(i);
            }
        }

        // ---------- boulder ----------

        private Transform _boulder, _boulderRock;
        private ParticleSystem[] _boulderFx;
        private int _boulderDir;
        private float _boulderSpin;

        // A burning boulder: the rock (spinning) wrapped in additive flames that stream back as it rolls,
        // a heat glow, embers and a trail of smoke. The root doesn't spin, so the fire always rises.
        private void SpawnBoulder()
        {
            var go = new GameObject("BurningBoulder");
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = _boulderRadius;
            go.AddComponent<TRexRoarTarget2D>().Roared += OnBoulderRoared;
            int o = _sortingOrder + 1;

            var glow = new GameObject("Glow").AddComponent<SpriteRenderer>();
            glow.transform.SetParent(go.transform, false);
            glow.sprite = HazardFx.Glow(); glow.sortingOrder = o - 1; glow.sortingLayerID = _sprite.sortingLayerID;
            if (_fireAdditive != null) glow.sharedMaterial = _fireAdditive;
            glow.color = new Color(1f, 0.45f, 0.12f, 0.75f);
            glow.transform.localScale = Vector3.one * _boulderRadius * 4.2f / glow.sprite.bounds.size.x;

            var sr = new GameObject("Rock").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(go.transform, false);
            sr.sprite = _boulderSprite != null ? _boulderSprite : HazardFx.Fireball();
            sr.sortingOrder = o + 1; sr.sortingLayerID = _sprite.sortingLayerID;
            sr.color = new Color(1f, 0.62f, 0.4f);   // incandescent
            sr.transform.localScale = Vector3.one * (_boulderRadius * 2f) / sr.sprite.bounds.size.x;
            _boulderRock = sr.transform;

            // Flames licking around the rock and streaming back (world space: they stay behind it).
            var flames = FireSystem("Flames", go.transform, _fireAdditive, 160, o + 2, true, 0.3f, 0.55f, 0.6f, 1.1f);
            var fs = flames.shape; fs.enabled = true; fs.shapeType = ParticleSystemShapeType.Circle; fs.radius = _boulderRadius * 0.8f;
            var fv = flames.velocityOverLifetime; fv.enabled = true; fv.space = ParticleSystemSimulationSpace.World;
            fv.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); fv.y = new ParticleSystem.MinMaxCurve(2f, 3.4f); fv.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var fc = flames.colorOverLifetime;
            fc.color = Ramp(new[] { new Color(1f, 0.95f, 0.75f), new Color(1f, 0.65f, 0.18f), new Color(1f, 0.35f, 0.06f), new Color(0.55f, 0.07f, 0.02f) }, new[] { 0.9f, 0.8f, 0.5f, 0f });
            HazardFx.SetSizeOverLifetime(flames, 1f, 0.25f);
            Noise(flames, 0.7f, 2f);
            var fe = flames.emission; fe.enabled = true; fe.rateOverTime = 110f;

            var embers = FireSystem("Embers", go.transform, _fireAdditive, 60, o + 3, true, 0.4f, 0.9f, 0.05f, 0.1f);
            var em = embers.main; em.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f); em.gravityModifier = 0.2f;
            em.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.5f), new Color(1f, 0.5f, 0.1f));
            var es = embers.shape; es.enabled = true; es.shapeType = ParticleSystemShapeType.Circle; es.radius = _boulderRadius;
            var er = embers.GetComponent<ParticleSystemRenderer>(); er.renderMode = ParticleSystemRenderMode.Stretch; er.velocityScale = 0.06f; er.lengthScale = 1.5f;
            HazardFx.SetSizeOverLifetime(embers, 1f, 0.2f);
            var ee = embers.emission; ee.enabled = true; ee.rateOverTime = 30f;

            var smoke = FireSystem("Smoke", go.transform, _fireSmoke, 50, o, true, 0.8f, 1.3f, 0.7f, 1.2f);
            var sm = smoke.main; sm.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            var ss = smoke.shape; ss.enabled = true; ss.shapeType = ParticleSystemShapeType.Circle; ss.radius = _boulderRadius * 0.6f; ss.position = new Vector3(0f, _boulderRadius * 0.6f, 0f);
            var sc = smoke.colorOverLifetime;
            sc.color = Ramp(new[] { new Color(0.35f, 0.18f, 0.1f), new Color(0.12f, 0.1f, 0.1f), new Color(0.08f, 0.07f, 0.07f) }, new[] { 0f, 0.5f, 0f });
            HazardFx.SetSizeOverLifetime(smoke, 0.7f, 2f);
            Noise(smoke, 0.5f, 0.8f);
            var se = smoke.emission; se.enabled = true; se.rateOverTime = 16f;
            _boulderFx = new[] { flames, embers, smoke };
            _boulderDir = _dir;
            _boulderBack = false;
            go.transform.position = new Vector3(_feet.x + _dir * (_snoutX * U + _boulderRadius), _floorY + _boulderRadius, 0f);
            _boulder = go.transform;
        }

        private void DestroyBoulder()
        {
            if (_boulderFx != null) foreach (var ps in _boulderFx) HazardFx.DestroyMaterial(ps);
            if (_boulder != null) Destroy(_boulder.gameObject);
            _boulder = _boulderRock = null; _boulderFx = null;
        }

        // Roared from in front (Alma facing it), it rolls back twice as fast towards his jaws.
        private void OnBoulderRoared(Vector2 origin, int direction)
        {
            if (_boulder == null || _boulderBack || direction != -_boulderDir) return;
            _boulderBack = true;
            _boulderDir = -_boulderDir;
            Shake(0.2f, 0.2f);
        }

        private void UpdateBoulder()
        {
            if (_boulder == null) return;
            float speed = P.BoulderSpeed * (_boulderBack ? 2f : 1f);
            Vector3 p = _boulder.position;
            p.x += _boulderDir * speed * Time.deltaTime;
            _boulder.position = p;
            _boulderSpin -= _boulderDir * speed * Time.deltaTime / _boulderRadius * Mathf.Rad2Deg;
            if (_boulderRock != null) _boulderRock.localRotation = Quaternion.Euler(0f, 0f, _boulderSpin);
            var glowR = _boulder.Find("Glow") != null ? _boulder.Find("Glow").GetComponent<SpriteRenderer>() : null;
            if (glowR != null) glowR.color = new Color(1f, 0.45f, 0.12f, 0.6f + 0.15f * Mathf.Sin(Time.time * 21f));
            if (!_boulderBack && _alma != null && !_alma.IsDead && _almaCol != null)
            {
                Vector2 c = p;
                float r = _boulderRadius * 0.85f;
                if (((Vector2)_almaCol.ClosestPoint(c) - c).sqrMagnitude < r * r) _alma.Die();
            }
            if (_boulderBack)
            {
                // Back into his jaws: stunned (the window).
                float snout = _feet.x + _dir * _snoutX * U;
                if ((p.x - snout) * _boulderDir >= -_boulderRadius && _state != State.Window && !IsDefeated && _state != State.Hurt)
                {
                    Shake(0.5f, 0.4f);
                    NinjaSmoke2D.Burst(p, 0f);
                    DestroyBoulder();
                    OpenWindow(_stunnedShout);
                    return;
                }
            }
            if (p.x - _boulderRadius <= _wallLeftX || p.x + _boulderRadius >= _wallRightX)
            {
                NinjaSmoke2D.Burst(p, 0f);
                DestroyBoulder();
            }
        }

        private void ClearSpawned()
        {
            foreach (var c in _curtains) if (c.Root != null) { foreach (var ps in c.Systems) HazardFx.DestroyMaterial(ps); Destroy(c.Root.gameObject); }
            _curtains.Clear();
            foreach (var m in _meteors) { if (m.Rock != null) Destroy(m.Rock.gameObject); if (m.Mark != null) Destroy(m.Mark.gameObject); }
            _meteors.Clear();
            DestroyBoulder();
        }

        // ---------- contact and respawn ----------

        private void CheckContact()
        {
            UpdateSinking();
            if (_alma == null || _alma.IsDead || _almaCol == null) return;
            Rect box;
            switch (_state)
            {
                case State.Charge: case State.ChargeWarn: box = Box(_chargeBox); break;
                case State.Rest: case State.Stomp: case State.Roar: case State.Kick: case State.Crash: case State.Walk: box = Box(_standBox); break;
                default: return;   // intro, windows, hurt, defeat: harmless
            }
            if (box.Contains(_almaCol.ClosestPoint(box.center))) _alma.Die();
        }

        // Alma died: the attacks are cleared and the phase's cycle starts again (hits, the collapse and the
        // cracked stalactite are kept). He goes back to his place, away from her.
        private void OnAlmaRespawned()
        {
            ClearSpawned();
            if (_state == State.Intro || IsDefeated || _state == State.Hurt) return;
            _sprite.color = Color.white;
            if (_stalactite != null && _state == State.Finale) _stalactite.position = _stalactiteHome;
            _step = 0;
            if (_phase >= _phases.Length - 1)
            {
                if (!_collapsed) Collapse(true);
                _feet.x = _centerX;
            }
            else
            {
                float ax = _alma != null ? _alma.transform.position.x : 0f;
                _feet.x = ax < 0f ? _introX : -_introX;
            }
            _puffPending = true;   // next frame: Respawn also runs while the scene is being unloaded
            FaceAlma();
            Place();
            Enter(State.Rest);
        }

        private void Shake(float amp, float time)
        {
            if (_cam != null) _cam.Shake(amp, time);
        }

        // ---------- dizzy stars ----------

        private SpriteRenderer[] _stars;

        private void UpdateStars(bool on)
        {
            if (_stars == null)
            {
                if (!on) return;
                Sprite star = MakeStar();
                _stars = new SpriteRenderer[3];
                for (int i = 0; i < 3; i++)
                {
                    _stars[i] = new GameObject("Star").AddComponent<SpriteRenderer>();
                    _stars[i].transform.SetParent(transform, false);
                    _stars[i].sprite = star;
                }
            }
            foreach (var st in _stars) st.enabled = on;
            if (!on) return;
            // Over his head (lowered in front of him).
            Vector2 c = _feet + new Vector2(_dir * 1.9f * U, 2.9f * U);
            for (int i = 0; i < 3; i++)
            {
                float a = Time.time * 4f + i * Mathf.PI * 2f / 3f, depth = Mathf.Sin(a);
                _stars[i].transform.position = c + new Vector2(Mathf.Cos(a) * 1f, depth * 0.28f);
                _stars[i].transform.localScale = Vector3.one * (0.45f + 0.1f * depth) / _stars[i].sprite.bounds.size.x;
                _stars[i].transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 200f + i * 40f);
                _stars[i].sortingOrder = _sortingOrder + (depth > 0f ? -1 : 1);
                _stars[i].color = Color.Lerp(Color.white, _accent, 0.35f);
            }
        }

        private static Sprite MakeStar()
        {
            const int n = 64; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++) { float r = i % 2 == 0 ? 30f : 13f, a = Mathf.PI / 2f + i * Mathf.PI / 5f; pts[i] = new Vector2(32f + Mathf.Cos(a) * r, 32f + Mathf.Sin(a) * r); }
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f); bool inside = false;
                for (int i = 0, j = 9; i < 10; j = i++)
                    if ((pts[i].y > p.y) != (pts[j].y > p.y) && p.x < (pts[j].x - pts[i].x) * (p.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x) inside = !inside;
                float d = float.MaxValue;
                for (int i = 0, j = 9; i < 10; j = i++)
                {
                    Vector2 e = pts[i] - pts[j]; float t = Mathf.Clamp01(Vector2.Dot(p - pts[j], e) / e.sqrMagnitude);
                    d = Mathf.Min(d, Vector2.Distance(p, pts[j] + e * t));
                }
                Color c = d < 2.5f ? new Color(0.25f, 0.1f, 0.05f) : inside ? new Color(1f, 0.92f, 0.6f) : Color.clear;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.75f, 0.25f, 0.9f);
            Gizmos.DrawLine(new Vector3(_wallLeftX, _floorY), new Vector3(_wallLeftX, _floorY + 8f));
            Gizmos.DrawLine(new Vector3(_wallRightX, _floorY), new Vector3(_wallRightX, _floorY + 8f));
            Gizmos.DrawLine(new Vector3(_wallLeftX, _floorY), new Vector3(_wallRightX, _floorY));
            Gizmos.DrawWireSphere(new Vector3(_centerX, _floorY), 0.3f);
        }
    }
}
