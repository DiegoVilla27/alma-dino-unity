using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Peñasco pesado de basalto volcánico que puede ser empujado caminando o lanzado
    /// con fuerza mediante el Rugido de Onda Expansiva. Si cae en un foso de lava,
    /// solidifica el magma y se convierte en una plataforma segura.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PushableBoulder2D : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float _boulderMass = 5.0f;
        [Tooltip("Si se convierte en plataforma estática al caer en lava")]
        [SerializeField] private bool _solidifyInLava = true;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _boulderRenderer;
        [SerializeField] private Color _solidifiedColor = new Color(0.2f, 0.2f, 0.22f, 1f);

        private Rigidbody2D _rigidbody;
        private CircleCollider2D _collider;
        private bool _isSolidified;

        public bool IsSolidified => _isSolidified;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
            if (_boulderRenderer == null)
                _boulderRenderer = GetComponentInChildren<SpriteRenderer>();

            _rigidbody.mass = _boulderMass;
            _rigidbody.linearDamping = 0.5f;
            _rigidbody.angularDamping = 1.0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isSolidified || !_solidifyInLava) return;

            // Si entra en contacto con un peligro de lava
            if (other.TryGetComponent<IHazard2D>(out _) || other.gameObject.name.ToLower().Contains("lava"))
            {
                Solidify();
            }
        }

        public void Solidify()
        {
            _isSolidified = true;
            _rigidbody.bodyType = RigidbodyType2D.Static;

            if (_boulderRenderer != null)
            {
                _boulderRenderer.color = _solidifiedColor;
            }

            Debug.Log($"<color=#FFAA00><b>[PushableBoulder2D]</b> Peñasco solidificado como plataforma segura en lava en {transform.position}.</color>");
        }
    }
}
