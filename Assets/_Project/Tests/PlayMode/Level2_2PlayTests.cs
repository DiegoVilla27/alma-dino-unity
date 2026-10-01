using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.MobileUI.Controllers;
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
    public class Level2_2PlayTests
    {
        private PlayerController _player;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput();
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_2_2");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            ClearInput();
            GameProgression.ResetProgression();
        }

        [Test]
        public void FreshEntry_HasRequiredAbilitiesAndAVisibleConnectedHUD()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked);
            Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsFalse(_player.IsDashUnlocked);
            Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.IsNotNull(_player.GetComponent<PlayerSpriteAnimator>().SpriteRenderer.sprite);
            Assert.AreEqual(VirtualButtonType.GroundPound,
                GameObject.Find("Button_GroundPound").GetComponent<VirtualTouchButton>().ButtonType);
            Assert.AreEqual("Level_2_3", GameObject.Find("Portal_Exit_To_2_3").GetComponent<LevelExit2D>().NextSceneName);
        }

        [UnityTest]
        public IEnumerator NormalLanding_TiltsButDoesNotLaunchTheCounterweight()
        {
            _player.RespawnAt(new Vector2(7.7f, 3.5f));
            yield return new WaitForSeconds(0.7f);
            var seesaw = GameObject.Find("Seesaw_Tutorial").GetComponent<SeesawPlatform2D>();
            Assert.Greater(seesaw.CurrentAngle, 1f);
            Assert.IsFalse(GameObject.Find("Seesaw_Tutorial_Counterweight").GetComponent<CatapultWeight2D>().IsLaunched);
            Assert.IsFalse(GameObject.Find("Gate_Tutorial").GetComponent<TimedRuneGate2D>().IsOpen);
        }

        [UnityTest]
        public IEnumerator UIPound_LaunchesWeight_OpensGateForFourSeconds_AndAllowsRetry()
        {
            yield return StandOn(7.7f);
            yield return JumpAndPound(true);
            var weight = GameObject.Find("Seesaw_Tutorial_Counterweight").GetComponent<CatapultWeight2D>();
            var rune = GameObject.Find("Seesaw_Tutorial_Ceiling_Rune").GetComponent<RuneSwitch2D>();
            var gate = GameObject.Find("Gate_Tutorial").GetComponent<TimedRuneGate2D>();
            yield return WaitFor(() => gate.IsOpen, "Counterweight should hit the ceiling rune and open the gate.");
            Assert.IsTrue(weight.IsLaunched);
            Assert.That(rune.RemainingTime, Is.InRange(3.8f, 4f));
            yield return new WaitForSeconds(4.1f);
            Assert.IsFalse(gate.IsOpen);
            yield return WaitFor(() => !weight.IsLaunched, "The counterweight should reset for another attempt.");
            yield return StandOn(7.7f);
            yield return JumpAndPound(true);
            yield return WaitFor(() => gate.IsOpen, "The mechanism should be repeatable.");
        }

        [UnityTest]
        public IEnumerator GateExpiry_WaitsUntilAlmaClearsTheDoorway()
        {
            yield return StandOn(7.7f);
            yield return JumpAndPound(false);
            var gate = GameObject.Find("Gate_Tutorial").GetComponent<TimedRuneGate2D>();
            yield return WaitFor(() => gate.IsOpen, "The rune should open the gate.");
            _player.Rigidbody.position = new Vector2(20f, 0.7f);
            _player.transform.position = _player.Rigidbody.position;
            _player.Rigidbody.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(4.1f);
            Assert.IsTrue(gate.IsOpen, "Closing must wait while Alma occupies the doorway.");
            _player.Rigidbody.position = new Vector2(23f, 0.7f);
            _player.transform.position = _player.Rigidbody.position;
            yield return new WaitForSeconds(0.08f);
            Assert.IsFalse(gate.IsOpen, "The expired gate must close once the doorway is clear.");
        }

        [UnityTest]
        public IEnumerator Catapult_ReachesTheHighLedgeAndRespawnRestoresMechanisms()
        {
            yield return StandOn(37.7f);
            yield return JumpAndPound(false);
            yield return WalkTo(49f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 6f,
                "Running to the raised end should catapult Alma onto the high ledge.");
            _player.SetCheckpoint(new Vector2(30f, 0.7f));
            _player.KillAndRespawn();
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(GameObject.Find("Gate_High_Ledge").GetComponent<TimedRuneGate2D>().IsOpen);
            Assert.IsFalse(GameObject.Find("Seesaw_High_Ledge_Counterweight").GetComponent<CatapultWeight2D>().IsLaunched);
            Assert.IsTrue(_player.IsGroundPoundUnlocked);
        }

        [UnityTest]
        public IEnumerator LinkedGate_RequiresBothRunesAndResetsOnDeath()
        {
            yield return StandOn(69.7f);
            yield return JumpAndPound(false);
            var first = GameObject.Find("Seesaw_Chain_First_Ceiling_Rune").GetComponent<RuneSwitch2D>();
            var gate = GameObject.Find("Gate_Linked_Runes").GetComponent<TimedRuneGate2D>();
            yield return WaitFor(() => first.IsActive, "The first linked rune should activate.");
            Assert.IsFalse(gate.IsOpen, "One rune must not open the linked gate.");
            yield return WalkTo(79.7f);
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => gate.IsOpen, "Both linked runes should open the final gate.");
            _player.KillAndRespawn();
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(gate.IsOpen);
            Assert.IsFalse(first.IsActive);
        }

        [UnityTest]
        public IEnumerator CompleteRoute_WithPlayerInputs_ReachesTheExit()
        {
            yield return ClimbAndPound(7.7f);
            yield return WalkTo(30f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "First puzzle should lead to the checkpoint.");
            yield return ClimbAndPound(37.7f);
            yield return WalkTo(49f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 6f,
                "Catapult must reach the high ledge during the full route.");
            yield return WalkTo(65f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "The deep-gallery checkpoint should be reachable.");
            yield return ClimbAndPound(69.7f);
            yield return WalkTo(79.7f);
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => GameObject.Find("Seesaw_Chain_Second_Counterweight").GetComponent<CatapultWeight2D>().IsLaunched,
                "The catapult from the first seesaw should allow an airborne pound onto the second.");
            yield return WalkTo(94.5f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Alma should land beyond the linked gate.");
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.28f);
            VirtualInputBridge.TriggerJump();
            yield return WalkTo(103f);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Double jump should cross the final spike gap.");
            VirtualInputBridge.MoveVector = Vector2.right;
            var exit = GameObject.Find("Portal_Exit_To_2_3").GetComponent<LevelExit2D>();
            yield return WaitFor(() => exit.IsCompleted, "The full route should reach the exit portal.");
            VirtualInputBridge.MoveVector = Vector2.zero;
        }

        private IEnumerator StandOn(float x)
        {
            _player.RespawnAt(new Vector2(x, 2.3f));
            yield return new WaitForSeconds(0.3f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Alma should stand on the marked seesaw end.");
        }

        private IEnumerator ClimbAndPound(float x)
        {
            yield return WalkTo(x - 1.5f);
            VirtualInputBridge.TriggerJump();
            yield return WalkTo(x);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 1.3f,
                "A normal jump should reach the seesaw's marked end.");
            VirtualInputBridge.ReleaseJump();
            yield return new WaitForSeconds(0.04f);
            yield return JumpAndPound(false);
        }

        private IEnumerator JumpAndPound(bool useUI)
        {
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.16f);
            VirtualInputBridge.ReleaseJump();
            if (useUI)
            {
                var button = GameObject.Find("Button_GroundPound");
                ExecuteEvents.Execute(button, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
            }
            else VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.GroundPound,
                "Airborne pound should start.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.GroundPound,
                "Pound should impact the seesaw.");
        }

        private IEnumerator WalkTo(float x)
        {
            float deadline = Time.time + 7f;
            while (Mathf.Abs(_player.Rigidbody.position.x - x) > 0.2f && Time.time < deadline)
            {
                VirtualInputBridge.MoveVector = new Vector2(Mathf.Sign(x - _player.Rigidbody.position.x), 0f);
                yield return null;
                Assert.IsNotNull(_player.GetComponent<PlayerSpriteAnimator>().SpriteRenderer.sprite);
            }
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x - x), 0.3f, "Cannot reach x=" + x + "; position=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(0.14f);
        }

        private static IEnumerator WaitFor(Func<bool> condition, string message)
        {
            float deadline = Time.time + 5f;
            while (!condition() && Time.time < deadline) yield return null;
            Assert.IsTrue(condition(), message);
        }

        private static void ClearInput()
        {
            VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.JumpHeld = false;
            VirtualInputBridge.ConsumeFrameTriggers();
        }
    }
}
