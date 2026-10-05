using UnityEngine;

namespace AlmaGame.Player
{
    // Groups Alma's world-space effects (rings, afterimages, arcs, bursts) under one scene object,
    // "<Alma> FX", instead of loose root objects. They can't be Alma's children: they must stay
    // where the effect happened while she moves, flips and scales. Created on first use.
    [DisallowMultipleComponent]
    public sealed class AlmaFxRoot : MonoBehaviour
    {
        private Transform _root;

        public static Transform For(GameObject owner)
        {
            if (!owner.TryGetComponent(out AlmaFxRoot fx)) fx = owner.AddComponent<AlmaFxRoot>();
            if (fx._root == null) fx._root = new GameObject(owner.name + " FX").transform;
            return fx._root;
        }

        private void OnDestroy()
        {
            if (_root != null) Destroy(_root.gameObject);
        }
    }
}
