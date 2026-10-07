using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Fixed toad that spits a poison glob towards Alma's side when she enters its detection zone.
    // Cycle: Rest (idle) → Windup (throat swells, red warning, side locked) → Spit → Recover → Rest.
    // The glob flies on a fixed lob (see PoisonSpit2D), so it can be jumped over or dodged by
    // leaving the zone. Touching the toad's body kills Alma at any time.
    // The Attack sheet faces right; spitting left mirrors it with flipX.
    // The script drives the Attack clip frame by frame so each phase matches its duration exactly.
    [DisallowMultipleComponent, RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class PoisonToad2D : MonoBehaviour
    {
        [Header("Detection (relative to the toad's pivot)")]
        [SerializeField, Min(0.1f)] private float _detectRange = 8f;
        [SerializeField] private Vector2 _detectHeight = new Vector2(-1.5f, 3f);

        [Header("Timing (seconds)")]
        [SerializeField, Min(0.05f)] private float _windupTime = 0.6f;
        [SerializeField, Min(0.05f)] private float _spitTime = 0.3f;
        [SerializeField, Min(0.05f)] private float _recoverTime = 0.15f;
        [SerializeField, Min(0f)] private float _restTime = 1f;

        [Header("Warning")]
        [SerializeField] private Color _warningTint = new Color(1f, 0.6f, 0.6f, 1f);
        [SerializeField, Min(0f)] private float _windupSwell = 0.08f;

        [Header("Body hitbox (facing right; mirrored when spitting left)")]
        [SerializeField] private Vector2 _bodyOffset = new Vector2(0.05f, -0.15f);
        [SerializeField] private Vector2 _bodySize = new Vector2(2.5f, 1.6f);

        [Header("Spit (facing right; mirrored when spitting left)")]
        [SerializeField] private Vector2 _mouthOffset = new Vector2(1.25f, 0.35f);
        [SerializeField] private Vector2 _spitVelocity = new Vector2(8f, 3f);
        [SerializeField, Min(0f)] private float _spitGravity = 6f;
        [SerializeField, Min(0.2f)] private float _spitLifetime = 3f;
        [SerializeField] private Color _poisonColor = new Color(0.65f, 1f, 0.2f, 1f);

        private const int AttackFrames = 4;
        private const float MouthOpenTime = 0.08f;
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int AttackState = Animator.StringToHash("Attack");

        private enum Phase { Rest, Windup, Spit, Recover }

        private readonly Collider2D[] _hits = new Collider2D[8];
        private Animator _animator;
        private SpriteRenderer _renderer;
        private AlmaMotor2D _player;
        private Phase _phase = Phase.Rest;
        private float _phaseStartedAt;
        private float _restUntil;
        private int _direction = 1;
        private bool _spat;
        private Vector3 _baseScale;
        private ParticleSystem _trailFx;
        private ParticleSystem _splashFx;
        private Material _particleMaterial;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _particleMaterial = new Material(_renderer.sharedMaterial) { mainTexture = PoisonSpit2D.GlobTexture() };
            _trailFx = CreateParticles("PoisonSpitTrail", new Vector2(0.25f, 0.45f), new Vector2(0f, 0f),
                new Vector2(0.06f, 0.12f), 2f, 64);
            _splashFx = CreateParticles("PoisonSpitSplash", new Vector2(0.35f, 0.6f), new Vector2(1.5f, 3.5f),
                new Vector2(0.08f, 0.18f), 1.8f, 48);
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

        private void OnDestroy()
        {
            if (_trailFx != null) Destroy(_trailFx.gameObject);
            if (_splashFx != null) Destroy(_splashFx.gameObject);
            if (_particleMaterial != null) Destroy(_particleMaterial);
        }

        // Alma reappeared: back to rest (with its normal rest time); globs in flight vanish.
        private void ResetToStart()
        {
            PoisonSpit2D.DestroyAll();
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
                    // Frame 1: throat sac swollen; it throbs and blinks red as the warning.
                    float t = elapsed / _windupTime;
                    _renderer.color = Color.Lerp(Color.white, _warningTint, Mathf.PingPong(elapsed * 8f, 1f));
                    float swell = 1f + _windupSwell * t * (0.6f + 0.4f * Mathf.Sin(elapsed * 30f));
                    transform.localScale = Vector3.Scale(_baseScale, new Vector3(swell, swell, 1f));
                    if (elapsed >= _windupTime)
                    {
                        _renderer.color = Color.white;
                        transform.localScale = _baseScale;
                        EnterPhase(Phase.Spit);
                    }
                    break;

                case Phase.Spit:
                    // Frame 2 opens the mouth, frame 3 (wide open) spits and holds.
                    ShowFrame(elapsed < MouthOpenTime ? 1 : 2);
                    if (!_spat && elapsed >= MouthOpenTime) Spit();
                    if (elapsed >= _spitTime) EnterPhase(Phase.Recover);
                    break;

                case Phase.Recover:
                    // Frame 4: mouth closed again, then back to idle.
                    ShowFrame(3);
                    if (elapsed >= _recoverTime) EndCycle();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_player == null || _player.IsDead) return;
            Vector2 scale = Abs(transform.lossyScale);
            Vector2 center = (Vector2)transform.position
                + Vector2.Scale(new Vector2(_bodyOffset.x * _direction, _bodyOffset.y), scale);
            int count = Physics2D.OverlapBox(center, Vector2.Scale(_bodySize, scale), 0f,
                ContactFilter2D.noFilter, _hits);
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

        // The side is locked for the whole attack so the warning can be read and dodged.
        private void BeginWindup(int side)
        {
            _direction = side;
            _renderer.flipX = side < 0;
            _animator.speed = 0f;
            _spat = false;
            EnterPhase(Phase.Windup);
            ShowFrame(0);
        }

        private void Spit()
        {
            _spat = true;
            Vector3 mouth = MouthPosition();
            var velocity = new Vector2(_spitVelocity.x * _direction, _spitVelocity.y);
            PoisonSpit2D.Launch(mouth, velocity, _spitGravity, _spitLifetime, _poisonColor,
                _renderer.sharedMaterial, _renderer.sortingLayerID, _renderer.sortingOrder + 1, _trailFx, _splashFx);
            // A few droplets spray out of the mouth with the glob.
            var spray = new ParticleSystem.EmitParams { position = mouth, applyShapeToPosition = true };
            for (int i = 0; i < 5; i++)
            {
                spray.velocity = new Vector2(_direction * Random.Range(1.5f, 3f), Random.Range(-0.5f, 1.5f));
                _splashFx.Emit(spray, 1);
            }
        }

        private void EndCycle()
        {
            _phase = Phase.Rest;
            _restUntil = Time.time + _restTime;
            _renderer.flipX = false;
            _renderer.color = Color.white;
            transform.localScale = _baseScale;
            _direction = 1;
            _animator.speed = 1f;
            _animator.Play(IdleState, 0, 0f);
        }

        private void EnterPhase(Phase phase)
        {
            _phase = phase;
            _phaseStartedAt = Time.time;
        }

        private void ShowFrame(int frame) => _animator.Play(AttackState, 0, (frame + 0.5f) / AttackFrames);

        private Vector3 MouthPosition()
        {
            Vector2 scale = Abs(transform.lossyScale);
            return transform.position + new Vector3(_mouthOffset.x * _direction * scale.x, _mouthOffset.y * scale.y, 0f);
        }

        // World-space burst system shared by this toad's globs: drips while flying, splash on impact.
        private ParticleSystem CreateParticles(string name, Vector2 lifetime, Vector2 speed, Vector2 size,
            float gravity, int maxParticles)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startColor = new ParticleSystem.MinMaxGradient(_poisonColor, _poisonColor * new Color(0.6f, 0.75f, 0.6f, 1f));
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maxParticles;

            var emission = particles.emission;
            emission.enabled = false;

            // Splash droplets fly mostly upwards in a half-dome; drips use the velocity they are given.
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.arc = 180f;
            shape.rotation = new Vector3(0f, 0f, 0f);

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            var color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            var shrink = particles.sizeOverLifetime;
            shrink.enabled = true;
            shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.4f));

            var particleRenderer = go.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _particleMaterial;
            particleRenderer.sortingLayerID = _renderer.sortingLayerID;
            particleRenderer.sortingOrder = _renderer.sortingOrder + 2;
            return particles;
        }

        private static Vector2 Abs(Vector3 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        private void OnDrawGizmosSelected()
        {
            Vector3 position = transform.position;
            Vector2 scale = Abs(transform.lossyScale);
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(
                position + new Vector3(0f, (_detectHeight.x + _detectHeight.y) * 0.5f, 0f),
                new Vector3(_detectRange * 2f, _detectHeight.y - _detectHeight.x, 0f));

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            int facing = _renderer != null && _renderer.flipX ? -1 : 1;
            Vector2 offset = new Vector2(_bodyOffset.x * facing, _bodyOffset.y);
            Gizmos.DrawWireCube((Vector2)position + Vector2.Scale(offset, scale), Vector2.Scale(_bodySize, scale));

            // Glob path on each side until its lifetime ends (it splashes earlier on any solid).
            Gizmos.color = new Color(0.65f, 1f, 0.2f, 0.8f);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 previous = position + new Vector3(_mouthOffset.x * side * scale.x, _mouthOffset.y * scale.y, 0f);
                Vector3 start = previous;
                const int steps = 30;
                for (int i = 1; i <= steps; i++)
                {
                    float time = _spitLifetime * i / steps;
                    Vector3 point = start + new Vector3(_spitVelocity.x * side * time,
                        _spitVelocity.y * time - 0.5f * _spitGravity * time * time, 0f);
                    Gizmos.DrawLine(previous, point);
                    previous = point;
                }
            }
        }
    }
}
