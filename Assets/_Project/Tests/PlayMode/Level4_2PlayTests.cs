using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Environment.Controllers;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AlmaDino.Tests.PlayMode
{
    public class Level4_2PlayTests
    {
        private PlayerController _player;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput(); GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_4_2"); yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Spawn must be on solid ground.");
        }
        [TearDown]
        public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }
        private static void ClearInput() { VirtualInputBridge.MoveVector = Vector2.zero; VirtualInputBridge.ReleaseJump(); }
        private static ResonanceBell2D Bell(int i) => GameObject.Find("Resonance_Bell_" + i).GetComponent<ResonanceBell2D>();
        private static BellFlameDoor2D Door(int i) => GameObject.Find("Bell_Flame_Door_" + i).GetComponent<BellFlameDoor2D>();
        private static IEnumerator WaitFor(Func<bool> condition, string message, float timeout = 6f)
        {
            float end = Time.time + timeout;
            while (!condition() && Time.time < end) yield return null;
            Assert.IsTrue(condition(), message);
        }
        [Test]
        public void DirectEntryKeepsAllFourAbilitiesAndCameraSizeSix()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked); Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsTrue(_player.IsDashUnlocked); Assert.IsTrue(_player.IsRoarUnlocked); Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.AreEqual("Level_4_3", GameObject.Find("Portal_Exit_To_4_3").GetComponent<LevelExit2D>().NextSceneName);
            Assert.IsFalse(GameProgression.IsWorldCompleted(4)); Assert.IsTrue(Door(0).IsDangerous);
        }
        [UnityTest]
        public IEnumerator RearWallBlocksWalkingAndDoubleJump()
        {
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            VirtualInputBridge.MoveVector = Vector2.left; yield return new WaitForSeconds(2f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f); VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(1f);
            _player.OnRespawned -= count; Assert.AreEqual(0, deaths); Assert.Greater(_player.Rigidbody.position.x, -5f);
        }
        [UnityTest]
        public IEnumerator DistantRoarActivatesTriggerBellAndOpensLinkedDoorForFiveSeconds()
        {
            yield return WalkTo(6f); yield return Ring(0);
            Assert.Greater(Vector2.Distance(_player.Rigidbody.position, Bell(0).transform.position), 3f);
            Assert.IsFalse(Door(0).IsDangerous); Assert.That(Bell(0).Remaining, Is.InRange(4.7f, 5f));
            yield return new WaitForSeconds(5.1f); Assert.IsFalse(Bell(0).IsOpen); Assert.IsTrue(Door(0).IsDangerous);
        }
        [UnityTest]
        public IEnumerator WrongDirectionAndOutOfRangeRoarsDoNotRingBell()
        {
            VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.3f); Assert.AreEqual(0, Bell(0).RingCount);
            yield return WalkTo(6f);
            VirtualInputBridge.MoveVector = Vector2.left; yield return new WaitForSeconds(.06f); VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.3f); Assert.AreEqual(0, Bell(0).RingCount);
            yield return Ring(0); Assert.AreEqual(1, Bell(0).RingCount);
        }
        [UnityTest]
        public IEnumerator HighBellRejectsGroundRoarAndAcceptsActualDoubleJumpRoar()
        {
            _player.RespawnAt(new Vector2(40f, .7f)); yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Stand at the aerial launch point.");
            yield return FaceRight(); VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.3f);
            Assert.IsFalse(Bell(1).IsOpen); yield return AirRing(); Assert.IsFalse(_player.GroundDetector.IsGrounded);
            Assert.IsTrue(Bell(1).IsOpen); Assert.IsFalse(Door(1).IsDangerous);
        }
        [UnityTest]
        public IEnumerator AerialBellAllowsEarlyAndLateRoarsWithReleasedJump()
        {
            foreach (float roarDelay in new[] { .08f, .2f, .35f })
            {
                VirtualInputBridge.ReleaseJump();
                _player.RespawnAt(new Vector2(40f, .7f));
                yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Reset the aerial launch.");
                yield return FaceRight();
                VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f);
                VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.04f);
                VirtualInputBridge.ReleaseJump(); yield return new WaitForSeconds(roarDelay);
                VirtualInputBridge.TriggerRoar();
                yield return WaitFor(() => Bell(1).IsOpen, "Released jump must allow Roar after " + roarDelay + "s.");
                Assert.IsFalse(Door(1).IsDangerous);
                yield return new WaitForSeconds(.3f);
            }
        }
        [UnityTest]
        public IEnumerator TouchingAndPoundingTheBellDoNotActivateIt()
        {
            _player.RespawnAt(new Vector2(45.5f, 6.5f)); yield return new WaitForSeconds(.1f);
            Assert.AreEqual(0, Bell(1).RingCount); VirtualInputBridge.TriggerGroundPound(); yield return new WaitForSeconds(.4f);
            Assert.AreEqual(0, Bell(1).RingCount); Assert.IsTrue(Door(1).IsDangerous);
        }
        [UnityTest]
        public IEnumerator FlamesReigniteAndDamagePlayerWhenCountdownExpires()
        {
            _player.RespawnAt(new Vector2(6f, .7f)); yield return Ring(0);
            _player.GetComponent<Rigidbody2D>().position = new Vector2(16f, .7f);
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            yield return WaitFor(() => deaths > 0, "Expired flame door must damage a player still inside it.");
            _player.OnRespawned -= count; Assert.IsTrue(Door(0).IsDangerous);
        }
        [UnityTest]
        public IEnumerator DashDoesNotProtectFromActiveFlames()
        {
            _player.RespawnAt(new Vector2(14.5f, .7f)); yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Stand before the closed fire door.");
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => deaths > 0, "Dash must not bypass active fire.", .5f);
            _player.OnRespawned -= count; Assert.AreEqual(0, Bell(0).RingCount);
        }
        [UnityTest]
        public IEnumerator DeathCancelsBellWindowsAndRespawnsAtSafeChainCheckpoint()
        {
            _player.RespawnAt(new Vector2(62f, .7f)); yield return new WaitForSeconds(.2f); yield return Ring(2);
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(Bell(2).IsOpen); Assert.IsTrue(Door(2).IsDangerous);
            Assert.That(_player.Rigidbody.position.x, Is.EqualTo(62f).Within(.2f)); Assert.IsTrue(_player.IsRoarUnlocked);
        }
        [UnityTest]
        public IEnumerator Level41ExitLoadsLevel42()
        {
            yield return SceneManager.LoadSceneAsync("Level_4_1"); yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>(); _player.RespawnAt(new Vector2(120f, 1.5f));
            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Level_4_2", "The 4-1 exit must load the new level.");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<ResonanceBell2D>());
        }
        [UnityTest]
        public IEnumerator CompleteRouteUsesAerialRoarAndThreeBellChainWithoutDeaths()
        {
            int deaths = 0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            try
            {
                yield return WalkTo(6f); yield return Ring(0); yield return CrossGap(19.5f, 30f);
                yield return WalkTo(40f); yield return AirRing(); VirtualInputBridge.ReleaseJump();
                yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Land after ringing the high bell.");
                yield return CrossGap(49.5f, 62f);
                yield return Ring(2); yield return CrossGap(69.5f, 79.5f);
                yield return Ring(3); yield return CrossGap(83.5f, 93.5f);
                yield return Ring(4); yield return CrossGap(97.5f, 108f);
                yield return WalkTo(116f); VirtualInputBridge.MoveVector = Vector2.right;
                var exit = GameObject.Find("Portal_Exit_To_4_3").GetComponent<LevelExit2D>();
                yield return WaitFor(() => exit.IsCompleted, "The final chain must reach the portal.");
                Assert.AreEqual(0, deaths);
                for (int i = 0; i < 5; i++) Assert.GreaterOrEqual(Bell(i).RingCount, 1);
                Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            }
            finally { _player.OnRespawned -= count; }
        }
        private IEnumerator FaceRight()
        {
            VirtualInputBridge.MoveVector = Vector2.right; yield return new WaitForFixedUpdate(); yield return null; VirtualInputBridge.MoveVector = Vector2.zero;
        }
        private IEnumerator Ring(int i)
        {
            yield return FaceRight(); VirtualInputBridge.TriggerRoar(); yield return WaitFor(() => Bell(i).IsOpen, "Roar must ring bell " + i);
        }
        private IEnumerator AirRing()
        {
            yield return FaceRight(); VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.2f); VirtualInputBridge.TriggerRoar();
            yield return WaitFor(() => Bell(1).IsOpen, "Double Jump must put the high bell inside the Roar cone.");
        }
        private IEnumerator WalkTo(float x)
        {
            float end = Time.time + 6f;
            while (Mathf.Abs(_player.Rigidbody.position.x - x) > .2f && Time.time < end)
            { VirtualInputBridge.MoveVector = Vector2.right * Mathf.Sign(x - _player.Rigidbody.position.x); yield return null; }
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x - x), .3f, "Cannot reach " + x + "; player=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(.14f);
        }
        private IEnumerator CrossGap(float launch, float landing)
        {
            yield return WalkTo(launch); VirtualInputBridge.MoveVector = Vector2.right; VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(.34f); VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.3f); VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Air Dash must start.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.Dash, "Air Dash must finish.");
            yield return WalkTo(landing); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Land after the lava gap."); yield return new WaitForSeconds(.2f);
        }
    }
}
