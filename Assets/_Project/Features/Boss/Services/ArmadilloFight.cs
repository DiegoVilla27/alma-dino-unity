namespace AlmaDino.Features.Boss.Services
{
    public sealed class ArmadilloFight
    {
        public int Hits { get; private set; }
        public int Bounces { get; private set; }
        public bool IsRolling { get; private set; }
        public bool IsStunned { get; private set; }
        public bool IsDefeated => Hits >= 3;

        public void BeginRoll()
        {
            if (IsDefeated) return;
            Bounces = 0;
            IsStunned = false;
            IsRolling = true;
        }

        public void HitPillar()
        {
            if (!IsRolling) return;
            Bounces++;
            if (Bounces < 3) return;
            IsRolling = false;
            IsStunned = true;
        }

        public bool TryDamage()
        {
            if (!IsStunned || IsDefeated) return false;
            Hits++;
            IsStunned = false;
            return true;
        }

        public void ResetCycle()
        {
            Bounces = 0;
            IsRolling = false;
            IsStunned = false;
        }
    }
}
