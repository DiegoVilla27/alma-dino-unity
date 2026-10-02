using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Environment.Services;
using AlmaDino.Features.Player.Services;
using NUnit.Framework;
using UnityEngine;
namespace AlmaDino.Tests.EditMode
{
    public class ResonanceBellTests
    {
        [Test]
        public void RingOpensExactlyFiveSecondsThenCloses()
        {
            var window = new BellSuppressionWindow(5f); Assert.IsFalse(window.IsOpen);
            window.Ring(); window.Tick(4.9f); Assert.IsTrue(window.IsOpen);
            window.Tick(.1f); Assert.IsFalse(window.IsOpen); Assert.AreEqual(0f, window.Remaining);
        }
        [Test]
        public void ReringRefreshesTheFullWindow()
        {
            var window = new BellSuppressionWindow(5f); window.Ring(); window.Tick(4f); window.Ring();
            Assert.AreEqual(5f, window.Remaining); window.Tick(4f); Assert.IsTrue(window.IsOpen);
        }
        [Test]
        public void ResetClosesImmediately()
        {
            var window = new BellSuppressionWindow(5f); window.Ring(); window.Reset(); Assert.IsFalse(window.IsOpen);
        }
        [Test]
        public void NegativeDeltaDoesNotExtendCountdown()
        {
            var window = new BellSuppressionWindow(5f); window.Ring(); window.Tick(-10f); Assert.AreEqual(5f, window.Remaining);
        }
        [Test]
        public void ElevatedBellRequiresAnAirborneRoarButFitsResonanceRange()
        {
            var bell = new Vector2(45.5f, 6.5f);
            Assert.IsFalse(RoarTargeting.Contains(new Vector2(40f, .62f), bell, Vector2.right, 8f, 45f));
            Assert.IsTrue(RoarTargeting.Contains(new Vector2(40f, 3.1f), bell, Vector2.right, 8f, 45f));
            Assert.IsFalse(RoarTargeting.Contains(new Vector2(40f, 3.1f), bell, Vector2.right, 3f, 45f));
        }
        [Test]
        public void Level42BaselineGrantsAllFourAbilitiesWithoutCompletingTheWorld()
        {
            GameProgression.ResetProgression(); GameProgression.EnsureLevelBaseline("Level_4_2");
            foreach (AbilityType ability in System.Enum.GetValues(typeof(AbilityType))) Assert.IsTrue(GameProgression.IsAbilityUnlocked(ability));
            Assert.IsFalse(GameProgression.IsWorldCompleted(4)); GameProgression.ResetProgression();
        }
    }
}
