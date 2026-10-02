using AlmaDino.Features.Enemies.Services;
using NUnit.Framework;
namespace AlmaDino.Tests.EditMode
{
    public class PoisonShotCycleTests
    {
        [Test]
        public void Shot_HasWarningBeforeTwoSecondInterval()
        {
            var cycle = new PoisonShotCycle(2f, 0.6f);
            Assert.IsFalse(cycle.Tick(1f));
            Assert.IsFalse(cycle.IsWarning);
            Assert.IsFalse(cycle.Tick(0.5f));
            Assert.IsTrue(cycle.IsWarning);
            Assert.IsTrue(cycle.Tick(0.5f));
            Assert.IsFalse(cycle.IsWarning);
        }
        [Test]
        public void Reset_CancelsWarningAndStartsFullInterval()
        {
            var cycle = new PoisonShotCycle(2f, 0.6f);
            cycle.Tick(1.5f);
            cycle.Reset();
            Assert.IsFalse(cycle.IsWarning);
            Assert.IsFalse(cycle.Tick(1.9f));
            Assert.IsTrue(cycle.Tick(0.11f));
        }
    }
}
