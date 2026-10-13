using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Bosses
{
    // Boss 2 — the Prehistoric Armadillo (Docs/Levels/World_2_Caves/Boss_2.md). Same structure as the Monkey
    // (GiantMonkeyBoss2D): intro with lines and title card, one life per phase, red flash on a hit, defeat with
    // lines and the exit portal. He is a tank: rolled up in a ball he is lethal and cannot be hurt. He rolls
    // from wall to wall (sometimes hopping onto a floating block and rolling across it); after the phase's
    // number of wall hits he crashes and lies dizzy, with a glowing crack on top of his shell — only a Ground
    // Pound on it hurts him. Then he burrows and comes out of the ground for the next phase. Wall hits shake
    // the cave and drop the scene's stalactites; standing on a floating block brings a stalactite down on
    // Alma every couple of seconds, so she cannot camp up there.
    [DisallowMultipleComponent]
    public sealed class ArmadilloBoss2D : MonoBehaviour
    {
        [System.Serializable]
        public struct Phase
        {
            public float RollSpeed;   // units/s
            public int WallHits;      // wall hits before he crashes and gets dizzy
            public float Dizzy;       // seconds lying dizzy
            public float HopChance;   // chance to hop onto a floating block on each crossing
            public int Stalactites;   // scene stalactites dropped by each wall hit
        }

        [Header("Arena (world)")]
        [Tooltip("Inner faces of the side blocks.")]
        [SerializeField] private float _wallLeftX = -13.73f;
        [SerializeField] private float _wallRightX = 13.73f;
        [SerializeField] private float _floorY = -1.52f;
        [Tooltip("Floating blocks: x = left edge, y = right edge, z = top.")]
        [SerializeField] private Vector3[] _blocks = { new Vector3(-9f, -5f, 1.33f), new Vector3(5f, 9f, 1.33f) };
        [Tooltip("Height the anti-camping stalactites fall from.")]
        [SerializeField] private float _ceilingY = 13.6f;

        [Header("Fight")]
        [SerializeField]
        private Phase[] _phases =
        {
            new Phase { RollSpeed = 9f, WallHits = 2, Dizzy = 4.5f, HopChance = 0.35f, Stalactites = 1 },
            new Phase { RollSpeed = 10.35f, WallHits = 3, Dizzy = 3.8f, HopChance = 0.45f, Stalactites = 2 },
            new Phase { RollSpeed = 11.9f, WallHits = 4, Dizzy = 3.2f, HopChance = 0.55f, Stalactites = 3 },
        };
        [SerializeField, Min(1f)] private float _gravity = 40f;
        [Tooltip("How high above a block's top the ball goes when it hops onto it.")]
        [SerializeField, Min(0f)] private float _hopClearance = 0.9f;
        [SerializeField] private float _stompBounce = 13.5f;
        [Tooltip("Bounce when Alma lands on him without a Ground Pound (no damage).")]
        [SerializeField] private float _softBounce = 9f;
        [Tooltip("Shout on screen when he gets dizzy (HudBossAlert).")]
        [SerializeField] private string _dizzyShout = "¡Mareado!";
        [Tooltip("Seconds Alma must stay on a floating block between two stalactites.")]
        [SerializeField, Min(0.3f)] private float _campInterval = 2f;
        [SerializeField] private ProjectileTrap2D _stalactitePrefab;
        [Tooltip("Lethal radius of the rolling ball, as a fraction of the drawn ball (the crystal spikes on its rim don't kill).")]
        [SerializeField, Range(0.4f, 1f)] private float _ballHitScale = 0.78f;
        [Tooltip("Lethal box of his body while warning and curling (from his feet up).")]
        [SerializeField] private Vector2 _bodyHitSize = new Vector2(2f, 1.35f);

        [Header("Art")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [SerializeField] private string _tauntState = "Boss_Armadillo_Taunt";
        [SerializeField] private string _curlState = "Boss_Armadillo_Curl";
        [SerializeField] private string _stunnedState = "Boss_Armadillo_Stunned";
        [Tooltip("Size of the art (1 = 45 px per unit). 0.62 lets the ball roll under the floating blocks.")]
        [SerializeField, Min(0.1f)] private float _scale = 0.62f;
        [SerializeField] private int _sortingOrder = 4;
        [Tooltip("Top of the crack above the feet, per Stunned frame (pixels of the 256 cell).")]
        [SerializeField] private float[] _crackTopPx = { 106f, 114f, 127f, 137f, 130f, 118f, 112f, 112f };
        [SerializeField] private float _crackXPx = 12f;
        [Tooltip("Centre of the ball (last Curl frame) relative to the feet, in pixels of the 256 cell.")]
        [SerializeField] private Vector2 _ballCenterPx = new Vector2(12f, 84.5f);
        [SerializeField] private float _ballRadiusPx = 84f;

        [Header("Story")]
        [SerializeField, TextArea(1, 2)] private string[] _introLines = { "¡Grrr! Nadie cruza mi túnel, lagartija.", "¡Te aplastaré contra los cristales!" };
        [SerializeField, TextArea(1, 2)] private string[] _defeatLines = { "Mi caparazón... ¡crack!", "Sigue tu camino, madre... el pantano no será tan amable." };
        [SerializeField] private Sprite _bubbleSprite, _bubbleTailSprite;
        [Header("Egg (reunion after the defeat)")]
        [SerializeField] private string _eggId = "Blue";
        [Tooltip("The reunion room shown after the defeat (prefab with EggReunionRoom on its root).")]
        [SerializeField] private GameObject _reunionRoom;
        [SerializeField] private Color _accent = new Color(0.35f, 0.9f, 1f);
        [SerializeField] private string _bossName = "Armadillo Prehistórico";
        [SerializeField] private string _bossEpithet = "El Acorazado Subterráneo";
        [SerializeField] private string _bossKicker = "Jefe del Mundo 2";
        [SerializeField, Range(3f, 8f)] private float _introZoom = 5.5f;
        [Tooltip("Where he stands for the intro, relative to Alma (x).")]
        [SerializeField] private float _introDistance = 6f;
        [Tooltip("Exit portal (inactive) shown after the defeat, at (Portal X, Portal Y).")]
        [SerializeField] private GameObject _exitPortal;
        [SerializeField] private float _portalX;
        [SerializeField] private float _portalY = 0.5f;

        private enum State { Intro, Warn, Curl, Roll, Uncurl, Dizzy, Hurt, Burrow, Defeat, Done }

        private State _state;
        private float _stateAt;
        private int _phase, _wallHits;
        private AlmaMotor2D _alma;
        private Collider2D _almaCol;
        private BossBackdropPhases _backdrop;
        private SpeechBubble2D _bubble;
        private AlmaCameraFollow _cam;
        private Transform _pivot;              // ball centre: the sprite turns around it while rolling
        private SpriteRenderer _sprite;
        private Animator _animator;
        private string _playing;
        private Vector2 _feet;                 // where his feet are (world)
        private int _dir = 1;                  // rolling direction
        private float _vy, _spin;
        private bool _grounded;
        private int _onBlock = -1, _hopTarget = -1;
        private float _campTimer;
        private readonly List<ProjectileTrap2D> _sceneStalactites = new List<ProjectileTrap2D>();

        public int Hits { get; private set; }
        public bool IsDizzy => _state == State.Dizzy;
        public bool IsRolling => _state == State.Roll;
        public bool IsDefeated => _state == State.Defeat || _state == State.Done;
        public Vector2 BallCenter => _feet + BallOffset;
        public event System.Action<int> Hit;
        public event System.Action Defeated;

        private float Px => _scale / 45f;
        private float Radius => _ballRadiusPx * Px;
        private Vector2 BallOffset => new Vector2(_ballCenterPx.x * Px * (_sprite != null && _sprite.flipX ? -1f : 1f), _ballCenterPx.y * Px);
        private Phase P => _phases[Mathf.Clamp(_phase, 0, _phases.Length - 1)];
        private float T => Time.time - _stateAt;
        private void Enter(State s)
        {
            // Dizzy = the window to hit him: the HUD tells the player wherever he is (shout + edge badge).
            if (s == State.Dizzy && _state != State.Dizzy) HudBossAlert.Show(() => new Vector2(CrackX, CrackTop()), P.Dizzy, _dizzyShout, _accent);
            else if (_state == State.Dizzy && s != State.Dizzy) HudBossAlert.Hide();
            _state = s; _stateAt = Time.time;
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
                _alma.SetUnlocked(AlmaAbility.DoubleJump, true);
                _alma.SetUnlocked(AlmaAbility.GroundPound, true);
            }
            // The stalactites placed in the arena only fall when he hits a wall.
            foreach (var trap in FindObjectsByType<ProjectileTrap2D>(FindObjectsSortMode.None))
                if (trap.Motion == ProjectileTrapMotion.Fall) { trap.Manual = true; _sceneStalactites.Add(trap); }

            _pivot = new GameObject("Pivot").transform;
            _pivot.SetParent(transform, false);
            _sprite = new GameObject("Sprite").AddComponent<SpriteRenderer>();
            _sprite.transform.SetParent(_pivot, false);
            _sprite.sortingOrder = _sortingOrder;
            _animator = _sprite.gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = _animatorController;

            if (_introLines != null && _introLines.Length > 0 && HudText.Ready)
                _bubble = SpeechBubble2D.Create(_sprite.transform, _bubbleSprite, _bubbleTailSprite, _bossName, _accent);
            if (_backdrop != null) _backdrop.SetPhase(0);

            Vector2 almaPos = _alma != null ? (Vector2)_alma.transform.position : Vector2.zero;
            _feet = new Vector2(Mathf.Clamp(almaPos.x + _introDistance, _wallLeftX + 2f, _wallRightX - 2f), _floorY);
            _sprite.flipX = almaPos.x < _feet.x;   // the art faces right; he faces Alma
            Place(_feet, 0f);
            LevelFadeIn.Focus = _feet + Vector2.up * 1.5f;
            AlmaTouchControls.InputLocked = true;
            CinematicState.Begin();
            if (Hud2D.Instance != null) Hud2D.Instance.HideNow();
            if (HudLevelTitle.Instance != null) HudLevelTitle.Instance.enabled = false;   // the boss card replaces it
            Enter(State.Intro);
        }

        private void OnDestroy()
        {
            HudBossAlert.Track(this, null, default);
            if (_alma != null) _alma.Respawned -= OnAlmaRespawned;
            AlmaTouchControls.InputLocked = false;
        }

        // Feet at `feet`, the art turned `angle` degrees around the ball centre.
        private void Place(Vector2 feet, float angle)
        {
            _feet = feet;
            Vector2 c = feet + BallOffset;
            _pivot.position = c;
            _pivot.localRotation = Quaternion.Euler(0f, 0f, angle);
            _sprite.transform.localPosition = feet - c;
            _sprite.transform.localScale = Vector3.one * _scale;
        }

        // Plays an animator state normally, or shows the frame at `frame` (0..1) when given.
        private void Play(string state, float frame = -1f)
        {
            if (_playing != state) { _animator.Play(state, 0, 0f); _playing = state; }
            if (frame < 0f) { _animator.speed = 1f; return; }
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
                case State.Warn: UpdateWarn(); break;
                case State.Curl: UpdateCurl(); break;
                case State.Roll: UpdateRoll(); break;
                case State.Uncurl: UpdateUncurl(); break;
                case State.Dizzy: UpdateDizzy(); break;
                case State.Hurt: UpdateHurt(); break;
                case State.Burrow: UpdateBurrow(); break;
                case State.Defeat: UpdateDefeat(); break;
            }
            UpdateStars(_state == State.Dizzy);
            UpdateCamping();
            CheckContact();
        }

        // ---------- intro ----------
        // Alma can't act; the camera frames him: says his lines → rears up and slams the ground → title card → fight.

        private const float LinesAt = 1.1f, LineTime = 3f, TauntTime = 0.75f, ShotOut = 0.9f;
        private float _tauntAt = -1f, _cardAt, _endAt = -1f;
        private bool _slammed, _cardShown;

        private void UpdateIntro()
        {
            if (LevelFadeIn.IsOpening) _stateAt = Time.time;   // the intro clock starts once the opening iris is done
            if (_tauntAt < 0f) { _tauntAt = LinesAt + (_introLines?.Length ?? 0) * LineTime + 0.2f; _cardAt = _tauntAt + TauntTime + 0.5f; }
            float t = T;
            AlmaTouchControls.InputLocked = true;   // held every frame: other systems clear the touch state on start
            float w = _endAt > 0f ? 1f - Mathf.Clamp01((t - _endAt) / ShotOut) : 1f;
            if (_cam != null) _cam.SetShot(_feet + Vector2.up * 1.6f, _introZoom, w);

            if (t < _tauntAt) Play(_tauntState, 0f);
            else Taunt(t - _tauntAt);
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
                StartPhase();
            }
        }

        // The Taunt once (rears up, slams the ground on frame 7: dust and a shake).
        private void Taunt(float t)
        {
            float u = Mathf.Clamp01(t / TauntTime);
            Play(_tauntState, u);
            if (u < 0.75f) _slammed = false;
            else if (!_slammed)
            {
                _slammed = true;
                Shake(0.25f, 0.3f);
                NinjaSmoke2D.Burst(_feet + new Vector2(_sprite.flipX ? -1.2f : 1.2f, 0.2f), 0f);
            }
        }

        // ---------- attack ----------

        private void StartPhase()
        {
            // A new phase, or a new try after he recovered, counts wall hits from zero. After Alma dies the
            // count is kept: restarting it made the fast phases endless for a player who keeps getting hit.
            if (!_keepWallHits) _wallHits = 0;
            _keepWallHits = false;
            if (_alma != null) _sprite.flipX = _alma.transform.position.x < _feet.x;
            Place(new Vector2(_feet.x, _floorY), 0f);
            Enter(State.Warn);
        }

        // Warning: rears up and slams the ground facing Alma, then curls up.
        private void UpdateWarn()
        {
            Taunt(T);
            if (T >= TauntTime + 0.15f) Enter(State.Curl);
        }

        private const float CurlTime = 0.6f;

        private void UpdateCurl()
        {
            Play(_curlState, T / CurlTime);
            if (T < CurlTime) return;
            _dir = _sprite.flipX ? -1 : 1;
            _vy = 0f; _grounded = true; _onBlock = -1; _spin = 0f;
            PlanCrossing();
            Enter(State.Roll);
        }

        // Each crossing: roll along the floor, or hop onto one of the floating blocks on the way.
        private void PlanCrossing()
        {
            _hopTarget = -1;
            if (_blocks == null || _blocks.Length == 0 || Random.value > P.HopChance) return;
            var ahead = new List<int>();
            for (int i = 0; i < _blocks.Length; i++)
            {
                float near = _dir > 0 ? _blocks[i].x : _blocks[i].y;
                if ((near - BallCenter.x) * _dir > 2f) ahead.Add(i);
            }
            if (ahead.Count > 0) _hopTarget = ahead[Random.Range(0, ahead.Count)];
        }

        private void UpdateRoll()
        {
            float dt = Time.deltaTime, speed = P.RollSpeed, r = Radius;
            Vector2 c = BallCenter;
            // Hop onto the planned block: take off so the top of the arc is over its near edge.
            if (_grounded && _onBlock < 0 && _hopTarget >= 0)
            {
                Vector3 b = _blocks[_hopTarget];
                float near = _dir > 0 ? b.x : b.y;
                float rise = b.z + r + _hopClearance - c.y;
                float vy0 = Mathf.Sqrt(2f * _gravity * Mathf.Max(0.5f, rise));
                float takeoff = speed * (vy0 / _gravity) * 0.9f;
                if ((near - c.x) * _dir <= takeoff) { _vy = vy0; _grounded = false; _hopTarget = -1; }
            }
            c.x += _dir * speed * dt;
            if (!_grounded)
            {
                _vy -= _gravity * dt;
                c.y += _vy * dt;
                float support = Support(c.x, c.y, out int block);
                if (_vy <= 0f && c.y <= support + r) { c.y = support + r; _vy = 0f; _grounded = true; _onBlock = block; Shake(0.12f, 0.15f); }
            }
            else if (_onBlock >= 0)
            {
                Vector3 b = _blocks[_onBlock];
                if (c.x < b.x || c.x > b.y) { _grounded = false; _onBlock = -1; }   // rolls off the end and drops
            }
            // Wall.
            float wall = _dir > 0 ? _wallRightX - r : _wallLeftX + r;
            bool hitWall = (c.x - wall) * _dir >= 0f;
            if (hitWall) c.x = wall;
            _spin -= _dir * speed * dt / r * Mathf.Rad2Deg;
            Play(_curlState, 1f);
            Place(c - BallOffset, _spin);
            if (!hitWall) return;

            _wallHits++;
            Shake(0.45f, 0.35f);
            NinjaSmoke2D.Burst(new Vector2(_dir > 0 ? _wallRightX : _wallLeftX, c.y), 0f);
            DropStalactites(P.Stalactites);
            if (_wallHits >= P.WallHits)
            {
                // Crashes (even if he reaches the wall still dropping from a block): lies on the floor against
                // the wall, dizzy, facing the wall he hit.
                _sprite.flipX = _dir < 0;
                float half = 1.5f;
                Place(new Vector2(_dir > 0 ? _wallRightX - half : _wallLeftX + half, _floorY), 0f);
                Enter(State.Uncurl);
                return;
            }
            _dir = -_dir;
            PlanCrossing();
        }

        // Floor, or the top of a floating block the ball is over (and above).
        private float Support(float x, float y, out int block)
        {
            block = -1;
            for (int i = 0; i < _blocks.Length; i++)
                if (x >= _blocks[i].x && x <= _blocks[i].y && y >= _blocks[i].z) { block = i; return _blocks[i].z; }
            return _floorY;
        }

        private void UpdateUncurl()
        {
            Play(_curlState, 1f - T / CurlTime);
            if (T >= CurlTime) Enter(State.Dizzy);
        }

        // ---------- dizzy, hit, burrow ----------

        private float CrackTop()
        {
            int frame = 0;
            string n = _sprite.sprite != null ? _sprite.sprite.name : "";
            int us = n.LastIndexOf('_');
            if (us >= 0) int.TryParse(n.Substring(us + 1), out frame);
            frame = Mathf.Clamp(frame, 0, _crackTopPx.Length - 1);
            return _feet.y + _crackTopPx[frame] * Px;
        }

        private float CrackX => _feet.x + _crackXPx * Px * (_sprite.flipX ? -1f : 1f);

        private void UpdateDizzy()
        {
            Play(_stunnedState);
            if (StompCheck()) return;
            if (T >= P.Dizzy) { _nextPhase = false; Enter(State.Burrow); }
        }

        // Only a Ground Pound on the crack hurts him; a plain landing on his back just bounces Alma off.
        private bool StompCheck()
        {
            if (_alma == null || _alma.IsDead || _almaCol == null || _alma.Velocity.y > 0.5f) return false;
            Bounds a = _almaCol.bounds;
            float top = CrackTop();
            bool overCrack = a.max.x > CrackX - 0.9f && a.min.x < CrackX + 0.9f;
            bool overBody = a.max.x > _feet.x - 1.4f && a.min.x < _feet.x + 1.4f;
            if (_alma.IsGroundPounding && overCrack && a.min.y <= top + 0.4f && a.min.y >= top - 1.6f)
            {
                Hits = _phase + 1;   // one life per phase
                _alma.Bounce(_stompBounce, 1f, true);
                _hitFlashAt = Time.time;
                Shake(0.5f, 0.45f);
                NinjaSmoke2D.Burst(new Vector2(CrackX, top), 0f);
                Hit?.Invoke(Hits);
                Enter(State.Hurt);
                return true;
            }
            if (!_alma.IsGroundPounding && overBody && a.min.y <= top + 0.3f && a.min.y >= top - 0.6f)
                _alma.Bounce(_softBounce, 1f, false);
            return false;
        }

        private float _hitFlashAt = -10f;
        private bool _nextPhase, _keepWallHits;

        // Flashes red and shakes, then burrows (or loses, on the last hit).
        private void UpdateHurt()
        {
            float u = Mathf.Clamp01(T / 0.6f);
            float pulse = 0.5f + 0.5f * Mathf.Cos(T * 40f);
            _sprite.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), (1f - u) * pulse);
            float shake = (1f - u) * 0.12f;
            _pivot.position = _feet + BallOffset + new Vector2(Random.Range(-shake, shake), Random.Range(-shake * 0.5f, shake * 0.5f));
            if (u < 1f) return;
            _sprite.color = Color.white;
            Place(_feet, 0f);
            if (_phase >= _phases.Length - 1) { BeginDefeat(); return; }
            _nextPhase = true;
            Enter(State.Burrow);
        }

        // Digs into the ground in a cloud of dust and comes out somewhere else, away from Alma.
        private void UpdateBurrow()
        {
            if (T < Time.deltaTime * 1.5f) NinjaSmoke2D.Burst(_feet + Vector2.up * 0.8f, 0.2f);
            _sprite.enabled = T < 0.08f;
            if (T < 0.9f) return;
            if (_nextPhase)
            {
                _phase = Mathf.Min(Hits, _phases.Length - 1);   // phase N+1 after the Nth hit
                if (_backdrop != null) _backdrop.SetPhase(_phase);
            }
            float ax = _alma != null ? _alma.transform.position.x : 0f;
            float x = Mathf.Clamp(ax >= 0f ? ax - 7f : ax + 7f, _wallLeftX + 2f, _wallRightX - 2f);
            if (Mathf.Abs(x - ax) < 4f) x = ax >= 0f ? _wallLeftX + 3f : _wallRightX - 3f;
            _sprite.enabled = true;
            _feet = new Vector2(x, _floorY);
            NinjaSmoke2D.Burst(_feet + Vector2.up * 0.8f, 0.2f);
            StartPhase();
        }

        // ---------- defeat ----------

        private const float DefeatLineTime = 3f;
        private bool _defeatSmoked, _portalShown;

        private void BeginDefeat()
        {
            AlmaTouchControls.Clear();
            ClearSpawned();
            CinematicState.Begin();
            if (_backdrop != null) TensionState.Clear(_backdrop);
            _sprite.flipX = _alma != null && _alma.transform.position.x < _feet.x;
            Place(_feet, 0f);
            _defeatSmoked = _portalShown = false;
            HideAlma(true);
            Enter(State.Defeat);
        }

        // Stands facing Alma (taunt frame 1), says his defeat lines and burrows away; the exit portal appears.
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
            float w = 1f - Mathf.Clamp01((t - (lines + 2.6f)) / ShotOut);
            if (_cam != null && !_reunionDone) _cam.SetShot(_feet + Vector2.up * 1.6f, _introZoom, Mathf.Clamp01(t / 0.9f) * w);   // after the room the view stays on Alma
            if (!_defeatSmoked)
            {
                Play(_tauntState, 0f);
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
                NinjaSmoke2D.Burst(_feet + Vector2.up * 0.8f, 0.2f);
                HideAlma(false);
            }
            if (_defeatSmoked && t >= 0.8f + lines + 0.08f) _sprite.enabled = false;
            if (_defeatSmoked && Reunion(t, 0.8f + lines + 0.4f)) return;
            if (!_portalShown && t >= 0.8f + lines + 0.9f)
            {
                _portalShown = true;
                if (_exitPortal != null)
                {
                    Vector2 at = new Vector2(_portalX, _portalY);
                    _exitPortal.transform.SetParent(null, true);
                    _exitPortal.transform.position = at;
                    _exitPortal.SetActive(true);
                    NinjaSmoke2D.Burst(at);
                }
            }
            if (t >= lines + 2.6f + ShotOut + 0.8f)
            {
                if (_cam != null) _cam.ClearShot();
                AlmaTouchControls.InputLocked = false;
                CinematicState.End();
                Defeated?.Invoke();
                Enter(State.Done);
            }
        }

        // During his defeat speech only he is on screen.
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

        // ---------- stalactites ----------

        // A wall hit shakes some of the arena's stalactites loose (random ones that are hanging).
        private void DropStalactites(int count)
        {
            var ready = _sceneStalactites.FindAll(s => s != null && s.Ready);
            for (int i = 0; i < count && ready.Count > 0; i++)
            {
                int k = Random.Range(0, ready.Count);
                ready[k].Drop();
                ready.RemoveAt(k);
            }
        }

        // Standing on a floating block during the fight: a stalactite falls on Alma every `Camp Interval`.
        private void UpdateCamping()
        {
            bool fight = _state != State.Intro && !IsDefeated;
            if (!fight || _alma == null || _alma.IsDead || _almaCol == null || !_alma.IsGrounded || OnBlock() < 0) { _campTimer = 0f; return; }
            _campTimer += Time.deltaTime;
            if (_campTimer < _campInterval || _stalactitePrefab == null) return;
            _campTimer = 0f;
            var s = Instantiate(_stalactitePrefab, new Vector3(_alma.transform.position.x, _ceilingY, 0f), Quaternion.identity);
            s.Manual = true;
            s.DestroyAfterBurst = true;
            s.Drop();
        }

        private int OnBlock()
        {
            Bounds a = _almaCol.bounds;
            for (int i = 0; i < _blocks.Length; i++)
                if (a.center.x >= _blocks[i].x && a.center.x <= _blocks[i].y && Mathf.Abs(a.min.y - _blocks[i].z) < 0.25f) return i;
            return -1;
        }

        private void ClearSpawned()
        {
            foreach (var s in FindObjectsByType<ProjectileTrap2D>(FindObjectsSortMode.None))
                if (s.DestroyAfterBurst) Destroy(s.gameObject);
        }

        // ---------- contact and respawn ----------

        // Rolling (and while warning or curling) he is lethal; dizzy he is harmless.
        private void CheckContact()
        {
            if (_alma == null || _alma.IsDead || _almaCol == null) return;
            // Against Alma's real (capsule) shape, not her bounding box, whose corners stick out.
            if (_state == State.Roll)
            {
                Vector2 c = BallCenter;
                float r = Radius * _ballHitScale;
                if (((Vector2)_almaCol.ClosestPoint(c) - c).sqrMagnitude < r * r) _alma.Die();
            }
            else if (_state == State.Warn || _state == State.Curl)
            {
                var body = new Rect(_feet.x - _bodyHitSize.x * 0.5f, _feet.y, _bodyHitSize.x, _bodyHitSize.y);
                if (body.Contains(_almaCol.ClosestPoint(body.center))) _alma.Die();
            }
        }

        // Alma died: the stalactites are cleared and he burrows and comes back for the same phase (hits are kept).
        private void OnAlmaRespawned()
        {
            ClearSpawned();
            _campTimer = 0f;
            if (_state == State.Intro || IsDefeated || _state == State.Burrow) return;
            _sprite.color = Color.white;
            _nextPhase = false;
            _keepWallHits = _state != State.Uncurl && _state != State.Dizzy;   // dizzy already: his next try starts over
            Enter(State.Burrow);
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
            Vector2 c = new Vector2(CrackX, CrackTop() + 0.45f);
            for (int i = 0; i < 3; i++)
            {
                float a = Time.time * 4f + i * Mathf.PI * 2f / 3f, depth = Mathf.Sin(a);
                _stars[i].transform.position = c + new Vector2(Mathf.Cos(a) * 0.9f, depth * 0.25f);
                _stars[i].transform.localScale = Vector3.one * (0.4f + 0.1f * depth) / _stars[i].sprite.bounds.size.x;
                _stars[i].transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 200f + i * 40f);
                _stars[i].sortingOrder = _sortingOrder + (depth > 0f ? -1 : 1);
                _stars[i].color = Color.Lerp(Color.white, _accent, 0.35f);   // crystal stars
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
                Color c = d < 2.5f ? new Color(0.1f, 0.15f, 0.25f) : inside ? new Color(0.75f, 0.97f, 1f) : Color.clear;
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.35f, 0.9f, 1f, 0.9f);
            Gizmos.DrawLine(new Vector3(_wallLeftX, _floorY), new Vector3(_wallLeftX, _floorY + 6f));
            Gizmos.DrawLine(new Vector3(_wallRightX, _floorY), new Vector3(_wallRightX, _floorY + 6f));
            Gizmos.DrawLine(new Vector3(_wallLeftX, _floorY), new Vector3(_wallRightX, _floorY));
            if (_blocks != null)
                foreach (var b in _blocks) Gizmos.DrawLine(new Vector3(b.x, b.z), new Vector3(b.y, b.z));
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.8f);
            Gizmos.DrawLine(new Vector3(_wallLeftX, _ceilingY), new Vector3(_wallRightX, _ceilingY));
        }
    }
}
