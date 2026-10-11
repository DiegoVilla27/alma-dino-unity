using System.Collections.Generic;
using AlmaGame.Level;
using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Bosses
{
    // Boss 1 — the Giant Thief Monkey (Docs/Levels/World_1_Jungle/Boss_1.md). Cut-out character: body, head,
    // jaw, crown, arms and legs are separate parts animated by code (placeholder shapes until each part gets
    // its sprite in `Parts`). Hangs from one of three lianas and throws waves of rolling fruit and bouncing
    // coconuts; after each wave he drops exhausted to the ground — jumping on his head is the only way to hurt
    // him. 3 hits win; each hit raises the backdrop phase. Touching his body or a projectile kills Alma; after
    // dying the current cycle restarts (hits are kept).
    [DisallowMultipleComponent]
    public sealed class GiantMonkeyBoss2D : MonoBehaviour
    {
        [System.Serializable]
        public struct Cycle
        {
            public int Projectiles;
            public float Speed;      // roll speed / throw strength (m/s)
            public float Interval;   // seconds between throws
            public float Fatigue;    // seconds lying exhausted
            public bool Swings;      // moves between lianas while throwing
            public bool Doubles;     // throws a fruit and a coconut at once
        }

        [System.Serializable]
        public struct PartArt
        {
            public Sprite Body, Head, Jaw, Crown, Arm, Leg, Fruit, Coconut;
        }

        [Header("Arena (world)")]
        [SerializeField] private float[] _lianaX = { -4.5f, 0f, 4.5f };
        [SerializeField] private float _hangY = 4.5f;          // where his hand grips the liana
        [SerializeField] private float _lianaTopY = 10f;
        [SerializeField] private float _floorY = -6.2f;
        [SerializeField] private float _arenaHalfWidth = 15f;
        [Tooltip("Liana art, repeated vertically to the needed length (import it with Mesh Type: Full Rect). Empty = green placeholder bar.")]
        [SerializeField] private Sprite _lianaSprite;
        [Tooltip("Width of a liana in world units (the sprite keeps its proportions when repeating).")]
        [SerializeField, Min(0.05f)] private float _lianaWidth = 0.6f;

        [Header("Fight")]
        [SerializeField] private Cycle[] _cycles =
        {
            new Cycle { Projectiles = 4, Speed = 6.5f, Interval = 1.3f, Fatigue = 3.2f },
            new Cycle { Projectiles = 5, Speed = 8f, Interval = 1f, Fatigue = 2.8f, Swings = true },
            new Cycle { Projectiles = 7, Speed = 9.5f, Interval = 0.75f, Fatigue = 2.4f, Swings = true, Doubles = true },
        };
        [SerializeField] private float _headBounce = 13.5f;
        [SerializeField, Min(1f)] private float _size = 1f;
        [Tooltip("Order in Layer of the boss sprite; the lianas draw just behind it.")]
        [SerializeField] private int _sortingOrder = -1;

        [Header("Story")]
        [SerializeField, TextArea(1, 2)] private string[] _introLines = { "¡Kikiki! ¿Vienes a por tu huevito, mamá lagarto?", "¡Te enterraré bajo mi cosecha!" };
        [SerializeField] private Sprite _bubbleSprite, _bubbleTailSprite;
        [SerializeField] private Color _accent = new Color(1f, 0.65f, 0.25f);

        [Header("Frame animations")]
        [Tooltip("Animator controller with the states below (Animations/Taunt/Boss_GiantMonkey.controller).")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [SerializeField] private string _tauntState = "Boss_GiantMonkey_Taunt";
        [SerializeField] private string _hangState = "Boss_GiantMonkey_Hang";
        [SerializeField] private string _throwState = "Boss_GiantMonkey_Throw";
        [SerializeField] private string _tiredState = "Boss_GiantMonkey_Tired";
        [Tooltip("Jump between lianas: 4 frames (let go, reach, fly, grab) driven by the jump's progress.")]
        [SerializeField] private string _swingState = "Boss_GiantMonkey_Swing";

        [Header("Cycle: waves, exhaustion, hits")]
        [Tooltip("Alma's bounce when she lands on his head.")]
        [SerializeField] private float _stompBounce = 13.5f;
        [Tooltip("Shout on screen when he lies exhausted (HudBossAlert).")]
        [SerializeField] private string _tiredShout = "¡Agotado!";
        [Tooltip("Top of his head above the feet in each Tired frame (units), measured from the sheet; he breathes, so it moves.")]
        [SerializeField] private float[] _tiredHeadTop = { 3.6f, 4.2f, 4.6f, 5f, 4.5f, 4.05f, 3.85f, 3.9f };
        [SerializeField, Min(0.3f)] private float _stompHalfWidth = 1.4f;
        [SerializeField, TextArea(1, 2)] private string[] _defeatLines = { "¡Kiii! ¡Mi cabeza!", "Esto no se queda así, mamá lagarto... ¡Te esperamos en las cuevas!" };
        [Tooltip("Exit portal (inactive child) shown after the defeat, at Portal X on the ground.")]
        [SerializeField] private GameObject _exitPortal;
        [SerializeField] private float _portalX = 0f;

        [Header("Throwing (after the intro)")]
        [Tooltip("Seconds after Alma gets control back before the first throw.")]
        [SerializeField, Min(0f)] private float _firstThrowDelay = 2f;
        [SerializeField, Min(0.5f)] private float _throwEvery = 3f;
        [Tooltip("Seconds into the throw animation when the fruit leaves his hand (frame 5 at 12 fps).")]
        [SerializeField, Min(0f)] private float _releaseTime = 0.33f;
        [Tooltip("Where the fruit leaves his hand, relative to the fist (units, as drawn facing the camera).")]
        [SerializeField] private Vector2 _releaseOffset = new Vector2(2.21f, -3.47f);
        [SerializeField, Min(0.5f)] private float _throwAnimTime = 0.75f;
        [SerializeField] private float _fruitRollSpeed = 6.5f;
        [SerializeField] private float _fruitRadius = 0.45f;
        [Tooltip("Where he stands for the intro, relative to Alma (x) on the floor.")]
        [SerializeField] private float _introDistance = 6f;
        [Tooltip("Off: after the intro he just stays hanging (no attacks, no contact). The fight is built step by step.")]
        [SerializeField] private bool _fightEnabled = false;

        [Header("Art (leave empty for placeholder shapes)")]
        [SerializeField] private PartArt _art;

        private enum State { Intro, Idle, Throwing, Fall, Tired, Hurt, Vanish, Return, Defeat, Hang, Swing, Drop, Exhausted, Climb, Defeated, Done }

        private State _state;
        private float _stateAt, _nextThrow;
        private int _cycle, _thrown, _liana = 1, _swingFrom;
        private Vector2 _hand;                // hand position on the liana (world)
        private Vector2 _restPos;             // root position when exhausted
        private AlmaMotor2D _alma;
        private BossBackdropPhases _backdrop;
        private SpeechBubble2D _bubble;
        private Transform _root, _body, _head, _jaw, _crown, _armL, _armR, _legL, _legR;
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private readonly List<Transform> _lianas = new List<Transform>();
        private Sprite _blob;
        private float _hitFlashAt = -10f;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private string _playing;
        private Vector2 _introGround;

        public int Hits { get; private set; }
        public bool IsExhausted => _state == State.Exhausted;
        public bool IsDefeated => _state == State.Defeated || _state == State.Done;
        public Vector2 HeadTop => (Vector2)_head.position + Vector2.up * 0.6f * _size;
        public event System.Action<int> Hit;
        public event System.Action Defeated;

        // ---------- setup ----------

        private void Start()
        {
            _alma = FindAnyObjectByType<AlmaMotor2D>();
            _backdrop = FindAnyObjectByType<BossBackdropPhases>();
            if (_alma != null) { _alma.Respawned += OnAlmaRespawned; _alma.SetUnlocked(AlmaAbility.DoubleJump, true); }
            BuildRig();
            BuildSprite();
            foreach (float x in _lianaX) _lianas.Add(MakeLiana(x));
            _hand = new Vector2(_lianaX[_liana], _lianaTopY + 6f);
            if (_backdrop != null) _backdrop.SetPhase(0);
            if (_introLines != null && _introLines.Length > 0 && HudText.Ready)
                _bubble = SpeechBubble2D.Create(_sprite != null ? _sprite.transform : _head, _bubbleSprite, _bubbleTailSprite, "Mono ladrón gigante", _accent);
            Vector2 almaPos = _alma != null ? (Vector2)_alma.transform.position : Vector2.zero;
            _introGround = new Vector2(almaPos.x + _introDistance, GroundUnder(new Vector2(almaPos.x, almaPos.y)) + _groundOffset);
            LevelFadeIn.Focus = _introGround + Vector2.up * 2.6f;   // the opening iris starts on him
            AlmaTouchControls.InputLocked = true;
            CinematicState.Begin();                                         // HUD controls hide during the intro
            if (Hud2D.Instance != null) Hud2D.Instance.HideNow();
            if (HudLevelTitle.Instance != null) HudLevelTitle.Instance.enabled = false;   // the boss card replaces it
            Enter(State.Intro);
        }

        private void OnDestroy()
        {
            if (_alma != null) _alma.Respawned -= OnAlmaRespawned;
            AlmaTouchControls.InputLocked = false;
        }

        private Sprite Blob()
        {
            if (_blob != null) return _blob;
            const int n = 64; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x - 31.5f) - 14f), dy = Mathf.Max(0f, Mathf.Abs(y - 31.5f) - 14f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                tex.SetPixel(x, y, d < 15f ? (d > 12.5f ? new Color(0.12f, 0.08f, 0.08f) : Color.white) : Color.clear);
            }
            tex.Apply();
            return _blob = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        // A joint at `pivot` (local to parent) holding a part drawn from it, `offset` away, `size` big.
        private Transform Part(string name, Transform parent, Vector2 pivot, Sprite art, Vector2 size, Vector2 offset, Color tint, int order)
        {
            var joint = new GameObject(name).transform;
            joint.SetParent(parent, false); joint.localPosition = pivot;
            var sr = new GameObject(name + "_Art").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(joint, false); sr.transform.localPosition = offset;
            sr.sprite = art != null ? art : Blob();
            sr.color = art != null ? Color.white : tint;
            sr.sortingOrder = _sortingOrder - 24 + order;   // placeholder rig keeps its internal order, below the boss order
            var b = sr.sprite.bounds.size;
            sr.transform.localScale = new Vector3(size.x / b.x, size.y / b.y, 1f);
            _renderers.Add(sr);
            return joint;
        }

        private void BuildRig()
        {
            Color fur = new Color(0.42f, 0.27f, 0.16f), mane = new Color(0.86f, 0.84f, 0.8f), skin = new Color(0.78f, 0.55f, 0.45f), leaf = new Color(0.45f, 0.75f, 0.3f);
            _root = new GameObject("Rig").transform; _root.SetParent(transform, false);
            _root.localScale = Vector3.one * _size;
            _body = Part("Body", _root, Vector2.zero, _art.Body, new Vector2(2.1f, 2.3f), Vector2.zero, fur, 20);
            _legL = Part("LegL", _body, new Vector2(-0.55f, -0.95f), _art.Leg, new Vector2(0.65f, 1.25f), new Vector2(0f, -0.55f), fur * 0.85f, 19);
            _legR = Part("LegR", _body, new Vector2(0.55f, -0.95f), _art.Leg, new Vector2(0.65f, 1.25f), new Vector2(0f, -0.55f), fur * 0.85f, 19);
            _armL = Part("ArmL", _body, new Vector2(-1.05f, 0.75f), _art.Arm, new Vector2(0.6f, 1.95f), new Vector2(0f, -0.85f), fur * 0.92f, 21);
            _armR = Part("ArmR", _body, new Vector2(1.05f, 0.75f), _art.Arm, new Vector2(0.6f, 1.95f), new Vector2(0f, -0.85f), fur * 0.92f, 21);
            _head = Part("Head", _body, new Vector2(0f, 1.35f), _art.Head, new Vector2(1.45f, 1.3f), new Vector2(0f, 0.25f), mane, 22);
            _jaw = Part("Jaw", _head, new Vector2(0f, -0.05f), _art.Jaw, new Vector2(0.75f, 0.4f), new Vector2(0f, -0.1f), skin, 23);
            _crown = Part("Crown", _head, new Vector2(0f, 0.85f), _art.Crown, new Vector2(1.25f, 0.35f), Vector2.zero, leaf, 24);
            _armL.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
        }

        private void BuildSprite()
        {
            if (_animatorController == null) return;
            _sprite = new GameObject("Sprite").AddComponent<SpriteRenderer>();
            _sprite.transform.SetParent(transform, false);
            _sprite.sortingOrder = _sortingOrder;
            _animator = _sprite.gameObject.AddComponent<Animator>();
            _animator.runtimeAnimatorController = _animatorController;
        }

        // Plays an animator state (frame 0 and frozen if `freeze`).
        private void Play(string state, bool freeze = false)
        {
            if (_animator == null) return;
            if (_playing != state)
            {
                _animator.Play(state, 0, 0f); _playing = state;
                _baseScale = state == _hangState || state == _throwState || state == _swingState ? _hangScale : 1f;
                if (_sprite != null) _sprite.transform.localScale = Vector3.one * _baseScale;
            }
            _animator.speed = freeze ? 0f : Tempo;
        }

        // The frame-animated sprite is used where an animation exists; the placeholder rig otherwise.
        private void ShowSprite(bool on)
        {
            if (_sprite != null) _sprite.enabled = on;
            bool rig = _sprite == null;   // the placeholder rig only exists for a boss without art
            foreach (var r in _renderers) r.enabled = rig;
        }

        private Transform MakeLiana(float x)
        {
            var sr = new GameObject("Liana").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(transform, false);
            sr.sortingOrder = _sortingOrder - 1;
            float len = _lianaTopY + 6f - _hangY;
            sr.transform.position = new Vector3(x, _hangY + len * 0.5f, 0f);
            if (_lianaSprite != null)
            {
                // Real art: tiled vertically; the scale keeps the sprite's proportions at `Liana Width`.
                sr.sprite = _lianaSprite;
                sr.drawMode = SpriteDrawMode.Tiled; sr.tileMode = SpriteTileMode.Continuous;
                float k = _lianaWidth / _lianaSprite.bounds.size.x;
                sr.transform.localScale = new Vector3(k, k, 1f);
                sr.size = new Vector2(_lianaSprite.bounds.size.x, len / k);
            }
            else
            {
                sr.sprite = Blob(); sr.color = new Color(0.3f, 0.5f, 0.2f);
                sr.transform.localScale = new Vector3(0.18f / sr.sprite.bounds.size.x, len / sr.sprite.bounds.size.y, 1f);
            }
            return sr.transform;
        }

        // ---------- state machine ----------

        private void Enter(State s)
        {
            // Exhausted on the ground = the window to hit him: the HUD tells the player wherever he is.
            if (s == State.Tired && _state != State.Tired) HudBossAlert.Show(() => new Vector2(_groundPos.x, TiredHeadY()), C.Fatigue, _tiredShout, _accent);
            else if (_state == State.Tired && s != State.Tired) HudBossAlert.Hide();
            _state = s; _stateAt = Time.time;
        }
        private float T => Time.time - _stateAt;
        private Cycle C => _cycles[Mathf.Clamp(_cycle, 0, _cycles.Length - 1)];

        private void Update()
        {
            if (_sprite != null && _state != State.Intro)
            {
                // Hanging and swinging use the Hang animation (hand on the liana); other states wait for their art.
                bool hanging = _state == State.Hang || _state == State.Swing || _state == State.Idle || _state == State.Throwing;
                bool cycle = _state >= State.Fall && _state <= State.Defeat;
                if (!cycle) ShowSprite(hanging);
                if (hanging) { if (_state == State.Swing && !_fightEnabled) PlaySwingFrame(); else Play(_state == State.Throwing ? _throwState : _hangState); _sprite.transform.position = _hand; }
            }
            switch (_state)
            {
                case State.Intro: UpdateIntro(); break;
                case State.Fall: UpdateFall(); break;
                case State.Tired: UpdateTired(); break;
                case State.Hurt: UpdateHurt(); break;
                case State.Vanish: UpdateVanish(); break;
                case State.Return: UpdateReturn(); break;
                case State.Defeat: UpdateDefeat(); break;
                case State.Idle: UpdateIdle(); break;
                case State.Throwing: UpdateThrowing(); break;
                case State.Swing when !_fightEnabled: UpdateSwingTo(); break;
                case State.Hang: UpdateHang(); break;
                case State.Swing: UpdateSwing(); break;
                case State.Drop: UpdateDrop(); break;
                case State.Exhausted: UpdateExhausted(); break;
                case State.Climb: UpdateClimb(); break;
                case State.Defeated: UpdateDefeated(); break;
            }
            Animate();
            CheckContact();
        }

        // Intro cinematic (Alma can't act; the camera frames the monkey):
        // 1. on the ground in front of Alma, facing her, taunt frame 1 → 2. says his lines → 3. beats his chest →
        // 4. vanishes in a burst of smoke and leaves, like a ninja → 5. drops from the top of the screen hanging
        // from the centre liana → 6. boss title card (HudBanners BossIntro) → 7. the fight starts.
        [Header("Intro")]
        [SerializeField] private string _bossName = "Mono Ladrón Gigante";
        [SerializeField] private string _bossEpithet = "El Rey de la Copa";
        [SerializeField] private string _bossKicker = "Jefe del Mundo 1";
        [SerializeField, Range(3f, 8f)] private float _introZoom = 5.5f;
        [Tooltip("Vertical nudge of the monkey on the ground (units; the art has a little empty space under the feet).")]
        [SerializeField] private float _groundOffset = -0.11f;
        [Tooltip("How far the liana goes into his fist (units), so there is never a gap.")]
        [SerializeField] private float _lianaGrip = 0.35f;

        private const float ShotIn = 0.9f, LinesAt = 1.1f, LineTime = 3f, TauntTime = 1.4f, VanishTime = 0.9f, DescendTime = 1.4f, ShotOut = 0.9f;
        private float _tauntAt, _vanishAt, _descendAt, _cardAt, _endAt = -1f;
        private bool _smoked, _cardShown;
        private AlmaCameraFollow _cam;

        private void PlanIntro()
        {
            _tauntAt = LinesAt + (_introLines?.Length ?? 0) * LineTime + 0.2f;
            _vanishAt = _tauntAt + TauntTime;
            _descendAt = _vanishAt + VanishTime;
            _cardAt = _descendAt + DescendTime + 0.3f;
            _cam = FindAnyObjectByType<AlmaCameraFollow>();
            if (_sprite != null) { _sprite.flipX = true; _sprite.transform.position = _introGround; }   // the art faces right; he faces Alma
        }

        private float GroundUnder(Vector2 p)
        {
            foreach (var h in Physics2D.RaycastAll(p + Vector2.up * 0.5f, Vector2.down, 20f))
            {
                if (h.collider.isTrigger || (h.rigidbody != null && h.rigidbody.GetComponent<AlmaMotor2D>() != null)) continue;
                return h.point.y;
            }
            return _floorY;
        }

        private void UpdateIntro()
        {
            if (_tauntAt == 0f) PlanIntro();
            float t = T;
            AlmaTouchControls.InputLocked = true;   // held every frame: other systems clear the touch state on start
            Vector2 hangPos = new Vector2(_lianaX[_liana], _hangY);
            ShowSprite(_sprite != null && (t < _vanishAt + 0.08f || t >= _descendAt));
            // Camera: frames the monkey on the ground, then the liana he comes down.
            Vector2 groundShot = _introGround + Vector2.up * 2.6f, hangShot = hangPos + Vector2.down * 2.8f;
            float move = Mathf.Clamp01((t - _descendAt) / 0.9f);
            Vector2 shot = Vector2.Lerp(groundShot, hangShot, move * move * (3f - 2f * move));
            float w = 1f;   // already framing him from the first frame (hidden by the fade from black)
            // The fade from black at the start comes from LevelFadeIn (every level and boss).
            if (_endAt > 0f) w = 1f - Mathf.Clamp01((t - _endAt) / ShotOut);
            if (_cam != null) _cam.SetShot(shot, _introZoom, w);

            if (_sprite != null)
            {
                if (t < _tauntAt) { Play(_tauntState, true); _sprite.transform.position = _introGround; }
                else if (t < _vanishAt) { Play(_tauntState); _sprite.transform.position = _introGround; }
                else if (t >= _descendAt)
                {
                    float u = Mathf.Clamp01((t - _descendAt) / DescendTime);
                    float e = 1f - Mathf.Pow(1f - u, 3f);
                    Play(_hangState); _sprite.flipX = false;
                    _sprite.transform.position = Vector2.Lerp(hangPos + Vector2.up * 12f, hangPos, e);
                    _hand = _sprite.transform.position;
                }
            }
            // Lines.
            if (_bubble != null)
                for (int i = 0; i < (_introLines?.Length ?? 0); i++)
                {
                    float at = LinesAt + i * LineTime;
                    if (t >= at && t - Time.deltaTime < at) _bubble.Say(_introLines[i]);
                }
            if (_bubble != null && t >= _tauntAt - 0.2f && t - Time.deltaTime < _tauntAt - 0.2f) _bubble.Hide();
            // Ninja vanish.
            if (!_smoked && t >= _vanishAt) { _smoked = true; Smoke(_introGround + Vector2.up * 2.4f); }
            // Title card, then the fight.
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
                _hand = hangPos;
                if (_fightEnabled) StartWave(); else Enter(State.Idle);
            }
        }

        // Ninja smoke (shared with the story moments).
        private void Smoke(Vector2 at) => NinjaSmoke2D.Burst(at);

        // Attack loop (no fight rules yet): every few seconds he picks a throw that reaches Alma — from his own
        // liana if possible (normal drop, low roll under the platforms, or a lob at her), otherwise he swings to
        // the liana from which one does — and throws it with the Throw animation.
        private enum ThrowKind { Drop, Low, Lob }
        private bool _released;
        private Vector2 _throwVelocity;
        private float _throwSide = 1f;
        private int _swingTo = -1;
        [Header("Swinging between lianas")]
        [SerializeField, Min(0.2f)] private float _swingTime = 0.8f;
        [Header("Size and tempo")]
        [Tooltip("Scale of the Hang and Throw art (anchored at the fist, so he stays on the liana).")]
        [SerializeField, Min(0.5f)] private float _hangScale = 1.3f;
        [Tooltip("Each new phase plays this much faster: animations, swings, throws, falls and returns (0.15 = +15 % per phase).")]
        [SerializeField, Range(0f, 0.5f)] private float _phaseSpeedUp = 0.15f;
        private float _baseScale = 1f;
        private float Tempo => 1f + Mathf.Max(0, _cycle) * _phaseSpeedUp;
        [SerializeField, Min(0f)] private float _swingHeight = 1.6f;

        private void UpdateIdle()
        {
            if (_alma == null || _animator == null || _alma.IsDead) return;
            if (T < (_thrown == 0 ? _firstThrowDelay : (_thrownInWave == 0 ? 1f : C.Interval))) return;
            if (_thrownInWave >= C.Projectiles) { BeginFall(); return; }
            int liana = PlanThrow();
            if (liana != _liana) { _swingFrom = _liana; _swingTo = liana; Enter(State.Swing); return; }
            BeginThrow();
        }

        private void BeginThrow()
        {
            if (_sprite != null) { _sprite.flipX = _throwSide < 0f; _sprite.transform.localRotation = Quaternion.identity; }
            _released = false;
            Enter(State.Throwing);
        }

        private void UpdateThrowing()
        {
            if (!_released && T >= _releaseTime / Tempo)
            {
                _released = true; _thrown++; _thrownInWave++;
                Vector2 from = ReleasePoint(_hand, _throwSide);
                MonkeyProjectile2D.Throw(MonkeyProjectile2D.Kind.RollingFruit, from, _throwVelocity, C.Speed, _fruitRadius,
                    _art.Fruit != null ? _art.Fruit : Blob(), _art.Fruit != null ? Color.white : new Color(0.95f, 0.45f, 0.2f), _arenaHalfWidth, _alma);
            }
            if (T >= _throwAnimTime / Tempo) Enter(State.Idle);
        }

        // ---------- wave → exhausted → hit / recover ----------

        private int _thrownInWave;
        private bool _vanishNext;            // after the vanish: true = next phase, false = same phase
        private Vector2 _groundPos;          // feet position while exhausted
        private SpriteRenderer[] _stars;
        private Sprite _starSprite;

        private void BeginFall()
        {
            _groundPos = new Vector2(_hand.x, GroundUnder(new Vector2(_hand.x, _hand.y - 3f)) + _groundOffset);
            if (_sprite != null) { _sprite.flipX = false; _sprite.transform.localRotation = Quaternion.identity; }
            Enter(State.Fall);
        }

        // Lets go of the liana and drops to the ground below it; lands with a squash, dust and a shake.
        private void UpdateFall()
        {
            float time = 0.55f / Tempo;
            float u = Mathf.Clamp01(T / time);
            ShowSprite(true);
            if (u < 1f)
            {
                Play(_hangState, true);
                _sprite.transform.position = Vector2.Lerp(_hand, _groundPos + Vector2.up * 5.6f * _hangScale, u * u);
                _sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 10f * u);
                return;
            }
            _sprite.transform.localRotation = Quaternion.identity;
            _sprite.transform.position = _groundPos;
            Play(_tiredState);
            Dust(_groundPos);
            CameraShake(0.25f, 0.3f);
            Enter(State.Tired);
        }

        private void UpdateTired()
        {
            ShowSprite(true);
            Play(_tiredState);
            float land = Mathf.Clamp01(T / 0.25f);
            float squash = (1f - land) * 0.18f;
            _sprite.transform.localScale = new Vector3(1f + squash, 1f - squash, 1f);
            _sprite.transform.position = _groundPos;
            UpdateStars(true);
            if (StompCheck()) return;
            if (T >= C.Fatigue) { UpdateStars(false); _vanishNext = false; Enter(State.Vanish); }
        }

        // Top of the head right now (world), from the frame being shown.
        private float TiredHeadY()
        {
            int frame = 0;
            string n = _sprite != null && _sprite.sprite != null ? _sprite.sprite.name : "";
            int us = n.LastIndexOf('_');
            if (us >= 0) int.TryParse(n.Substring(us + 1), out frame);
            frame = Mathf.Clamp(frame, 0, Mathf.Max(0, _tiredHeadTop.Length - 1));
            return _groundPos.y + (_tiredHeadTop.Length > 0 ? _tiredHeadTop[frame] : 4f);
        }

        // Alma landing on his head (falling onto it) hurts him. While exhausted he is harmless.
        private bool StompCheck()
        {
            if (_alma == null || _alma.IsDead) return false;
            var col = _alma.GetComponent<Collider2D>(); if (col == null) return false;
            Bounds a = col.bounds;
            float top = TiredHeadY();
            bool overX = a.max.x > _groundPos.x - _stompHalfWidth && a.min.x < _groundPos.x + _stompHalfWidth;
            if (overX && _alma.Velocity.y <= 0.5f && a.min.y <= top + 0.4f && a.min.y >= top - 0.8f)
            {
                Hits = _cycle + 1;   // one life per phase: the hit always belongs to the current phase
                _alma.Bounce(_stompBounce, 1f, true);
                _hitFlashAt = Time.time;
                CameraShake(0.4f, 0.4f);
                UpdateStars(false);
                Hit?.Invoke(Hits);
                Enter(State.Hurt);
                return true;
            }
            return false;
        }

        // Hit reaction: flashes red and shakes, then vanishes (or loses, on the last hit).
        private void UpdateHurt()
        {
            float u = Mathf.Clamp01(T / 0.6f);
            float pulse = 0.5f + 0.5f * Mathf.Cos(T * 40f);
            _sprite.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), (1f - u) * pulse);
            float shake = (1f - u) * 0.12f;
            _sprite.transform.position = _groundPos + new Vector2(Random.Range(-shake, shake), Random.Range(-shake * 0.5f, shake * 0.5f));
            _sprite.transform.localScale = Vector3.one;
            if (u < 1f) return;
            _sprite.color = Color.white;
            _sprite.transform.position = _groundPos;
            if (_cycle >= _cycles.Length - 1) { BeginDefeat(); return; }
            _vanishNext = true;
            Enter(State.Vanish);
        }

        // Ninja exit: smoke, gone; then he returns hanging from a liana.
        private void UpdateVanish()
        {
            if (T < Time.deltaTime * 1.5f) { Smoke((Vector2)_sprite.bounds.center); }
            ShowSprite(T < 0.08f);
            if (T < 0.9f) return;
            if (_vanishNext)
            {
                _cycle = Mathf.Min(Hits, _cycles.Length - 1);   // phase N+1 after the Nth hit
                if (_backdrop != null) _backdrop.SetPhase(_cycle);
            }
            _thrownInWave = 0;
            _liana = _lianaX.Length / 2;
            Enter(State.Return);
        }

        // Drops from the top of the screen hanging from the liana, like in the intro, then the next wave.
        private void UpdateReturn()
        {
            ShowSprite(true);
            Play(_hangState);
            _sprite.flipX = false; _sprite.transform.localScale = Vector3.one * _baseScale; _sprite.color = Color.white;
            Vector2 hangPos = new Vector2(_lianaX[_liana], _hangY);
            float u = Mathf.Clamp01(T / (DescendTime / Tempo)), e = 1f - Mathf.Pow(1f - u, 3f);
            _hand = Vector2.Lerp(hangPos + Vector2.up * 12f, hangPos, e);
            _sprite.transform.position = _hand;
            if (u >= 1f) { _hand = hangPos; _thrown = 1; Enter(State.Idle); }
        }

        // ---------- defeat ----------

        private bool _defeatSmoked, _portalShown;
        private const float DefeatLineTime = 3f;

        private void BeginDefeat()
        {
            AlmaTouchControls.Clear();
            ClearProjectiles();
            CinematicState.Begin();
            if (_backdrop != null) TensionState.Clear(_backdrop);
            if (_sprite != null) _sprite.flipX = _alma != null && _alma.transform.position.x < _groundPos.x;   // taunt art faces right
            _defeatSmoked = _portalShown = false;
            HideAlma(true);
            Enter(State.Defeat);
        }

        // Stands facing Alma (taunt frame 1), says his defeat lines, vanishes; the exit portal appears.
        private void UpdateDefeat()
        {
            AlmaTouchControls.InputLocked = true;
            float t = T, lines = (_defeatLines?.Length ?? 0) * DefeatLineTime;
            float w = 1f - Mathf.Clamp01((t - (lines + 2.6f)) / ShotOut);
            if (_cam != null) _cam.SetShot(_groundPos + Vector2.up * 2.6f, _introZoom, Mathf.Clamp01(t / ShotIn) * w);
            if (!_defeatSmoked)
            {
                ShowSprite(true);
                Play(_tauntState, true);
                _sprite.transform.position = _groundPos;
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
                Smoke((Vector2)_sprite.bounds.center);
                HideAlma(false);
            }
            if (_defeatSmoked && t >= 0.8f + lines + 0.08f) ShowSprite(false);
            if (!_portalShown && t >= 0.8f + lines + 0.9f)
            {
                _portalShown = true;
                if (_exitPortal != null)
                {
                    float gy = GroundUnder(new Vector2(_portalX, _hangY));
                    _exitPortal.transform.SetParent(null, true);
                    _exitPortal.transform.position = new Vector3(_portalX, gy + 1.4f, 0f);
                    _exitPortal.SetActive(true);
                    Smoke(new Vector2(_portalX, gy + 1.4f));
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

        // During his defeat speech only he is on screen: Alma's renderers are switched off and restored when he vanishes.
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

        private void Dust(Vector2 at)
        {
            Smoke(at + Vector2.up * 0.4f);
        }

        // Three little stars circling above his head while he is exhausted.
        private void UpdateStars(bool on)
        {
            if (_stars == null)
            {
                _starSprite = MakeStar();
                _stars = new SpriteRenderer[3];
                for (int i = 0; i < 3; i++)
                {
                    _stars[i] = new GameObject("Star").AddComponent<SpriteRenderer>();
                    _stars[i].transform.SetParent(transform, false);
                    _stars[i].sprite = _starSprite; _stars[i].sortingOrder = _sortingOrder + 1;
                }
            }
            foreach (var st in _stars) st.enabled = on;
            if (!on) return;
            Vector2 c = new Vector2(_groundPos.x, TiredHeadY() + 0.35f);
            for (int i = 0; i < 3; i++)
            {
                float a = Time.time * 4f + i * Mathf.PI * 2f / 3f;
                float depth = Mathf.Sin(a);
                _stars[i].transform.position = c + new Vector2(Mathf.Cos(a) * 1.1f, depth * 0.3f);
                _stars[i].transform.localScale = Vector3.one * (0.45f + 0.12f * depth) / _starSprite.bounds.size.x;
                _stars[i].transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * 200f + i * 40f);
                _stars[i].sortingOrder = _sortingOrder + (depth > 0f ? -1 : 1);
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
                Color c = !inside ? (d < 2.5f ? new Color(0.15f, 0.1f, 0.05f) : Color.clear) : (d < 2.5f ? new Color(0.15f, 0.1f, 0.05f) : new Color(1f, 0.86f, 0.25f));
                tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        // Swing from `_swingFrom` to `_swingTo`: lets go, flies in an arc leaning into the jump, grabs the next liana.
        private void UpdateSwingTo()
        {
            float u = Mathf.Clamp01(T / (_swingTime / Tempo)), e = u * u * (3f - 2f * u);
            float x0 = _lianaX[_swingFrom], x1 = _lianaX[_swingTo], dir = Mathf.Sign(x1 - x0);
            _hand = new Vector2(Mathf.Lerp(x0, x1, e), _hangY + Mathf.Sin(u * Mathf.PI) * _swingHeight);
            // The art reaches to the right of the image: mirror it when he jumps to the left.
            if (_sprite != null) { _sprite.flipX = dir < 0f; _sprite.transform.localRotation = Quaternion.identity; }
            if (u >= 1f)
            {
                _liana = _swingTo;
                _hand = new Vector2(_lianaX[_liana], _hangY);
                if (_sprite != null) { _sprite.transform.localRotation = Quaternion.identity; _sprite.transform.localScale = Vector3.one * _baseScale; }
                PlanThrow();          // Alma may have moved during the swing
                BeginThrow();
            }
        }

        // Shows the swing frame for the jump's progress (frame 1 at take-off … frame 4 grabbing the liana).
        private void PlaySwingFrame()
        {
            if (_animator == null) return;
            Play(_swingState, true);
            float u = Mathf.Clamp01(T / (_swingTime / Tempo));
            _animator.Play(_swingState, 0, Mathf.Min(u, 0.999f));
            _animator.Update(0f);   // paused animator: apply the chosen frame now
        }

        private Vector2 ReleasePoint(Vector2 hand, float side) => hand + new Vector2(_releaseOffset.x * side, _releaseOffset.y) * _hangScale;

        // Chooses the liana and the throw: own liana first (drop, low, lob), then the others by distance to Alma.
        // Sets _throwVelocity/_throwSide for the chosen one and returns its liana index.
        public string LastPlan { get; private set; } = "";

        private int PlanThrow()
        {
            Vector2 alma = _alma.transform.position;
            var order = new List<int> { _liana };
            var others = new List<int>(); for (int i = 0; i < _lianaX.Length; i++) if (i != _liana) others.Add(i);
            others.Sort((a, b) => Mathf.Abs(_lianaX[a] - alma.x).CompareTo(Mathf.Abs(_lianaX[b] - alma.x)));
            order.AddRange(others);
            foreach (int li in order)
            {
                Vector2 hand = new Vector2(_lianaX[li], _hangY);
                float side = alma.x < hand.x ? -1f : 1f;
                Vector2 from = ReleasePoint(hand, side);
                foreach (ThrowKind k in new[] { ThrowKind.Drop, ThrowKind.Low, ThrowKind.Lob })
                {
                    Vector2 v = Launch(k, from, side);
                    if (WouldHit(from, v))
                    {
                        _throwVelocity = v; _throwSide = side;
                        LastPlan = $"liana {li} {k} v {v}";
                        return li;
                    }
                }
            }
            LastPlan = "none";
            // Nothing reaches her: nearest liana, normal throw.
            int best = order[order.Count > 1 ? 1 : 0];
            float s2 = alma.x < _lianaX[best] ? -1f : 1f;
            _throwSide = s2; _throwVelocity = Launch(ThrowKind.Drop, ReleasePoint(new Vector2(_lianaX[best], _hangY), s2), s2);
            return Mathf.Abs(_lianaX[_liana] - alma.x) <= Mathf.Abs(_lianaX[best] - alma.x) ? _liana : best;
        }

        private Vector2 Launch(ThrowKind k, Vector2 from, float side)
        {
            switch (k)
            {
                case ThrowKind.Low: return new Vector2(side * 0.7f, -1f);          // drops short, then rolls under platforms
                case ThrowKind.Lob:
                {
                    Vector2 target = (Vector2)_alma.transform.position + Vector2.up * 0.6f; const float t = 0.85f, g = 22f;
                    return new Vector2((target.x - from.x) / t, (target.y - from.y + 0.5f * g * t * t) / t);
                }
                default: return new Vector2(side * 2.5f, -2f);
            }
        }

        // Same rules as MonkeyProjectile2D: falls, rolls on whatever is under it, bursts on walls; true if it
        // touches Alma within 3.5 s.
        private bool WouldHit(Vector2 pos, Vector2 vel)
        {
            var almaCol = _alma.GetComponent<Collider2D>(); if (almaCol == null) return false;
            Bounds ab = almaCol.bounds; ab.Expand(_fruitRadius * 2f);
            const float dt = 0.02f, g = 22f; bool rolling = false;
            for (float t = 0f; t < 3.5f; t += dt)
            {
                if (!rolling) vel.y -= g * dt;
                Vector2 next = pos + vel * dt;
                if (vel.y <= 0f && SimGround(pos, out float gy) && next.y - _fruitRadius <= gy)
                {
                    next.y = gy + _fruitRadius; rolling = true;
                    vel = new Vector2(Mathf.Sign(vel.x == 0f ? 1f : vel.x) * C.Speed, 0f);
                }
                else if (rolling && !SimGround(next + Vector2.up * _fruitRadius, out _)) rolling = false;
                pos = next;
                if (ab.Contains(new Vector3(pos.x, pos.y, ab.center.z))) return true;
                foreach (var c in Physics2D.OverlapCircleAll(pos, _fruitRadius))
                {
                    if (c.isTrigger || c == almaCol || (c.attachedRigidbody != null && c.attachedRigidbody.GetComponent<AlmaMotor2D>() != null)) continue;
                    Vector2 d = c.ClosestPoint(pos) - pos;
                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y) && Mathf.Sign(d.x) == Mathf.Sign(vel.x)) return false;   // wall
                }
                if (Mathf.Abs(pos.x) > _arenaHalfWidth || pos.y < -30f) return false;
            }
            return false;
        }

        private bool SimGround(Vector2 from, out float y)
        {
            y = 0f;
            foreach (var h in Physics2D.RaycastAll(from + Vector2.up * 0.2f, Vector2.down, 3f))
            {
                if (h.collider.isTrigger || (h.rigidbody != null && h.rigidbody.GetComponent<AlmaMotor2D>() != null)) continue;
                y = h.point.y; return true;
            }
            return false;
        }

        private void StartWave() { _thrown = 0; _nextThrow = Time.time + 0.9f; Enter(State.Hang); }

        private void UpdateHang()
        {
            _hand = new Vector2(_lianaX[_liana], _hangY);
            if (Time.time < _nextThrow) return;
            if (_thrown >= C.Projectiles) { Enter(State.Drop); _restPos = new Vector2(_hand.x, _floorY + 1.6f * _size); return; }
            ThrowNow();
            _thrown++;
            _nextThrow = Time.time + C.Interval;
            if (C.Swings && _thrown % 2 == 0 && _thrown < C.Projectiles) { _swingFrom = _liana; _liana = PickLiana(); Enter(State.Swing); }
        }

        private int PickLiana()
        {
            int next; do next = Random.Range(0, _lianaX.Length); while (next == _liana && _lianaX.Length > 1);
            return next;
        }

        private void UpdateSwing()
        {
            float u = Mathf.Clamp01(T / 0.8f), e = u * u * (3f - 2f * u);
            float x = Mathf.Lerp(_lianaX[_swingFrom], _lianaX[_liana], e);
            _hand = new Vector2(x, _hangY - Mathf.Sin(e * Mathf.PI) * 1.4f);
            if (u >= 1f) Enter(State.Hang);
        }

        private Vector2 ThrowHand => (Vector2)_armR.position + Vector2.down * 1.4f * _size;

        private void ThrowNow()
        {
            if (_alma == null) return;
            _throwAt = Time.time;
            Vector2 from = ThrowHand;
            float dir = Mathf.Sign(_alma.transform.position.x - from.x); if (dir == 0f) dir = 1f;
            bool almaHigh = _alma.transform.position.y > _floorY + 1.2f;
            bool coconut = C.Doubles || almaHigh || (C.Swings && _thrown % 2 == 1);
            bool fruit = C.Doubles || !coconut;
            if (fruit)
                MonkeyProjectile2D.Throw(MonkeyProjectile2D.Kind.RollingFruit, from, new Vector2(dir * 2.5f, -2f), C.Speed, 0.45f,
                    _art.Fruit != null ? _art.Fruit : Blob(), _art.Fruit != null ? Color.white : new Color(0.95f, 0.45f, 0.2f), _arenaHalfWidth, _alma);
            if (coconut)
            {
                Vector2 target = _alma.transform.position;
                float tt = Mathf.Lerp(1.3f, 0.95f, (C.Speed - 6.5f) / 3f);
                Vector2 v = new Vector2((target.x - from.x) / tt, (target.y - from.y + 0.5f * 22f * tt * tt) / tt);
                MonkeyProjectile2D.Throw(MonkeyProjectile2D.Kind.Coconut, from, v, C.Speed * 0.7f, 0.5f,
                    _art.Coconut != null ? _art.Coconut : Blob(), _art.Coconut != null ? Color.white : new Color(0.5f, 0.33f, 0.2f), _arenaHalfWidth, _alma);
            }
        }

        private float _throwAt = -10f;

        private void UpdateDrop()
        {
            // Lets go and falls to the floor under his liana.
            float u = Mathf.Clamp01(T / 0.55f);
            if (u >= 1f) { CameraShake(0.25f, 0.3f); Enter(State.Exhausted); }
        }

        private void UpdateExhausted()
        {
            if (T >= C.Fatigue) { Enter(State.Climb); }
        }

        private void UpdateClimb()
        {
            if (T >= 1f) StartWave();
        }

        private void UpdateDefeated()
        {
            // Jumps up onto the liana and flees off the top of the screen, toward the caves.
            if (T >= 4.5f) { Enter(State.Done); gameObject.SetActive(false); }
        }

        // ---------- contact ----------

        private void CheckContact()
        {
            if (!_fightEnabled || _alma == null || _alma.IsDead || _state == State.Intro || _state == State.Idle || IsDefeated) return;
            var col = _alma.GetComponent<Collider2D>(); if (col == null) return;
            Bounds a = col.bounds;
            Vector2 headTop = HeadTop;
            // Stomp: only while exhausted, Alma coming down onto the top of his head.
            if (_state == State.Exhausted && _alma.Velocity.y <= 0.5f && Mathf.Abs(a.center.x - headTop.x) < 0.95f * _size
                && a.min.y <= headTop.y + 0.25f && a.min.y >= headTop.y - 0.7f * _size)
            {
                TakeHit(); return;
            }
            // Body: lethal on contact.
            Rect body = BodyRect();
            if (a.max.x > body.xMin && a.min.x < body.xMax && a.max.y > body.yMin && a.min.y < body.yMax) _alma.Die();
        }

        private Rect BodyRect()
        {
            Vector2 c = _body.position; float w = 1.9f * _size, h = 2.2f * _size;
            if (_state == State.Exhausted || _state == State.Drop) h *= 0.7f;
            return new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
        }

        private void TakeHit()
        {
            Hits++;
            _alma.Bounce(_headBounce, 1f, true);
            _hitFlashAt = Time.time;
            CameraShake(0.35f, 0.35f);
            ClearProjectiles();
            Hit?.Invoke(Hits);
            if (Hits >= _cycles.Length)
            {
                Enter(State.Defeated);
                if (HudBanners.Instance != null) HudBanners.Instance.Show(new Banner { Kind = BannerKind.Title, Kicker = "Mundo 1 completado", Headline = "Jungla Esmeralda",
                    Text = "El gran simio huye hacia el abismo de las cuevas.", Accent = new Color(0.55f, 1f, 0.4f) });
                Defeated?.Invoke();
                return;
            }
            _cycle = Hits;
            if (_backdrop != null) _backdrop.SetPhase(_cycle);
            HudBanners.Show(BannerKind.Shout, Hits == 1 ? "¡Uno!" : "¡Dos!", null, _accent);
            Enter(State.Climb);
        }

        private void OnAlmaRespawned()
        {
            if (!_fightEnabled)
            {
                // Dying clears the fruit; the wave keeps its count, so he still gets tired after his throws
                // (restarting it made the fast phases endless for a player who keeps getting hit).
                ClearProjectiles();
                if (_state == State.Fall || _state == State.Tired) { _vanishNext = false; Enter(State.Vanish); }
                else if (_state == State.Idle || _state == State.Throwing) { _thrown = 1; Enter(State.Idle); }
                return;
            }
            // Dying restarts the current cycle (hits are kept): he climbs back and throws a fresh wave.
            ClearProjectiles();
            if (!_fightEnabled || _state == State.Intro || _state == State.Idle || IsDefeated) return;
            Enter(State.Climb);
        }

        private static void ClearProjectiles()
        {
            foreach (var p in FindObjectsByType<MonkeyProjectile2D>(FindObjectsSortMode.None)) Destroy(p.gameObject);
        }

        private static void CameraShake(float amp, float time)
        {
            var cam = FindAnyObjectByType<AlmaCameraFollow>();
            if (cam != null) cam.Shake(amp, time);
        }

        // ---------- cut-out animation ----------

        private static float Ease(float t) => 1f - (1f - t) * (1f - t);

        private void Animate()
        {
            float t = Time.time, s = _size;
            Vector3 rootPos; float bodyRot = 0f, armL = 0f, armR = 0f, legL = 0f, legR = 0f, head = 0f, jaw = 0f, bob = 0f;
            Vector2 hanging = _hand + Vector2.down * 2.6f * s;
            switch (_state)
            {
                case State.Intro:
                case State.Hang:
                {
                    // Hangs by the left hand, swaying; the right arm winds up and throws.
                    float sway = Mathf.Sin(t * 2.2f) * 6f;
                    rootPos = hanging; bodyRot = sway;
                    armL = 180f - sway * 0.5f;
                    float th = Mathf.Clamp01((t - _throwAt) / 0.45f);
                    float wind = _nextThrow - t < 0.35f && _state == State.Hang ? Mathf.InverseLerp(0.35f, 0f, _nextThrow - t) : 0f;
                    armR = th < 1f ? Mathf.Lerp(150f, -40f, Ease(th)) : Mathf.Lerp(-20f, 150f, wind);
                    legL = 15f + Mathf.Sin(t * 3f) * 10f; legR = -15f - Mathf.Sin(t * 3f + 1f) * 10f;
                    jaw = _state == State.Intro ? Mathf.Abs(Mathf.Sin(t * 9f)) * 0.25f : (th < 0.5f ? 0.3f : 0f);
                    head = Mathf.Sin(t * 1.7f) * 5f;
                    break;
                }
                case State.Swing:
                {
                    float u = Mathf.Clamp01(T / 0.8f);
                    rootPos = hanging; bodyRot = Mathf.Lerp(-25f, 25f, u) * Mathf.Sign(_lianaX[_liana] - _lianaX[_swingFrom]);
                    armL = 180f; armR = 120f; legL = 40f; legR = -40f; jaw = 0.2f;
                    break;
                }
                case State.Drop:
                {
                    float u = Mathf.Clamp01(T / 0.55f);
                    Vector2 from = hanging; Vector2 to = new Vector2(_hand.x, _floorY + 1.15f * s);
                    rootPos = Vector2.Lerp(from, to, u * u);
                    bodyRot = Mathf.Lerp(0f, 12f, u); armL = Mathf.Lerp(180f, 140f, u); armR = 140f; legL = 30f; legR = -30f; jaw = 0.3f;
                    break;
                }
                case State.Exhausted:
                {
                    // Slumped on the floor, panting; the head droops (that is where to land).
                    float pant = Mathf.Sin(t * 7f);
                    rootPos = new Vector2(_hand.x, _floorY + 1.15f * s + pant * 0.04f);
                    bodyRot = 12f; armL = 20f; armR = -20f; legL = 70f; legR = -70f; head = -12f + pant * 3f; jaw = 0.35f + pant * 0.08f;
                    bob = pant;
                    break;
                }
                case State.Climb:
                {
                    float u = Mathf.Clamp01(T / 1f);
                    Vector2 from = new Vector2(_hand.x, _floorY + 1.15f * s), to = new Vector2(_lianaX[_liana], _hangY) + Vector2.down * 2.6f * s;
                    rootPos = Vector2.Lerp(from, to, Ease(u)) + Vector2.up * Mathf.Sin(u * Mathf.PI) * 1.2f;
                    _hand = new Vector2(_lianaX[_liana], _hangY);
                    armL = 180f; armR = 170f; legL = 50f; legR = -50f; jaw = 0.25f;
                    break;
                }
                case State.Defeated:
                {
                    float u = Mathf.Clamp01((T - 1.2f) / 1.6f);
                    Vector2 start = _restPos.x != 0f || _restPos.y != 0f ? new Vector2(_hand.x, _floorY + 1.15f * s) : (Vector2)_root.position;
                    float shake = T < 1.2f ? Mathf.Sin(t * 60f) * 0.08f : 0f;
                    rootPos = start + new Vector2(shake - 9f * u, 14f * u - 4f * u * u);
                    bodyRot = -540f * u * u; armL = 160f; armR = 160f; legL = 40f; legR = -40f; jaw = 0.4f;
                    break;
                }
                default: rootPos = _root.position; break;
            }
            _root.position = rootPos;
            _root.localRotation = Quaternion.Euler(0f, 0f, bodyRot);
            _armL.localRotation = Quaternion.Euler(0f, 0f, -armL);
            _armR.localRotation = Quaternion.Euler(0f, 0f, armR);
            _legL.localRotation = Quaternion.Euler(0f, 0f, -legL);
            _legR.localRotation = Quaternion.Euler(0f, 0f, legR);
            _head.localRotation = Quaternion.Euler(0f, 0f, head);
            _jaw.localPosition = new Vector3(0f, -0.05f - jaw, 0f);
            _body.localScale = new Vector3(1f + bob * 0.03f, 1f - bob * 0.03f, 1f);
            // Hit flash and growing anger (redder each cycle).
            float flash = Mathf.Clamp01(1f - (Time.time - _hitFlashAt) / 0.35f);
            Color anger = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.5f), _cycle / 2f * 0.6f);
            foreach (var r in _renderers)
            {
                if (r.sprite != _blob) r.color = Color.Lerp(anger, Color.white, flash);
            }
            // The liana he holds is drawn down to his hand; the others hang still.
            for (int i = 0; i < _lianas.Count; i++)
            {
                bool follows = _fightEnabled && i == _liana && (_state == State.Hang || _state == State.Intro || _state == State.Swing);
                float bottom = follows ? _hand.y : _hangY;
                float x = _lianaX[i] + (follows && _state == State.Swing ? _hand.x - _lianaX[i] : 0f);
                bottom -= _lianaGrip;
                float top = _lianaTopY + 6f, len = top - bottom;
                _lianas[i].position = new Vector3(Mathf.Lerp(_lianaX[i], x, 0.5f), bottom + len * 0.5f, 0f);
                var lr = _lianas[i].GetComponent<SpriteRenderer>();
                if (_lianaSprite != null) lr.size = new Vector2(lr.size.x, len / _lianas[i].localScale.y);
                else _lianas[i].localScale = new Vector3(_lianas[i].localScale.x, len / _blob.bounds.size.y, 1f);
            }
        }
    }
}
