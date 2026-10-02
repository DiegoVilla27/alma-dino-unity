using UnityEngine;
namespace AlmaDino.Features.Environment.Services
{
    public sealed class BoulderPushMotion
    {
        private readonly float _duration;
        private readonly float _arcHeight;
        private Vector2 _start, _end;
        private float _elapsed;
        public bool IsMoving { get; private set; }
        public Vector2 Position { get; private set; }
        public BoulderPushMotion(float duration, float arcHeight) { _duration = Mathf.Max(.01f, duration); _arcHeight = arcHeight; }
        public bool Start(Vector2 start, Vector2 end)
        {
            if (IsMoving) return false;
            _start = start; _end = end; Position = start; _elapsed = 0f; IsMoving = true;
            return true;
        }
        public void Tick(float deltaTime)
        {
            if (!IsMoving) return;
            _elapsed += deltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);
            Position = Vector2.Lerp(_start, _end, Mathf.SmoothStep(0f, 1f, t)) + Vector2.up * (_arcHeight * Mathf.Sin(Mathf.PI * t));
            if (t >= 1f) { Position = _end; IsMoving = false; }
        }
        public void Reset(Vector2 start) { Position = start; IsMoving = false; }
    }
}
