namespace AlmaDino.Features.Boss.Services
{
    public sealed class ThiefKingFight
    {
        public int Hits { get; private set; }
        public bool Vulnerable { get; private set; }
        public bool SteamOpen { get; private set; }
        public bool AnchorCracked { get; private set; }
        public bool IsDefeated => Hits == 3;
        public void Expose() { if(Hits < 2) Vulnerable = true; }
        public void Close() => Vulnerable = false;
        public bool Pound()
        {
            if(IsDefeated || (Hits < 2 ? !Vulnerable : !AnchorCracked)) return false;
            Hits++; ResetAttempt(); return true;
        }
        public void OpenSteam() { if(Hits == 2) SteamOpen = true; }
        public void CrackAnchor() { if(Hits == 2 && SteamOpen) AnchorCracked = true; }
        public void ResetAttempt() { Vulnerable = false; SteamOpen = false; AnchorCracked = false; }
    }
}
