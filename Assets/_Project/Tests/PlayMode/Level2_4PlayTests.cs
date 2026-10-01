using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Enemies;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AlmaDino.Tests.PlayMode
{
    public class Level2_4PlayTests
    {
        private PlayerController _player;
        private CrushingCeiling2D Ceiling => GameObject.Find("Ceiling_First").GetComponent<CrushingCeiling2D>();
        [UnitySetUp] public IEnumerator SetUp()
        {
            ClearInput(); GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_2_4"); yield return null;
            _player=GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
        }
        [TearDown] public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }
        [Test] public void FreshEntry_HasVisibleAlmaRequiredAbilitiesAndLockedExit()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked); Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsFalse(_player.IsDashUnlocked); Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.IsNotNull(_player.GetComponent<PlayerSpriteAnimator>().SpriteRenderer.sprite);
            Assert.IsFalse(FindExit().gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator Ceiling_WarnsBeforeDescentAndRetractsInTwoSeconds()
        {
            _player.RespawnAt(new Vector2(9f,.7f));
            yield return WaitFor(()=>Ceiling.Phase==CeilingPhase.Warning,"Ceiling must warn.");
            Assert.Greater(Ceiling.transform.position.y,5.9f);
            yield return WaitFor(()=>Ceiling.Phase==CeilingPhase.Descending,"Ceiling must descend after warning.");
            yield return WaitFor(()=>Ceiling.Phase==CeilingPhase.Retracting,"Ceiling must retract.");
            float start=Time.time;
            yield return WaitFor(()=>Ceiling.Phase==CeilingPhase.Open,"Ceiling must reopen.");
            Assert.That(Time.time-start,Is.InRange(1.9f,2.1f));
        }
        [UnityTest] public IEnumerator ExposedPlayer_IsCrushedAndCycleRestarts()
        {
            _player.RespawnAt(new Vector2(13f,.7f));
            yield return WaitFor(()=>_player.Rigidbody.position.x<1f,"Ceiling must damage exposed Alma.");
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(CeilingPhase.Open,Ceiling.Phase);
            Assert.That(Ceiling.transform.position.y,Is.EqualTo(6f).Within(.1f));
        }
        [UnityTest] public IEnumerator UIPound_OpensSafeRefugeAndDeathRestoresIt()
        {
            _player.RespawnAt(new Vector2(13f,.7f));yield return new WaitForSeconds(.1f);
            yield return Pound(true);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < -2f,"Pound must reach the refuge.");
            yield return new WaitForSeconds(4.2f);
            Assert.That(_player.Rigidbody.position.x,Is.InRange(12.7f,13.3f));
            _player.SetCheckpoint(new Vector2(32f,.7f));_player.KillAndRespawn();yield return null;
            Assert.IsFalse(GameObject.Find("First_Refuge_Slab").GetComponent<BreakableGround2D>().IsBroken);
            Assert.That(_player.Rigidbody.position.x,Is.EqualTo(32f).Within(.2f));
        }
        [UnityTest] public IEnumerator BlueEgg_UnlocksPortalRevealsGuardianAndPersistsOnReload()
        {
            _player.RespawnAt(new Vector2(92f,2f));yield return new WaitForSeconds(.15f);
            Assert.IsTrue(GameProgression.IsEggRescued(EggType.BlueEgg));
            Assert.IsTrue(FindExit().gameObject.activeSelf);
            Assert.IsTrue(GameObject.Find("Armadillo_Guardian_Teaser").activeSelf);
            yield return SceneManager.LoadSceneAsync("Level_2_4");yield return null;
            Assert.IsTrue(FindExit().gameObject.activeSelf);
            Assert.IsTrue(GameObject.Find("Armadillo_Guardian_Teaser").activeSelf);
            Assert.AreEqual(1,GameProgression.RescuedEggCount);
        }
        [UnityTest]
        public IEnumerator MissedCatapult_CanReturnAfterGateExpiryAndRetry()
        {
            _player.SetCheckpoint(new Vector2(32f, 0.7f));
            _player.RespawnAt(new Vector2(50f, 0.7f));
            yield return new WaitForSeconds(4.2f);
            var gate = GameObject.Find("Gate_Pressure").GetComponent<TimedRuneGate2D>();
            Assert.IsTrue(gate.IsOpen, "The lower chamber must allow a return after the timer expires.");
            yield return WalkTo(43.5f);
            yield return WaitFor(() => !gate.IsOpen, "The gate must close again after Alma returns.");
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Alma should settle onto the recovery ramp.");
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.16f);
            yield return WalkTo(37.7f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 1.3f,
                "Alma must be able to reach the marked end again.");
            VirtualInputBridge.ReleaseJump();
            yield return new WaitForSeconds(0.04f);
            yield return Pound(false);
            yield return WalkTo(49f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 6f,
                "The retry must catapult Alma onto the intended upper route.");
        }

        [UnityTest] public IEnumerator CompleteRoute_WithInputs_RescuesBlueEggAndReachesExit()
        {
            yield return WalkTo(13f);yield return Pound(true);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < -2f,"Reach first refuge.");
            yield return WalkTo(19f);yield return JumpAcross(21f);yield return JumpAcross(25f);yield return WalkTo(32f);
            yield return WalkTo(36.2f);VirtualInputBridge.TriggerJump();yield return WalkTo(37.7f);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y>1.3f,"Climb pressure seesaw.");
            VirtualInputBridge.ReleaseJump();yield return new WaitForSeconds(.04f);yield return Pound(false);
            yield return WalkTo(49f);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y>6f,"Catapult must reach pressure ledge.");
            yield return WalkTo(57.1f);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y<1f,"Drop under the beetle roof.");
            yield return WalkTo(59.8f);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Land before beetle.");
            var beetle=GameObject.Find("Beetle_Emergency").GetComponent<CrystalBeetle2D>();
            yield return WaitFor(()=>beetle.transform.position.x<61.25f,"Beetle should enter shock range.");
            yield return Pound(false);
            Assert.IsTrue(beetle.IsFlipped,"Shock range: Alma="+_player.Rigidbody.position+" beetle="+beetle.transform.position);
            VirtualInputBridge.TriggerJump(); yield return WalkTo(61.3f);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y>1.1f,"Use exposed belly as an emergency platform.");
            VirtualInputBridge.ReleaseJump();
            yield return JumpAcross(66f);yield return WalkTo(68f);yield return WalkTo(76f);yield return Pound(false);
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < -2f,"Reach final refuge.");
            yield return WalkTo(82f);yield return JumpAcross(84f);yield return JumpAcross(88f);yield return JumpAcross(92f);
            yield return WaitFor(()=>GameProgression.IsEggRescued(EggType.BlueEgg),"Route must rescue Blue Egg.");
            VirtualInputBridge.MoveVector=Vector2.right;
            yield return WaitFor(()=>FindExit().IsCompleted,"Route must reach boss exit.");
        }
        private static LevelExit2D FindExit() => UnityEngine.Object.FindObjectsByType<LevelExit2D>(FindObjectsInactive.Include,FindObjectsSortMode.None)[0];
        private IEnumerator Pound(bool useUI)
        {
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(0.16f); VirtualInputBridge.ReleaseJump();
            if (useUI) ExecuteEvents.Execute(GameObject.Find("Button_GroundPound"),new PointerEventData(EventSystem.current),ExecuteEvents.pointerDownHandler);
            else VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.GroundPound, "Airborne pound should begin.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.GroundPound, "Pound should reach physical ground.");
        }
        private IEnumerator JumpAcross(float x)
        {
            VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.28f); VirtualInputBridge.TriggerJump();
            yield return WalkTo(x); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Jump should reach a safe surface at " + x);
        }
        private IEnumerator WalkTo(float x)
        {
            float deadline = Time.time + 6f;
            while (Mathf.Abs(_player.Rigidbody.position.x-x)>0.2f && Time.time<deadline)
            {
                VirtualInputBridge.MoveVector = new Vector2(Mathf.Sign(x-_player.Rigidbody.position.x),0f);
                yield return null;
            }
            VirtualInputBridge.MoveVector=Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x-x),0.3f,"Cannot reach "+x+"; position="+_player.Rigidbody.position);
            yield return new WaitForSeconds(0.1f);
        }
        private static IEnumerator WaitFor(Func<bool> predicate,string message)
        {
            float deadline=Time.time+6f;
            while (!predicate() && Time.time<deadline) yield return null;
            Assert.IsTrue(predicate(),message);
        }
        private static void ClearInput() { VirtualInputBridge.MoveVector=Vector2.zero;VirtualInputBridge.JumpHeld=false;VirtualInputBridge.ConsumeFrameTriggers(); }
    }
}
