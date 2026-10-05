using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Crushing ceiling block: waits at the top until Alma walks into the zone below it, trembles and
    // sheds dust for a brief warning, then falls with strong acceleration until it touches the floor
    // or Alma. Touching Alma kills her. Either way it shatters into rock chunks and a dust cloud right
    // there and disappears, then re-forms at the top. Solid while resting (HazardZone2D solid part).
    [DisallowMultipleComponent, RequireComponent(typeof(HazardZone2D), typeof(SpriteRenderer))]
    public sealed class CrushingCeiling2D : MonoBehaviour
    {
        [Header("Cycle (seconds)")]
        [SerializeField, Min(0f)] private float _warningTime = 0.25f;
        [SerializeField, Min(0f)] private float _hiddenTime = 2.4f;
        [SerializeField, Min(0.05f)] private float _reformTime = 0.3f;

        [Header("Drop")]
        [SerializeField, Min(1f)] private float _dropAcceleration = 50f;
        [SerializeField, Min(0.5f)] private float _maxDropDistance = 20f;
        [SerializeField, Min(0f)] private float _detectMargin = 0.5f;

        [Header("Look")]
        [SerializeField] private Color _dustColor = new Color(0.75f, 0.7f, 0.62f, 0.8f);
        [SerializeField, Min(0f)] private float _shakeAmplitude = 0.1f;
        [SerializeField, Min(0f)] private float _shakeDuration = 0.15f;

        private enum State { Ready, Warning, Drop, Hidden, Reform }

        private readonly RaycastHit2D[] _casts = new RaycastHit2D[8];
        private readonly Collider2D[] _overlaps = new Collider2D[8];
        private float _fallSpeed;
        private float _detectDepth;
        private HazardZone2D _zone;
        private SpriteRenderer _renderer;
        private AlmaMotor2D _player;
        private AlmaCameraFollow _camera;
        private ParticleSystem _warningDust;
        private ParticleSystem _debris;
        private ParticleSystem _impactDust;
        private State _state = State.Ready;
        private float _stateStartedAt;
        private Vector3 _topPosition;
        private Color _baseColor;

        private float HalfHeight => _zone.Size.y * 0.5f;

        private void Awake()
        {
            _zone = GetComponent<HazardZone2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _player = FindAnyObjectByType<AlmaMotor2D>();
            _topPosition = transform.position;
            _baseColor = _renderer.color;
            float width = _zone.Size.x;
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;
            Material material = _renderer.sharedMaterial;

            // Warning: fine dust trickling from the underside.
            _warningDust = HazardFx.CreateParticles("WarningDust", transform, material, HazardFx.Puff(), 20, layer, order + 1);
            _warningDust.transform.localPosition = new Vector3(0f, -HalfHeight, 0f);
            var dustMain = _warningDust.main;
            dustMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            dustMain.startSpeed = 0f;
            dustMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            dustMain.startColor = _dustColor;
            dustMain.gravityModifier = 0.6f;
            var dustShape = _warningDust.shape;
            dustShape.shapeType = ParticleSystemShapeType.Box;
            dustShape.scale = new Vector3(width * 0.9f, 0.05f, 0f);

            // Impact: rock chunks thrown up and out, tinted like the block.
            _debris = HazardFx.CreateParticles("Debris", transform, material, HazardFx.Chunk(), 20, layer, order + 2);
            var debrisMain = _debris.main;
            debrisMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
            debrisMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4.5f);
            debrisMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            debrisMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            debrisMain.startColor = new ParticleSystem.MinMaxGradient(_baseColor, _baseColor * 0.7f);
            debrisMain.gravityModifier = 2f;
            var debrisShape = _debris.shape;
            debrisShape.shapeType = ParticleSystemShapeType.Circle;
            debrisShape.radius = width * 0.35f;
            debrisShape.arc = 180f;
            var spin = _debris.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            // Impact: soft dust cloud rolling out along the floor.
            _impactDust = HazardFx.CreateParticles("ImpactDust", transform, material, HazardFx.Puff(), 16, layer, order + 1);
            var cloudMain = _impactDust.main;
            cloudMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
            cloudMain.startSpeed = 0f;
            cloudMain.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            cloudMain.startColor = _dustColor;
            var cloudShape = _impactDust.shape;
            cloudShape.shapeType = ParticleSystemShapeType.Box;
            cloudShape.scale = new Vector3(width, 0.1f, 0f);
            var cloudVelocity = _impactDust.velocityOverLifetime;
            cloudVelocity.enabled = true;
            cloudVelocity.x = new ParticleSystem.MinMaxCurve(-2f, 2f);
            cloudVelocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
            cloudVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            HazardFx.SetSizeOverLifetime(_impactDust, 0.8f, 1.6f);

            _zone.IsActive = false;
            _zone.SolidEnabled = true;
            Enter(State.Ready);
        }

        private void Start()
        {
            _camera = FindAnyObjectByType<AlmaCameraFollow>();
            _detectDepth = FloorDistance();
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += ResetToStart;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= ResetToStart;
        }

        // Alma reappeared: block back at the top, whole, solid and ready.
        private void ResetToStart()
        {
            SetWarningDust(false);
            transform.position = _topPosition;
            transform.localScale = Vector3.one;
            _renderer.enabled = true;
            _renderer.color = _baseColor;
            _zone.IsActive = false;
            _zone.SolidEnabled = true;
            Enter(State.Ready);
        }

        // Distance from the block's bottom to the first solid collider below (not Alma), so the
        // detection zone stops at the floor and doesn't reach rooms underneath.
        private float FloorDistance()
        {
            Vector2 size = _zone.Size - new Vector2(0.04f, 0.04f);
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.BoxCast(_topPosition, size, 0f, Vector2.down, filter, _casts, _maxDropDistance);
            float nearest = _maxDropDistance;
            for (int i = 0; i < count; i++)
            {
                if (_casts[i].distance <= 0f || _casts[i].collider.transform.IsChildOf(transform)) continue;
                if (IsAlma(_casts[i].collider)) continue;
                nearest = Mathf.Min(nearest, _casts[i].distance);
            }
            return nearest + HalfHeight;
        }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_warningDust);
            HazardFx.DestroyMaterial(_debris);
            HazardFx.DestroyMaterial(_impactDust);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Ready:
                    if (AlmaBelow())
                    {
                        SetWarningDust(true);
                        Enter(State.Warning);
                    }
                    break;

                case State.Warning:
                    // Brief tremble and dust, then it drops even if Alma has moved on.
                    transform.position = _topPosition + new Vector3(Mathf.Sin(Time.time * 55f) * 0.05f, 0f, 0f);
                    if (elapsed >= _warningTime) BeginDrop();
                    break;

                case State.Drop:
                    Fall();
                    break;

                case State.Hidden:
                    if (elapsed >= _hiddenTime) Enter(State.Reform);
                    break;

                case State.Reform:
                    // Grows down from the ceiling while fading in.
                    float r = Mathf.Clamp01(elapsed / _reformTime);
                    _renderer.enabled = true;
                    Color color = _baseColor;
                    color.a *= r;
                    _renderer.color = color;
                    transform.localScale = new Vector3(1f, Mathf.Lerp(0.2f, 1f, r), 1f);
                    if (r >= 1f)
                    {
                        transform.localScale = Vector3.one;
                        _renderer.color = _baseColor;
                        _zone.SolidEnabled = true;
                        Enter(State.Ready);
                    }
                    break;
            }
        }

        // Alma under the block: within its width plus a margin on each side, and between the block
        // and the floor below it.
        private bool AlmaBelow()
        {
            if (_player == null || _player.IsDead) return false;
            Vector2 offset = (Vector2)(_player.transform.position - _topPosition);
            float halfWidth = _zone.Size.x * 0.5f + _detectMargin;
            return Mathf.Abs(offset.x) <= halfWidth && offset.y < 0f && -offset.y <= _detectDepth;
        }

        private void BeginDrop()
        {
            SetWarningDust(false);
            transform.position = _topPosition;
            // Contact is resolved by casts in Fall(), so the trigger stays off.
            _zone.SolidEnabled = false;
            _zone.IsActive = false;
            _fallSpeed = 0f;
            Enter(State.Drop);
        }

        // Moves down this frame's distance, sweeping the block's box so it never passes through.
        // The first solid collider below (floor, platform or Alma) stops it; Alma also dies.
        private void Fall()
        {
            // Slightly smaller than the block so resting contacts (ceiling, walls) don't count.
            Vector2 size = _zone.Size - new Vector2(0.04f, 0.04f);
            Vector2 center = transform.position;
            var filter = new ContactFilter2D { useTriggers = false };

            // Alma already overlapping it (e.g. walked into its side mid-fall).
            int overlaps = Physics2D.OverlapBox(center, size, 0f, filter, _overlaps);
            for (int i = 0; i < overlaps; i++)
                if (TryCrush(_overlaps[i]))
                {
                    Shatter();
                    return;
                }

            _fallSpeed += _dropAcceleration * Time.deltaTime;
            float step = _fallSpeed * Time.deltaTime;
            int count = Physics2D.BoxCast(center, size, 0f, Vector2.down, filter, _casts, step);
            float nearest = float.MaxValue;
            Collider2D hitCollider = null;
            for (int i = 0; i < count; i++)
            {
                if (_casts[i].collider.transform.IsChildOf(transform)) continue;
                // Colliders already overlapping at the start (e.g. the ceiling it hangs from) are not hits.
                if (_casts[i].distance <= 0f && !IsAlma(_casts[i].collider)) continue;
                if (_casts[i].distance < nearest)
                {
                    nearest = _casts[i].distance;
                    hitCollider = _casts[i].collider;
                }
            }

            if (hitCollider != null)
            {
                transform.position = center + Vector2.down * nearest;
                TryCrush(hitCollider);
                Shatter();
                return;
            }

            transform.position = center + Vector2.down * step;
            if (_topPosition.y - transform.position.y >= _maxDropDistance) Shatter();
        }

        private static bool IsAlma(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            return body != null && body.TryGetComponent(out AlmaMotor2D _);
        }

        private static bool TryCrush(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out AlmaMotor2D alma)) return false;
            if (!alma.IsDead) alma.Die();
            return true;
        }

        private void Shatter()
        {
            Vector3 impact = transform.position + Vector3.down * HalfHeight;
            _debris.transform.position = impact;
            _debris.Emit(14);
            _impactDust.transform.position = impact;
            _impactDust.Emit(10);
            if (_camera != null && _player != null
                && Vector2.Distance(_player.transform.position, impact) < 10f)
                _camera.Shake(_shakeAmplitude, _shakeDuration);

            _zone.IsActive = false;
            _renderer.enabled = false;
            transform.position = _topPosition;
            Enter(State.Hidden);
        }

        private void SetWarningDust(bool on)
        {
            var emission = _warningDust.emission;
            emission.enabled = on;
            emission.rateOverTime = 15f;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            var zone = GetComponent<HazardZone2D>();
            if (zone == null) return;
            Vector3 top = Application.isPlaying ? _topPosition : transform.position;
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.9f);
            // Maximum fall; in play it stops at the first solid collider below.
            Gizmos.DrawLine(top, top + Vector3.down * _maxDropDistance);
            // Detection zone below the block.
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
            float width = zone.Size.x + 2f * _detectMargin;
            Gizmos.DrawWireCube(top + Vector3.down * (_maxDropDistance * 0.5f), new Vector3(width, _maxDropDistance, 0f));
        }
    }
}
