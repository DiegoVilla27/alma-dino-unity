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
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
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
            ApplyLayout();
            IsActive = _active;
            if (!_visibleInGame) _renderer.enabled = false;
            if (_showLabel && _visibleInGame)
                _labelText = CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnValidate() => ApplyLayout();

        // Keeps sprite, lethal trigger and solid collider in sync with the serialized sizes.
        private void ApplyLayout()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            _renderer.size = _size;

            if (_lethalCollider != null)
            {
                _lethalCollider.isTrigger = true;
                if (_customHitbox)
                {
                    _lethalCollider.offset = _hitboxOffset;
                    _lethalCollider.size = _hitboxSize;
                }
                else
                {
                    _lethalCollider.offset = Vector2.zero;
                    _lethalCollider.size = new Vector2(Mathf.Max(0.01f, _size.x - 2f * _hitboxInset),
                        Mathf.Max(0.01f, _size.y - 2f * _hitboxInset));
                }
            }

            if (_solidCollider != null)
            {
                _solidCollider.isTrigger = false;
                _solidCollider.enabled = _solid;
                _solidCollider.offset = _solidOffset;
                _solidCollider.size = _solidSize;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryKill(other);

        private void OnTriggerStay2D(Collider2D other) => TryKill(other);

        private void TryKill(Collider2D other)
        {
            if (!_active) return;
            Rigidbody2D body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
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
