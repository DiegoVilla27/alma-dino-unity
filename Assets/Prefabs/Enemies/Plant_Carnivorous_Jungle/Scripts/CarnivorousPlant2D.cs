using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Fixed plant that bites towards Alma when she enters its detection zone.
    // Cycle: Rest (safe) → Windup (red warning, side locked) → Bite → Return → Rest.
    // When the jaws snap, an impact wave runs along the ground on the bite side: its front is lethal
    // while it travels and is drawn as arcs, so the danger is always visible and can be jumped over.
    // The closed head is lethal too. The Bite sheet faces right; biting left mirrors it with flipX.
    // The script drives the Bite clip frame by frame so each phase matches its duration exactly.
    [DisallowMultipleComponent, RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class CarnivorousPlant2D : MonoBehaviour
    {
        [Header("Detection (relative to the plant's pivot)")]
        [SerializeField, Min(0.1f)] private float _detectRange = 4.4f;
        [SerializeField] private Vector2 _detectHeight = new Vector2(-1.5f, 2.5f);

        [Header("Timing (seconds)")]
        [SerializeField, Min(0.05f)] private float _windupTime = 0.25f;
        [SerializeField, Min(0.05f)] private float _biteTime = 0.7f;
        [SerializeField, Min(0.05f)] private float _returnTime = 0.2f;
        [SerializeField, Min(0f)] private float _restTime = 1f;

        [Header("Warning")]
        [SerializeField] private Color _warningTint = new Color(1f, 0.6f, 0.6f, 1f);

        [Header("Head hitbox (facing right; mirrored when biting left)")]
        [SerializeField] private Vector2 _hitboxOffset = new Vector2(0.8f, 0.4f);
        [SerializeField] private Vector2 _hitboxSize = new Vector2(1.3f, 1f);
        [SerializeField, Min(0f)] private float _lungeTime = 0.04f;

        [Header("Impact wave (facing right; mirrored when biting left)")]
        [SerializeField] private float _waveOriginX = 1.1f;
        [SerializeField, Min(0.1f)] private float _waveDistance = 2.2f;
        [SerializeField, Min(0.05f)] private float _waveTime = 0.2f;
        [SerializeField, Min(0.1f)] private float _waveHeight = 1.2f;
        [SerializeField] private float _groundOffsetY = -1.45f;
        [SerializeField] private Color _waveColor = new Color(1f, 0.5f, 0.3f, 0.9f);

        private const int BiteFrames = 8;
        private const float ShakeFrameTime = 0.08f;
        private const int WaveArcs = 3;
        private const float ArcLag = 0.05f;
        private const float ArcFade = 0.15f;
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int BiteState = Animator.StringToHash("Bite");
        private static Sprite s_arcSprite;

        private enum Phase { Rest, Windup, Bite, Return }

        private readonly Collider2D[] _hits = new Collider2D[8];
        private readonly SpriteRenderer[] _arcs = new SpriteRenderer[WaveArcs];
        private Animator _animator;
        private SpriteRenderer _renderer;
        private AlmaMotor2D _player;
        private Phase _phase = Phase.Rest;
        private float _phaseStartedAt;
        private float _restUntil;
        private int _direction = 1;
        private bool _waveStarted;
        private bool _waveVisible;
        private float _waveStartedAt;
        private float _lastFront;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            for (int i = 0; i < WaveArcs; i++)
            {
                var arc = new GameObject("BiteWaveArc").AddComponent<SpriteRenderer>();
                arc.transform.SetParent(transform, false);
                arc.sprite = ArcSprite();
                arc.sharedMaterial = _renderer.sharedMaterial;
                arc.sortingLayerID = _renderer.sortingLayerID;
                arc.sortingOrder = _renderer.sortingOrder + 1;
                arc.enabled = false;
                _arcs[i] = arc;
            }
        }

        private void OnEnable()
        {
            if (_player == null) _player = FindAnyObjectByType<AlmaMotor2D>();
            if (_player != null) _player.Respawned += ResetToStart;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= ResetToStart;
        }

        // Alma reappeared: back to rest (with its normal rest time), no wave, no warning tint.
        private void ResetToStart()
        {
            _waveStarted = false;
            _waveVisible = false;
            foreach (var arc in _arcs) arc.enabled = false;
            EndCycle();
        }

        private void Update()
        {
            float elapsed = Time.time - _phaseStartedAt;
            switch (_phase)
            {
                case Phase.Rest:
                    if (Time.time >= _restUntil && TryFindTarget(out int side)) BeginWindup(side);
                    break;

                case Phase.Windup:
                    // Frames 1–3: turn towards the side and pull back, tinting red as a warning.
                    ShowFrame(Mathf.Min(2, (int)(elapsed / _windupTime * 3f)));
                    _renderer.color = Color.Lerp(Color.white, _warningTint, Mathf.PingPong(elapsed * 8f, 1f));
                    if (elapsed >= _windupTime)
                    {
                        _renderer.color = Color.white;
                        EnterPhase(Phase.Bite);
                    }
                    break;

                case Phase.Bite:
                    // Frame 4 lunges open, then frames 5–6 alternate as a closed-jaw shake.
                    ShowFrame(elapsed < _lungeTime ? 3 : 4 + (int)((elapsed - _lungeTime) / ShakeFrameTime) % 2);
                    if (!_waveStarted && elapsed >= _lungeTime) StartWave();
                    if (elapsed >= _biteTime) EnterPhase(Phase.Return);
                    break;

                case Phase.Return:
                    // Frames 7–8: pull back and face the camera again.
                    ShowFrame(6 + Mathf.Min(1, (int)(elapsed / _returnTime * 2f)));
                    if (elapsed >= _returnTime) EndCycle();
                    break;
            }
            UpdateWaveArcs();
        }

        private void FixedUpdate()
        {
            // Lethal only once the jaws have snapped shut.
            if (_phase != Phase.Bite || Time.time - _phaseStartedAt < _lungeTime) return;
            KillAlmaIn(HitboxCenter(_direction), _hitboxSize);

            // Sweep the wave front between steps so a fast front can't skip over Alma.
            if (!_waveStarted) return;
            float t = (Time.time - _waveStartedAt) / _waveTime;
            if (t > 1f) return;
            float front = WaveFront(t);
            float from = Mathf.Min(_lastFront, front);
            float to = Mathf.Max(_lastFront, front);
            float ground = transform.position.y + _groundOffsetY * Mathf.Abs(transform.lossyScale.y);
            KillAlmaIn(new Vector2((from + to) * 0.5f, ground + _waveHeight * 0.5f),
                new Vector2(to - from + 0.3f, _waveHeight));
            _lastFront = front;
        }

        private void KillAlmaIn(Vector2 center, Vector2 size)
        {
            int count = Physics2D.OverlapBox(center, size, 0f, ContactFilter2D.noFilter, _hits);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = _hits[i].attachedRigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
            }
        }

        // Side is the sign of Alma's offset; directly above keeps the last side used.
        private bool TryFindTarget(out int side)
        {
            side = _direction;
            if (_player == null || _player.IsDead) return false;
            Vector2 offset = (Vector2)(_player.transform.position - transform.position);
            if (Mathf.Abs(offset.x) > _detectRange || offset.y < _detectHeight.x || offset.y > _detectHeight.y)
                return false;
            if (Mathf.Abs(offset.x) > 0.1f) side = offset.x > 0f ? 1 : -1;
            return true;
        }

        // The side is locked for the whole bite so the warning can be read and dodged.
        private void BeginWindup(int side)
        {
            _direction = side;
            _renderer.flipX = side < 0;
            _animator.speed = 0f;
            _waveStarted = false;
            EnterPhase(Phase.Windup);
            ShowFrame(0);
        }

        private void EndCycle()
        {
            _phase = Phase.Rest;
            _restUntil = Time.time + _restTime;
            _renderer.flipX = false;
            _renderer.color = Color.white;
            _animator.speed = 1f;
            _animator.Play(IdleState, 0, 0f);
        }

        private void EnterPhase(Phase phase)
        {
            _phase = phase;
            _phaseStartedAt = Time.time;
        }

        private void ShowFrame(int frame) => _animator.Play(BiteState, 0, (frame + 0.5f) / BiteFrames);

        private void StartWave()
        {
            _waveStarted = true;
            _waveVisible = true;
            _waveStartedAt = Time.time;
            _lastFront = WaveFront(0f);
        }

        // World X of the wave front: eases out from the mouth to its full distance.
        private float WaveFront(float t)
        {
            float eased = 1f - (1f - Mathf.Clamp01(t)) * (1f - Mathf.Clamp01(t));
            float scaleX = Mathf.Abs(transform.lossyScale.x);
            return transform.position.x + _direction * scaleX * (_waveOriginX + _waveDistance * eased);
        }

        // Lead arc marks the lethal front; two trailing arcs show motion. All fade once it stops.
        private void UpdateWaveArcs()
        {
            if (!_waveVisible) return;
            float elapsed = Time.time - _waveStartedAt;
            if (elapsed > _waveTime + ArcLag * (WaveArcs - 1) + ArcFade)
            {
                _waveVisible = false;
                foreach (var hiddenArc in _arcs) hiddenArc.enabled = false;
                return;
            }
            float ground = transform.position.y + _groundOffsetY * Mathf.Abs(transform.lossyScale.y);
            for (int i = 0; i < WaveArcs; i++)
            {
                float arcTime = elapsed - i * ArcLag;
                SpriteRenderer arc = _arcs[i];
                if (arcTime < 0f)
                {
                    arc.enabled = false;
                    continue;
                }
                float t = arcTime / _waveTime;
                float fade = t <= 1f ? 1f : 1f - (arcTime - _waveTime) / ArcFade;
                Color color = _waveColor;
                color.a *= Mathf.Clamp01(fade) * (1f - 0.3f * i);
                arc.enabled = color.a > 0f;
                arc.color = color;
                arc.flipX = _direction < 0;
                // Place the arc so its apex, not its centre, sits on the lethal front.
                arc.transform.position = new Vector3(WaveFront(t) - _direction * ArcApexOffset * ArcScaleX,
                    ground + _waveHeight * 0.5f, transform.position.z);
                arc.transform.localScale = new Vector3(ArcScaleX, _waveHeight * 1.15f / 2f, 1f);
            }
        }

        private const float ArcScaleX = 0.6f;
        private const float ArcApexOffset = 0.15f;

        private Vector2 HitboxCenter(int side)
        {
            Vector3 scale = transform.lossyScale;
            return (Vector2)transform.position
                + new Vector2(_hitboxOffset.x * side * Mathf.Abs(scale.x), _hitboxOffset.y * Mathf.Abs(scale.y));
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 position = transform.position;
            Vector3 scale = transform.lossyScale;
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(
                position + new Vector3(0f, (_detectHeight.x + _detectHeight.y) * 0.5f, 0f),
                new Vector3(_detectRange * 2f, _detectHeight.y - _detectHeight.x, 0f));
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(HitboxCenter(1), _hitboxSize);
            Gizmos.DrawWireCube(HitboxCenter(-1), _hitboxSize);
            // Area swept by the wave on each side.
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.8f);
            float ground = position.y + _groundOffsetY * Mathf.Abs(scale.y);
            float centerX = (_waveOriginX + _waveDistance * 0.5f) * Mathf.Abs(scale.x);
            var size = new Vector3(_waveDistance * Mathf.Abs(scale.x), _waveHeight, 0f);
            Gizmos.DrawWireCube(new Vector3(position.x + centerX, ground + _waveHeight * 0.5f, 0f), size);
            Gizmos.DrawWireCube(new Vector3(position.x - centerX, ground + _waveHeight * 0.5f, 0f), size);
        }

        // Soft ")" arc opening to +X, 1×2 units; its apex is ArcApexOffset right of the pivot. Generated once.
        private static Sprite ArcSprite()
        {
            if (s_arcSprite != null) return s_arcSprite;
            const int width = 64;
            const int height = 128;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "PlantBiteWaveArc",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f) / width * 2f - 1f;
                    float ny = (y + 0.5f) / height * 2f - 1f;
                    float curve = 0.6f * (1f - ny * ny) - 0.3f;
                    float band = Mathf.Clamp01(1f - Mathf.Abs(nx - curve) / 0.16f);
                    float tips = Mathf.Sqrt(Mathf.Clamp01(1f - ny * ny));
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)(band * band * tips * 255f));
                }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_arcSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
            s_arcSprite.name = "PlantBiteWaveArc";
            return s_arcSprite;
        }
    }
}
