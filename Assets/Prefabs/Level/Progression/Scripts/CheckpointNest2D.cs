using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Level
{
    // Abandoned nest with embers (GDD 8.3). When Alma passes over it, the embers light up with a golden
    // flame, it becomes her respawn point and the game autosaves (through GameProgress, if present).
    // Only the latest checkpoint burns; earlier ones go back to embers. No text: feedback is visual.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class CheckpointNest2D : MonoBehaviour, ICheckpoint
    {
        [SerializeField] private Vector2 _size = new Vector2(1.4f, 0.5f);
        [SerializeField] private Vector2 _triggerSize = new Vector2(1.6f, 3f);
        [SerializeField] private Vector2 _respawnOffset = new Vector2(0f, 1.5f);
        [SerializeField] private Color _emberColor = new Color(0.8f, 0.3f, 0.1f, 0.7f);
        [SerializeField] private Color _flameColorA = new Color(1f, 0.85f, 0.3f, 0.9f);
        [SerializeField] private Color _flameColorB = new Color(1f, 0.55f, 0.15f, 0.85f);
        [SerializeField] private string _label = "Nido checkpoint";
        [SerializeField] private bool _showLabel = true;

        private SpriteRenderer _renderer;
        private BoxCollider2D _trigger;
        private ParticleSystem _embers;
        private ParticleSystem _flame;
        private ParticleSystem _burst;
        private bool _lit;

        public Vector2 RespawnPoint => (Vector2)transform.position + _respawnOffset;
        public bool IsLit => _lit;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            ApplyLayout();
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;
            Material material = _renderer.sharedMaterial;
            Vector3 top = new Vector3(0f, _size.y * 0.5f, 0f);

            // Dim embers: a few small sparks rising slowly.
            _embers = HazardFx.CreateParticles("Embers", transform, material, HazardFx.Puff(), 8, layer, order + 1);
            _embers.transform.localPosition = top;
            var embersMain = _embers.main;
            embersMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
            embersMain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            embersMain.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
            embersMain.startColor = _emberColor;
            var embersShape = _embers.shape;
            embersShape.shapeType = ParticleSystemShapeType.Box;
            embersShape.scale = new Vector3(_size.x * 0.7f, 0.05f, 0f);
            embersShape.rotation = new Vector3(-90f, 0f, 0f);

            // Golden flame: bigger puffs rising and shrinking.
            _flame = HazardFx.CreateParticles("Flame", transform, material, HazardFx.Puff(), 24, layer, order + 2);
            _flame.transform.localPosition = top;
            var flameMain = _flame.main;
            flameMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            flameMain.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            flameMain.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            flameMain.startColor = new ParticleSystem.MinMaxGradient(_flameColorA, _flameColorB);
            var flameShape = _flame.shape;
            flameShape.shapeType = ParticleSystemShapeType.Cone;
            flameShape.angle = 10f;
            flameShape.radius = _size.x * 0.25f;
            flameShape.rotation = new Vector3(-90f, 0f, 0f);
            HazardFx.SetSizeOverLifetime(_flame, 1f, 0.2f);

            // Lighting up: a ring of golden sparks.
            _burst = HazardFx.CreateParticles("LightBurst", transform, material, HazardFx.Puff(), 16, layer, order + 3);
            _burst.transform.localPosition = top;
            var burstMain = _burst.main;
            burstMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 3.5f);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            burstMain.startColor = _flameColorA;
            burstMain.gravityModifier = 0.4f;
            var burstShape = _burst.shape;
            burstShape.shapeType = ParticleSystemShapeType.Circle;
            burstShape.radius = 0.2f;
            burstShape.arc = 180f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.1f);

            SetLit(false, false);
            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnEnable() => GameProgress.Register(this);

        private void OnDisable() => GameProgress.Unregister(this);

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_embers);
            HazardFx.DestroyMaterial(_flame);
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
                // Tall trigger standing on the nest, so passing over it (even mid-jump) counts.
                trigger.isTrigger = true;
                trigger.size = _triggerSize;
                trigger.offset = new Vector2(0f, (_triggerSize.y - _size.y) * 0.5f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!LevelPieceUtility.IsAlma(other, out AlmaMotor2D alma) || alma.IsDead) return;
            // Touching any nest makes it the current one, even if it was lit before.
            bool wasLit = _lit;
            alma.SetRespawnPosition(RespawnPoint);
            GameProgress.SetCurrentCheckpoint(this);
            SetLit(true, !wasLit);
            if (GameProgress.Instance != null) GameProgress.Instance.CheckpointReached(RespawnPoint);
        }

        public void SetLit(bool lit, bool celebrate)
        {
            _lit = lit;
            var embers = _embers.emission;
            embers.enabled = true;
            embers.rateOverTime = lit ? 0f : 3f;
            var flame = _flame.emission;
            flame.enabled = lit;
            flame.rateOverTime = 14f;
            if (celebrate) _burst.Emit(14);
        }

        private void OnDrawGizmos()
        {
            // Where Alma reappears.
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(RespawnPoint, 0.25f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
