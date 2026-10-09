using UnityEngine;

namespace AlmaGame.Level
{
    // Plain solid terrain (ground blocks, walls, floating platforms): a collider fitted to `Size` and art
    // that repeats instead of stretching. Resize it with `Size` or the Rect tool (via PieceArt2D).
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class SolidPlatform2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(6f, 3f);

        public Vector2 Size => _size;

        private void Awake() => Apply();

        private void OnValidate() => HazardFx.DeferInEditor(this, Apply);

        private void Apply()
        {
            var collider = GetComponent<BoxCollider2D>();
            collider.isTrigger = false;
            LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), collider, _size);
        }

#if UNITY_EDITOR
        [ContextMenu("Ajustar collider al tamaño del sprite")]
        private void ResetColliderToSize()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                UnityEditor.Undo.RecordObject(col, "Fit Collider To Size");
                col.offset = Vector2.zero;
                col.size = _size;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(col);
            }
        }
#endif
    }
}
