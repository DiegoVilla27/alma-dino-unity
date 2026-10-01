using System.Collections.Generic;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Zona de corriente de viento direccional (vertical u horizontal) que aplica fuerza
    /// continua a cualquier Rigidbody2D que se encuentre dentro de su volumen.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WindCurrentZone2D : MonoBehaviour
    {
        [Header("Wind Properties")]
        [Tooltip("Dirección del viento normalizada")]
        [SerializeField] private Vector2 _direction = Vector2.up;
        [Tooltip("Fuerza o aceleración de empuje en m/s²")]
        [SerializeField] private float _windStrength = 22.0f;
        [Tooltip("Si anula la gravedad normal mientras se está dentro")]
        [SerializeField] private bool _counteractGravity = true;

        private readonly List<Rigidbody2D> _affectedBodies = new List<Rigidbody2D>();

        public Vector2 Direction => _direction;
        public float WindStrength => _windStrength;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            if (rb != null && !_affectedBodies.Contains(rb))
            {
                _affectedBodies.Add(rb);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                _affectedBodies.Remove(rb);
            }
        }

        private void FixedUpdate()
        {
            for (int i = _affectedBodies.Count - 1; i >= 0; i--)
            {
                var rb = _affectedBodies[i];
                if (rb == null)
                {
                    _affectedBodies.RemoveAt(i);
                    continue;
                }

                if (rb.TryGetComponent<IWindAffected2D>(out var windAffected) && windAffected.IgnoresWind)
                {
                    continue;
                }

                Vector2 acceleration = _direction.normalized * _windStrength;
                if (_counteractGravity && rb.gravityScale > 0f)
                {
                    // Contrarrestar caída para permitir flotar o ascender suavemente
                    acceleration -= Physics2D.gravity * rb.gravityScale * 0.5f;
                }

                rb.AddForce(acceleration * rb.mass, ForceMode2D.Force);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.7f, 0.4f);
            Vector3 center = transform.position;
            Vector3 end = center + (Vector3)(_direction.normalized * 2f);
            Gizmos.DrawLine(center, end);
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}
