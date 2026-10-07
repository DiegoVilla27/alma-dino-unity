using System;
using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Level
{
    // One of Alma's four stolen eggs (one per world). It floats over a pulsing aura in its colour; when
    // Alma touches it, it flies into her, bursts into sparkles, the rescue line from GDD 5.2 appears for
    // a few seconds and the rescue is saved (GameProgress.RescueEgg). An egg already rescued in the save
    // doesn't appear. `Rescued` lets other pieces (exit portal, sanctuary) react later.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class RescueEgg2D : MonoBehaviour
    {
        [SerializeField] private string _eggId = "Green";
        [SerializeField] private Color _eggColor = new Color(0.45f, 0.85f, 0.35f, 1f);
        [SerializeField, TextArea] private string _rescueText = "Aún estás tibio...\nMamá llegó a tiempo. Ya estás a salvo.";
        [SerializeField] private Vector2 _size = new Vector2(0.6f, 0.8f);
        [SerializeField] private Vector2 _triggerSize = new Vector2(1.2f, 1.6f);
        [SerializeField, Min(0.05f)] private float _collectTime = 0.5f;
        [SerializeField, Min(0.5f)] private float _textTime = 4.5f;
        [SerializeField] private string _label = "Huevo verde";
        [SerializeField] private bool _showLabel = true;

        private enum State { Waiting, Collecting, Rescued }

        private SpriteRenderer _renderer;
        private BoxCollider2D _trigger;
        private SpriteRenderer _aura;
        private ParticleSystem _sparkle;
        private ParticleSystem _burst;
        private TextMesh _rescueLine;
        private TextMesh _placeholderLabel;
        private AlmaMotor2D _collector;
        private State _state = State.Waiting;
        private float _stateStartedAt;
        private Vector3 _home;

        public string EggId => _eggId;
        public bool IsRescued => _state != State.Waiting;
        public event Action Rescued;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            ApplyLayout();
            _home = transform.position;
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;
            Material material = _renderer.sharedMaterial;

            // Soft aura behind the egg.
            _aura = new GameObject("Aura").AddComponent<SpriteRenderer>();
            _aura.transform.SetParent(transform, false);
            _aura.sprite = HazardFx.Glow();
            _aura.color = new Color(_eggColor.r, _eggColor.g, _eggColor.b, 0.45f);
            _aura.sortingLayerID = layer;
            _aura.sortingOrder = order - 1;

            _sparkle = HazardFx.CreateParticles("Sparkle", transform, material, HazardFx.Puff(), 10, layer, order + 1);
            var sparkleMain = _sparkle.main;
            sparkleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
            sparkleMain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
            sparkleMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            sparkleMain.startColor = new ParticleSystem.MinMaxGradient(_eggColor, Color.white);
            var sparkleShape = _sparkle.shape;
            sparkleShape.shapeType = ParticleSystemShapeType.Circle;
            sparkleShape.radius = 0.5f;
            var sparkleEmission = _sparkle.emission;
            sparkleEmission.enabled = true;
            sparkleEmission.rateOverTime = 5f;

            _burst = HazardFx.CreateParticles("Burst", transform, material, HazardFx.Puff(), 24, layer, order + 3);
            var burstMain = _burst.main;
            burstMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            burstMain.startColor = new ParticleSystem.MinMaxGradient(_eggColor, Color.white);
            var burstShape = _burst.shape;
            burstShape.shapeType = ParticleSystemShapeType.Circle;
            burstShape.radius = 0.2f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.1f);

            _rescueLine = HazardZone2D.CreatePlaceholderLabel(transform, _rescueText, Vector3.zero, _renderer);
            _rescueLine.characterSize = 0.07f;
            _rescueLine.gameObject.SetActive(false);

            if (_showLabel)
                _placeholderLabel = HazardZone2D.CreatePlaceholderLabel(transform, _label,
                    new Vector3(0f, -_size.y * 0.5f - 0.3f, 0f), _renderer);
        }

        private void Start()
        {
            // Already rescued in the save: it isn't here any more.
            if (GameProgress.Instance != null && GameProgress.Instance.IsEggRescued(_eggId))
            {
                Hide();
                Enter(State.Rescued);
            }
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
                trigger.offset = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryRescue(other);

        private void OnTriggerStay2D(Collider2D other) => TryRescue(other);

        private void TryRescue(Collider2D other)
        {
            if (_state != State.Waiting) return;
            if (!LevelPieceUtility.IsAlma(other, out AlmaMotor2D alma) || alma.IsDead) return;
            _collector = alma;
            _trigger.enabled = false;
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
                    // Gentle float; the aura breathes.
                    transform.position = _home + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.1f);
                    _aura.transform.localScale = Vector3.one * (1.6f + 0.15f * Mathf.Sin(Time.time * 3f));
                    break;

                case State.Collecting:
                    // The egg flies into Alma, shrinking.
                    float t = Mathf.Clamp01(elapsed / _collectTime);
                    float eased = t * t;
                    Vector3 target = _collector != null ? _collector.transform.position : _home;
                    transform.position = Vector3.Lerp(_home, target, eased);
                    transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, eased);
                    if (t >= 1f) CompleteRescue(target);
                    break;

                case State.Rescued:
                    if (_rescueLine.gameObject.activeSelf)
                    {
                        // The rescue line rises slowly, then fades out.
                        float shown = elapsed / _textTime;
                        _rescueLine.transform.position = _home + new Vector3(0f, 2.2f + shown * 0.4f, 0f);
                        Color color = Color.white;
                        color.a = 1f - Mathf.Clamp01((shown - 0.8f) / 0.2f);
                        _rescueLine.color = color;
                        if (shown >= 1f) _rescueLine.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        private void CompleteRescue(Vector3 at)
        {
            _burst.transform.position = at;
            _burst.Emit(20);
            if (GameProgress.Instance != null) GameProgress.Instance.RescueEgg(_eggId);
            else Debug.LogWarning($"Egg '{_eggId}' rescued but not saved: no System_GameProgress in the scene.", this);
            Hide();
            _rescueLine.gameObject.SetActive(true);
            Enter(State.Rescued);
            Rescued?.Invoke();
        }

        // Hides the egg itself; the burst and the rescue line keep playing.
        private void Hide()
        {
            transform.position = _home;
            transform.localScale = Vector3.one;
            _renderer.enabled = false;
            _aura.enabled = false;
            _trigger.enabled = false;
            if (_placeholderLabel != null) _placeholderLabel.gameObject.SetActive(false);
            var emission = _sparkle.emission;
            emission.enabled = false;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateStartedAt = Time.time;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() =>
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, -_size.y * 0.5f - 0.5f, 0f), _label);
#endif
    }
}
