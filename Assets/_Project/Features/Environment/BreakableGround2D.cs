using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class BreakableGround2D : MonoBehaviour, IBreakable2D
    {
        [SerializeField] private GameObject _debrisEffect;

        public void Break()
        {
            Debug.Log($"[BreakableGround2D] ¡Bloque {gameObject.name} destruido con Pisotón Sísmico!");
            if (_debrisEffect != null)
            {
                Instantiate(_debrisEffect, transform.position, Quaternion.identity);
            }
            gameObject.SetActive(false);
        }
    }
}
