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
    }
}
