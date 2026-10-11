using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Bosses
{
    // Something of the final boss's arena that answers Alma's Roar (the rolling boulder, the stalactite).
    // It only forwards the roar to whoever created it; put it next to a Collider2D (triggers count).
    [DisallowMultipleComponent]
    public sealed class TRexRoarTarget2D : MonoBehaviour, IRoarTarget
    {
        public System.Action<Vector2, int> Roared;

        public bool ResonatesWithRoar => false;

        public void ReceiveRoar(Vector2 origin, int direction) => Roared?.Invoke(origin, direction);
    }
}
