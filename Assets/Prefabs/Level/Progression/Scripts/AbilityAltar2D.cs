using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Level
{
    // Ability altar: a pedestal with a floating orb in the ability's colour. When Alma touches it, the
    // orb flies into her (0.4 s), bursts into sparks, the ability's name shows briefly and the ability
    // is unlocked (and saved through GameProgress, if present). An altar whose ability is already
    // unlocked starts spent (no orb, dimmed). One prefab per ability, all variants of the same base.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class AbilityAltar2D : MonoBehaviour
    {
        [SerializeField] private AlmaAbility _ability = AlmaAbility.DoubleJump;
        [SerializeField] private string _title = "Doble Salto";
        [SerializeField] private Color _orbColor = new Color(0.55f, 0.9f, 1f, 1f);
        [SerializeField] private Vector2 _size = new Vector2(1.2f, 1f);
        [SerializeField] private Vector2 _triggerSize = new Vector2(1.4f, 2.8f);
        [SerializeField, Min(0.05f)] private float _collectTime = 0.4f;
        [SerializeField, Min(0.2f)] private float _titleTime = 2f;
        [SerializeField] private bool _showLabel = true;

        private enum State { Waiting, Collecting, Spent }

        private SpriteRenderer _renderer;
        private BoxCollider2D _trigger;
        private Transform _orb;
        private ParticleSystem _sparkle;
        private ParticleSystem _burst;
        private TextMesh _titleText;
        private AlmaMotor2D _collector;
        private State _state = State.Waiting;
        private float _stateStartedAt;
        private Vector3 _orbHome;
        private Color _baseColor;

        public AlmaAbility Ability => _ability;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            ApplyLayout();
            _baseColor = _renderer.color;
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;
            Material material = _renderer.sharedMaterial;

            _orbHome = new Vector3(0f, _size.y * 0.5f + 0.7f, 0f);
            _orb = new GameObject("Orb").transform;
            _orb.SetParent(transform, false);
            _orb.localPosition = _orbHome;
            AddGlow(_orb, "Glow", _orbColor * new Color(1f, 1f, 1f, 0.6f), 1.1f, layer, order + 2);
            AddGlow(_orb, "Core", Color.Lerp(_orbColor, Color.white, 0.6f), 0.45f, layer, order + 3);

            _sparkle = HazardFx.CreateParticles("Sparkle", _orb, material, HazardFx.Puff(), 10, layer, order + 1);
            var sparkleMain = _sparkle.main;
            sparkleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1f);
            sparkleMain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
            sparkleMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
            sparkleMain.startColor = _orbColor;
            var sparkleShape = _sparkle.shape;
            sparkleShape.shapeType = ParticleSystemShapeType.Circle;
            sparkleShape.radius = 0.35f;
            var sparkleEmission = _sparkle.emission;
            sparkleEmission.enabled = true;
            sparkleEmission.rateOverTime = 6f;

            _burst = HazardFx.CreateParticles("Burst", transform, material, HazardFx.Puff(), 20, layer, order + 4);
            var burstMain = _burst.main;
            burstMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            burstMain.startColor = new ParticleSystem.MinMaxGradient(_orbColor, Color.white);
            var burstShape = _burst.shape;
            burstShape.shapeType = ParticleSystemShapeType.Circle;
            burstShape.radius = 0.2f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.1f);

            _titleText = HazardZone2D.CreatePlaceholderLabel(transform, _title, Vector3.zero, _renderer);
            _titleText.characterSize = 0.1f;
            _titleText.color = _orbColor;
            _titleText.gameObject.SetActive(false);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, "Altar: " + _title, new Vector3(0f, -_size.y * 0.5f - 0.3f, 0f), _renderer);
        }

        private void Start()
        {
            // Already unlocked (from the save or the level's starting abilities): show it spent.
            bool unlocked = GameProgress.Instance != null && GameProgress.Instance.IsUnlocked(_ability);
            if (unlocked) SetSpent();
        }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_sparkle);
            HazardFx.DestroyMaterial(_burst);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this, ApplyLayout);

        private void ApplyLayout()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var trigger = GetComponent<BoxCollider2D>();
            if (renderer != null)
            {
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = _size;
            }
            if (trigger != null)
            {
                trigger.isTrigger = true;
                trigger.size = _triggerSize;
                trigger.offset = new Vector2(0f, (_triggerSize.y - _size.y) * 0.5f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryCollect(other);

        private void OnTriggerStay2D(Collider2D other) => TryCollect(other);

        private void TryCollect(Collider2D other)
        {
            if (_state != State.Waiting) return;
            if (!LevelPieceUtility.IsAlma(other, out AlmaMotor2D alma) || alma.IsDead) return;
            _collector = alma;
            var emission = _sparkle.emission;
            emission.enabled = false;
            Enter(State.Collecting);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Waiting:
                    // Float and pulse.
                    _orb.localPosition = _orbHome + Vector3.up * (Mathf.Sin(Time.time * 2.5f) * 0.12f);
                    _orb.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.time * 5f));
                    break;

                case State.Collecting:
                    // The orb flies into Alma and shrinks, then bursts.
                    float t = Mathf.Clamp01(elapsed / _collectTime);
                    float eased = t * t;
                    Vector3 target = _collector != null ? _collector.transform.position : transform.position;
                    _orb.position = Vector3.Lerp(transform.TransformPoint(_orbHome), target, eased);
                    _orb.localScale = Vector3.one * Mathf.Lerp(1f, 0.3f, eased);
                    if (t >= 1f) Unlock(target);
                    break;

                case State.Spent:
                    if (_titleText.gameObject.activeSelf)
                    {
                        // Title rises a little and fades out.
                        float shown = elapsed / _titleTime;
                        _titleText.transform.localPosition = new Vector3(0f, _size.y * 0.5f + 1.8f + shown * 0.5f, 0f);
                        Color color = _orbColor;
                        color.a = 1f - Mathf.Clamp01((shown - 0.7f) / 0.3f);
                        _titleText.color = color;
                        if (shown >= 1f) _titleText.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private void Unlock(Vector3 at)
        {
            _burst.transform.position = at;
            _burst.Emit(18);
            if (GameProgress.Instance != null) GameProgress.Instance.UnlockAbility(_ability);
            else if (_collector != null) _collector.SetUnlocked(_ability, true);
            SetSpent();
            _titleText.gameObject.SetActive(true);
            Enter(State.Spent);
        }

        private void SetSpent()
        {
            _orb.gameObject.SetActive(false);
            Color dim = _baseColor * 0.6f;
            dim.a = _baseColor.a;
            _renderer.color = dim;
            _trigger.enabled = false;
            Enter(State.Spent);
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

        private static SpriteRenderer AddGlow(Transform parent, string name, Color color, float size, int layer, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(parent, false);
            renderer.transform.localScale = Vector3.one * size;
            renderer.sprite = HazardFx.Glow();
            renderer.color = color;
            renderer.sortingLayerID = layer;
            renderer.sortingOrder = order;
            return renderer;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, -_size.y * 0.5f - 0.5f, 0f), "Altar: " + _title);
#endif
    }
}
