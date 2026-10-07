using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Magma salamander: walks between two points, flipping its sprite to face the way it goes.
    // Touching its body kills Alma. When Alma enters its sight box (either side) it stops, turns to
    // her and spits fireballs at her: Windup (mouth glows with embers, red blink) → Fire → Recover,
    // then waits facing her until the next shot. When she leaves the box it resumes patrolling.
    // The fireball flies straight towards where Alma was when it left (see MagmaFireball2D).
    // The Walk and Attack sheets face right; facing left mirrors them with flipX.
    // The script drives the Attack clip frame by frame so each phase matches its duration exactly.
    [DisallowMultipleComponent, RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class MagmaSalamander2D : MonoBehaviour
    {
        [Header("Patrol (offsets from the start position)")]
        [SerializeField] private float _pointA = -3f;
        [SerializeField] private float _pointB = 3f;
        [SerializeField, Min(0.05f)] private float _walkSpeed = 2.2f;
        [SerializeField, Min(0f)] private float _turnPause = 0.3f;

        [Header("Sight (relative to the pivot, both sides)")]
        [SerializeField, Min(0.5f)] private float _sightRange = 8f;
        [SerializeField] private Vector2 _sightHeight = new Vector2(-2f, 3f);

        [Header("Attack timing (seconds)")]
        [SerializeField, Min(0.05f)] private float _windupTime = 0.65f;
        [SerializeField, Min(0.1f)] private float _fireTime = 0.25f;
        [SerializeField, Min(0.05f)] private float _recoverTime = 0.15f;
        [SerializeField, Min(0.5f)] private float _shotInterval = 2.2f;

        [Header("Warning")]
        [SerializeField] private Color _warningTint = new Color(1f, 0.6f, 0.6f, 1f);

        [Header("Body hitbox (facing right; mirrored when facing left)")]
        [SerializeField] private Vector2 _bodyOffset = new Vector2(0.15f, -0.6f);
        [SerializeField] private Vector2 _bodySize = new Vector2(3.2f, 1.4f);

        [Header("Fireball (facing right; mirrored when facing left)")]
        [SerializeField] private Vector2 _mouthOffset = new Vector2(1.75f, -0.3f);
        [SerializeField, Min(0.5f)] private float _fireballSpeed = 7f;
        [SerializeField, Min(0.2f)] private float _fireballLifetime = 2f;
        [SerializeField, Range(0f, 80f)] private float _maxAimAngle = 40f;

        private const int AttackFrames = 4;
        private const float MouthOpenTime = 0.08f;
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int AttackState = Animator.StringToHash("Attack");

        private enum State { Walking, Turning, Attacking }
        private enum Shot { Waiting, Windup, Fire, Recover }

        private readonly Collider2D[] _hits = new Collider2D[8];
        private Animator _animator;
        private SpriteRenderer _renderer;
        private AlmaMotor2D _player;
        private State _state = State.Walking;
        private float _stateStartedAt;
        private Shot _shot;
        private float _shotStartedAt;
        private float _nextWindupAt;
        private bool _fired;
        private float _nextEmberAt;
        private float _startX;
        private float _startY;
        private int _facing = 1;
        private ParticleSystem _trailFx;
        private ParticleSystem _burstFx;
        private Material _particleMaterial;

        private float MinX => _startX + Mathf.Min(_pointA, _pointB);
        private float MaxX => _startX + Mathf.Max(_pointA, _pointB);

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _startX = transform.position.x;
            _startY = transform.position.y;
            _player = FindAnyObjectByType<AlmaMotor2D>();

            _particleMaterial = new Material(_renderer.sharedMaterial) { mainTexture = MagmaFireball2D.FireTexture() };
            _trailFx = CreateParticles("MagmaFireTrail", new Vector2(0.25f, 0.4f), Vector2.zero,
                new Vector2(0.3f, 0.5f), -0.3f, 360f, 128);
            _burstFx = CreateParticles("MagmaFireBurst", new Vector2(0.3f, 0.6f), new Vector2(2f, 5f),
                new Vector2(0.08f, 0.16f), 1f, 360f, 64);
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += ResetToStart;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= ResetToStart;
        }

        private void OnDestroy()
        {
            if (_trailFx != null) Destroy(_trailFx.gameObject);
            if (_burstFx != null) Destroy(_burstFx.gameObject);
            if (_particleMaterial != null) Destroy(_particleMaterial);
        }

        // Alma reappeared: back to its start position walking right; fireballs in flight vanish.
        private void ResetToStart()
        {
            MagmaFireball2D.DestroyAll();
            transform.position = new Vector3(_startX, _startY, transform.position.z);
            Face(1);
            ResumeWalking();
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Walking:
                    if (AlmaInSight()) BeginAttack();
                    else Walk();
                    break;

                case State.Turning:
                    if (AlmaInSight()) BeginAttack();
                    else if (Time.time - _stateStartedAt >= _turnPause)
                    {
                        Face(-_facing);
                        ResumeWalking();
                    }
                    break;

                case State.Attacking:
                    UpdateAttack();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_player == null || _player.IsDead) return;
            Vector2 scale = Abs(transform.lossyScale);
            Vector2 center = (Vector2)transform.position
                + Vector2.Scale(new Vector2(_bodyOffset.x * _facing, _bodyOffset.y), scale);
            int count = Physics2D.OverlapBox(center, Vector2.Scale(_bodySize, scale), 0f,
                ContactFilter2D.noFilter, _hits);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = _hits[i].attachedRigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
            }
        }

        private void Walk()
        {
            Vector3 position = transform.position;
            position.x += _facing * _walkSpeed * Time.deltaTime;
            bool reachedEnd = _facing > 0 ? position.x >= MaxX : position.x <= MinX;
            if (reachedEnd)
            {
                position.x = Mathf.Clamp(position.x, MinX, MaxX);
                _animator.speed = 0f;
                Enter(State.Turning);
            }
            transform.position = position;
        }

        // Alma inside the sight box on either side, at roughly its height.
        private bool AlmaInSight()
        {
            if (_player == null || _player.IsDead) return false;
            Vector2 offset = (Vector2)(_player.transform.position - transform.position);
            return Mathf.Abs(offset.x) <= _sightRange && offset.y >= _sightHeight.x && offset.y <= _sightHeight.y;
        }

        // Stops and starts the first windup right away.
        private void BeginAttack()
        {
            _animator.speed = 0f;
            FaceAlma();
            _nextWindupAt = Time.time;
            EnterShot(Shot.Waiting);
            ShowFrame(0);
            Enter(State.Attacking);
        }

        private void UpdateAttack()
        {
            float elapsed = Time.time - _shotStartedAt;
            switch (_shot)
            {
                case Shot.Waiting:
                    if (!AlmaInSight())
                    {
                        ResumeWalking();
                        return;
                    }
                    FaceAlma();
                    if (Time.time >= _nextWindupAt) EnterShot(Shot.Windup);
                    break;

                case Shot.Windup:
                    // Frame 1, side locked: blinks red while embers gather at the mouth.
                    if (!AlmaInSight())
                    {
                        ResumeWalking();
                        return;
                    }
                    _renderer.color = Color.Lerp(Color.white, _warningTint, Mathf.PingPong(elapsed * 8f, 1f));
                    EmitChargeEmbers(elapsed / _windupTime);
                    if (elapsed >= _windupTime)
                    {
                        _renderer.color = Color.white;
                        _fired = false;
                        EnterShot(Shot.Fire);
                    }
                    break;

                case Shot.Fire:
                    // Frame 2 opens the mouth, frame 3 (wide open) spits the fireball and holds.
                    ShowFrame(elapsed < MouthOpenTime ? 1 : 2);
                    if (!_fired && elapsed >= MouthOpenTime) Fire();
                    if (elapsed >= _fireTime) EnterShot(Shot.Recover);
                    break;

                case Shot.Recover:
                    // Frame 4: mouth closed. Then it waits (frame 1) until the next windup.
                    ShowFrame(3);
                    if (elapsed >= _recoverTime)
                    {
                        ShowFrame(0);
                        EnterShot(Shot.Waiting);
                    }
                    break;
            }
        }

        // Straight shot at Alma's centre, kept within _maxAimAngle of the facing direction.
        private void Fire()
        {
            _fired = true;
            Vector3 mouth = MouthPosition();
            Vector2 delta = _player != null ? (Vector2)(_player.transform.position - mouth) : Vector2.right * _facing;
            float angle = Mathf.Atan2(delta.y, Mathf.Abs(delta.x)) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, -_maxAimAngle, _maxAimAngle) * Mathf.Deg2Rad;
            var velocity = new Vector2(Mathf.Cos(angle) * _facing, Mathf.Sin(angle)) * _fireballSpeed;
            MagmaFireball2D.Launch(mouth, velocity, _fireballLifetime, _renderer.sharedMaterial,
                _renderer.sortingLayerID, _renderer.sortingOrder + 1, _trailFx, _burstFx);
            _nextWindupAt = Time.time + _shotInterval - _windupTime - MouthOpenTime;

            // Sparks spray out of the mouth with the shot.
            var spark = new ParticleSystem.EmitParams { position = mouth, applyShapeToPosition = false };
            for (int i = 0; i < 6; i++)
            {
                spark.velocity = velocity.normalized * Random.Range(2f, 4f) + Random.insideUnitCircle * 1.5f;
                _burstFx.Emit(spark, 1);
            }
        }

        // Small flames that appear around the mouth and get pulled into it, more as the windup ends.
        private void EmitChargeEmbers(float progress)
        {
            if (Time.time < _nextEmberAt) return;
            _nextEmberAt = Time.time + Mathf.Lerp(0.08f, 0.025f, progress);
            Vector3 mouth = MouthPosition();
            Vector2 from = Random.insideUnitCircle.normalized * Random.Range(0.4f, 0.7f);
            var ember = new ParticleSystem.EmitParams
            {
                position = (Vector2)mouth + from,
                velocity = -from * 2.5f,
                startSize = Mathf.Lerp(0.12f, 0.28f, progress),
                startLifetime = 0.2f,
                applyShapeToPosition = false,
            };
            _trailFx.Emit(ember, 1);
        }

        private void ResumeWalking()
        {
            _renderer.color = Color.white;
            _animator.speed = 1f;
            _animator.Play(WalkState, 0, 0f);
            Enter(State.Walking);
        }

        private void FaceAlma()
        {
            if (_player == null) return;
            float dx = _player.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.1f) Face(dx > 0f ? 1 : -1);
        }

        private void Face(int direction)
        {
            _facing = direction;
            _renderer.flipX = direction < 0;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private void EnterShot(Shot shot)
        {
            _shot = shot;
            _shotStartedAt = Time.time;
        }

        private void ShowFrame(int frame) => _animator.Play(AttackState, 0, (frame + 0.5f) / AttackFrames);

        private Vector3 MouthPosition()
        {
            Vector2 scale = Abs(transform.lossyScale);
            return transform.position + new Vector3(_mouthOffset.x * _facing * scale.x, _mouthOffset.y * scale.y, 0f);
        }

        // World-space burst system shared by this salamander's fireballs: flames and embers.
        private ParticleSystem CreateParticles(string name, Vector2 lifetime, Vector2 speed, Vector2 size,
            float gravity, float arc, int maxParticles)
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
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maxParticles;

            var emission = particles.emission;
            emission.enabled = false;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.arc = arc;

            // Yellow-white → orange → dark red, fading out.
            var fire = new Gradient();
            fire.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.4f),
                    new GradientColorKey(new Color(0.6f, 0.1f, 0.05f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) });
            var color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = fire;

            var shrink = particles.sizeOverLifetime;
            shrink.enabled = true;
            shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

            var particleRenderer = go.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _particleMaterial;
            particleRenderer.sortingLayerID = _renderer.sortingLayerID;
            particleRenderer.sortingOrder = _renderer.sortingOrder + 2;
            return particles;
        }

        private static Vector2 Abs(Vector3 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        private void OnDrawGizmosSelected()
        {
            float startX = Application.isPlaying ? _startX : transform.position.x;
            Vector3 position = transform.position;
            Vector2 scale = Abs(transform.lossyScale);

            // Patrol.
            Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.9f);
            Vector3 a = new Vector3(startX + _pointA, position.y, 0f);
            Vector3 b = new Vector3(startX + _pointB, position.y, 0f);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.15f);
            Gizmos.DrawWireSphere(b, 0.15f);

            // Sight box.
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(position + new Vector3(0f, (_sightHeight.x + _sightHeight.y) * 0.5f, 0f),
                new Vector3(_sightRange * 2f, _sightHeight.y - _sightHeight.x, 0f));

            // Body.
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            int facing = _renderer != null && _renderer.flipX ? -1 : 1;
            Vector2 offset = new Vector2(_bodyOffset.x * facing, _bodyOffset.y);
            Gizmos.DrawWireCube((Vector2)position + Vector2.Scale(offset, scale), Vector2.Scale(_bodySize, scale));

            // Fireball reach and aim limits on each side.
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.7f);
            float reach = _fireballSpeed * _fireballLifetime;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 mouth = position + new Vector3(_mouthOffset.x * side * scale.x, _mouthOffset.y * scale.y, 0f);
                Gizmos.DrawWireSphere(mouth, 0.1f);
                for (int limit = -1; limit <= 1; limit += 2)
                {
                    float angle = _maxAimAngle * limit * Mathf.Deg2Rad;
                    Gizmos.DrawLine(mouth, mouth + new Vector3(Mathf.Cos(angle) * side, Mathf.Sin(angle), 0f) * reach);
                }
            }
        }
    }
}
