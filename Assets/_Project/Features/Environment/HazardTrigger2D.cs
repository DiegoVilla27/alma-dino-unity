using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class HazardTrigger2D : MonoBehaviour, IHazard2D
    {
        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        public void OnHazardTouch()
        {
            Debug.Log($"[HazardTrigger2D] Jugador cayó en peligro: {gameObject.name}");
        }
    }
}
