using UnityEngine;

namespace AlmaDino.Shared.Visuals
{
    /// <summary>Optional artwork slot. Rendering changes never resize the physics transform.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class PrefabSprite2D : MonoBehaviour
    {
        [Tooltip("Optional final artwork. Empty keeps the original prototype artwork.")]
        [SerializeField] private Sprite _sprite;
        [SerializeField, HideInInspector] private SpriteRenderer _renderer;
        [SerializeField, HideInInspector] private Sprite _placeholder;
        [SerializeField, HideInInspector] private Vector2 _size;
        [SerializeField, HideInInspector] private SpriteDrawMode _originalDrawMode;
        [SerializeField, HideInInspector] private bool _initialized;

        public Sprite Artwork { get => _sprite; set { _sprite = value; Apply(); } }

        private void OnEnable() => Apply();
        private void OnValidate() => Apply();

        public void Initialize()
        {
            if (_initialized) return;
            _renderer = GetComponent<SpriteRenderer>();
            _placeholder = _renderer.sprite;
            _originalDrawMode = _renderer.drawMode;
            _size = _originalDrawMode == SpriteDrawMode.Simple && _placeholder != null
                ? (Vector2)_placeholder.bounds.size : _renderer.size;
            _initialized = true;
        }

        private bool _artworkApplied;

        private void Apply()
        {
            Initialize();
            if (_sprite == null && !_artworkApplied) return;
            _artworkApplied = _sprite != null;
            // Unity may compensate the transform scale when switching draw modes.
            Vector3 physicsScale = transform.localScale;
            _renderer.sprite = _sprite != null ? _sprite : _placeholder;
            _renderer.drawMode = _sprite != null ? SpriteDrawMode.Sliced : _originalDrawMode;
            _renderer.size = _size;
            transform.localScale = physicsScale;
        }
    }
}
