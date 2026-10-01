using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AlmaDino.Features.MobileUI.Controllers;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class Level2_1PlayTests
    {
        private PlayerController _player;
        private BreakableGround2D _tutorial;
        private BreakableGround2D _upper;
        private BreakableGround2D _lower;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput();
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_2_1");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            _tutorial = GameObject.Find("Cracked_Floor_Tutorial").GetComponent<BreakableGround2D>();
            _upper = GameObject.Find("Cracked_Floor_Chain_Upper").GetComponent<BreakableGround2D>();
            _lower = GameObject.Find("Cracked_Floor_Chain_Lower").GetComponent<BreakableGround2D>();
        }

        [TearDown]
        public void TearDown()
        {
            ClearInput();
            GameProgression.ResetProgression();
        }

        [Test]
        public void FreshEntry_HasOnlyDoubleJump_AndExitPointsToTheNextLevel()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked);
            Assert.IsFalse(_player.IsGroundPoundUnlocked);
            Assert.IsFalse(_player.IsDashUnlocked);
            Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.AreEqual("Level_2_2", GameObject.Find("Portal_Exit_To_2_2").GetComponent<LevelExit2D>().NextSceneName);
        }

        [UnityTest]
        public IEnumerator AnimationFrames_KeepAlmaVisibleWhileStandingRunningAndFalling()
        {
            var animator = _player.GetComponent<PlayerSpriteAnimator>();
            foreach (var frames in new[] { animator.IdleFrames, animator.RunFrames, animator.JumpFrames, animator.FallFrames })
            {
                Assert.IsNotEmpty(frames);
                foreach (var frame in frames) Assert.IsNotNull(frame, "Alma must have valid animation references.");
            }
            yield return new WaitForSeconds(0.15f);
            Assert.IsNotNull(animator.SpriteRenderer.sprite);
            VirtualInputBridge.MoveVector = Vector2.right;
            for (int i = 0; i < 100; i++)
            {
                yield return null;
                Assert.IsNotNull(animator.SpriteRenderer.sprite, "Animation must never hide Alma.");
                Assert.IsTrue(animator.SpriteRenderer.enabled);
                Assert.Greater(animator.SpriteRenderer.color.a, 0f);
                Assert.IsTrue(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main),
                    animator.SpriteRenderer.bounds), "Alma must remain inside the camera view.");
            }
            VirtualInputBridge.MoveVector = Vector2.zero;
        }

        [UnityTest]
        public IEnumerator PoundUIButton_AfterUnlock_StartsPoundAndBreaksTheFloor()
        {
            yield return CollectAltar();
            var button = GameObject.Find("Button_GroundPound");
            Assert.IsNotNull(button);
            Assert.AreEqual(VirtualButtonType.GroundPound, button.GetComponent<VirtualTouchButton>().ButtonType);
            _player.RespawnAt(new Vector2(4.5f, 5.25f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Player should stand on the tutorial slab.");
            yield return null;
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current);
            pointer.position = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.IsNotEmpty(hits, "UI button must receive pointer raycasts.");
            Assert.AreEqual(button, ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject));
            ExecuteEvents.Execute(button, pointer, ExecuteEvents.pointerDownHandler);
            yield return new WaitForSeconds(0.08f);
            Assert.AreNotEqual(PlayerStateEnum.GroundPound, _player.StateMachine.CurrentStateType,
                "Pound still requires being airborne.");
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.16f);
            VirtualInputBridge.ReleaseJump();
            ExecuteEvents.Execute(button, pointer, ExecuteEvents.pointerDownHandler);
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.GroundPound,
                "UI pointer press should activate pound while airborne.");
            ExecuteEvents.Execute(button, pointer, ExecuteEvents.pointerUpHandler);
            yield return WaitFor(() => _tutorial.IsBroken, "The UI-triggered pound must break the tutorial floor.");
        }

        [UnityTest]
        public IEnumerator NormalFall_WithLockedPound_LeavesTutorialFloorIntact()
        {
            _player.RespawnAt(new Vector2(4.5f, 11f));
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < 6f,
                "Normal fall should land on the intact tutorial floor.");
            Assert.IsFalse(_tutorial.IsBroken);
            Assert.IsFalse(_player.IsGroundPoundUnlocked);
        }

        [UnityTest]
        public IEnumerator AltarUnlocksPound_AndFirstDescentHasASafeLanding()
        {
            yield return CollectAltar();
            _player.RespawnAt(new Vector2(4.5f, 5.25f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Player should stand on the tutorial slab.");
            yield return JumpAndPound();
            yield return WaitFor(() => _tutorial.IsBroken && _player.GroundDetector.IsGrounded
                && _player.Rigidbody.position.y < 0f, "Pound should break the slab and reach safe ground.");
            Assert.That(_player.Rigidbody.position.y, Is.InRange(-2.1f, -1.7f));
        }

        [UnityTest]
        public IEnumerator ChainedDescent_AndRespawn_RestoreFloorsWithoutLosingTheAbility()
        {
            yield return CollectAltar();
            _player.RespawnAt(new Vector2(28f, -7.3f));
            yield return new WaitForSeconds(0.15f);
            _player.RespawnAt(new Vector2(44f, -6.45f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Player should land on the upper slab.");
            yield return JumpAndPound();
            yield return WaitFor(() => _upper.IsBroken && _lower.IsBroken && _player.GroundDetector.IsGrounded
                && _player.Rigidbody.position.y < -13f, "A single pound should break both slabs.");
            _player.KillAndRespawn();
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(_upper.IsBroken);
            Assert.IsFalse(_lower.IsBroken);
            Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.That(_player.Rigidbody.position.x, Is.InRange(27.8f, 28.2f));
            Assert.That(_player.Rigidbody.position.y, Is.InRange(-7.5f, -7.1f));
        }

        [UnityTest]
        public IEnumerator CompleteRoute_WithPlayerInputs_ReachesTheExit()
        {
            yield return WalkTo(4.5f);
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < 6f,
                "Initial shaft should lead to the tutorial chamber.");
            yield return WalkTo(0.5f);
            yield return WaitFor(() => _player.IsGroundPoundUnlocked, "The geode should unlock ground pound.");
            yield return WalkTo(4.5f);
            yield return JumpAndPound();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < 0f,
                "The first descent should end on safe ground.");
            yield return WalkTo(20f);
            yield return JumpAndPound();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < -7f,
                "The practice descent should reach the deep gallery.");
            yield return WalkTo(31.7f);
            yield return JumpAcross(39f);
            yield return WalkTo(44f);
            yield return JumpAndPound();
            yield return WaitFor(() => _upper.IsBroken && _lower.IsBroken && _player.GroundDetector.IsGrounded
                && _player.Rigidbody.position.y < -13f, "The final chain should land on its safe island.");
            yield return WalkTo(47.4f);
            yield return JumpAcross(54f);
            var exit = GameObject.Find("Portal_Exit_To_2_2").GetComponent<LevelExit2D>();
            VirtualInputBridge.MoveVector = Vector2.right;
            yield return WaitFor(() => exit.IsCompleted, "Entering the portal should complete the level.");
            VirtualInputBridge.MoveVector = Vector2.zero;
        }

        private IEnumerator CollectAltar()
        {
            _player.RespawnAt(new Vector2(0.5f, 5.25f));
            yield return WaitFor(() => _player.IsGroundPoundUnlocked, "Touching the altar should unlock the pound.");
        }

        private IEnumerator JumpAndPound()
        {
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.16f);
            VirtualInputBridge.ReleaseJump();
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.GroundPound,
                "Airborne pound input should start the action.");
        }

        private IEnumerator JumpAcross(float destination)
        {
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.28f);
            VirtualInputBridge.TriggerJump();
            yield return WalkTo(destination);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Jump should reach the next safe ledge.");
        }

        private IEnumerator WalkTo(float destination)
        {
            float deadline = Time.time + 5f;
            while (Mathf.Abs(_player.Rigidbody.position.x - destination) > 0.2f && Time.time < deadline)
            {
                VirtualInputBridge.MoveVector = new Vector2(Mathf.Sign(destination - _player.Rigidbody.position.x), 0f);
                yield return null;
            }
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x - destination), 0.3f,
                "Cannot reach x=" + destination + "; position=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(0.14f);
        }

        private static IEnumerator WaitFor(Func<bool> condition, string message)
        {
            float deadline = Time.time + 4f;
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
