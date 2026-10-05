using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Crystal beetle: walks between two points, flipping its sprite to face the way it goes.
    // Touching it kills Alma (jumping on its shell too). A Pisotón landing nearby knocks it onto
    // its back with a hop and a half-turn; upside down it is harmless, its walk animation pauses and
    // its belly is a platform. After a while it trembles, rolls back over and resumes patrolling.
    // Only the Pisotón flips it (it listens to AlmaMotor2D.GroundPoundLanded); the Rugido does nothing.
    // While upright, if Alma is within its shooting range it stops, turns to her side and throws a
    // crystal shard in an arc every few seconds (a shard grows on its back first as the warning);
    // when she leaves the range it resumes patrolling.
    [DisallowMultipleComponent, RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class CrystalBeetle2D : MonoBehaviour
    {
        [Header("Patrol (offsets from the start position)")]
        [SerializeField] private float _pointA = -2f;
        [SerializeField] private float _pointB = 2f;
        [SerializeField, Min(0.05f)] private float _walkSpeed = 2f;
        [SerializeField, Min(0f)] private float _turnPause = 0.3f;

        [Header("Danger (local, facing right)")]
        [SerializeField] private Vector2 _bodyOffset = new Vector2(0.05f, -0.2f);
        [SerializeField] private Vector2 _bodySize = new Vector2(2.4f, 2f);

        [Header("Pisotón")]
        [SerializeField, Min(0f)] private float _flipRadius = 2f;
        [SerializeField, Min(0f)] private float _flipMaxHeight = 1.5f;
        [SerializeField, Min(0.5f)] private float _flippedTime = 3.5f;

        [Header("Flip motion")]
        [SerializeField, Min(0.05f)] private float _flipDuration = 0.4f;
        [SerializeField, Min(0f)] private float _flipHopHeight = 0.6f;
        [SerializeField, Min(0f)] private float _flipKnockback = 0.4f;
        [SerializeField] private float _flippedYOffset = -0.1f;
        [SerializeField, Min(0f)] private float _warningTime = 0.8f;
        [SerializeField] private Color _warningTint = new Color(1f, 0.7f, 0.7f, 1f);

        [Header("Shooting")]
        [SerializeField] private bool _canShoot = true;
        [SerializeField, Min(0.5f)] private float _shootRange = 5f;
        [SerializeField] private float _shootMinHeight = -1f;
        [SerializeField, Min(0.2f)] private float _shotInterval = 2.5f;
        [SerializeField, Min(0.05f)] private float _chargeTime = 0.5f;
        [SerializeField] private Vector2 _shardOrigin = new Vector2(0.1f, 1.1f);
        [SerializeField, Min(0.5f)] private float _shardSpeed = 7f;
        [SerializeField, Min(0f)] private float _shardGravity = 15f;
        [SerializeField, Min(0.2f)] private float _shardLifetime = 3f;
        [SerializeField] private Color _shardColor = new Color(0.6f, 0.95f, 1f, 1f);

        [Header("Belly platform while flipped (local, upright)")]
        [SerializeField] private Vector2 _platformOffset = new Vector2(0.03f, -0.36f);
        [SerializeField] private Vector2 _platformSize = new Vector2(2.5f, 1.6f);

        private enum State { Walking, Turning, Attacking, Flipping, Flipped, Recovering }

        private readonly Collider2D[] _hits = new Collider2D[8];
        private Animator _animator;
        private SpriteRenderer _renderer;
        private BoxCollider2D _platform;
        private AlmaMotor2D _player;
        private State _state = State.Walking;
        private float _stateStartedAt;
        private float _startX;
        private float _groundY;
        private int _facing = 1;
        private Vector3 _motionFrom;
        private Vector3 _motionTo;
        private float _spinFrom;
        private float _spinTo;
        private Vector3 _baseScale;
        private float _nextShotAt;
        private CrystalShard2D _chargingShard;
        private ParticleSystem _shardBreak;
        private Material _shardMaterial;

        private float MinX => _startX + Mathf.Min(_pointA, _pointB);
        private float MaxX => _startX + Mathf.Max(_pointA, _pointB);
        private bool IsDangerous => _state == State.Walking || _state == State.Turning || _state == State.Attacking;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
            _startX = transform.position.x;
            _groundY = transform.position.y;
            _baseScale = transform.localScale;
            _player = FindAnyObjectByType<AlmaMotor2D>();

            _platform = gameObject.AddComponent<BoxCollider2D>();
            _platform.offset = _platformOffset;
            _platform.size = _platformSize;
            _platform.enabled = false;

            if (_canShoot) CreateShardBreak();
        }

        private void OnEnable()
        {
            if (_player == null) return;
            _player.GroundPoundLanded += OnGroundPound;
            _player.Respawned += ResetToStart;
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.GroundPoundLanded -= OnGroundPound;
                _player.Respawned -= ResetToStart;
            }
            CancelCharge();
        }

        // Alma reappeared: back to its start position, upright and walking; shards in flight vanish.
        private void ResetToStart()
        {
            CancelCharge();
            CrystalShard2D.DestroyAll();
            transform.SetPositionAndRotation(new Vector3(_startX, _groundY, transform.position.z), Quaternion.identity);
            transform.localScale = _baseScale;
            _platform.enabled = false;
            _renderer.color = Color.white;
            _facing = 1;
            _renderer.flipX = false;
            _animator.speed = 1f;
            Enter(State.Walking);
        }

        private void OnDestroy()
        {
            if (_shardBreak != null) Destroy(_shardBreak.gameObject);
            if (_shardMaterial != null) Destroy(_shardMaterial);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Walking:
                    if (AlmaInShootRange()) BeginAttack();
                    else Walk();
                    break;

                case State.Turning:
                    if (AlmaInShootRange()) BeginAttack();
                    else if (elapsed >= _turnPause)
                    {
                        _facing = -_facing;
                        _renderer.flipX = _facing < 0;
                        _animator.speed = 1f;
                        Enter(State.Walking);
                    }
                    break;

                case State.Attacking:
                    UpdateAttack();
                    break;

                case State.Flipping:
                    if (Tumble(elapsed / _flipDuration)) Enter(State.Flipped);
                    break;

                case State.Flipped:
                    // Tremble and blink red before rolling back over.
                    float left = _flippedTime - elapsed;
                    if (left <= _warningTime)
                    {
                        float shake = Mathf.Sin(Time.time * 60f) * 0.04f;
                        transform.position = _motionTo + new Vector3(shake, 0f, 0f);
                        _renderer.color = Color.Lerp(Color.white, _warningTint, Mathf.PingPong(Time.time * 8f, 1f));
                    }
                    if (left <= 0f) BeginRecover();
                    break;

                case State.Recovering:
                    if (Tumble(elapsed / _flipDuration))
                    {
                        _animator.speed = 1f;
                        Enter(State.Walking);
                    }
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (!IsDangerous || _player == null || _player.IsDead) return;
            // Plunging down in a Pisotón goes through; landing next to it flips it instead.
            if (_player.IsGroundPounding) return;
            Vector2 offset = new Vector2(_bodyOffset.x * _facing, _bodyOffset.y);
            Vector2 center = (Vector2)transform.position + Vector2.Scale(offset, Abs(transform.lossyScale));
            int count = Physics2D.OverlapBox(center, Vector2.Scale(_bodySize, Abs(transform.lossyScale)), 0f,
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

        // Pisotón within _flipRadius of its body edge, at roughly the same height, flips it.
        private void OnGroundPound()
        {
            if (_state == State.Flipping || _player == null) return;
            Vector2 offset = (Vector2)(_player.transform.position - transform.position);
            float halfWidth = _bodySize.x * 0.5f * Mathf.Abs(transform.lossyScale.x);
            if (Mathf.Abs(offset.x) - halfWidth > _flipRadius || Mathf.Abs(offset.y) > _flipMaxHeight) return;

            if (_state == State.Flipped)
            {
                // Already on its back: another Pisotón just restarts the timer.
                _renderer.color = Color.white;
                transform.position = _motionTo;
                Enter(State.Flipped);
                return;
            }

            CancelCharge();
            // Knocked away from Alma, spinning the same way it flies. Also works mid-recovery.
            int away = offset.x > 0f ? -1 : 1;
            Vector3 start = transform.position;
            var end = new Vector3(Mathf.Clamp(start.x + away * _flipKnockback, MinX, MaxX),
                _groundY + _flippedYOffset, start.z);
            BeginTumble(start, end, CurrentSpin(), -away * 180f);
            _animator.speed = 0f;
            _renderer.color = Color.white;
            Enter(State.Flipping);
        }

        // Alma within the shooting circle and not clearly below it (e.g. on a lower floor).
        private bool AlmaInShootRange()
        {
            if (!_canShoot || _player == null || _player.IsDead) return false;
            Vector2 offset = (Vector2)(_player.transform.position - transform.position);
            return offset.sqrMagnitude <= _shootRange * _shootRange && offset.y >= _shootMinHeight;
        }

        private void BeginAttack()
        {
            _animator.speed = 0f;
            _nextShotAt = Time.time + _chargeTime;
            Enter(State.Attacking);
        }

        // Stopped while Alma stays in range: face her side, charge a shard, throw it, repeat.
        private void UpdateAttack()
        {
            if (!AlmaInShootRange())
            {
                CancelCharge();
                _animator.speed = 1f;
                Enter(State.Walking);
                return;
            }

            float dx = _player.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.1f)
            {
                _facing = dx > 0f ? 1 : -1;
                _renderer.flipX = _facing < 0;
            }

            float chargeStart = _nextShotAt - _chargeTime;
            if (Time.time < chargeStart) return;
            if (_chargingShard == null)
                _chargingShard = CrystalShard2D.Create(_shardColor, _renderer.sharedMaterial,
                    _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            _chargingShard.Charge(ShardOrigin(), (Time.time - chargeStart) / _chargeTime);

            if (Time.time >= _nextShotAt)
            {
                Fire();
                _nextShotAt = Time.time + _shotInterval;
            }
        }

        // Ballistic arc that lands where Alma is when the shard leaves; flight time grows with distance.
        private void Fire()
        {
            Vector2 from = ShardOrigin();
            Vector2 delta = (Vector2)_player.transform.position - from;
            float flightTime = Mathf.Clamp(delta.magnitude / _shardSpeed, 0.45f, 1.2f);
            var velocity = new Vector2(delta.x / flightTime,
                (delta.y + 0.5f * _shardGravity * flightTime * flightTime) / flightTime);
            _chargingShard.Launch(velocity, _shardGravity, _shardLifetime, _shardBreak);
            _chargingShard = null;
        }

        private void CancelCharge()
        {
            if (_chargingShard == null) return;
            Destroy(_chargingShard.gameObject);
            _chargingShard = null;
        }

        private Vector3 ShardOrigin()
        {
            Vector2 scale = Abs(transform.lossyScale);
            return transform.position + new Vector3(_shardOrigin.x * _facing * scale.x, _shardOrigin.y * scale.y, 0f);
        }

        // Small burst of crystal bits shared by all this beetle's shards when they shatter.
        private void CreateShardBreak()
        {
            var go = new GameObject("CrystalShardBreak");
            go.transform.SetParent(transform, false);
            _shardBreak = go.AddComponent<ParticleSystem>();
            _shardBreak.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _shardBreak.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = _shardColor;
            main.gravityModifier = 1.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;

            var emission = _shardBreak.emission;
            emission.enabled = false;

            var shape = _shardBreak.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            var color = _shardBreak.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            _shardMaterial = new Material(_renderer.sharedMaterial) { mainTexture = CrystalShard2D.ShardTexture() };
            var particleRenderer = go.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _shardMaterial;
            particleRenderer.sortingLayerID = _renderer.sortingLayerID;
            particleRenderer.sortingOrder = _renderer.sortingOrder + 1;
        }

        private void BeginRecover()
        {
            _renderer.color = Color.white;
            _platform.enabled = false;
            Vector3 start = _motionTo;
            BeginTumble(start, new Vector3(start.x, _groundY, start.z), CurrentSpin(), 0f);
            Enter(State.Recovering);
        }

        private void BeginTumble(Vector3 from, Vector3 to, float spinFrom, float spinTo)
        {
            _motionFrom = from;
            _motionTo = to;
            _spinFrom = spinFrom;
            _spinTo = spinTo;
        }

        // Hop along a parabola while turning half a circle, then a small squash on landing.
        // Returns true when finished.
        private bool Tumble(float t)
        {
            float clamped = Mathf.Clamp01(t);
            float eased = clamped * clamped * (3f - 2f * clamped);
            Vector3 position = Vector3.Lerp(_motionFrom, _motionTo, eased);
            position.y += 4f * _flipHopHeight * clamped * (1f - clamped);
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(_spinFrom, _spinTo, eased));

            const float squashTime = 0.3f;
            float squash = clamped > 1f - squashTime ? (clamped - (1f - squashTime)) / squashTime : 0f;
            float bounce = Mathf.Sin(squash * Mathf.PI) * 0.15f;
            transform.localScale = Vector3.Scale(_baseScale, new Vector3(1f + bounce, 1f - bounce, 1f));

            if (t < 1f) return false;
            transform.localScale = _baseScale;
            if (_state == State.Flipping) _platform.enabled = true;
            return true;
        }

        private float CurrentSpin()
        {
            float z = transform.eulerAngles.z;
            return z > 180f ? z - 360f : z;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private static Vector2 Abs(Vector3 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        private void OnDrawGizmosSelected()
        {
            float startX = Application.isPlaying ? _startX : transform.position.x;
            float y = transform.position.y;
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.9f);
            Vector3 a = new Vector3(startX + _pointA, y, 0f);
            Vector3 b = new Vector3(startX + _pointB, y, 0f);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.15f);
            Gizmos.DrawWireSphere(b, 0.15f);

            Vector2 scale = Abs(transform.lossyScale);
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Vector2 offset = new Vector2(_bodyOffset.x * (_renderer != null && _renderer.flipX ? -1 : 1), _bodyOffset.y);
            Gizmos.DrawWireCube((Vector2)transform.position + Vector2.Scale(offset, scale), Vector2.Scale(_bodySize, scale));

            // Shooting range.
            if (_canShoot)
            {
                Gizmos.color = new Color(0.6f, 0.95f, 1f, 0.6f);
                Gizmos.DrawWireSphere(transform.position, _shootRange);
            }

            // Pisotón landing zone that flips it.
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            float reach = _bodySize.x * 0.5f * scale.x + _flipRadius;
            Gizmos.DrawWireCube(transform.position, new Vector3(reach * 2f, _flipMaxHeight * 2f, 0f));
        }
    }
}
