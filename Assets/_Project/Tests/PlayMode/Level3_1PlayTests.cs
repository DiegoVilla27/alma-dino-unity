using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class Level3_1PlayTests
    {
        private PlayerController _player;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput();
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_3_1");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Spawn must be safe ground.");
        }

        [TearDown]
        public void TearDown()
        {
            ClearInput();
            GameProgression.ResetProgression();
        }

        [Test]
        public void FreshEntry_PreservesEarlierAbilities_ButTeachesDashAtTheAltar()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked);
            Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsFalse(_player.IsDashUnlocked);
            Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.AreEqual("Level_3_2", GameObject.Find("Portal_Exit_To_3_2").GetComponent<LevelExit2D>().NextSceneName);
            Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Level_3_1"));
        }

        [UnityTest]
        public IEnumerator WalkingBackFromSpawn_IsBlockedByRootsWithoutFalling()
        {
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                VirtualInputBridge.MoveVector = Vector2.left;
                yield return new WaitForSeconds(2f);
                Assert.AreEqual(0, deaths);
                Assert.That(_player.Rigidbody.position.x, Is.InRange(-5f, -4f));
                Assert.IsTrue(_player.GroundDetector.IsGrounded);
                VirtualInputBridge.TriggerJump();
                yield return new WaitForSeconds(0.34f);
                VirtualInputBridge.TriggerJump();
                yield return new WaitForSeconds(1f);
                Assert.AreEqual(0, deaths);
                Assert.Greater(_player.Rigidbody.position.x, -5f, "Double Jump must not bypass the entrance boundary.");
            }
            finally
            {
                _player.OnRespawned -= onRespawn;
                ClearInput();
            }
        }

        [UnityTest]
        public IEnumerator DoubleJumpAlone_CannotCrossTutorialMud_AndRespawnKeepsDash()
        {
            yield return WalkTo(12.5f);
            Assert.IsTrue(_player.IsDashUnlocked, "Walking through the spore must unlock Dash.");
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.34f);
            VirtualInputBridge.TriggerJump();
            yield return WaitFor(() => deaths > 0, "Double Jump alone must fall into toxic mud.");
            ClearInput();
            _player.OnRespawned -= onRespawn;
            Assert.IsTrue(_player.IsDashUnlocked);
            Assert.That(_player.Rigidbody.position.x, Is.InRange(9.8f, 10.2f));
        }

        [UnityTest]
        public IEnumerator CompleteRoute_WithDoubleJumpAndDash_ReachesExitWithoutDeaths()
        {
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return WalkTo(12.5f);
                Assert.IsTrue(_player.IsDashUnlocked);
                yield return CrossGap(27f);
                yield return WalkTo(31.5f);
                yield return CrossGap(46f);
                yield return WalkTo(50.5f);
                yield return CrossGap(65f);
                yield return WalkTo(69.5f);
                yield return CrossGap(84f);
                yield return WalkTo(89f);
                var exit = GameObject.Find("Portal_Exit_To_3_2").GetComponent<LevelExit2D>();
                VirtualInputBridge.MoveVector = Vector2.right;
                yield return WaitFor(() => exit.IsCompleted, "The route must reach the exit portal.");
                Assert.AreEqual(0, deaths, "The normal route must be traversable without respawning.");
            }
            finally
            {
                _player.OnRespawned -= onRespawn;
                ClearInput();
            }
        }

        private IEnumerator CrossGap(float landing)
        {
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.34f);
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.30f);
            VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash,
                "Airborne input must start Dash.");
            yield return WalkTo(landing);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Dash must reach the next island.");
            yield return new WaitForSeconds(0.42f);
            Assert.IsTrue(_player.CanAirDash, "Landing must restore the air charge.");
        }

        private IEnumerator WalkTo(float x)
        {
            float deadline = Time.time + 6f;
            while (Mathf.Abs(_player.Rigidbody.position.x - x) > 0.2f && Time.time < deadline)
            {
                VirtualInputBridge.MoveVector = new Vector2(Mathf.Sign(x - _player.Rigidbody.position.x), 0f);
                yield return null;
            }
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x - x), 0.3f, "Cannot reach x=" + x);
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
