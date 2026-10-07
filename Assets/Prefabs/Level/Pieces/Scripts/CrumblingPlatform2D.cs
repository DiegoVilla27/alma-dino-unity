using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Temporary platform: when Alma lands on it, it trembles for `Collapse Time`, then crumbles into
    // falling bits and disappears (sprite and collider off). After `Respawn Time` it re-forms (fading
    // in) as long as Alma isn't standing in its space. Restores immediately when Alma respawns.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class CrumblingPlatform2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(2.5f, 0.4f);
        [SerializeField, Min(0.05f)] private float _collapseTime = 1f;
        [SerializeField, Min(0.1f)] private float _respawnTime = 2.5f;
        [SerializeField, Min(0f)] private float _shakeIntensity = 0.05f;
        [SerializeField, Min(0.05f)] private float _reformTime = 0.25f;
        [Tooltip("Colour of the pieces it crumbles into (the sprite itself is not tinted).")]
        [SerializeField] private Color _debrisColor = new Color(0.45f, 0.75f, 0.3f, 1f);
        [SerializeField] private string _label = "Plataforma que se desmorona";
        [SerializeField] private bool _showLabel = true;

        private enum State { Solid, Shaking, Gone, Reforming }

        private readonly Collider2D[] _overlaps = new Collider2D[4];
        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private AlmaMotor2D _player;
        private ParticleSystem _crumbs;
        private State _state = State.Solid;
        private float _stateStartedAt;
        private Vector3 _home;
        private Color _baseColor;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _home = transform.position;
            _baseColor = _renderer.color;
            _player = FindAnyObjectByType<AlmaMotor2D>();

            // Bits that crumble off and fall, tinted like the platform.
            _crumbs = HazardFx.CreateParticles("Crumbs", transform, _renderer.sharedMaterial, HazardFx.Chunk(), 16,
                _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            var main = _crumbs.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(_debrisColor, _debrisColor * 0.75f);
            main.gravityModifier = 1.5f;
            var shape = _crumbs.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_size.x, _size.y, 0f);
            var spin = _crumbs.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-5f, 5f);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += Restore;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= Restore;
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_crumbs);

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        private void OnCollisionEnter2D(Collision2D collision) => CheckLanding(collision);

        private void OnCollisionStay2D(Collision2D collision) => CheckLanding(collision);

        private void CheckLanding(Collision2D collision)
        {
            if (_state != State.Solid) return;
            if (!LevelPieceUtility.IsAlma(collision.collider, out _)) return;
            if (!LevelPieceUtility.IsOnTop(collision.collider, _collider)) return;
            Enter(State.Shaking);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Shaking:
                    // Trembles harder as it gets closer to breaking.
                    float strength = _shakeIntensity * (0.5f + elapsed / _collapseTime);
                    transform.position = _home + new Vector3(Mathf.Sin(Time.time * 70f) * strength, 0f, 0f);
                    if (elapsed >= _collapseTime) Crumble();
                    break;

                case State.Gone:
                    if (elapsed >= _respawnTime && !AlmaInside()) Enter(State.Reforming);
                    break;

                case State.Reforming:
                    float t = Mathf.Clamp01(elapsed / _reformTime);
                    Color color = _baseColor;
                    color.a *= t;
                    _renderer.color = color;
                    if (t >= 1f) Restore();
                    break;
            }
        }

        private void Crumble()
        {
            transform.position = _home;
            _crumbs.Emit(12);
            _renderer.enabled = false;
            _collider.enabled = false;
            Enter(State.Gone);
        }

        private void Restore()
        {
            transform.position = _home;
            _renderer.enabled = true;
            _renderer.color = _baseColor;
            _collider.enabled = true;
            Enter(State.Solid);
        }

        // Re-forming on top of Alma would trap her inside it, so wait until she's out.
        private bool AlmaInside()
        {
            int count = Physics2D.OverlapBox(_home, _size, 0f, ContactFilter2D.noFilter, _overlaps);
            for (int i = 0; i < count; i++)
                if (LevelPieceUtility.IsAlma(_overlaps[i], out _)) return true;
            return false;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
            if (state == State.Reforming) _renderer.enabled = true;
            if (state == State.Reforming) _collider.enabled = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
    }
}
