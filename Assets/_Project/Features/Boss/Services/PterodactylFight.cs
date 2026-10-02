namespace AlmaDino.Features.Boss.Services
{
    public enum PterodactylPhase { Wind, Warning, Dive, Recovery, Defeated }

    public sealed class PterodactylFight
    {
        public int Hits { get; private set; }
        public PterodactylPhase Phase { get; private set; } = PterodactylPhase.Wind;
        public bool IsDefeated => Phase == PterodactylPhase.Defeated;
        public void BeginWarning() { if (!IsDefeated) Phase = PterodactylPhase.Warning; }
        public void BeginDive() { if (Phase == PterodactylPhase.Warning) Phase = PterodactylPhase.Dive; }
        public bool TryStrike(bool frontal)
        {
            if (Phase != PterodactylPhase.Dive || !frontal) return false;
            Hits++;
            Phase = Hits == 3 ? PterodactylPhase.Defeated : PterodactylPhase.Recovery;
            return true;
        }
        public void ResetCycle() { if (!IsDefeated) Phase = PterodactylPhase.Wind; }
    }
}
