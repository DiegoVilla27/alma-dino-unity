using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Environment.Services;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AlmaDino.Tests.PlayMode
{
    public class Level4_1PlayTests
    {
        private PlayerController _player;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput(); GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_4_1"); yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Spawn must be safe.");
        }
        [TearDown]
        public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }
        private static void ClearInput() { VirtualInputBridge.MoveVector = Vector2.zero; VirtualInputBridge.ReleaseJump(); }
        private static IEnumerator WaitFor(Func<bool> condition, string message, float timeout = 5f)
        {
            float end = Time.time + timeout;
            while (!condition() && Time.time < end) yield return null;
            Assert.IsTrue(condition(), message);
        }
        private static PushableBoulder2D Boulder(int index) => GameObject.Find("Roar_Boulder_" + index).GetComponent<PushableBoulder2D>();
        [Test]
        public void FreshEntryTeachesRoarAndKeepsEarlierAbilitiesAndSharedCamera()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked); Assert.IsTrue(_player.IsGroundPoundUnlocked); Assert.IsTrue(_player.IsDashUnlocked);
            Assert.IsFalse(_player.IsRoarUnlocked); Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.AreEqual("Level_4_2", GameObject.Find("Portal_Exit_To_4_2").GetComponent<LevelExit2D>().NextSceneName);
            Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Level_4_1"));
            Assert.IsFalse(GameProgression.IsWorldCompleted(4));
        }
        [UnityTest]
        public IEnumerator RearBoundaryBlocksWalkingAndDoubleJumpWithoutDeaths()
        {
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            VirtualInputBridge.MoveVector = Vector2.left; yield return new WaitForSeconds(2f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f); VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(1f);
            _player.OnRespawned -= count;
            Assert.AreEqual(0, deaths); Assert.Greater(_player.Rigidbody.position.x, -5f);
        }
        [UnityTest]
        public IEnumerator WalkingAndDashCannotMoveTheHeavyBoulder()
        {
            yield return WalkTo(10f);
            var rock = Boulder(0); Vector2 original = rock.transform.position;
            VirtualInputBridge.MoveVector = Vector2.right; yield return new WaitForSeconds(1f);
            VirtualInputBridge.TriggerDash(); yield return new WaitForSeconds(.4f);
            Assert.That(Vector2.Distance(original, rock.transform.position), Is.LessThan(.01f));
            Assert.Less(_player.Rigidbody.position.x, 12.5f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.3f);
            VirtualInputBridge.TriggerDash(); yield return new WaitForSeconds(.6f);
            Assert.Less(_player.Rigidbody.position.x, 16f, "The basalt gorge must prevent climbing over the unshifted rock.");
            Assert.IsFalse(rock.IsMoving);
        }
        [UnityTest]
        public IEnumerator RoarRejectsDistantAndBackwardTargets()
        {
            yield return WalkTo(8f); Assert.IsTrue(_player.IsRoarUnlocked);
            VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.3f); Assert.IsFalse(Boulder(0).IsMoving);
            _player.RespawnAt(new Vector2(11.5f, .7f));
            VirtualInputBridge.MoveVector = Vector2.left; yield return new WaitForSeconds(.08f); VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.3f); Assert.IsFalse(Boulder(0).IsMoving);
            Assert.That(Boulder(0).transform.position.x, Is.EqualTo(14f).Within(.01f));
        }
        [UnityTest]
        public IEnumerator RoarMovesRockFiveMetersAndCreatesSafeFlatLavaSupport()
        {
            yield return PrepareFirstBridge();
            Assert.That(Boulder(0).transform.position.x, Is.EqualTo(19f).Within(.02f));
            yield return WalkTo(15f); yield return JumpTo(19f);
            Assert.IsTrue(_player.GroundDetector.IsGrounded);
            Assert.That(_player.Rigidbody.position.y, Is.EqualTo(.82f).Within(.15f));
            yield return new WaitForSeconds(1f); Assert.That(_player.Rigidbody.position.x, Is.EqualTo(19f).Within(.3f));
        }
        [UnityTest]
        public IEnumerator DoubleJumpDashCannotSkipTheUnbridgedTutorialRiver()
        {
            Boulder(0).gameObject.SetActive(false);
            _player.RespawnAt(new Vector2(15.5f, .7f));
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            yield return JumpDash(); yield return new WaitForSeconds(1f);
            _player.OnRespawned -= count; Assert.Greater(deaths, 0, "The initial 14m river must need its Roar bridge.");
        }
        [UnityTest]
        public IEnumerator DeathResetsCurrentRockAndKeepsCompletedBridgesAndRoar()
        {
            yield return PrepareFirstBridge();
            _player.RespawnAt(new Vector2(34f, .7f)); yield return new WaitForSeconds(.2f);
            _player.RespawnAt(new Vector2(43.5f, .7f)); yield return new WaitForSeconds(.15f);
            VirtualInputBridge.MoveVector = Vector2.right; yield return new WaitForFixedUpdate(); VirtualInputBridge.TriggerRoar();
            yield return WaitFor(() => Boulder(1).IsMoving, "Roar must start the second rock.");
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(Boulder(0).IsSolidified); Assert.IsFalse(Boulder(1).IsMoving); Assert.IsFalse(Boulder(1).IsSolidified);
            Assert.That(Boulder(1).transform.position.x, Is.EqualTo(46f).Within(.01f));
            Assert.IsTrue(_player.IsRoarUnlocked); GameProgression.LoadProgression(); Assert.IsTrue(GameProgression.IsAbilityUnlocked(AlmaDino.Core.Interfaces.AbilityType.Roar));
            Assert.That(_player.Rigidbody.position.x, Is.EqualTo(34f).Within(.3f));
        }
        [UnityTest]
        public IEnumerator SteamWarnsDealsDamageAndResetsToSafeWindow()
        {
            _player.RespawnAt(new Vector2(74f, .7f));
            var geyser = GameObject.Find("Steam_Geyser_74").GetComponent<LavaGeyser2D>();
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            yield return WaitFor(() => geyser.Phase == GeyserPhase.Warning, "Steam must warn before becoming dangerous.");
            Assert.IsFalse(geyser.IsDangerous);
            yield return WaitFor(() => deaths > 0, "Steam must damage Alma during eruption.");
            _player.OnRespawned -= count;
            Assert.AreEqual(GeyserPhase.Dormant, geyser.Phase); Assert.IsFalse(geyser.IsDangerous);
        }
        [UnityTest]
        public IEnumerator LavaDealsDamageDuringDash()
        {
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            _player.RespawnAt(new Vector2(25f, -.1f));
            VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerDash(); float start = Time.time;
            yield return WaitFor(() => deaths > 0, "Dash must not grant lava immunity.", .2f);
            _player.OnRespawned -= count;
            Assert.Less(Time.time - start, .2f);
        }
        [UnityTest]
        public IEnumerator CompleteRouteWithRealRoarJumpAndDashInputsReachesExitWithoutDeaths()
        {
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            try
            {
                yield return PrepareFirstBridge();
                yield return CrossBridge(15f, 19f, 20.5f, 32f);
                yield return WalkTo(43.5f); yield return RoarAt(1);
                yield return CrossBridge(47f, 51f, 52.5f, 66f);
                yield return WalkTo(71.5f); yield return PassSteam(74f);
                yield return WalkTo(79.5f); yield return PassSteam(82f);
                yield return WalkTo(85.5f); yield return RoarAt(2);
                yield return CrossBridge(89f, 93f, 94.5f, 108f);
                yield return WalkTo(118f); VirtualInputBridge.MoveVector = Vector2.right;
                var exit = GameObject.Find("Portal_Exit_To_4_2").GetComponent<LevelExit2D>();
                yield return WaitFor(() => exit.IsCompleted, "The route must reach the exit.");
                Assert.AreEqual(0, deaths); Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            }
            finally { _player.OnRespawned -= count; }
        }
        private IEnumerator PrepareFirstBridge() { yield return WalkTo(11.5f); Assert.IsTrue(_player.IsRoarUnlocked); yield return RoarAt(0); }
        private IEnumerator RoarAt(int index)
        {
            VirtualInputBridge.MoveVector = Vector2.right; yield return new WaitForFixedUpdate(); VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.TriggerRoar();
            yield return WaitFor(() => Boulder(index).IsSolidified, "Roar must move and solidify rock " + index);
            yield return new WaitForSeconds(.15f);
        }
        private IEnumerator WalkTo(float x)
        {
            float end = Time.time + 6f;
            while (Mathf.Abs(_player.Rigidbody.position.x - x) > .2f && Time.time < end)
            { VirtualInputBridge.MoveVector = Vector2.right * Mathf.Sign(x - _player.Rigidbody.position.x); yield return null; }
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x - x), .3f, "Cannot reach " + x + "; position=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(.14f);
        }
        private IEnumerator JumpTo(float x)
        {
            VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.28f);
            VirtualInputBridge.TriggerJump(); yield return WalkTo(x); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Land on bridge " + x);
            yield return new WaitForSeconds(.2f);
        }
        private IEnumerator JumpDash()
        {
            VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.3f); VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Dash must start.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.Dash, "Dash must finish.");
        }
        private IEnumerator CrossBridge(float shore, float bridge, float launch, float landing)
        {
            yield return WalkTo(shore); yield return JumpTo(bridge); yield return WalkTo(launch);
            yield return JumpDash(); yield return WalkTo(landing); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Land beyond lava."); yield return new WaitForSeconds(.2f);
        }
        private IEnumerator PassSteam(float x)
        {
            var geyser = GameObject.Find("Steam_Geyser_" + x).GetComponent<LavaGeyser2D>();
            yield return WaitFor(() => geyser.Phase != GeyserPhase.Dormant, "Wait for the current steam cycle.");
            yield return WaitFor(() => geyser.Phase == GeyserPhase.Dormant, "Cross during the full safe window.");
            yield return WalkTo(x + 2.5f);
        }
    }
}
