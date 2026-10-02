using System;
namespace AlmaDino.Features.Enemies.Services
{
    public sealed class PoisonShotCycle
    {
        private readonly float _interval;
        private readonly float _warning;
        private float _elapsed;
        public bool IsWarning => _elapsed >= _interval - _warning;
        public PoisonShotCycle(float interval, float warning)
        {
            _interval = Math.Max(0.1f, interval);
            _warning = Math.Min(Math.Max(0f, warning), _interval);
        }
        public bool Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed < _interval) return false;
            _elapsed %= _interval;
            return true;
        }
        public void Reset() => _elapsed = 0f;
    }
}
