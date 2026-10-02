using UnityEngine;
namespace AlmaDino.Features.Environment.Services
{
    public enum GeyserPhase { Dormant, Warning, Erupting }
    public sealed class GeyserCycle
    {
        private readonly float _dormant, _warning, _eruption;
        private float _elapsed;
        public GeyserPhase Phase => _elapsed < _dormant ? GeyserPhase.Dormant : _elapsed < _dormant + _warning ? GeyserPhase.Warning : GeyserPhase.Erupting;
        public GeyserCycle(float dormant, float warning, float eruption) { _dormant = dormant; _warning = warning; _eruption = eruption; }
        public void Tick(float dt) => _elapsed = Mathf.Repeat(_elapsed + dt, _dormant + _warning + _eruption);
        public void Reset() => _elapsed = 0f;
    }
}
