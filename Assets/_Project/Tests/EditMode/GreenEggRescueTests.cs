using AlmaDino.Core.Progression;
using AlmaDino.Features.Environment;
using NUnit.Framework;
using UnityEngine;

namespace AlmaDino.Tests.EditMode
{
    [TestFixture]
    public class GreenEggRescueTests
    {
        [SetUp]
        public void SetUp()
        {
            GameProgression.ResetProgression();
        }

        [TearDown]
        public void TearDown()
        {
            GameProgression.ResetProgression();
        }

        [Test]
        public void InitialState_IsNotRescued()
        {
            var go = new GameObject("TestEgg");
            go.AddComponent<BoxCollider2D>();
            var egg = go.AddComponent<GreenEggRescue2D>();

            Assert.IsFalse(egg.IsRescued);
            Assert.AreEqual(EggType.GreenEgg, egg.RescuedEggType);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void TriggerRescue_MarksRescued_AndPersistsInProgression()
        {
            var go = new GameObject("TestEgg");
            go.AddComponent<BoxCollider2D>();
            var egg = go.AddComponent<GreenEggRescue2D>();

            bool eventInvoked = false;
            EggType rescuedType = 0;
            egg.OnEggRescued += (t) =>
            {
                eventInvoked = true;
                rescuedType = t;
            };

            egg.TriggerRescue();

            Assert.IsTrue(egg.IsRescued);
            Assert.IsTrue(eventInvoked);
            Assert.AreEqual(EggType.GreenEgg, rescuedType);
            Assert.IsTrue(GameProgression.IsEggRescued(EggType.GreenEgg));
            Assert.AreEqual(1, GameProgression.RescuedEggCount);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Configure_SetsCustomEggProperties()
        {
            var go = new GameObject("TestEgg");
            go.AddComponent<BoxCollider2D>();
            var egg = go.AddComponent<GreenEggRescue2D>();

            egg.Configure(EggType.BlueEgg, "CUSTOM TITLE", "CUSTOM TEXT", Color.cyan);

            Assert.AreEqual(EggType.BlueEgg, egg.RescuedEggType);

            Object.DestroyImmediate(go);
        }
    }
}
