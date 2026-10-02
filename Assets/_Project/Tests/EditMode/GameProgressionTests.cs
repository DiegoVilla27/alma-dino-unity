using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using NUnit.Framework;

namespace AlmaDino.Tests.EditMode
{
    [TestFixture]
    public class GameProgressionTests
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
        public void InitialState_HasNoAbilitiesUnlocked()
        {
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.GroundPound));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Roar));
        }

        [Test]
        public void UnlockAbility_PersistsAbility()
        {
            GameProgression.UnlockAbility(AbilityType.DoubleJump);

            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.GroundPound));
        }

        [Test]
        public void EnsureLevelBaseline_ForLevel12_UnlocksDoubleJump()
        {
            GameProgression.EnsureLevelBaseline("Level_1_2");

            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
        }

        [TestCase("Boss_2")]
        [TestCase("Level_3_1")]
        public void CaveBossAndSwampEntry_HaveEarlierAbilities_WithoutSkippingDashAltar(string scene)
        {
            GameProgression.EnsureLevelBaseline(scene);
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.GroundPound));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Roar));
        }

        [TestCase("Level_3_2")]
        [TestCase("Level_3_3")]
        [TestCase("Level_3_4")]
        public void SwampPractice_BaselineIncludesDashWithoutRoar(string scene)
        {
            GameProgression.EnsureLevelBaseline(scene);
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.GroundPound));
            Assert.IsTrue(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Roar));
        }

        [Test]
        public void ResetProgression_ClearsAllAbilities()
        {
            GameProgression.UnlockAbility(AbilityType.DoubleJump);
            GameProgression.UnlockAbility(AbilityType.Dash);

            GameProgression.ResetProgression();

            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
        }

        [Test]
        public void RescueEgg_PersistsEggAndIncrementsCount()
        {
            Assert.AreEqual(0, GameProgression.RescuedEggCount);
            Assert.IsFalse(GameProgression.IsEggRescued(EggType.GreenEgg));

            GameProgression.RescueEgg(EggType.GreenEgg);

            Assert.AreEqual(1, GameProgression.RescuedEggCount);
            Assert.IsTrue(GameProgression.IsEggRescued(EggType.GreenEgg));
            Assert.IsFalse(GameProgression.IsEggRescued(EggType.BlueEgg));
        }

        [Test]
        public void PurpleEgg_UsesThirdSlotAndPersistsWithoutCompletingWorld()
        {
            Assert.AreEqual(3, (int)EggType.PurpleEgg);
            GameProgression.RescueEgg(EggType.PurpleEgg);
            GameProgression.LoadProgression();
            Assert.IsTrue(GameProgression.IsEggRescued(EggType.PurpleEgg));
            Assert.AreEqual(1, GameProgression.RescuedEggCount);
            Assert.IsFalse(GameProgression.IsWorldCompleted(3));
        }

        [Test]
        public void LegacyThirdEggSave_LoadsAsPurple_AndResetClearsIt()
        {
            UnityEngine.PlayerPrefs.SetInt("AlmaDino_EggRescued_YellowEgg", 1);
            GameProgression.LoadProgression();
            Assert.IsTrue(GameProgression.IsEggRescued(EggType.PurpleEgg));
            GameProgression.RescueEgg(EggType.PurpleEgg);
            Assert.AreEqual(1, GameProgression.RescuedEggCount);
            GameProgression.ResetProgression();
            GameProgression.LoadProgression();
            Assert.IsFalse(GameProgression.IsEggRescued(EggType.PurpleEgg));
        }

        [Test]
        public void ResetProgression_ClearsRescuedEggs()
        {
            GameProgression.RescueEgg(EggType.GreenEgg);
            GameProgression.RescueEgg(EggType.BlueEgg);
            Assert.AreEqual(2, GameProgression.RescuedEggCount);

            GameProgression.ResetProgression();

            Assert.AreEqual(0, GameProgression.RescuedEggCount);
            Assert.IsFalse(GameProgression.IsEggRescued(EggType.GreenEgg));
        }
    }
}
