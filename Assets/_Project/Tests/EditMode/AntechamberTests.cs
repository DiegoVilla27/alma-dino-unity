using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using NUnit.Framework;
namespace AlmaDino.Tests.EditMode
{
    public class AntechamberTests
    {
        [Test] public void DirectEntryUnlocksFourAbilitiesWithoutGrantingEggsOrWorldCompletion()
        {
            GameProgression.ResetProgression();
            try
            {
                GameProgression.EnsureLevelBaseline("Level_4_4");
                foreach(AbilityType ability in new[]{AbilityType.DoubleJump,AbilityType.GroundPound,AbilityType.Dash,AbilityType.Roar})Assert.IsTrue(GameProgression.IsAbilityUnlocked(ability));
                Assert.AreEqual(0,GameProgression.RescuedEggCount);Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            }
            finally{GameProgression.ResetProgression();}
        }
        [Test] public void RedEggPersistenceDoesNotCompleteWorldOrDuplicateRescue()
        {
            GameProgression.ResetProgression();
            try
            {
                GameProgression.RescueEgg(EggType.RedEgg);GameProgression.RescueEgg(EggType.RedEgg);
                Assert.AreEqual(1,GameProgression.RescuedEggCount);Assert.IsTrue(GameProgression.IsEggRescued(EggType.RedEgg));Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            }
            finally{GameProgression.ResetProgression();}
        }
    }
}
