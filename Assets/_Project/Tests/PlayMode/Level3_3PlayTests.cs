using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Enemies.Controllers;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace AlmaDino.Tests.PlayMode
{
    public class Level3_3PlayTests
    {
        private PlayerController _player;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput();
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_3_3");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Safe spawn required.");
        }
        [TearDown]
        public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }
        [Test]
        public void FreshEntry_HasThreeAbilities_EightHorizontalSpores_AndCorrectDestination()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked);
            Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsTrue(_player.IsDashUnlocked);
            Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.AreEqual(8, UnityEngine.Object.FindObjectsByType<DashRefillPickup2D>().Length);
            foreach (var spore in UnityEngine.Object.FindObjectsByType<DashRefillPickup2D>())
                Assert.That(spore.transform.position.y, Is.EqualTo(2.7f).Within(0.2f));
            Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.AreEqual("Level_3_4", GameObject.Find("Portal_Exit_To_3_4").GetComponent<LevelExit2D>().NextSceneName);
        }
        [UnityTest]
        public IEnumerator EntranceBoundary_BlocksWalkingBackWithoutFalling()
        {
            VirtualInputBridge.MoveVector = Vector2.left;
            yield return new WaitForSeconds(2f);
            Assert.That(_player.Rigidbody.position.x, Is.InRange(-5f, -4f));
            Assert.IsTrue(_player.GroundDetector.IsGrounded);
        }
        [UnityTest]
        public IEnumerator WithoutSporeRefills_TheFirstLakeCannotBeCrossed()
        {
            foreach (var spore in UnityEngine.Object.FindObjectsByType<DashRefillPickup2D>())
                spore.GetComponent<Collider2D>().enabled = false;
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return WalkTo(8.5f);
                yield return LaunchJumpDash();
                yield return WaitFor(() => deaths > 0, "A single charge must not cross the 16m lake.");
            }
            finally { _player.OnRespawned -= onRespawn; ClearInput(); }
        }
        [UnityTest]
        public IEnumerator RealToadShot_HitsAnExposedPlayerAfterWarning()
        {
            _player.RespawnAt(new Vector2(73.8f, 0.7f));
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                var toad = GameObject.Find("Poison_Toad_Two").GetComponent<PoisonToad2D>();
                yield return WaitFor(() => toad.IsWarning, "Toad must warn before the real attack.");
                Assert.AreEqual(0, deaths);
                yield return WaitFor(() => deaths > 0, "An exposed player must be hit by the real toad projectile.");
            }
            finally { _player.OnRespawned -= onRespawn; }
        }
        [UnityTest]
        public IEnumerator RootGate_BlocksWalking_AndPoundOpensSafeTunnel()
        {
            _player.RespawnAt(new Vector2(32f, 0.7f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Cracked floor must support Alma.");
            var floor = GameObject.Find("Cracked_Root_Floor").GetComponent<BreakableGround2D>();
            VirtualInputBridge.MoveVector = Vector2.right;
            yield return new WaitForSeconds(0.6f);
            Assert.Less(_player.Rigidbody.position.x, 33.6f, "Root must prevent walking past the cracked floor.");
            Assert.IsFalse(floor.IsBroken);
            yield return WalkTo(32f);
            yield return PoundThroughFloor(floor);
            Assert.That(_player.Rigidbody.position.y, Is.InRange(-2.5f, -2.2f));
            yield return WalkTo(39f);
            Assert.Greater(_player.Rigidbody.position.x, 38.8f, "The lower passage must be traversable.");
        }
        [UnityTest]
        public IEnumerator ReedGate_BlocksWalking_AndBreaksWithRealDash()
        {
            _player.RespawnAt(new Vector2(63f, 0.7f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Gate approach must be solid.");
            var gate = GameObject.Find("Reed_Dash_Gate").GetComponent<DashBreakableBarrier2D>();
            VirtualInputBridge.MoveVector = Vector2.right;
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(gate.IsBroken);
            Assert.Less(_player.Rigidbody.position.x, 64.5f);
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.2f);
            VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => gate.IsBroken, "Real Dash collision must break the reeds.");
            ClearInput();
        }
        [UnityTest]
        public IEnumerator PoisonBubble_DamagesAlmaDuringDash()
        {
            var template = UnityEngine.Object.FindObjectsByType<PoisonBubble2D>(FindObjectsInactive.Include)[0];
            var bubble = UnityEngine.Object.Instantiate(template);
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                VirtualInputBridge.MoveVector = Vector2.right;
                VirtualInputBridge.TriggerJump();
                yield return new WaitForSeconds(0.15f);
                VirtualInputBridge.TriggerDash();
                yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Dash must start.");
                bubble.Launch(_player.Rigidbody.position + Vector2.right, Vector2.zero, 0f, 2f);
                yield return WaitFor(() => deaths > 0, "Dash must not grant immunity to poison.");
            }
            finally
            {
                _player.OnRespawned -= onRespawn;
                UnityEngine.Object.Destroy(bubble.gameObject);
                ClearInput();
            }
        }
        [UnityTest]
        public IEnumerator Spore_RefillsBothAbilitiesDuringDash_AndRegenerates()
        {
            yield return WalkTo(8.5f);
            yield return LaunchJumpDash();
            var spore = GameObject.Find("Refill_Spore_14").GetComponent<DashRefillPickup2D>();
            yield return WaitFor(() => !spore.IsAvailable, "First Dash must touch the floating spore.");
            Assert.IsTrue(_player.CanAirDash);
            Assert.IsTrue(_player.HasDoubleJump);
            Assert.IsFalse(_player.GroundDetector.IsGrounded);
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.Dash, "Dash should end.");
            VirtualInputBridge.TriggerJump();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.DoubleJump,
                "Spore must allow another Double Jump without landing.");
            // Stay airborne over the lake long enough to observe the spore's own timer.
            _player.RespawnAt(new Vector2(0f, 0.7f));
            spore.Consume(_player);
            yield return new WaitForSeconds(2.6f);
            Assert.IsTrue(spore.IsAvailable);
        }
        [UnityTest]
        public IEnumerator Death_RestoresSpores_ClearsPoison_AndKeepsCheckpointAbilities()
        {
            _player.RespawnAt(new Vector2(28f, 0.7f));
            yield return new WaitForSeconds(0.2f);
            var spore = GameObject.Find("Refill_Spore_14").GetComponent<DashRefillPickup2D>();
            spore.Consume(_player);
            var floor = GameObject.Find("Cracked_Root_Floor").GetComponent<BreakableGround2D>();
            floor.Break();
            var toad = GameObject.Find("Poison_Toad_One").GetComponent<PoisonToad2D>();
            yield return WaitFor(() => toad.ShotsFired > 0, "Nearby toad must shoot.");
            _player.KillAndRespawn();
            yield return null;
            Assert.IsTrue(spore.IsAvailable);
            Assert.IsFalse(floor.IsBroken);
            Assert.IsTrue(floor.gameObject.activeSelf);
            Assert.AreEqual(0, toad.ActiveProjectileCount);
            Assert.AreEqual(0, toad.ShotsFired);
            Assert.That(_player.Rigidbody.position.x, Is.InRange(27.8f, 28.2f));
            Assert.IsTrue(_player.IsDashUnlocked);
        }
        [UnityTest]
        public IEnumerator ShelteredCheckpoint_StaysSafeDuringRepeatedToadShots()
        {
            _player.RespawnAt(new Vector2(28f, 0.7f));
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            var toad = GameObject.Find("Poison_Toad_One").GetComponent<PoisonToad2D>();
            yield return WaitFor(() => toad.IsWarning, "A written warning must precede firing.");
            Assert.AreEqual(0, toad.ShotsFired);
            yield return new WaitForSeconds(5f);
            _player.OnRespawned -= onRespawn;
            Assert.GreaterOrEqual(toad.ShotsFired, 2);
            Assert.AreEqual(0, deaths, "Checkpoint canopy must intercept poison.");
        }
        [UnityTest]
        public IEnumerator CompleteRoute_WithAirborneRefills_ReachesExitWithoutDeaths()
        {
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return CrossLake(8.5f, 28f, 1);
                yield return WalkTo(32f);
                var floor = GameObject.Find("Cracked_Root_Floor").GetComponent<BreakableGround2D>();
                yield return PoundThroughFloor(floor);
                yield return WalkTo(39f);
                VirtualInputBridge.MoveVector = Vector2.right;
                VirtualInputBridge.TriggerJump();
                yield return new WaitForSeconds(0.34f);
                VirtualInputBridge.TriggerJump();
                yield return LandAt(41f);
                // Shelter before vaulting the cover and toad into the horizontal chain.
                yield return CrossLake(41f, 63f, 2);
                VirtualInputBridge.MoveVector = Vector2.right;
                VirtualInputBridge.TriggerDash();
                yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Ground Dash must break the gate.");
                yield return FinishDash();
                Assert.IsTrue(UnityEngine.Object.FindObjectsByType<DashBreakableBarrier2D>(FindObjectsInactive.Include)[0].IsBroken);
                yield return LandAt(68.5f);
                yield return LaunchJumpDash();
                yield return FinishDash();
                yield return LandAt(76f);
                yield return CrossLake(80.5f, 114f, 4);
                yield return WalkTo(120f);
                VirtualInputBridge.MoveVector = Vector2.right;
                var exit = GameObject.Find("Portal_Exit_To_3_4").GetComponent<LevelExit2D>();
                yield return WaitFor(() => exit.IsCompleted, "The final horizontal chain must reach the portal.");
                Assert.AreEqual(0, deaths);
            }
            finally { _player.OnRespawned -= onRespawn; ClearInput(); }
        }
        private IEnumerator LaunchJumpDash()
        {
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.34f);
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.3f);
            VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Dash must start.");
        }
        private IEnumerator FinishDash()
        {
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.Dash,
                "Dash must finish; position=" + _player.Rigidbody.position);
        }
        private IEnumerator CrossLake(float launchX, float landingX, int extraDashes)
        {
            yield return WalkTo(launchX);
            yield return LaunchJumpDash();
            yield return FinishDash();
            for (int i = 0; i < extraDashes; i++)
            {
                yield return WaitFor(() => _player.CanAirDash, "Horizontal spore must refill Dash at " + _player.Rigidbody.position);
                VirtualInputBridge.TriggerDash();
                yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Refilled Dash must start.");
                yield return FinishDash();
            }
            yield return LandAt(landingX);
        }
        private IEnumerator PoundThroughFloor(BreakableGround2D floor)
        {
            VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.2f);
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => floor.IsBroken, "Pound must break the real cracked floor.");
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y < -2f,
                "Pound descent must land safely in the tunnel.");
            yield return new WaitForSeconds(0.2f);
        }
        private IEnumerator LandAt(float x)
        {
            yield return WalkTo(x);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Must land at " + x + "; position=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(0.42f);
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
