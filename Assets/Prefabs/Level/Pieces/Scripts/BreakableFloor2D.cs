using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Cracked floor that breaks when Alma lands a Pisotón on it (AlmaMotor2D.GroundPoundLanded while
    // she stands on top). It shatters into rock chunks and dust, its collider disappears and Alma keeps
    // falling. It stays broken until Alma respawns, when it is restored.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class BreakableFloor2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(3f, 0.6f);
        [SerializeField] private bool _restoreOnRespawn = true;
        [SerializeField] private Color _dustColor = new Color(0.7f, 0.66f, 0.6f, 0.8f);
        [Tooltip("Colour of the rock pieces it breaks into (the sprite itself is not tinted).")]
        [SerializeField] private Color _debrisColor = new Color(0.4f, 0.36f, 0.38f, 1f);
        [SerializeField, Min(0f)] private float _shakeAmplitude = 0.12f;
        [SerializeField, Min(0f)] private float _shakeDuration = 0.18f;
        [SerializeField] private string _label = "Piso rompible";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private AlmaMotor2D _player;
        private Collider2D _playerCollider;
        private AlmaCameraFollow _camera;
        private ParticleSystem _debris;
        private ParticleSystem _dust;
        private bool _broken;

        public bool IsBroken => _broken;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _player = FindAnyObjectByType<AlmaMotor2D>();
            if (_player != null) _playerCollider = _player.GetComponent<Collider2D>();
            Color color = _debrisColor;
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;

            _debris = HazardFx.CreateParticles("Debris", transform, _renderer.sharedMaterial, HazardFx.Chunk(), 24, layer, order + 1);
            var debrisMain = _debris.main;
            debrisMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
            debrisMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
            debrisMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            debrisMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            debrisMain.startColor = new ParticleSystem.MinMaxGradient(color, color * 0.7f);
            debrisMain.gravityModifier = 2f;
            var debrisShape = _debris.shape;
            debrisShape.shapeType = ParticleSystemShapeType.Box;
            debrisShape.scale = new Vector3(_size.x, _size.y, 0f);
            var spin = _debris.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            _dust = HazardFx.CreateParticles("Dust", transform, _renderer.sharedMaterial, HazardFx.Puff(), 12, layer, order + 1);
            var dustMain = _dust.main;
            dustMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            dustMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            dustMain.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            dustMain.startColor = _dustColor;
            var dustShape = _dust.shape;
            dustShape.shapeType = ParticleSystemShapeType.Box;
            dustShape.scale = new Vector3(_size.x, 0.1f, 0f);
            HazardFx.SetSizeOverLifetime(_dust, 0.8f, 1.5f);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void Start() => _camera = FindAnyObjectByType<AlmaCameraFollow>();

        private void OnEnable()
        {
            if (_player == null) return;
            _player.GroundPoundLanded += OnGroundPound;
            _player.Respawned += OnRespawned;
        }

        private void OnDisable()
        {
            if (_player == null) return;
            _player.GroundPoundLanded -= OnGroundPound;
            _player.Respawned -= OnRespawned;
        }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_debris);
            HazardFx.DestroyMaterial(_dust);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        // Only a Pisotón landing on this floor breaks it.
        private void OnGroundPound()
        {
            if (_broken || _playerCollider == null) return;
            if (!LevelPieceUtility.IsOnTop(_playerCollider, _collider, 0.2f)) return;
            Break();
        }

        public void Break()
        {
            if (_broken) return;
            _broken = true;
            _debris.Emit(16);
            _dust.Emit(10);
            _renderer.enabled = false;
            _collider.enabled = false;
            if (_camera != null) _camera.Shake(_shakeAmplitude, _shakeDuration);
        }

        private void OnRespawned()
        {
            if (!_restoreOnRespawn || !_broken) return;
            _broken = false;
            _renderer.enabled = true;
            _collider.enabled = true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
    }
}
