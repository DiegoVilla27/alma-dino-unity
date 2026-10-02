using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Player.Controllers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AlmaDino.Tests.PlayMode
{
    public class BossFinalPlayTests
    {
        private PlayerController _player;
        private ThiefKingBoss2D _boss;
        [UnitySetUp] public IEnumerator Setup()
        {
            Clear();GameProgression.ResetProgression();yield return SceneManager.LoadSceneAsync("Boss_Final");yield return null;
            _player=UnityEngine.Object.FindAnyObjectByType<PlayerController>();_boss=UnityEngine.Object.FindAnyObjectByType<ThiefKingBoss2D>();yield return new WaitForSeconds(.15f);
        }
        [TearDown] public void Cleanup() {Clear();GameProgression.ResetProgression();}
        private static void Clear() {VirtualInputBridge.MoveVector=Vector2.zero;VirtualInputBridge.ReleaseJump();VirtualInputBridge.ConsumeFrameTriggers();}
        private static IEnumerator Wait(Func<bool> condition,string message,float timeout=8f)
        {float end=Time.time+timeout;while(!condition()&&Time.time<end)yield return null;Assert.IsTrue(condition(),message);}
        [Test] public void EntryHasAllAbilitiesCameraSixAndNoAudio()
        {Assert.IsTrue(_player.IsDoubleJumpUnlocked&&_player.IsGroundPoundUnlocked&&_player.IsDashUnlocked&&_player.IsRoarUnlocked);Assert.AreEqual(6f,Camera.main.orthographicSize);Assert.AreEqual(0,UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length);Assert.AreEqual(0,_boss.Fight.Hits);Assert.IsFalse(GameProgression.IsWorldCompleted(4));}
        [Test] public void PrematurePoundAndRoarCannotSkipPhases()
        {_boss.Strike();_boss.OpenSteam();_boss.CrackAnchor();_boss.Finish();Assert.AreEqual(0,_boss.Fight.Hits);Assert.IsFalse(GameProgression.IsWorldCompleted(4));}
        [UnityTest] public IEnumerator GroundDashCannotExposeArmorAndExpiredCounterClosesIt()
        {_boss.CounterCharge();Assert.IsFalse(_boss.Fight.Vulnerable);_player.Rigidbody.position=new Vector2(8f,3f);_boss.CounterCharge();Assert.IsTrue(_boss.Fight.Vulnerable);_player.Rigidbody.position=new Vector2(0f,.7f);yield return new WaitForSeconds(4.1f);Assert.IsFalse(_boss.Fight.Vulnerable);}
        [UnityTest] public IEnumerator LowJawSweepKillsAndRestoresCurrentPhase()
        {
            int deaths=0;Action<Vector2> count=_=>deaths++;_player.OnRespawned+=count;
            yield return Wait(()=>deaths>0,"The low jaw sweep must hit grounded Alma.",7f);
            _player.OnRespawned-=count;Assert.AreEqual(0,_boss.Fight.Hits);Assert.IsFalse(_boss.Fight.Vulnerable);
        }
        [UnityTest] public IEnumerator FullFightUsesRealInputsAndReachesSilentEpilogue()
        {
            for(int i=1;i<=4;i++)GameProgression.RescueEgg((EggType)i);
            int respawns=0;_player.OnRespawned += p => respawns++;
            yield return FirstHit();Assert.AreEqual(1,_boss.Fight.Hits);
            yield return Walk(9f);yield return new WaitForSeconds(.2f);
            VirtualInputBridge.MoveVector=Vector2.right;yield return new WaitForSeconds(.03f);VirtualInputBridge.MoveVector=Vector2.zero;
            yield return Wait(()=>GameObject.Find("Roar_Return_Meteor").transform.position.x<11.5f,"Meteor enters roar range.");
            VirtualInputBridge.TriggerRoar();yield return Wait(()=>_boss.Fight.Vulnerable,"Roar returns meteor.",2f);
            yield return JumpTo(12f,false);yield return Pound();yield return Wait(()=>_boss.Fight.Hits==2,"Second plate strike.");
            yield return JumpTo(24f,true);yield return JumpTo(29f,true);
            VirtualInputBridge.MoveVector=Vector2.right;VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.25f);VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.3f);VirtualInputBridge.TriggerDash();
            yield return Walk(38f);yield return Wait(()=>_player.GroundDetector.IsGrounded,"Land on steam seal.");yield return Pound();
            yield return Wait(()=>_boss.Fight.SteamOpen,"Pound opens steam.");
            yield return Wait(()=>_player.Rigidbody.position.y>9.5f,"Steam launches to crown.");
            VirtualInputBridge.MoveVector=Vector2.left;yield return Walk(34f);yield return Wait(()=>_player.GroundDetector.IsGrounded,"Land upper refuge.");
            yield return Walk(35.7f);
            VirtualInputBridge.MoveVector=Vector2.right;yield return new WaitForSeconds(.03f);VirtualInputBridge.MoveVector=Vector2.zero;VirtualInputBridge.TriggerRoar();
            yield return Wait(()=>_boss.Fight.AnchorCracked,"Roar cracks anchor.");yield return JumpTo(38f,true);yield return Pound();
            yield return Wait(()=>_boss.Fight.IsDefeated,"Final plunge.");yield return new WaitForSeconds(1.8f);
            Assert.AreEqual(4,GameProgression.RescuedEggCount);Assert.AreEqual(2,respawns,"Only scripted phase transition and epilogue relocation; no hazard deaths.");Assert.IsTrue(GameProgression.IsWorldCompleted(4));Assert.IsTrue(GameObject.Find("Sunset_Epilogue").activeSelf);Assert.Greater(_player.Rigidbody.position.y,20f);
        }
        private IEnumerator FirstHit()
        {
            yield return Walk(5.5f);VirtualInputBridge.MoveVector=Vector2.right;VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.28f);VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.25f);VirtualInputBridge.TriggerDash();
            yield return Wait(()=>_boss.Fight.Vulnerable,"Air dash counters charge.",1f);yield return Walk(12f);yield return new WaitForSeconds(.2f);
            VirtualInputBridge.TriggerGroundPound();yield return Wait(()=>_boss.Fight.Hits==1,"Pound breaks dorsal plate.");
        }
        private IEnumerator Walk(float x)
        {
            float end=Time.time+4f;while(Mathf.Abs(_player.Rigidbody.position.x-x)>.25f&&Time.time<end){VirtualInputBridge.MoveVector=_player.Rigidbody.position.x<x?Vector2.right:Vector2.left;yield return null;}
            VirtualInputBridge.MoveVector=Vector2.zero;Assert.AreEqual(x,_player.Rigidbody.position.x,.5f);
        }
        private IEnumerator JumpTo(float x,bool doubleJump)
        {VirtualInputBridge.MoveVector=_player.Rigidbody.position.x<x?Vector2.right:Vector2.left;VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.28f);if(doubleJump)VirtualInputBridge.TriggerJump();yield return Walk(x);yield return Wait(()=>_player.GroundDetector.IsGrounded,"Land at "+x);VirtualInputBridge.ReleaseJump();}
        private IEnumerator Pound()
        {VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.2f);VirtualInputBridge.TriggerGroundPound();yield return new WaitForSeconds(.5f);VirtualInputBridge.ReleaseJump();}
        [UnityTest] public IEnumerator RedEggPortalLoadsFinalBossWithoutCompletingWorld()
        {
            GameProgression.RescueEgg(EggType.RedEgg);yield return SceneManager.LoadSceneAsync("Level_4_4");yield return null;
            var player=UnityEngine.Object.FindAnyObjectByType<PlayerController>();player.RespawnAt(new Vector2(95f,1.5f));
            yield return Wait(()=>SceneManager.GetActiveScene().name=="Boss_Final","4-4 portal loads final boss.");
            Assert.IsTrue(GameProgression.IsEggRescued(EggType.RedEgg));Assert.IsFalse(GameProgression.IsWorldCompleted(4));Assert.AreEqual(6f,Camera.main.orthographicSize);
        }
        [UnityTest] public IEnumerator PhaseCheckpointsRestoreSafeLavaAndKeepHits()
        {
            yield return FirstHit();_player.KillAndRespawn();yield return null;Assert.AreEqual(1,_boss.Fight.Hits);Assert.AreEqual(0f,_player.Rigidbody.position.x,.3f);
            _boss.Fight.Expose();_boss.Strike();yield return null;Assert.AreEqual(2,_boss.Fight.Hits);
            _boss.OpenSteam();_boss.CrackAnchor();yield return new WaitForSeconds(8f);Assert.Greater(_boss.LavaHeight,-3f);
            _player.KillAndRespawn();yield return new WaitForFixedUpdate();Assert.AreEqual(2,_boss.Fight.Hits);Assert.IsFalse(_boss.Fight.SteamOpen);Assert.IsFalse(_boss.Fight.AnchorCracked);Assert.AreEqual(20f,_player.Rigidbody.position.x,.3f);Assert.AreEqual(-3f,_boss.LavaHeight,.01f);
        }
    }
}
