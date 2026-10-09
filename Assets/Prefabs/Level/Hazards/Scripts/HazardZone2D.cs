using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Static lethal zone shared by every hazard prefab (spikes, briers, mud, lava, death zones...).
    // A trigger kills Alma on contact; nothing else is affected. Size is set with `Size` and the
    // sprite is tiled (not stretched) to it, so art can replace the placeholder without touching
    // gameplay. The lethal area can be inset or fully custom, an optional solid part blocks or
    // supports Alma (spiked pillar), and `IsActive` lets timed traps switch the danger on and off.
    // While there is no final art, a small text label names the hazard in the Game view.
    // In the editor, resizing the sprite with the Rect tool (Tiled/Sliced) updates `Size` too, so
    // the drawn size and the lethal area never drift apart and Play keeps what was drawn.
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class HazardZone2D : MonoBehaviour
    {
        [Header("Area")]
        [SerializeField] private Vector2 _size = new Vector2(3f, 0.5f);
        [SerializeField, Min(0f)] private float _hitboxInset = 0.05f;
        [SerializeField] private bool _customHitbox;
        [SerializeField] private Vector2 _hitboxOffset;
        [SerializeField] private Vector2 _hitboxSize = Vector2.one;

        [Header("Solid part (optional)")]
        [SerializeField] private bool _solid;
        [SerializeField] private Vector2 _solidOffset;
        [SerializeField] private Vector2 _solidSize = Vector2.one;

        [Header("Behaviour")]
        [SerializeField] private bool _active = true;
        [SerializeField] private bool _visibleInGame = true;

        [Header("Placeholder label")]
        [SerializeField] private string _label = "Peligro";
        [SerializeField] private bool _showLabel = true;

        [Header("References (prefab)")]
        [SerializeField] private BoxCollider2D _lethalCollider;
        [SerializeField] private BoxCollider2D _solidCollider;

        private SpriteRenderer _renderer;
        private TextMesh _labelText;

        // Timed traps (geysers, flame jets...) toggle this; the visual stays as is.
        public bool IsActive
        {
            get => _active;
            set
            {
                _active = value;
                if (_lethalCollider != null) _lethalCollider.enabled = value;
            }
        }

        public Vector2 Size => _size;

        // Raised after this zone kills Alma, with her position (liquids splash there).
        public event System.Action<Vector2> Killed;

        // Rising or moving traps resize the zone at runtime; sprite, colliders and label follow.
        public void SetSize(Vector2 size)
        {
            _size = size;
            ApplyLayout();
            if (_labelText != null) _labelText.transform.localPosition = new Vector3(0f, _size.y * 0.5f + 0.3f, 0f);
        }

        public bool SolidEnabled
        {
            get => _solid;
            set
            {
                _solid = value;
                if (_solidCollider != null) _solidCollider.enabled = value;
            }
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;
            ApplyLayout();
            IsActive = _active;
            if (!_visibleInGame) _renderer.enabled = false;
            if (_showLabel && _visibleInGame)
                _labelText = CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this, ApplyLayout);

#if UNITY_EDITOR
        private Vector2? _appliedSize;

        // Edit mode only. Whichever side changed since the last layout wins: a size drawn with the
        // Rect tool on the SpriteRenderer updates `Size`; a new `Size` typed in the Inspector resizes the sprite.
        private void Update()
        {
            if (Application.isPlaying) return;
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer.drawMode == SpriteDrawMode.Simple) return;
            _appliedSize ??= _renderer.size;
            if (_renderer.size != _appliedSize.Value)
            {
                UnityEditor.Undo.RecordObject(this, "Resize Hazard");
                _size = _renderer.size;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
            else if (_size == _appliedSize.Value) return;
            ApplyLayout();
            _appliedSize = _size;
        }
#endif

        // Keeps sprite and collider trigger/solid modes in sync with serialized settings.
        // Does NOT overwrite collider offset or size so designers can customize them freely in the Inspector.
        private void ApplyLayout()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            _renderer.size = _size;

            if (_lethalCollider != null)
            {
                _lethalCollider.isTrigger = true;
            }

            if (_solidCollider != null)
            {
                _solidCollider.isTrigger = false;
                _solidCollider.enabled = _solid;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Ajustar colliders al tamaño del sprite")]
        private void ResetCollidersToSprite()
        {
            if (_lethalCollider != null)
            {
                UnityEditor.Undo.RecordObject(_lethalCollider, "Fit Lethal Collider");
                _lethalCollider.offset = Vector2.zero;
                _lethalCollider.size = new Vector2(Mathf.Max(0.01f, _size.x - 2f * _hitboxInset), Mathf.Max(0.01f, _size.y - 2f * _hitboxInset));
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(_lethalCollider);
            }
            if (_solidCollider != null)
            {
                UnityEditor.Undo.RecordObject(_solidCollider, "Fit Solid Collider");
                _solidCollider.offset = Vector2.zero;
                _solidCollider.size = _size;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(_solidCollider);
            }
        }
#endif

        private void OnTriggerEnter2D(Collider2D other) => TryKill(other);

        private void OnTriggerStay2D(Collider2D other) => TryKill(other);

        private void TryKill(Collider2D other)
        {
            if (!_active) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out AlmaMotor2D alma) || alma.IsDead) return;
            alma.Die();
            Killed?.Invoke(body.position);
        }

        // Small temporary text so each hazard can be told apart before it has art. Shared by all traps.
        public static TextMesh CreatePlaceholderLabel(Transform parent, string text, Vector3 localPosition, SpriteRenderer reference)
        {
            var labelObject = new GameObject("PlaceholderLabel");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            var label = labelObject.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.font = font;
            label.text = text;
            label.fontSize = 48;
            label.characterSize = 0.06f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            var textRenderer = labelObject.GetComponent<MeshRenderer>();
            textRenderer.sharedMaterial = font.material;
            textRenderer.sortingLayerID = reference.sortingLayerID;
            textRenderer.sortingOrder = reference.sortingOrder + 10;
            return label;
        }

        private void OnDrawGizmos()
        {
            // Lethal area in red and solid part in grey, also visible when the sprite is hidden.
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            Vector2 lethalOffset = _customHitbox ? _hitboxOffset : Vector2.zero;
            Vector2 lethalSize = _customHitbox ? _hitboxSize
                : new Vector2(_size.x - 2f * _hitboxInset, _size.y - 2f * _hitboxInset);
            Gizmos.DrawWireCube(lethalOffset, lethalSize);
            if (_solid)
            {
                Gizmos.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
                Gizmos.DrawWireCube(_solidOffset, _solidSize);
            }
#if UNITY_EDITOR
            Gizmos.matrix = Matrix4x4.identity;
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
