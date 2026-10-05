using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Helpers shared by the level pieces (platforms, mushroom, breakable floor, spore).
    public static class LevelPieceUtility
    {
        // Tiles the sprite to `size` (art repeats instead of stretching) and fits the collider to it.
        public static void ApplySize(SpriteRenderer renderer, BoxCollider2D collider, Vector2 size)
        {
            if (renderer != null)
            {
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = size;
            }
            if (collider != null)
            {
                collider.offset = Vector2.zero;
                collider.size = size;
            }
        }

        public static bool IsAlma(Collider2D other, out AlmaMotor2D alma)
        {
            alma = null;
            Rigidbody2D body = other != null ? other.attachedRigidbody : null;
            return body != null && body.TryGetComponent(out alma);
        }

        // Alma standing (or landing) on top of `surface`: her feet at its top edge and overlapping it in X.
        public static bool IsOnTop(Collider2D alma, Collider2D surface, float tolerance = 0.12f)
        {
            Bounds a = alma.bounds;
            Bounds s = surface.bounds;
            return a.min.y >= s.max.y - tolerance && a.min.y <= s.max.y + tolerance
                && a.max.x > s.min.x && a.min.x < s.max.x;
        }
    }
}
