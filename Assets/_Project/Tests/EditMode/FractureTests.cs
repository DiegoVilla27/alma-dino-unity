using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Player.Services;
using NUnit.Framework;
using UnityEngine;
namespace AlmaDino.Tests.EditMode
{
    public class FractureTests
    {
        [Test]
        public void MeteorLaunchHasComfortableOrdinaryRoarRangeAndAngle()
        {
            var origin=new Vector2(22f,-.85f); var meteor=origin+new Vector2(2.4f,1.35f);
            Assert.IsTrue(RoarTargeting.Contains(origin,meteor,Vector2.right,3f,45f));
            Assert.Less(Vector2.Distance(origin,meteor),2.8f);
            Assert.IsFalse(RoarTargeting.Contains(origin,meteor,Vector2.left,3f,45f));
        }
        [Test]
        public void DirectEntryProvidesAllAbilitiesWithoutCompletingWorld()
        {
            GameProgression.ResetProgression();
            try
            {
                GameProgression.EnsureLevelBaseline("Level_4_3");
                foreach(AbilityType ability in new[]{AbilityType.DoubleJump,AbilityType.GroundPound,AbilityType.Dash,AbilityType.Roar}) Assert.IsTrue(GameProgression.IsAbilityUnlocked(ability));
                Assert.IsFalse(GameProgression.IsWorldCompleted(4)); Assert.IsFalse(GameProgression.IsEggRescued(EggType.RedEgg));
            }
            finally { GameProgression.ResetProgression(); }
        }
    }
}
