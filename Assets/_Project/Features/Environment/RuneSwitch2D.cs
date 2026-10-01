using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class RuneSwitch2D : MonoBehaviour
    {
        [SerializeField] private SeesawConfigSO _config;
        [SerializeField] private SpriteRenderer _indicator;
        private float _activeUntil;
        public float RemainingTime => Mathf.Max(0f, _activeUntil - Time.time);
        public bool IsActive => RemainingTime > 0f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var weight = other.GetComponent<CatapultWeight2D>();
            if (weight == null || !weight.IsLaunched || other.attachedRigidbody.linearVelocity.y <= 0f) return;
            _activeUntil = Time.time + _config.GateOpenDuration;
        }

        private void Update()
        {
            if (_indicator != null) _indicator.color = IsActive ? new Color(0.32f, 0.9f, 0.55f) : new Color(1f, 0.65f, 0.15f);
        }

        public void ResetSignal() => _activeUntil = 0f;
    }
}
