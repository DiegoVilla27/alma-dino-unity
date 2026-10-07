using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Heavy basalt boulder pushed by Alma's Roar (IRoarTarget). A roar sends it forward along a short
    // arc (5 units in 0.8 s, 0.6 high), stopping early at walls. If it lands on its linked lava it sinks
    // in and turns into a solid bridge over the lava; otherwise it just rests where it landed and can be
    // roared again. It is solid (Alma can stand on it), deals no damage, and resets when Alma respawns.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class PushableBoulder2D : MonoBehaviour, IRoarTarget
    {
        [Header("Push")]
        [SerializeField] private Vector2 _size = new Vector2(1.8f, 1.8f);
        [SerializeField, Min(0.1f)] private float _pushDistance = 5f;
        [SerializeField, Min(0.1f)] private float _pushTime = 0.8f;
        [SerializeField, Min(0f)] private float _arcHeight = 0.6f;

        [Header("Lava bridge")]
        [SerializeField] private HazardZone2D _linkedLava;
        [SerializeField] private Vector2 _bridgeSize = new Vector2(4.2f, 0.2f);
        [SerializeField, Min(0.05f)] private float _sinkTime = 0.4f;

        [Header("Placeholder label")]
        [SerializeField] private string _label = "Roca movible";
        [SerializeField] private bool _showLabel = true;

        private enum State { Resting, Moving, Sinking, Bridge }

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];
        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private AlmaMotor2D _player;
        private BoxCollider2D _bridge;
        private ParticleSystem _dust;
        private ParticleSystem _splash;
        private State _state = State.Resting;
        private float _stateStartedAt;
        private Vector3 _home;
        private Vector3 _moveFrom;
        private Vector3 _moveTo;
        private Vector3 _sinkTo;

        public bool ResonatesWithRoar => false;
        public bool IsBridge => _state == State.Bridge;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _home = transform.position;
            _player = FindAnyObjectByType<AlmaMotor2D>();
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;

            // Solid lid over the lava, a sibling in world space so it doesn't move with the boulder.
            _bridge = new GameObject("LavaBridge").AddComponent<BoxCollider2D>();
            _bridge.transform.SetParent(transform.parent, false);
            _bridge.size = _bridgeSize;
            _bridge.enabled = false;

            _dust = HazardFx.CreateParticles("PushDust", transform, _renderer.sharedMaterial, HazardFx.Puff(), 16, layer, order + 1);
            var dustMain = _dust.main;
            dustMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            dustMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            dustMain.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            dustMain.startColor = new Color(0.6f, 0.55f, 0.5f, 0.7f);
            var dustShape = _dust.shape;
            dustShape.shapeType = ParticleSystemShapeType.Box;
            dustShape.scale = new Vector3(_size.x, 0.1f, 0f);
            HazardFx.SetSizeOverLifetime(_dust, 0.8f, 1.5f);

            _splash = HazardFx.CreateParticles("LavaSplash", transform, _renderer.sharedMaterial, HazardFx.Puff(), 20, layer, order + 2);
            var splashMain = _splash.main;
            splashMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            splashMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            splashMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            splashMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.45f, 0.1f, 1f), new Color(0.55f, 0.5f, 0.5f, 0.8f));
            splashMain.gravityModifier = 1f;
            var splashShape = _splash.shape;
            splashShape.shapeType = ParticleSystemShapeType.Circle;
            splashShape.radius = _size.x * 0.4f;
            splashShape.arc = 180f;

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
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
            HazardFx.DestroyMaterial(_dust);
            HazardFx.DestroyMaterial(_splash);
            if (_bridge != null) Destroy(_bridge.gameObject);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        // Roar: only while resting; pushed the way Alma faces, up to the first wall.
        public void ReceiveRoar(Vector2 origin, int direction)
        {
            if (_state != State.Resting || direction == 0) return;
            float distance = FreeDistance(direction);
            if (distance < 0.1f) return;
            _moveFrom = transform.position;
            _moveTo = _moveFrom + Vector3.right * (direction * distance);
            _dust.Emit(8);
            Enter(State.Moving);
        }

        // How far it can travel before touching a solid collider ahead (ignores itself, triggers and Alma).
        private float FreeDistance(int direction)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            Vector2 size = _size - new Vector2(0.05f, 0.05f);
            Vector2 start = (Vector2)transform.position + Vector2.up * (_arcHeight * 0.5f);
            int count = Physics2D.BoxCast(start, size, 0f, Vector2.right * direction, filter, _hits, _pushDistance);
            float free = _pushDistance;
            for (int i = 0; i < count; i++)
            {
                Collider2D other = _hits[i].collider;
                if (other == _collider || other == _bridge || _hits[i].distance <= 0f) continue;
                if (LevelPieceUtility.IsAlma(other, out _)) continue;
                free = Mathf.Min(free, _hits[i].distance);
            }
            return free;
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Moving:
                    float t = Mathf.Clamp01(elapsed / _pushTime);
                    float eased = 1f - (1f - t) * (1f - t);
                    Vector3 position = Vector3.Lerp(_moveFrom, _moveTo, eased);
                    position.y += 4f * _arcHeight * t * (1f - t);
                    transform.position = position;
                    if (t >= 1f) Land();
                    break;

                case State.Sinking:
                    float s = Mathf.Clamp01(elapsed / _sinkTime);
                    transform.position = Vector3.Lerp(_moveTo, _sinkTo, s * s);
                    if (s >= 1f) Enter(State.Bridge);
                    break;
            }
        }

        private void Land()
        {
            transform.position = _moveTo;
            if (_linkedLava != null && OverLava(out float lavaTop))
            {
                // Sinks until its top is level with the bridge surface; the bridge lid becomes solid.
                _sinkTo = new Vector3(_moveTo.x, lavaTop + _bridgeSize.y * 0.5f - _size.y * 0.5f, _moveTo.z);
                _bridge.transform.position = new Vector3(_moveTo.x, lavaTop, _moveTo.z);
                _bridge.enabled = true;
                _splash.transform.position = new Vector3(_moveTo.x, lavaTop, _moveTo.z);
                _splash.Emit(16);
                Enter(State.Sinking);
                return;
            }
            _dust.Emit(10);
            Enter(State.Resting);
        }

        private bool OverLava(out float lavaTop)
        {
            Vector3 lavaCenter = _linkedLava.transform.position;
            Vector2 lavaSize = _linkedLava.Size;
            lavaTop = lavaCenter.y + lavaSize.y * 0.5f;
            return Mathf.Abs(transform.position.x - lavaCenter.x) <= lavaSize.x * 0.5f;
        }

        private void ResetToStart()
        {
            transform.position = _home;
            _bridge.enabled = false;
            Enter(State.Resting);
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private void OnDrawGizmosSelected()
        {
            // Reach of one push each way.
            Vector3 home = Application.isPlaying ? _home : transform.position;
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.8f);
            Gizmos.DrawWireCube(home + Vector3.right * _pushDistance, _size);
            Gizmos.DrawWireCube(home + Vector3.left * _pushDistance, _size);
            if (_linkedLava != null)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.9f);
                Gizmos.DrawLine(transform.position, _linkedLava.transform.position);
            }
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
