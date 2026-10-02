using UnityEngine;
namespace AlmaDino.Features.Player.Services
{
    public static class RoarTargeting
    {
        public static bool Contains(Vector2 origin, Vector2 target, Vector2 direction, float radius, float halfAngle)
        {
            Vector2 delta = target - origin;
            return delta.sqrMagnitude <= radius * radius && delta.sqrMagnitude > .0001f
                && Vector2.Dot(delta.normalized, direction.normalized) >= Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        }
    }
}
