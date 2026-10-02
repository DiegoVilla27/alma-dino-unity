using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Environment.Services;
using AlmaDino.Features.Player.Services;
using NUnit.Framework;
using UnityEngine;
namespace AlmaDino.Tests.EditMode
{
    public class VolcanoMechanicsTests
    {
        [TestCase(2f, 0f, true)]
        [TestCase(-2f, 0f, false)]
        [TestCase(4f, 0f, false)]
        [TestCase(0f, 2f, false)]
        [TestCase(2f, 1f, true)]
        public void RoarTargetsOnlyTheFrontConeWithinThreeMeters(float x, float y, bool expected)
            => Assert.AreEqual(expected, RoarTargeting.Contains(Vector2.zero, new Vector2(x, y), Vector2.right, 3f, 45f));
        [Test]
        public void BoulderTravelsExactlyFiveMetersAndRejectsOverlappingPushes()
        {
            var motion = new BoulderPushMotion(.8f, .6f);
            Assert.IsTrue(motion.Start(new Vector2(14f, 1.8f), new Vector2(19f, -1.6f)));
            Assert.IsFalse(motion.Start(Vector2.zero, Vector2.one));
            motion.Tick(.4f); Assert.IsTrue(motion.IsMoving);
            motion.Tick(.4f); Assert.IsFalse(motion.IsMoving);
            Assert.AreEqual(new Vector2(19f, -1.6f), motion.Position);
        }
        [Test]
        public void BoulderResetCancelsMotionAtItsOriginalPosition()
        {
            var motion = new BoulderPushMotion(.8f, .6f);
            motion.Start(Vector2.zero, Vector2.right * 5f); motion.Tick(.3f); motion.Reset(Vector2.zero); motion.Tick(1f);
            Assert.AreEqual(Vector2.zero, motion.Position); Assert.IsFalse(motion.IsMoving);
        }
        [Test]
        public void SteamWarnsBeforeEruptingAndCyclesBackToSafety()
        {
            var cycle = new GeyserCycle(2.2f, .8f, 1.2f);
            cycle.Tick(2.3f); Assert.AreEqual(GeyserPhase.Warning, cycle.Phase);
            cycle.Tick(.8f); Assert.AreEqual(GeyserPhase.Erupting, cycle.Phase);
            cycle.Tick(1.2f); Assert.AreEqual(GeyserPhase.Dormant, cycle.Phase);
        }
        [Test]
        public void SteamResetProvidesAFullSafeWindow()
        {
            var cycle = new GeyserCycle(2.2f, .8f, 1.2f);
            cycle.Tick(3.2f); cycle.Reset(); cycle.Tick(2.1f);
            Assert.AreEqual(GeyserPhase.Dormant, cycle.Phase);
        }
        [Test]
        public void Level41BaselineKeepsThreeAbilitiesButDoesNotGrantRoar()
        {
            GameProgression.ResetProgression(); GameProgression.EnsureLevelBaseline("Level_4_1");
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.GroundPound));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Roar));
            GameProgression.ResetProgression();
        }
    }
}
