using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.Services;
using NUnit.Framework;

namespace AlmaDino.Tests.EditMode
{
    public class PterodactylFightTests
    {
        [Test]
        public void OnlyFrontalStrikesDuringDiveCanDamage()
        {
            var fight = new PterodactylFight();
            Assert.IsFalse(fight.TryStrike(true));
            fight.BeginWarning();
            Assert.IsFalse(fight.TryStrike(true));
            fight.BeginDive();
            Assert.IsFalse(fight.TryStrike(false));
            Assert.IsTrue(fight.TryStrike(true));
            Assert.IsFalse(fight.TryStrike(true));
            Assert.AreEqual(1, fight.Hits);
        }
        [Test]
        public void DeathResetsCurrentCycleAndKeepsSuccessfulHits()
        {
            var fight = new PterodactylFight();
            fight.BeginWarning(); fight.BeginDive(); fight.TryStrike(true);
            fight.ResetCycle();
            Assert.AreEqual(1, fight.Hits);
            Assert.AreEqual(PterodactylPhase.Wind, fight.Phase);
        }
        [Test]
        public void ThirdStrikeDefeatsBossAndCannotRestartIt()
        {
            var fight = new PterodactylFight();
            for (int i = 0; i < 3; i++)
            {
                fight.ResetCycle(); fight.BeginWarning(); fight.BeginDive();
                Assert.IsTrue(fight.TryStrike(true));
            }
            fight.ResetCycle(); fight.BeginWarning();
            Assert.IsTrue(fight.IsDefeated);
            Assert.IsFalse(fight.TryStrike(true));
        }
        [Test]
        public void BossBaselineProvidesAllThreeAbilitiesWithoutRoar()
        {
            GameProgression.ResetProgression();
            GameProgression.EnsureLevelBaseline("Boss_3");
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.GroundPound));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Roar));
            GameProgression.ResetProgression();
        }
    }
}
