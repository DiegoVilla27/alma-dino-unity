using AlmaDino.Features.Environment;
using NUnit.Framework;
using UnityEngine;

namespace AlmaDino.Tests.EditMode
{
    [TestFixture]
    public class EnvironmentComponentsTests
    {
        [Test]
        public void TimedRuneGate_InitialState_IsClosed()
        {
            var go = new GameObject("TestGate");
            go.AddComponent<BoxCollider2D>();
            var gate = go.AddComponent<TimedRuneGate2D>();

            Assert.IsFalse(gate.IsOpen);
            Assert.AreEqual(4.0f, gate.OpenDuration);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DashBreakableBarrier_BreakWithDash_MarksAsBroken()
        {
            var go = new GameObject("TestBarrier");
            go.AddComponent<BoxCollider2D>();
            var barrier = go.AddComponent<DashBreakableBarrier2D>();

            Assert.IsFalse(barrier.IsBroken);
            barrier.BreakWithDash();
            Assert.IsTrue(barrier.IsBroken);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void WindCurrentZone_DefaultProperties_AreValid()
        {
            var go = new GameObject("TestWind");
            go.AddComponent<BoxCollider2D>();
            var wind = go.AddComponent<WindCurrentZone2D>();

            Assert.AreEqual(Vector2.up, wind.Direction);
            Assert.AreEqual(22.0f, wind.WindStrength);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SeesawPlatform_InitialAngle_IsZero()
        {
            var go = new GameObject("TestSeesaw");
            go.AddComponent<BoxCollider2D>();
            var seesaw = go.AddComponent<SeesawPlatform2D>();

            Assert.AreEqual(0f, seesaw.CurrentAngle);
            Assert.IsFalse(seesaw.IsOccupied);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void NarrativePrologueTrigger_Configure_SetsPropertiesCorrectly()
        {
            var go = new GameObject("TestPrologue");
            go.AddComponent<BoxCollider2D>();
            var trigger = go.AddComponent<NarrativePrologueTrigger>();

            trigger.Configure("TEST TITLE", "TEST MESSAGE", Color.red, 5.0f);

            var titleField = typeof(NarrativePrologueTrigger).GetField("_title", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var msgField = typeof(NarrativePrologueTrigger).GetField("_message", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.AreEqual("TEST TITLE", titleField.GetValue(trigger));
            Assert.AreEqual("TEST MESSAGE", msgField.GetValue(trigger));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Camera2DFollow_SetBounds_SetsBoundsCorrectly()
        {
            var go = new GameObject("TestCamera");
            var follow = go.AddComponent<AlmaDino.Features.Camera.Camera2DFollow>();

            follow.SetBounds(new Vector2(-5f, -3f), new Vector2(108f, 30f));

            var minField = typeof(AlmaDino.Features.Camera.Camera2DFollow).GetField("_minBounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxField = typeof(AlmaDino.Features.Camera.Camera2DFollow).GetField("_maxBounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.AreEqual(new Vector2(-5f, -3f), minField.GetValue(follow));
            Assert.AreEqual(new Vector2(108f, 30f), maxField.GetValue(follow));

            Object.DestroyImmediate(go);
        }
    }
}
