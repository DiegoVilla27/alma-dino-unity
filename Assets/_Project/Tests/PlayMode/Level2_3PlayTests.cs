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
    public class Level2_3PlayTests
    {
        private PlayerController _player;
        private CrystalBeetle2D Beetle => GameObject.Find("Beetle_Tutorial").GetComponent<CrystalBeetle2D>();
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput(); GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_2_3");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
        }
        [TearDown] public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }

        [Test]
        public void FreshEntry_HasOnlyCaveAbilitiesAndVisibleAlma()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked); Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsFalse(_player.IsDashUnlocked); Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.IsNotNull(_player.GetComponent<PlayerSpriteAnimator>().SpriteRenderer.sprite);
            Assert.IsNotNull(GameObject.Find("Button_GroundPound"));
            Assert.AreEqual("Level_2_4", GameObject.Find("Portal_Exit_To_2_4").GetComponent<LevelExit2D>().NextSceneName);
        }

        [UnityTest]
        public IEnumerator NormalStomp_OnShell_RespawnsAlma()
        {
            _player.RespawnAt(new Vector2(12f, 2f));
            yield return WaitFor(() => _player.Rigidbody.position.x < 1f, "An ordinary landing on the shell should cause damage.");
            Assert.IsFalse(Beetle.IsFlipped);
        }

        [UnityTest]
        public IEnumerator UISeismicShock_FlipsNearbyBeetleForThreePointFiveSeconds()
        {
            _player.RespawnAt(new Vector2(10.5f, 0.7f));
            yield return new WaitForSeconds(0.1f);
            yield return Pound(true);
            Assert.IsTrue(Beetle.IsFlipped);
            Assert.IsFalse(Beetle.IsDangerous);
            Assert.That(Beetle.FlipTimeRemaining, Is.InRange(3.3f, 3.5f));
            yield return new WaitForSeconds(3.6f);
            Assert.IsFalse(Beetle.IsFlipped);
            Assert.IsTrue(Beetle.IsDangerous);
        }

        [UnityTest]
        public IEnumerator FlippedBeetle_IsSafeUntilItsShellRecovers()
        {
            _player.RespawnAt(new Vector2(10.5f, 0.7f));
            yield return new WaitForSeconds(0.1f);
            yield return Pound(false);
            yield return new WaitForSeconds(0.3f);
            _player.Rigidbody.position = (Vector2)Beetle.transform.position + Vector2.up * 1.05f;
            _player.transform.position = _player.Rigidbody.position;
            _player.Rigidbody.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(Beetle.IsFlipped);
            Assert.Greater(_player.Rigidbody.position.x, 10f, "The exposed belly should be safe to stand on.");
            yield return WaitFor(() => _player.Rigidbody.position.x < 1f,
                "Remaining on the recovered shell should cause damage.");
        }

        [UnityTest]
        public IEnumerator FlyingBat_ContactRespawnsAlmaAtTheCheckpoint()
        {
            _player.RespawnAt(new Vector2(63.5f, 3.4f));
            _player.SetCheckpoint(new Vector2(58f, 2.2f));
            var bat = GameObject.Find("Bat_Entrance").GetComponent<CaveBat2D>();
            yield return WaitFor(() => bat.Phase == CaveBatPhase.Flying, "The bat should begin its warned flight.");
            _player.Rigidbody.position = bat.GetComponent<Rigidbody2D>().position;
            _player.transform.position = _player.Rigidbody.position;
            _player.Rigidbody.linearVelocity = Vector2.zero;
            yield return WaitFor(() => Mathf.Abs(_player.Rigidbody.position.x - 58f) < 0.2f,
                "Contact during flight should respawn Alma at the checkpoint.");
        }

        [UnityTest]
        public IEnumerator FragileFloor_ProducesSafeCatchAndFlippedBridge_AndResetsOnDeath()
        {
            _player.RespawnAt(new Vector2(40.5f, 4.3f));
            yield return new WaitForSeconds(0.1f);
            var slab = GameObject.Find("Cracked_Beetle_Floor").GetComponent<BreakableGround2D>();
            yield return Pound(false);
            var beetle = GameObject.Find("Beetle_Fragile_Puzzle").GetComponent<CrystalBeetle2D>();
            yield return WaitFor(() => slab.IsBroken && _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < 1f,
                "The pound should break the slab and land on the recovery shelf.");
            Assert.IsTrue(beetle.IsFlipped);
            _player.SetCheckpoint(new Vector2(26f, 0.7f)); _player.KillAndRespawn();
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(slab.IsBroken); Assert.IsFalse(beetle.IsFlipped);
            Assert.That(_player.Rigidbody.position.x, Is.InRange(25.8f,26.2f));
        }

        [UnityTest]
        public IEnumerator Bat_WarnsThenFliesAndReturnsToItsRoost()
        {
            _player.RespawnAt(new Vector2(63.5f, 3.4f));
            var bat = GameObject.Find("Bat_Entrance").GetComponent<CaveBat2D>();
            yield return WaitFor(() => bat.Phase == CaveBatPhase.Warning, "The bat should warn before diving.");
            Assert.IsFalse(bat.IsDangerous);
            yield return WaitFor(() => bat.Phase == CaveBatPhase.Flying, "The warned bat should fly.");
            Assert.IsTrue(bat.IsDangerous);
            yield return WaitFor(() => bat.Phase == CaveBatPhase.Resting, "The bat should return and leave a safe window.");
            Assert.That(bat.transform.position.y, Is.EqualTo(6.8f).Within(0.1f));
            _player.SetCheckpoint(new Vector2(58f,2.2f)); _player.KillAndRespawn();
            yield return new WaitForSeconds(0.1f); Assert.AreEqual(CaveBatPhase.Sleeping,bat.Phase);
        }

        [UnityTest]
        public IEnumerator CompleteRoute_WithPlayerInputs_ReachesTheExit()
        {
            yield return WalkTo(9.8f);
            yield return WaitFor(() => Beetle.transform.position.x < 11.65f, "Wait for the beetle to enter seismic range.");
            yield return Pound(true);
            Assert.IsTrue(Beetle.IsFlipped);
            yield return JumpAcross(16f);
            yield return WalkTo(19f);
            var second = GameObject.Find("Beetle_Gallery").GetComponent<CrystalBeetle2D>();
            yield return WaitFor(() => second.transform.position.x < 20.7f, "The second beetle should approach.");
            yield return Pound(false); Assert.IsTrue(second.IsFlipped);
            yield return JumpAcross(26f);
            yield return WalkTo(27.4f);
            yield return JumpAcross(30.5f);
            yield return JumpAcross(34.5f);
            yield return JumpAcross(38f);
            yield return WalkTo(40.5f);
            yield return Pound(false);
            var bridge = GameObject.Find("Beetle_Fragile_Puzzle").GetComponent<CrystalBeetle2D>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < 1f,
                "Pound should reach the recovery shelf.");
            Assert.IsTrue(bridge.IsFlipped, "The floor impact must flip the bridge beetle; Alma=" + _player.Rigidbody.position + "; beetle=" + bridge.transform.position);
            yield return WaitFor(() => bridge.transform.position.y < 1f, "The flipped beetle should settle on the perch.");
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return WalkTo(41.9f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 1.3f,
                "A normal hop should land on the exposed belly.");
            VirtualInputBridge.ReleaseJump();
            Assert.IsTrue(bridge.IsFlipped, "Alma must reach the beetle before recovery; Alma="+_player.Rigidbody.position+"; remaining="+bridge.FlipTimeRemaining);
            yield return LowJumpUnderSeal();
            yield return WalkTo(58f);
            yield return JumpAcross(63.5f);
            yield return WaitForBat("Bat_Entrance");
            yield return JumpAcross(69f);
            yield return WalkTo(70.5f);
            yield return WaitForBat("Bat_Middle");
            yield return JumpAcross(75.5f);
            yield return WalkTo(77.2f);
            yield return WaitForBat("Bat_Upper");
            yield return JumpAcross(82f);
            yield return JumpAcross(89f);
            VirtualInputBridge.MoveVector = Vector2.right;
            var exit = GameObject.Find("Portal_Exit_To_2_4").GetComponent<LevelExit2D>();
            yield return WaitFor(() => exit.IsCompleted, "The full route should reach the exit.");
        }

        private IEnumerator WaitForBat(string name)
        {
            var bat = GameObject.Find(name).GetComponent<CaveBat2D>();
            yield return WaitFor(() => bat.Phase == CaveBatPhase.Resting, "Wait for the telegraphed bat to finish its flight: " + name);
        }
        private IEnumerator LowJumpUnderSeal()
        {
            VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.06f); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.Rigidbody.position.x > 44.8f, "A short hop should pass under the upper seal.");
            VirtualInputBridge.TriggerJump();
            yield return WalkTo(48f); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 2f, "The double jump should reach the puzzle exit.");
        }
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
