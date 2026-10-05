using UnityEngine;

namespace AlmaGame.Level
{
    // Bouncy mushroom: when Alma lands on its cap it launches her up (AlmaMotor2D.Bounce). Holding jump
    // gives a bigger bounce; it can also restore her double jump and Dash. Squashes and puffs spores
    // on each bounce. Solid like a platform; the sides don't bounce.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class BouncyMushroom2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(1.6f, 0.8f);
        [SerializeField, Min(1f)] private float _bounceSpeed = 17f;
        [SerializeField, Min(1f)] private float _heldJumpMultiplier = 1.18f;
        [SerializeField] private bool _restoreAirAbilities = true;
        [SerializeField, Min(0f)] private float _cooldown = 0.15f;
        [SerializeField, Min(0.05f)] private float _squashTime = 0.25f;
        [SerializeField] private Color _sporeColor = new Color(1f, 0.85f, 0.95f, 0.85f);
        [SerializeField] private string _label = "Hongo saltarín";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private ParticleSystem _spores;
        private Vector3 _home;
        private float _readyAt;
        private float _squashStartedAt = float.NegativeInfinity;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _home = transform.position;

            _spores = HazardFx.CreateParticles("Spores", transform, _renderer.sharedMaterial, HazardFx.Puff(), 12,
                _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            _spores.transform.localPosition = new Vector3(0f, _size.y * 0.5f, 0f);
            var main = _spores.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            main.startColor = _sporeColor;
            main.gravityModifier = 0.3f;
            var shape = _spores.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = _size.x * 0.4f;
            shape.arc = 180f;

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_spores);

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        private void OnCollisionEnter2D(Collision2D collision) => TryBounce(collision);

        private void OnCollisionStay2D(Collision2D collision) => TryBounce(collision);

        // Only landing on the cap (not touching the sides, not while moving up).
        private void TryBounce(Collision2D collision)
        {
            if (Time.time < _readyAt) return;
            if (!LevelPieceUtility.IsAlma(collision.collider, out var alma) || alma.IsDead) return;
            if (alma.Velocity.y > 0.1f || !LevelPieceUtility.IsOnTop(collision.collider, _collider)) return;
            alma.Bounce(_bounceSpeed, _heldJumpMultiplier, _restoreAirAbilities);
            _readyAt = Time.time + _cooldown;
            _squashStartedAt = Time.time;
            _spores.Emit(8);
        }

        // Squash flat then spring back past normal and settle, keeping the base on the ground.
        private void Update()
        {
            float t = (Time.time - _squashStartedAt) / _squashTime;
            if (t < 0f || t > 1f)
            {
                if (transform.localScale != Vector3.one)
                {
                    transform.localScale = Vector3.one;
                    transform.position = _home;
                }
                return;
            }
            float wobble = Mathf.Sin(t * Mathf.PI * 2.5f) * (1f - t);
            float scaleY = 1f - 0.3f * wobble;
            float scaleX = 1f + 0.2f * wobble;
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
            transform.position = _home + Vector3.down * ((1f - scaleY) * _size.y * 0.5f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
    }
}
