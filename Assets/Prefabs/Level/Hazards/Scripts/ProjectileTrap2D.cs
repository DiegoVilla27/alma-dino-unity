using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // How the projectile of a ProjectileTrap2D moves.
    //  Fall: hangs at its spot (a stalactite on the ceiling); when Alma walks under it (or every `Interval`)
    //        it shakes, drops and shatters on the ground, then grows back.
    //  Rise: comes out from below (a bubble from a toxic lake) every `Interval`, floats up `Distance`
    //        wobbling, and pops.
    //  Leap: a fireball. From below: jumps up `Distance` and falls back into its hole. From above
    //        (`From Above`): drops from the ceiling and bursts on the ground.
    public enum ProjectileTrapMotion { Fall, Rise, Leap }

    // Trap that throws a single projectile (stalactite, poison bubble, fireball). The art is the child
    // "Art" sprite (its pivot is the point the trap starts from); everything else is code: warning,
    // flight, lethal hitbox (a capsule around the art) and particles. Only the projectile kills.
    [DisallowMultipleComponent]
    public sealed class ProjectileTrap2D : MonoBehaviour
    {
        [SerializeField] private ProjectileTrapMotion _motion = ProjectileTrapMotion.Fall;
        [Tooltip("Leap only: the fireball comes from the ceiling and falls, instead of jumping up from below.")]
        [SerializeField] private bool _fromAbove;

        [Header("Trigger")]
        [Tooltip("Seconds between shots. Fall: 0 = only when Alma passes under it.")]
        [SerializeField, Min(0f)] private float _interval;
        [SerializeField, Min(0f)] private float _startDelay;
        [Tooltip("Fall: half-width of the column under it where Alma sets it off.")]
        [SerializeField, Min(0f)] private float _detectHalfWidth = 1.2f;
        [Tooltip("Fall: how far below it Alma is detected.")]
        [SerializeField, Min(0f)] private float _detectDepth = 10f;
        [SerializeField, Min(0f)] private float _warningTime = 0.5f;

        [Header("Flight")]
        [Tooltip("Rise and Leap from below: how far it travels. Falling ones (Fall, Leap from above) ignore it: they stop at the ground.")]
        [SerializeField, Min(0.5f)] private float _distance = 4f;
        [Tooltip("Fall and Leap: gravity (units/s²). Rise: speed (units/s).")]
        [SerializeField, Min(0.1f)] private float _gravity = 30f;
        [SerializeField, Min(0.1f)] private float _riseSpeed = 2.2f;
        [Tooltip("Fall: seconds until it grows back on the ceiling.")]
        [SerializeField, Min(0f)] private float _regrowTime = 2.5f;

        [Header("Lethal capsule (world units, relative to the art pivot; turns with the art)")]
        [SerializeField] private Vector2 _hitOffset;
        [SerializeField] private Vector2 _hitSize = new Vector2(0.6f, 0.6f);

        [Header("Particles")]
        [SerializeField] private Color _debrisColor = new Color(0.45f, 0.9f, 1f, 1f);
        [SerializeField] private Color _debrisColor2 = new Color(0.8f, 1f, 1f, 1f);

        [Header("References (prefab)")]
        [SerializeField] private SpriteRenderer _art;

        private enum State { Idle, Warning, Flying, Gone }

        private readonly Collider2D[] _hits = new Collider2D[6];
        private readonly RaycastHit2D[] _rays = new RaycastHit2D[6];
        private AlmaMotor2D _player;
        private ParticleSystem _burst, _hint;
        private State _state;
        private float _stateAt, _nextShot, _velocity;
        private Vector3 _home, _artScale;
        private float _wobbleSeed;

        public ProjectileTrapMotion Motion => _motion;
        // Controlled by someone else (a boss): it never fires on its own, only through Drop().
        public bool Manual { get; set; }
        // Spawned for a single shot: removed after it bursts (and when Alma respawns).
        public bool DestroyAfterBurst { get; set; }
        public bool Ready => _state == State.Idle;

        // Fires now (warning first), if it is waiting.
        public void Drop()
        {
            if (_state != State.Idle) return;
            Enter(State.Warning);
            SetHint(_motion == ProjectileTrapMotion.Fall ? 14f : 10f);
        }

        private Vector2 Down => _motion == ProjectileTrapMotion.Fall || (_motion == ProjectileTrapMotion.Leap && _fromAbove) ? Vector2.down : Vector2.up;

        private void Awake()
        {
            _home = _art.transform.localPosition;
            _artScale = _art.transform.localScale;
            int layer = _art.sortingLayerID, order = _art.sortingOrder;
            Material material = _art.sharedMaterial;
            bool fire = _motion == ProjectileTrapMotion.Leap;
            bool bubble = _motion == ProjectileTrapMotion.Rise;

            // Burst: crystal shards, poison drops or flames.
            _burst = HazardFx.CreateParticles("Burst", transform, material, bubble || fire ? HazardFx.Puff() : HazardFx.Chunk(), 24, layer, order + 1);
            var main = _burst.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(bubble ? 0.12f : 0.1f, bubble ? 0.28f : 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(_debrisColor, _debrisColor2);
            main.gravityModifier = fire ? 0.4f : 1.6f;
            var shape = _burst.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.3f);

            // Warning hint at the spot it comes out of: dust from the ceiling, bubbles or sparks.
            _hint = HazardFx.CreateParticles("Hint", transform, material, HazardFx.Puff(), 16, layer, order - 1);
            _hint.transform.localPosition = _home;
            var hm = _hint.main;
            hm.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            hm.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            hm.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            hm.startColor = _motion == ProjectileTrapMotion.Fall
                ? new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.5f, 0.5f, 0.8f), new Color(0.75f, 0.7f, 0.65f, 0.8f))
                : new ParticleSystem.MinMaxGradient(_debrisColor, _debrisColor2);
            hm.gravityModifier = _motion == ProjectileTrapMotion.Fall ? 1f : 0f;
            var hs = _hint.shape;
            hs.shapeType = ParticleSystemShapeType.Cone;
            hs.angle = 15f;
            hs.radius = 0.3f;
            hs.rotation = new Vector3(Down.y < 0f ? 90f : -90f, 0f, 0f);   // towards where it will fly
            HazardFx.SetSizeOverLifetime(_hint, 1f, 0.4f);

            _wobbleSeed = Random.value * 10f;
            _player = FindAnyObjectByType<AlmaMotor2D>();
            ResetToStart();
        }

        private void OnEnable() { if (_player != null) _player.Respawned += ResetToStart; }
        private void OnDisable() { if (_player != null) _player.Respawned -= ResetToStart; }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_burst);
            HazardFx.DestroyMaterial(_hint);
        }

        // Level start and Alma's respawn: back in place, no projectile flying.
        private void ResetToStart()
        {
            if (DestroyAfterBurst) { Destroy(gameObject); return; }
            PlaceArt(_home, 0f, 1f);
            _art.enabled = _motion == ProjectileTrapMotion.Fall;
            SetHint(0f);
            Enter(State.Idle);
            _nextShot = Time.time + _startDelay + (_motion == ProjectileTrapMotion.Fall ? 0f : _interval);
        }

        private void Update()
        {
            float t = Time.time - _stateAt;
            switch (_state)
            {
                case State.Idle:
                    if (ShouldShoot()) { Enter(State.Warning); SetHint(_motion == ProjectileTrapMotion.Fall ? 14f : 10f); }
                    break;

                case State.Warning:
                    if (_motion == ProjectileTrapMotion.Fall)
                    {
                        // Shakes in its spot.
                        float shake = 0.06f * Mathf.Sin(t * 70f);
                        PlaceArt(_home + new Vector3(shake, 0f, 0f), shake * 40f, 1f);
                    }
                    else if (_motion == ProjectileTrapMotion.Rise)
                    {
                        // The bubble swells at the surface.
                        _art.enabled = true;
                        PlaceArt(_home, 0f, Mathf.Clamp01(t / _warningTime) * 0.6f);
                    }
                    if (t >= _warningTime) Launch();
                    break;

                case State.Flying:
                    Fly(t);
                    break;

                case State.Gone:
                    if (_motion == ProjectileTrapMotion.Fall && t >= _regrowTime)
                    {
                        // Grows back on the ceiling, then waits for Alma again.
                        float g = Mathf.Clamp01((t - _regrowTime) / 0.6f);
                        _art.enabled = true;
                        PlaceArt(_home, 0f, g);
                        if (g >= 1f) { Enter(State.Idle); _nextShot = Time.time + _interval; }
                    }
                    else if (_motion != ProjectileTrapMotion.Fall) Enter(State.Idle);
                    break;
            }
        }

        private bool ShouldShoot()
        {
            if (Manual || Time.time < _nextShot) return false;
            if (_motion != ProjectileTrapMotion.Fall || _interval > 0f) return true;
            if (_player == null || _player.IsDead) return false;
            Vector2 p = _player.transform.position, home = transform.TransformPoint(_home);
            return Mathf.Abs(p.x - home.x) <= _detectHalfWidth && p.y < home.y && home.y - p.y <= _detectDepth;
        }

        private void Launch()
        {
            SetHint(0f);
            _art.enabled = true;
            PlaceArt(_home, 0f, 1f);
            // Leap from below: enough speed to peak at `Distance`.
            _velocity = _motion == ProjectileTrapMotion.Leap && !_fromAbove ? Mathf.Sqrt(2f * _gravity * _distance) : 0f;
            Enter(State.Flying);
        }

        private void Fly(float t)
        {
            Vector3 pos = _art.transform.localPosition;
            float travelled = Vector2.Dot(pos - _home, Down);
            switch (_motion)
            {
                case ProjectileTrapMotion.Rise:
                {
                    float d = _riseSpeed * t;
                    float wobble = 0.18f * Mathf.Sin((t + _wobbleSeed) * 5f);
                    float squash = 1f + 0.06f * Mathf.Sin((t + _wobbleSeed) * 9f);
                    PlaceArt(_home + new Vector3(wobble, d, 0f), 0f, 1f, new Vector2(squash, 2f - squash));
                    if (d >= _distance) Burst(12);
                    return;
                }
                case ProjectileTrapMotion.Leap when !_fromAbove:
                {
                    // Up and back down; the flame tongues trail behind it.
                    float v = _velocity - _gravity * t;
                    float y = _velocity * t - 0.5f * _gravity * t * t;
                    PlaceArt(_home + new Vector3(0f, Mathf.Max(0f, y), 0f), v >= 0f ? 90f : -90f, 1f);
                    if (y <= 0f && v < 0f) Burst(10);
                    return;
                }
                default:
                {
                    // Falls (stalactite, fireball from above) until it hits the ground.
                    float step = (_velocity + 0.5f * _gravity * Time.deltaTime) * Time.deltaTime;
                    _velocity += _gravity * Time.deltaTime;
                    Vector3 next = pos + (Vector3)(Down * step);
                    float angle = _motion == ProjectileTrapMotion.Leap ? -90f : 0f;
                    if (HitsGround(step) || travelled + step >= MaxFall)
                    {
                        PlaceArt(next, angle, 1f);
                        Burst(16);
                        return;
                    }
                    PlaceArt(next, angle, 1f);
                    return;
                }
            }
        }

        // Safety stop for something falling with no ground under it.
        private const float MaxFall = 40f;

        private float Grow => _art.transform.localScale.y / _artScale.y;
        private Vector2 HitCenter => _art.transform.position + _art.transform.rotation * (Vector3)(_hitOffset * Grow);

        // Solid ground right under the tip (Alma and triggers do not count).
        private bool HitsGround(float step)
        {
            Vector2 tip = HitCenter + Vector2.down * Mathf.Max(_hitSize.x, _hitSize.y) * 0.5f;
            var filter = new ContactFilter2D { useTriggers = false };
            int n = Physics2D.Raycast(tip, Vector2.down, filter, _rays, step + 0.02f);
            for (int i = 0; i < n; i++)
            {
                Rigidbody2D body = _rays[i].collider.attachedRigidbody;
                if (body == null || !body.TryGetComponent(out AlmaMotor2D _)) return true;
            }
            return false;
        }

        private void FixedUpdate()
        {
            if (_state != State.Flying && !(_state == State.Warning && _motion == ProjectileTrapMotion.Fall)) return;
            Vector2 size = _hitSize * Grow;
            var dir = size.x > size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            int count = Physics2D.OverlapCapsule(HitCenter, size, dir, _art.transform.eulerAngles.z, ContactFilter2D.noFilter, _hits);
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = _hits[i].attachedRigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead)
                {
                    alma.Die();
                    if (_state == State.Flying) Burst(16);
                }
            }
        }

        private void Burst(int count)
        {
            _burst.transform.position = _art.transform.position;
            _burst.Emit(count);
            _art.enabled = false;
            Enter(State.Gone);
            if (_motion != ProjectileTrapMotion.Fall) _nextShot = Time.time + _interval;
            if (DestroyAfterBurst) Destroy(gameObject, 1f);   // lets the burst particles play
        }

        // Art placement. The art is drawn facing right for a fireball (rotated to its flight) and
        // upright for the rest. `grow` scales it from nothing (regrowing, swelling).
        private void PlaceArt(Vector3 local, float angle, float grow, Vector2? squash = null)
        {
            Transform art = _art.transform;
            art.localPosition = local;
            art.localRotation = Quaternion.Euler(0f, 0f, angle);
            Vector2 s = squash ?? Vector2.one;
            art.localScale = new Vector3(_artScale.x * grow * s.x, _artScale.y * grow * s.y, 1f);
        }

        private void SetHint(float rate)
        {
            var emission = _hint.emission;
            emission.enabled = rate > 0f;
            emission.rateOverTime = rate;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateAt = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            if (_art == null) return;
            Vector3 home = _art.transform.position;
            Vector3 down = Down;
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            Gizmos.DrawLine(home, home + down * _distance);
            Gizmos.matrix = Matrix4x4.TRS(home + _art.transform.rotation * (Vector3)_hitOffset, _art.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, _hitSize);
            Gizmos.matrix = Matrix4x4.identity;
            if (_motion == ProjectileTrapMotion.Fall && _interval <= 0f)
            {
                Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.8f);
                Gizmos.DrawWireCube(home + Vector3.down * _detectDepth * 0.5f, new Vector3(_detectHalfWidth * 2f, _detectDepth, 0f));
            }
        }
    }
}
