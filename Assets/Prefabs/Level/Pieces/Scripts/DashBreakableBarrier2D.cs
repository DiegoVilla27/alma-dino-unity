using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Solid barrier that only breaks when Alma hits it while dashing (walking, jumping or landing on it
    // does nothing). On break, its collider turns off at once (the Dash carries on through), it sheds
    // debris and fades out. By default it stays open; optionally it grows back after a delay once Alma
    // is out of its space. It is restored when Alma respawns.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class DashBreakableBarrier2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(0.8f, 3f);
        [SerializeField, Min(0.01f)] private float _fadeTime = 0.2f;
        [SerializeField] private bool _canRespawn;
        [SerializeField, Min(0.1f)] private float _respawnDelay = 5f;
        [SerializeField] private bool _restoreOnRespawn = true;
        [SerializeField] private string _label = "Barrera Dash";
        [SerializeField] private bool _showLabel = true;

        private enum State { Solid, Fading, Open, Reforming }

        private readonly Collider2D[] _overlaps = new Collider2D[4];
        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private AlmaMotor2D _player;
        private ParticleSystem _debris;
        private TextMesh _labelText;
        private State _state = State.Solid;
        private float _stateStartedAt;
        private Color _baseColor;

        public bool IsBroken => _state != State.Solid;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _baseColor = _renderer.color;
            _player = FindAnyObjectByType<AlmaMotor2D>();

            _debris = HazardFx.CreateParticles("Debris", transform, _renderer.sharedMaterial, HazardFx.Chunk(), 24,
                _renderer.sortingLayerID, _renderer.sortingOrder + 1);
            var main = _debris.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(_baseColor, _baseColor * 0.7f);
            main.gravityModifier = 1.8f;
            var shape = _debris.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_size.x, _size.y, 0f);
            var spin = _debris.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            if (_showLabel)
                _labelText = HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnEnable()
        {
            if (_player != null) _player.Respawned += OnAlmaRespawned;
        }

        private void OnDisable()
        {
            if (_player != null) _player.Respawned -= OnAlmaRespawned;
        }

        private void OnDestroy() => HazardFx.DestroyMaterial(_debris);

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        private void OnCollisionEnter2D(Collision2D collision) => TryBreak(collision.collider);

        private void OnCollisionStay2D(Collision2D collision) => TryBreak(collision.collider);

        // Only a dashing Alma breaks it.
        private void TryBreak(Collider2D other)
        {
            if (_state != State.Solid) return;
            if (!LevelPieceUtility.IsAlma(other, out AlmaMotor2D alma) || !alma.IsDashing) return;
            Break(Mathf.Sign(transform.position.x - alma.transform.position.x));
        }

        public void Break(float pushDirection = 1f)
        {
            if (_state != State.Solid) return;
            _collider.enabled = false;
            // Debris flies the way the Dash was going.
            var velocity = _debris.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(2f * pushDirection, 4f * pushDirection);
            velocity.y = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            _debris.Emit(18);
            Enter(State.Fading);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Fading:
                    SetAlpha(1f - Mathf.Clamp01(elapsed / _fadeTime));
                    if (elapsed >= _fadeTime)
                    {
                        _renderer.enabled = false;
                        if (_labelText != null) _labelText.gameObject.SetActive(false);
                        Enter(State.Open);
                    }
                    break;

                case State.Open:
                    if (_canRespawn && elapsed >= _respawnDelay && !AlmaInside()) Enter(State.Reforming);
                    break;

                case State.Reforming:
                    _renderer.enabled = true;
                    SetAlpha(Mathf.Clamp01(elapsed / _fadeTime));
                    if (elapsed >= _fadeTime) Restore();
                    break;
            }
        }

        private void OnAlmaRespawned()
        {
            if (_restoreOnRespawn && _state != State.Solid) Restore();
        }

        private void Restore()
        {
            _renderer.enabled = true;
            SetAlpha(1f);
            _collider.enabled = true;
            if (_labelText != null) _labelText.gameObject.SetActive(true);
            Enter(State.Solid);
        }

        private bool AlmaInside()
        {
            int count = Physics2D.OverlapBox(transform.position, _size, 0f, ContactFilter2D.noFilter, _overlaps);
            for (int i = 0; i < count; i++)
                if (LevelPieceUtility.IsAlma(_overlaps[i], out _)) return true;
            return false;
        }

        private void SetAlpha(float alpha)
        {
            Color color = _baseColor;
            color.a *= alpha;
            _renderer.color = color;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
    }
}
