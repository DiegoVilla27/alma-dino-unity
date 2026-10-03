using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Player.Components
{
    public class GroundDetector2D : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundLayer = ~0;
        [SerializeField] private Vector2 _boxSize = new Vector2(0.65f, 0.25f);
        [SerializeField] private Vector2 _boxOffset = new Vector2(0f, -0.55f);

        private readonly Collider2D[] _hitBuffer = new Collider2D[8];
        private ContactFilter2D _contactFilter;
        private bool _isGrounded;
        private bool _collisionGrounded;

        public bool IsGrounded => _isGrounded || _collisionGrounded;

        private void Awake()
        {
            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(_groundLayer);
            _contactFilter.useTriggers = false;
        }

        public void ResetGroundState()
        {
            _isGrounded = false;
            _collisionGrounded = false;
        }

        public void CheckGround(Vector2 playerPosition)
        {
            Vector2 checkCenter = playerPosition + _boxOffset;
            int hitCount = Physics2D.OverlapBox(checkCenter, _boxSize, 0f, _contactFilter, _hitBuffer);

            _isGrounded = false;
            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col != null && !col.isTrigger && col.gameObject != gameObject && !col.transform.IsChildOf(transform))
                {
                    if (IsExcludedSurface(col))
                    {
                        continue;
                    }

                    _isGrounded = true;
                    break;
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject == gameObject || collision.transform.IsChildOf(transform)) return;
            if (IsExcludedSurface(collision.collider)) return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (contact.normal.y > 0.5f)
                {
                    _collisionGrounded = true;
                    return;
                }
            }
        }

        private static bool IsExcludedSurface(Collider2D surface)
        {
            if (surface.GetComponentInParent<IBouncySurface2D>() != null) return true;
            var hazard = surface.GetComponentInParent<IHazard2D>();
            if (hazard == null) return false;
            return !(hazard is IConditionalHazard2D conditional) || conditional.IsDangerous;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _collisionGrounded = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector2 checkCenter = (Vector2)transform.position + _boxOffset;
            Gizmos.DrawWireCube(checkCenter, new Vector3(_boxSize.x, _boxSize.y, 1f));
        }
    }
}
