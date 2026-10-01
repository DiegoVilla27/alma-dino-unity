using AlmaDino.Features.Boss.Services;
using NUnit.Framework;

namespace AlmaDino.Tests.EditMode
{
    public class ArmadilloFightTests
    {
        [Test]
        public void ThirdImpactExposesWeakPoint()
        {
            var fight = new ArmadilloFight();
            fight.BeginRoll();
            fight.HitPillar(); fight.HitPillar();
            Assert.That(fight.IsStunned, Is.False);
            fight.HitPillar();
            Assert.That(fight.IsStunned, Is.True);
        }

        [Test]
        public void DamageRequiresStunAndCannotRepeat()
        {
            var fight = new ArmadilloFight();
            Assert.That(fight.TryDamage(), Is.False);
            fight.BeginRoll();
            for (int i = 0; i < 3; i++) fight.HitPillar();
            Assert.That(fight.TryDamage(), Is.True);
            Assert.That(fight.TryDamage(), Is.False);
            Assert.That(fight.Hits, Is.EqualTo(1));
        }

        [Test]
        public void RetryPreservesCompletedHitsAndClearsBounces()
        {
            var fight = new ArmadilloFight();
            fight.BeginRoll();
            for (int i = 0; i < 3; i++) fight.HitPillar();
            fight.TryDamage(); fight.BeginRoll(); fight.HitPillar(); fight.ResetCycle();
            Assert.That(fight.Hits, Is.EqualTo(1));
            Assert.That(fight.Bounces, Is.Zero);
            Assert.That(fight.IsStunned, Is.False);
        }

        [Test]
        public void ThreeHitsDefeatBossPermanently()
        {
            var fight = new ArmadilloFight();
            for (int cycle = 0; cycle < 3; cycle++)
            {
                fight.BeginRoll();
                for (int bounce = 0; bounce < 3; bounce++) fight.HitPillar();
                fight.TryDamage();
            }
            fight.ResetCycle(); fight.BeginRoll();
            Assert.That(fight.IsDefeated, Is.True);
            Assert.That(fight.TryDamage(), Is.False);
        }
    }
}
