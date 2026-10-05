using UnityEngine;

namespace AlmaGame.Level
{
    // Platform Alma can jump through from below and stand on from above (no dropping down through it).
    // Uses a PlatformEffector2D in one-way mode, added and configured at runtime.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class OneWayPlatform2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(3f, 0.3f);
        [SerializeField, Range(10f, 180f)] private float _surfaceArc = 160f;
        [SerializeField] private string _label = "Plataforma atravesable";
        [SerializeField] private bool _showLabel = true;

        private void Awake()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(renderer, collider, _size);
            collider.isTrigger = false;
            collider.usedByEffector = true;

            if (!TryGetComponent(out PlatformEffector2D effector)) effector = gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = _surfaceArc;
            effector.useSideFriction = false;
            effector.useSideBounce = false;

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), renderer);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this, () =>
        {
            var collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), collider, _size);
            if (collider != null) collider.usedByEffector = true;
        });

        private void OnDrawGizmos()
        {
            // Arrow pointing up: passable from below.
            Vector3 center = transform.position;
            Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.9f);
            Gizmos.DrawLine(center + Vector3.down * 0.4f, center + Vector3.up * 0.4f);
            Gizmos.DrawLine(center + Vector3.up * 0.4f, center + new Vector3(-0.15f, 0.25f, 0f));
            Gizmos.DrawLine(center + Vector3.up * 0.4f, center + new Vector3(0.15f, 0.25f, 0f));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(center + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
