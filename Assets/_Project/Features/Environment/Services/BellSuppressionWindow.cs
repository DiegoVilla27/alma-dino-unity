using System;
namespace AlmaDino.Features.Environment.Services
{
    public sealed class BellSuppressionWindow
    {
        private readonly float _duration;
        public float Remaining { get; private set; }
        public bool IsOpen => Remaining > 0f;
        public BellSuppressionWindow(float duration) => _duration = Math.Max(.01f, duration);
        public void Ring() => Remaining = _duration;
        public void Tick(float dt) => Remaining = Math.Max(0f, Remaining - Math.Max(0f, dt));
        public void Reset() => Remaining = 0f;
    }
}
