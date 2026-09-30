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

        [Test]
        public void ResetProgression_ClearsAllAbilities()
        {
            GameProgression.UnlockAbility(AbilityType.DoubleJump);
            GameProgression.UnlockAbility(AbilityType.Dash);

            GameProgression.ResetProgression();

            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.DoubleJump));
            Assert.IsFalse(GameProgression.IsAbilityUnlocked(AbilityType.Dash));
        }
    }
}
