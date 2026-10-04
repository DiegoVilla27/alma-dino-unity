using UnityEngine;

namespace AlmaGame.Player
{
    // Implemented by anything Alma's roar can push or activate (boulders, bells, switches, enemies).
    public interface IRoarTarget
    {
        // Resonant targets (bells) answer from RoarResonanceRange instead of RoarRange.
        bool ResonatesWithRoar { get; }
        void ReceiveRoar(Vector2 origin, int direction);
    }
}
