using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Area of wind that pushes Alma (AlmaMotor2D.AddWind) and any dynamic physics body inside it in its
    // direction, optionally cancelling part of gravity (updrafts). Alma's Dash and Pisotón ignore it.
    // It never damages, but can push Alma into a hazard. Visual: translucent area with wind streaks
    // flowing in its direction. Strength is an acceleration (units/s²).
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class WindCurrentZone2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(6f, 3f);
        [SerializeField] private Vector2 _direction = Vector2.right;
        [SerializeField, Min(0f)] private float _strength = 22f;
        [SerializeField, Range(0f, 1.5f)] private float _gravityCompensation;
        [SerializeField] private bool _active = true;
        [SerializeField] private Color _streakColor = new Color(0.9f, 0.97f, 1f, 0.55f);
        [SerializeField, Min(0f)] private float _streaksPerUnit = 1.2f;
        [SerializeField] private string _label = "Corriente de viento";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _trigger;
        private ParticleSystem _streaks;

        public Vector2 Acceleration => _direction.sqrMagnitude > 0f ? _direction.normalized * _strength : Vector2.zero;

        public bool IsActive
        {
            get => _active;
            set
            {
                _active = value;
                if (_streaks == null) return;
                var emission = _streaks.emission;
                emission.enabled = value;
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _trigger, _size);
            _trigger.isTrigger = true;

            Vector2 direction = _direction.sqrMagnitude > 0f ? _direction.normalized : Vector2.right;
            float travelSpeed = 3f + _strength * 0.15f;
            _streaks = HazardFx.CreateParticles("WindStreaks", transform, _renderer.sharedMaterial, HazardFx.Streak(),
                Mathf.CeilToInt(_size.x * _size.y * _streaksPerUnit) + 4, _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            var main = _streaks.main;
            float crossTime = Mathf.Max(_size.x, _size.y) / travelSpeed;
            main.startLifetime = new ParticleSystem.MinMaxCurve(crossTime * 0.5f, crossTime * 0.8f);
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.04f, 0.07f);
            main.startSizeZ = 1f;
            // Streaks point along the wind.
            main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.Atan2(direction.y, direction.x));
            main.startColor = _streakColor;
            var shape = _streaks.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_size.x, _size.y, 0f);
            var velocity = _streaks.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(direction.x * travelSpeed);
            velocity.y = new ParticleSystem.MinMaxCurve(direction.y * travelSpeed);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);
            var emission = _streaks.emission;
            emission.rateOverTime = _size.x * _size.y * _streaksPerUnit / main.startLifetime.constantMax;
            emission.enabled = _active;

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_streaks);

        private void OnValidate() => HazardFx.DeferInEditor(this, () =>
        {
            LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size);
            var trigger = GetComponent<BoxCollider2D>();
            if (trigger != null) trigger.isTrigger = true;
        });

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_active) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null) return;
            if (body.TryGetComponent(out AlmaMotor2D alma))
            {
                if (!alma.IsDead) alma.AddWind(Acceleration, _gravityCompensation);
                return;
            }
            // Other dynamic bodies (counterweights...) get a plain force.
            if (body.bodyType != RigidbodyType2D.Dynamic) return;
            body.AddForce(Acceleration * body.mass);
            if (_gravityCompensation > 0f)
                body.AddForce(-Physics2D.gravity * body.gravityScale * _gravityCompensation * body.mass);
        }

        private void OnDrawGizmos()
        {
            // Arrow showing the wind direction.
            Vector3 center = transform.position;
            Vector3 direction = (_direction.sqrMagnitude > 0f ? _direction.normalized : Vector2.right);
            float length = Mathf.Min(_size.x, _size.y) * 0.4f;
            Vector3 tip = center + direction * length;
            Vector3 side = new Vector3(-direction.y, direction.x, 0f) * 0.2f;
            Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.9f);
            Gizmos.DrawLine(center - direction * length, tip);
            Gizmos.DrawLine(tip, tip - direction * 0.35f + side);
            Gizmos.DrawLine(tip, tip - direction * 0.35f - side);
            Gizmos.DrawWireCube(center, _size);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(center + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
