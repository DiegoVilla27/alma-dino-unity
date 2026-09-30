namespace AlmaDino.Core.Interfaces
{
    public interface IAbilityUnlockable
    {
        bool IsDoubleJumpUnlocked { get; }
        bool IsGroundPoundUnlocked { get; }
        bool IsDashUnlocked { get; }
        bool IsRoarUnlocked { get; }

        void UnlockAbility(AbilityType type);
    }
}
