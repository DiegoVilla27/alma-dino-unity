using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Level
{
    // Ability altar: a pedestal with a floating orb in the ability's colour. When Alma touches it, the
    // game pauses and the HUD presents the ability (HudAbilityButtons.Present: the rune flies to the centre, then to its action button); without a HUD the orb flies into Alma and the name floats over the altar. The ability
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

        [Header("Banner (HUD)")]
        [Tooltip("Name shown when the HUD presents the ability, under «¡Habilidad despertada!».")]
        [SerializeField] private string _bannerTitle = "Aleteo Materno";
        [Tooltip("One short line: how to use it (names the touch button).")]
        [SerializeField, TextArea(1, 3)] private string _bannerText = "";

        [Header("Art")]
        [Tooltip("Floating rune shard shown instead of the plain light orb; it levitates and flies to Alma.")]
        [SerializeField] private Sprite _runeSprite;
        [Tooltip("Height of the rune's centre above the pedestal's centre (units).")]
        [SerializeField, Min(0f)] private float _runeLift = 1.2f;
        [Tooltip("Size of the rune relative to its sprite (256 PPU).")]
        [SerializeField, Min(0.1f)] private float _runeScale = 1.35f;

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
        private SpriteRenderer _rune;
        private SpriteRenderer _glow;
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

            _orbHome = new Vector3(0f, _runeSprite != null ? _runeLift : _size.y * 0.5f + 0.7f, 0f);
            _orb = new GameObject("Orb").transform;
            _orb.SetParent(transform, false);
            _orb.localPosition = _orbHome;
            if (_runeSprite != null)
            {
                // Rune shard with a soft halo of its colour behind it.
                float runeHeight = _runeSprite.bounds.size.y * _runeScale;
                _glow = AddGlow(_orb, "Glow", _orbColor * new Color(1f, 1f, 1f, 0.55f), runeHeight * 1.7f, layer, order + 2);
                _rune = new GameObject("Rune").AddComponent<SpriteRenderer>();
                _rune.transform.SetParent(_orb, false);
                _rune.transform.localScale = Vector3.one * _runeScale;
                _rune.sprite = _runeSprite;
                _rune.sortingLayerID = layer;
                _rune.sortingOrder = order + 3;
            }
            else
            {
                AddGlow(_orb, "Glow", _orbColor * new Color(1f, 1f, 1f, 0.6f), 1.1f, layer, order + 2);
                AddGlow(_orb, "Core", Color.Lerp(_orbColor, Color.white, 0.6f), 0.45f, layer, order + 3);
            }

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
            // Keeps the prefab's draw mode (Sliced art scales as one picture) and any PieceArt2D margin.
            LevelPieceUtility.ApplySize(renderer, null, _size);
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
            // With the HUD in the scene the rune breaks free and flies to its button; without it, into Alma.
            if (HudAbilityButtons.Instance != null)
            {
                // The HUD pauses the game to present the ability, then engraves the rune on its button.
                Vector3 from = _orb.position;
                float height = _runeSprite != null ? _runeSprite.bounds.size.y * _runeScale * transform.lossyScale.y : 1f;
                Unlock(from, false);
                HudAbilityButtons.Instance.Present(_ability, from, height, _bannerTitle, _bannerText);
                return;
            }
            Enter(State.Collecting);
        }

        private void Update()
        {
            float elapsed = Time.time - _stateStartedAt;
            switch (_state)
            {
                case State.Waiting:
                    // Float and pulse; a rune also sways gently and its halo breathes.
                    _orb.localPosition = _orbHome + Vector3.up * (Mathf.Sin(Time.time * 2.5f) * 0.12f);
                    if (_rune != null)
                    {
                        _orb.localScale = Vector3.one;
                        _orb.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 1.6f) * 5f);
                        float runeHeight = _runeSprite.bounds.size.y * _runeScale;
                        _glow.transform.localScale = Vector3.one * runeHeight * (1.6f + 0.2f * Mathf.Sin(Time.time * 3f));
                    }
                    else
                    {
                        _orb.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.time * 5f));
                    }
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

        private void Unlock(Vector3 at, bool announce = true)
        {
            _burst.transform.position = at;
            _burst.Emit(18);
            if (GameProgress.Instance != null) GameProgress.Instance.UnlockAbility(_ability);
            else if (_collector != null) _collector.SetUnlocked(_ability, true);
            SetSpent();
            // Without a HUD the title floats over the altar (with one, HudAbilityButtons.Present announces it).
            if (announce) _titleText.gameObject.SetActive(true);
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
