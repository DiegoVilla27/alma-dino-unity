using System;

namespace AlmaDino.Features.Environment.Services
{
    public enum RisingGasPhase { Dormant, Warning, Rising, Stopped }

    public sealed class RisingGasCycle
    {
        private readonly float _warningDuration;
        private readonly float _riseSpeed;
        private float _warningRemaining;
        private float _ceiling;

        public RisingGasPhase Phase { get; private set; }
        public float Height { get; private set; }
        public bool IsDangerous => Phase == RisingGasPhase.Rising;

        public RisingGasCycle(float warningDuration, float riseSpeed)
        {
            _warningDuration = Math.Max(0f, warningDuration);
            _riseSpeed = Math.Max(0f, riseSpeed);
        }

        public void Reset(float height)
        {
            Height = height;
            Phase = RisingGasPhase.Dormant;
        }

        public void Arm(float initialHeight, float ceiling)
        {
            Height = initialHeight;
            _ceiling = Math.Max(initialHeight, ceiling);
            _warningRemaining = _warningDuration;
            Phase = RisingGasPhase.Warning;
        }

        public void Tick(float deltaTime)
        {
            float remaining = Math.Max(0f, deltaTime);
            if (Phase == RisingGasPhase.Warning)
            {
                float warningStep = Math.Min(remaining, _warningRemaining);
                _warningRemaining -= warningStep;
                remaining -= warningStep;
                if (_warningRemaining > 0f) return;
                Phase = RisingGasPhase.Rising;
            }
            if (Phase == RisingGasPhase.Rising)
                Height = Math.Min(_ceiling, Height + _riseSpeed * remaining);
        }

        public void Stop() => Phase = RisingGasPhase.Stopped;
    }
}
