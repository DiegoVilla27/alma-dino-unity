using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Enemies.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PoisonBubble2D : MonoBehaviour, IConditionalHazard2D
    {
        private Rigidbody2D _body;
        private float _remainingTime;
        public bool IsDangerous => gameObject.activeInHierarchy;
        private void Awake() => _body = GetComponent<Rigidbody2D>();
        public void Launch(Vector2 position, Vector2 velocity, float gravityScale, float lifetime)
        {
            gameObject.SetActive(true);
            _body.position = position;
            _body.gravityScale = gravityScale;
            _body.linearVelocity = velocity;
            _remainingTime = lifetime;
        }
        private void FixedUpdate()
        {
            _remainingTime -= Time.fixedDeltaTime;
            if (_remainingTime <= 0f) Clear();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.isTrigger && !other.TryGetComponent<IPlayerRespawnable>(out _)) Clear();
        }
        public void OnHazardTouch() => Clear();
        public void Clear() => gameObject.SetActive(false);
    }
}
