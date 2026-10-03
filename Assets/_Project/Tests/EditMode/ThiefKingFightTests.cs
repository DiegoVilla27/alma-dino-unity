using AlmaDino.Features.Boss.Services;
using NUnit.Framework;
namespace AlmaDino.Tests.EditMode
{
    public class ThiefKingFightTests
    {
        [Test] public void ArmorRejectsUnpreparedPounds() { var f=new ThiefKingFight();Assert.IsFalse(f.Pound());Assert.AreEqual(0,f.Hits); }
        [Test] public void EachHitRequiresItsOwnCounter() { var f=new ThiefKingFight();f.Expose();Assert.IsTrue(f.Pound());Assert.IsFalse(f.Pound());f.Expose();Assert.IsTrue(f.Pound());Assert.IsFalse(f.Pound()); }
        [Test] public void FinalAnchorRequiresSteamThenRoar() { var f=new ThiefKingFight();f.Expose();f.Pound();f.Expose();f.Pound();f.CrackAnchor();Assert.IsFalse(f.Pound());f.OpenSteam();f.CrackAnchor();Assert.IsTrue(f.Pound());Assert.IsTrue(f.IsDefeated);Assert.IsFalse(f.Pound()); }
        [Test] public void RetryKeepsHitsAndRestoresMechanisms() { var f=new ThiefKingFight();f.Expose();f.Pound();f.Expose();f.Pound();f.OpenSteam();f.CrackAnchor();f.ResetAttempt();Assert.AreEqual(2,f.Hits);Assert.IsFalse(f.SteamOpen);Assert.IsFalse(f.AnchorCracked); }
    }
}
