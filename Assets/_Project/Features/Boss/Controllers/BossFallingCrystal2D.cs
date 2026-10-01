using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class BossFallingCrystal2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private GameObject _warningMarker;
        private float _warning;
        private float _speed;
        private Rigidbody2D _body;
        public bool IsDangerous => _warning <= 0f;
        public void Initialize(float warning, float speed) { _warning = warning; _speed = speed; }
        private void Awake() => _body = GetComponent<Rigidbody2D>();
        private void FixedUpdate()
        {
            _warning -= Time.fixedDeltaTime;
            if (!IsDangerous) return;
            if (_warningMarker != null) _warningMarker.SetActive(false);
            _body.MovePosition(_body.position + Vector2.down * (_speed * Time.fixedDeltaTime));
            if (_body.position.y < -1f) Destroy(gameObject);
        }
        public void OnHazardTouch() { }
    }
}
