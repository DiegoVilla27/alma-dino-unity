using AlmaDino.Features.Environment.Services;
using NUnit.Framework;

namespace AlmaDino.Tests.EditMode
{
    public class RisingGasCycleTests
    {
        [Test]
        public void Warning_IsHarmless_AndOnlyRemainingTimeRaisesGas()
        {
            var gas = new RisingGasCycle(3f, 0.7f);
            gas.Arm(-4f, 7.2f);
            gas.Tick(2f);
            Assert.AreEqual(RisingGasPhase.Warning, gas.Phase);
            Assert.IsFalse(gas.IsDangerous);
            Assert.AreEqual(-4f, gas.Height);
            gas.Tick(2f);
            Assert.AreEqual(RisingGasPhase.Rising, gas.Phase);
            Assert.IsTrue(gas.IsDangerous);
            Assert.That(gas.Height, Is.EqualTo(-3.3f).Within(0.001f));
        }

        [Test]
        public void Gas_StopsBelowCheckpointCeiling()
        {
            var gas = new RisingGasCycle(3f, 0.7f);
            gas.Arm(-4f, 7.2f);
            gas.Tick(100f);
            Assert.That(gas.Height, Is.EqualTo(7.2f).Within(0.001f));
        }

        [Test]
        public void Rescue_StopsDamageAndRise()
        {
            var gas = new RisingGasCycle(0f, 1f);
            gas.Arm(8f, 19.2f);
            gas.Tick(2f);
            gas.Stop();
            gas.Tick(10f);
            Assert.IsFalse(gas.IsDangerous);
            Assert.AreEqual(10f, gas.Height);
        }

        [Test]
        public void Reset_RestoresSafeCheckpointAndFullWarning()
        {
            var gas = new RisingGasCycle(3f, 0.7f);
            gas.Arm(-4f, 7.2f);
            gas.Tick(10f);
            gas.Reset(3.5f);
            gas.Tick(100f);
            Assert.AreEqual(RisingGasPhase.Dormant, gas.Phase);
            Assert.AreEqual(3.5f, gas.Height);
            gas.Arm(3.5f, 11.7f);
            gas.Tick(2.9f);
            Assert.IsFalse(gas.IsDangerous);
        }
    }
}
