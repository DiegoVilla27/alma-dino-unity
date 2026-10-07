using UnityEngine;

namespace AlmaGame.Level
{
    // Art frame for a level piece. The sprite can extend past the gameplay box (the piece's `Size`) by
    // `Margin` on each side, so decorative parts (leaves above a log, a lily flower, rock teeth under a
    // ledge) don't change where Alma stands or collides; the art is centred on the box.
    // In the editor it also keeps `Size` in sync when the sprite is resized with the Rect tool, so the
    // drawn size survives entering Play.
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class PieceArt2D : MonoBehaviour
    {
        [Tooltip("Art drawn beyond the gameplay box on each side (units): x = left and right, y = top and bottom.")]
        [SerializeField] private Vector2 _margin;

        public Vector2 Margin => _margin;

#if UNITY_EDITOR
        private SpriteRenderer _renderer;
        private Vector2? _seenSize;

        // Edit mode only: a sprite resized with the Rect tool writes the new box into the piece's `_size`.
        // That runs the piece's OnValidate, which lays out sprite and collider again from it.
        private void Update()
        {
            if (Application.isPlaying) return;
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer.drawMode == SpriteDrawMode.Simple) return;
            Vector2 drawn = _renderer.size;
            if (_seenSize == null || drawn == _seenSize.Value)
            {
                _seenSize = drawn;
                return;
            }
            _seenSize = drawn;
            Vector2 size = Vector2.Max(drawn - 2f * _margin, Vector2.one * 0.05f);
            foreach (MonoBehaviour piece in GetComponents<MonoBehaviour>())
            {
                if (piece == this || piece == null) continue;
                var serialized = new UnityEditor.SerializedObject(piece);
                UnityEditor.SerializedProperty property = serialized.FindProperty("_size");
                if (property == null || property.propertyType != UnityEditor.SerializedPropertyType.Vector2) continue;
                if (property.vector2Value == size) continue;
                property.vector2Value = size;
                serialized.ApplyModifiedProperties();
            }
        }
#endif
    }
}
