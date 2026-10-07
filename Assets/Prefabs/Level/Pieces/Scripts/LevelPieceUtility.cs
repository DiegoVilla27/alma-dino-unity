using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Helpers shared by the level pieces (platforms, mushroom, breakable floor, spore).
    public static class LevelPieceUtility
    {
        // Fits the collider to `size` and the sprite to `size` plus the art margin of its PieceArt2D, if any.
        // A Simple sprite becomes Tiled (art repeats instead of stretching); Tiled and Sliced keep their mode,
        // so single objects (mushroom, spore) can use Sliced without borders to scale as one picture.
        public static void ApplySize(SpriteRenderer renderer, BoxCollider2D collider, Vector2 size)
        {
            if (renderer != null)
            {
                if (renderer.drawMode == SpriteDrawMode.Simple) renderer.drawMode = SpriteDrawMode.Tiled;
                Vector2 margin = renderer.TryGetComponent(out PieceArt2D art) ? art.Margin : Vector2.zero;
                renderer.size = size + 2f * margin;
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
