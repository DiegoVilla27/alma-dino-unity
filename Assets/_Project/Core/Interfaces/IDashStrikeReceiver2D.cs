using UnityEngine;

namespace AlmaDino.Core.Interfaces
{
    public interface IDashStrikeReceiver2D
    {
        float BounceVelocity { get; }
        bool TryReceiveAirDash(Vector2 velocity);
    }
}
