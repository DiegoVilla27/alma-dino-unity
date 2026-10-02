using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class ReflectableMeteor2D : MonoBehaviour, IRoarReactive2D, IConditionalHazard2D
    {
        [SerializeField] private SpriteRenderer _visual;
        private Rigidbody2D _body;
        private MeteorImpactGate2D _gate;
        private float _remaining;
        private float _reflectedSpeed;
        public bool IsReflected { get; private set; }
        public bool IsFlying { get; private set; }
        public bool IsDangerous => IsFlying && !IsReflected;
        private void Awake() => _body = GetComponent<Rigidbody2D>();
        public void Launch(Vector2 position, Vector2 velocity, float lifetime, float reflectedSpeed, MeteorImpactGate2D gate)
        {
            _gate = gate; _remaining = lifetime; _reflectedSpeed = reflectedSpeed;
            IsReflected = false; IsFlying = true; gameObject.SetActive(true);
            _body.position = position; _body.linearVelocity = velocity; _visual.color = new Color(1f, .33f, .02f);
        }
        public void ReceiveRoar(Vector2 direction)
        {
            if (!IsDangerous || Vector2.Dot(direction, (Vector2)_gate.transform.position - _body.position) <= 0f) return;
            IsReflected = true;
            _body.linearVelocity = ((Vector2)_gate.transform.position - _body.position).normalized * _reflectedSpeed;
            _visual.color = Color.cyan;
        }
        private void FixedUpdate()
        {
            _remaining -= Time.fixedDeltaTime;
            if (_remaining <= 0f) Clear();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<MeteorImpactGate2D>(out var gate))
            {
                if (IsReflected && gate == _gate) gate.OpenByMeteor();
                Clear();
            }
            else if (!other.isTrigger && !other.TryGetComponent<IPlayerRespawnable>(out _)) Clear();
        }
        public void OnHazardTouch() => Clear();
        public void Clear() { IsFlying = false; gameObject.SetActive(false); }
    }
}
