using System;
using System.Collections;
using System.Linq;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Enemies.Controllers;
using AlmaDino.Features.Environment.Controllers;
using AlmaDino.Features.Environment.Services;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class Level3_4PlayTests
    {
        private PlayerController _player;
        private RisingHazardFloor2D Gas => UnityEngine.Object.FindObjectsByType<RisingHazardFloor2D>()[0];
        private static LevelExit2D Exit => UnityEngine.Object.FindObjectsByType<LevelExit2D>(FindObjectsInactive.Include)[0];

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput();
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_3_4");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Entry must be safe.");
        }

        [TearDown]
        public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }

        [Test]
        public void FreshEntry_HasAbilities_HorizontalSpores_AndLockedBossExit()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked);
            Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsTrue(_player.IsDashUnlocked);
            Assert.IsFalse(_player.IsRoarUnlocked);
            Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.AreEqual(3, UnityEngine.Object.FindObjectsByType<PoisonToad2D>().Length);
            Assert.AreEqual("Boss_3", Exit.NextSceneName);
            Assert.IsFalse(Exit.gameObject.activeSelf);
            var spores = UnityEngine.Object.FindObjectsByType<DashRefillPickup2D>();
            Assert.AreEqual(2, spores.Length);
            foreach (var spore in spores) Assert.That(spore.transform.position.y, Is.EqualTo(10.2f).Within(0.2f));
        }

        [UnityTest]
        public IEnumerator Entrance_IsSafeWhileWaiting_AndRearWallPreventsFalling()
        {
            yield return new WaitForSeconds(5f);
            Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
            VirtualInputBridge.MoveVector = Vector2.left;
            yield return new WaitForSeconds(2f);
            Assert.That(_player.Rigidbody.position.x, Is.InRange(-5f, -4f));
            Assert.IsTrue(_player.GroundDetector.IsGrounded);
        }

        [UnityTest]
        public IEnumerator Gas_Warns_Rises_HitsIdlePlayer_AndResetsSafely()
        {
            _player.RespawnAt(new Vector2(12.05f, 0.7f));
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return WaitFor(() => Gas.Phase == RisingGasPhase.Warning, "Gas must warn first.");
                Assert.IsFalse(Gas.IsDangerous);
                float surface = Gas.SurfaceY;
                yield return new WaitForSeconds(2f);
                Assert.That(Gas.SurfaceY, Is.EqualTo(surface).Within(0.01f));
                Assert.AreEqual(0, deaths);
                yield return WaitFor(() => Gas.IsDangerous, "Warning must become rising gas.");
                float risingStart = Gas.SurfaceY;
                float startedAt = Time.fixedTime;
                yield return new WaitForSeconds(1f);
                Assert.That((Gas.SurfaceY - risingStart) / (Time.fixedTime - startedAt), Is.EqualTo(0.9f).Within(0.03f));
                yield return WaitFor(() => deaths > 0, "Standing still must be punished by the real gas.", 10f);
                Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
                Assert.That(_player.Rigidbody.position.x, Is.EqualTo(0f).Within(0.2f));
                Assert.IsFalse(Gas.GetComponent<Collider2D>().enabled);
            }
            finally { _player.OnRespawned -= onRespawn; }
        }

        [UnityTest]
        public IEnumerator CrownToad_WarnsAndItsRealShotHitsAnExposedPlayer()
        {
            _player.RespawnAt(new Vector2(112.8f, 20.2f));
            var toad = GameObject.Find("Willow_Toad_Crown").GetComponent<PoisonToad2D>();
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return WaitFor(() => toad.IsWarning, "The toad must announce its attack.");
                Assert.AreEqual(0, deaths);
                yield return WaitFor(() => deaths > 0, "An actual poison shot must damage exposed Alma.");
                Assert.AreEqual(0, toad.ActiveProjectileCount);
                Assert.AreEqual(0, toad.ShotsFired);
            }
            finally { _player.OnRespawned -= onRespawn; }
        }

        [UnityTest]
        public IEnumerator Toad_ReaimsAtAlmaWhenSheChangesSidesDuringWarning()
        {
            _player.RespawnAt(new Vector2(109.5f, 20.2f));
            var toad = GameObject.Find("Willow_Toad_Crown").GetComponent<PoisonToad2D>();
            yield return WaitFor(() => toad.ShotsFired == 1, "Fire toward Alma on the left.");
            var leftShot = UnityEngine.Object.FindObjectsByType<PoisonBubble2D>().Single(bubble => bubble.IsDangerous);
            Assert.Less(leftShot.GetComponent<Rigidbody2D>().linearVelocity.x, 0f);
            yield return WaitFor(() => toad.IsWarning, "Wait for the next attack warning.");
            _player.Rigidbody.position = new Vector2(119.5f, 20.2f);
            yield return WaitFor(() => toad.ShotsFired == 2, "Fire toward Alma after she changes sides.");
            var rightShot = UnityEngine.Object.FindObjectsByType<PoisonBubble2D>().Single(bubble => bubble.IsDangerous);
            Assert.Greater(rightShot.GetComponent<Rigidbody2D>().linearVelocity.x, 0f);
        }

        [UnityTest]
        public IEnumerator SavedPurpleEgg_DoesNotDisableGasOnANewAttempt()
        {
            GameProgression.RescueEgg(EggType.PurpleEgg);
            yield return SceneManager.LoadSceneAsync("Level_3_4");
            yield return null;
            _player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
            _player.RespawnAt(new Vector2(12.05f, 0.7f));
            yield return WaitFor(() => Gas.IsDangerous, "A replay must still start the rising gas.");
            float previousSurface = Gas.SurfaceY;
            float previousVisualY = GameObject.Find("Toxic_Gas_Surface").transform.position.y;
            yield return new WaitForSeconds(1f);
            Assert.Greater(Gas.SurfaceY, previousSurface + 0.8f);
            Assert.Greater(GameObject.Find("Toxic_Gas_Surface").transform.position.y, previousVisualY + 0.8f);
            Assert.That(GameObject.Find("Toxic_Gas_Surface").transform.position.y, Is.EqualTo(Gas.SurfaceY).Within(0.06f));
        }

        [UnityTest]
        public IEnumerator CrownCover_ProtectsPlayerFromRepeatedShots()
        {
            _player.RespawnAt(new Vector2(109.5f, 20.2f));
            var toad = GameObject.Find("Willow_Toad_Crown").GetComponent<PoisonToad2D>();
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return new WaitForSeconds(5f);
                Assert.GreaterOrEqual(toad.ShotsFired, 2);
                Assert.AreEqual(0, deaths, "Cover must block poison while preparing the final jump.");
            }
            finally { _player.OnRespawned -= onRespawn; }
        }

        [UnityTest]
        public IEnumerator Checkpoint_PausesGas_AndDeathRestartsTheCurrentSection()
        {
            _player.RespawnAt(new Vector2(40f, 8.2f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Checkpoint must have solid ground.");
            yield return new WaitForSeconds(4f);
            Assert.AreEqual(1, Gas.StageIndex);
            Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
            Assert.IsFalse(Gas.IsDangerous);
            _player.RespawnAt(new Vector2(61f, 8.2f));
            yield return WaitFor(() => Gas.Phase == RisingGasPhase.Warning, "Leaving the refuge arms a fresh warning.");
            var spore = UnityEngine.Object.FindObjectsByType<DashRefillPickup2D>()[0];
            spore.Consume(_player);
            _player.KillAndRespawn();
            yield return null;
            Assert.AreEqual(1, Gas.StageIndex);
            Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
            Assert.That(Gas.SurfaceY, Is.EqualTo(3.5f).Within(0.01f));
            Assert.That(_player.Rigidbody.position.x, Is.EqualTo(40f).Within(0.2f));
            Assert.IsTrue(spore.IsAvailable);
        }

        [UnityTest]
        public IEnumerator OrdinaryDoubleJump_CannotReachTheCrown()
        {
            _player.RespawnAt(new Vector2(94.3f, 14f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Stand on the launch end.");
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.34f);
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.3f);
            VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Dash must begin.");
            yield return FinishDash();
            Assert.Less(_player.Rigidbody.position.y - 0.6f, 19.5f, "The crown must require the Pound catapult.");
            Assert.IsFalse(_player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 20f);
            Assert.IsFalse(GameProgression.IsEggRescued(EggType.PurpleEgg));
        }

        [UnityTest]
        public IEnumerator PoundCatapult_LaunchesToCrown_AndDeathRestoresLever()
        {
            _player.RespawnAt(new Vector2(87f, 12.7f));
            yield return LaunchCrownCatapult();
            Assert.Greater(_player.Rigidbody.position.y, 20f);
            _player.SetCheckpoint(new Vector2(84f, 12.7f));
            _player.KillAndRespawn();
            yield return null;
            var lever = GameObject.Find("Crown_Catapult").GetComponent<SeesawPlatform2D>();
            Assert.IsFalse(lever.IsArmed);
            Assert.That(lever.CurrentAngle, Is.EqualTo(0f).Within(1f));
            Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
        }

        [UnityTest]
        public IEnumerator OptionalRootCatapult_CanSkipTheFirstBranch()
        {
            yield return WalkTo(4f);
            yield return JumpTo(5.7f);
            var lever = GameObject.Find("Root_Catapult_Shortcut").GetComponent<SeesawPlatform2D>();
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.16f);
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => lever.IsArmed, "Optional Pound must arm the root catapult.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.GroundPound, "Finish Pound.");
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.JumpHeld = true;
            yield return WaitFor(() => _player.Rigidbody.position.y > 4f, "Optional catapult must launch Alma.");
            yield return WalkTo(22f);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 3.5f,
                "The early catapult must land beyond the first branch.");
        }

        [UnityTest]
        public IEnumerator PurpleEgg_StopsGas_UnlocksPortal_RevealsGuardian_AndPersists()
        {
            _player.RespawnAt(new Vector2(118f, 20.2f));
            yield return WaitFor(() => GameProgression.IsEggRescued(EggType.PurpleEgg), "Physical egg contact must rescue it.");
            Assert.IsTrue(Exit.gameObject.activeSelf);
            Assert.IsTrue(GameObject.Find("Pterodactyl_Guardian_Teaser").activeSelf);
            Assert.AreEqual(RisingGasPhase.Stopped, Gas.Phase);
            Assert.IsFalse(Gas.IsDangerous);
            Assert.IsFalse(GameProgression.IsWorldCompleted(3));
            yield return SceneManager.LoadSceneAsync("Level_3_4");
            yield return null;
            Assert.IsTrue(Exit.gameObject.activeSelf);
            Assert.IsTrue(GameObject.Find("Pterodactyl_Guardian_Teaser").activeSelf);
            Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
            Assert.AreEqual(1, GameProgression.RescuedEggCount);
        }

        [UnityTest]
        public IEnumerator CompleteRoute_WithRealInputs_RescuesPurpleEggWithoutDeaths()
        {
            int deaths = 0;
            Action<Vector2> onRespawn = _ => deaths++;
            _player.OnRespawned += onRespawn;
            try
            {
                yield return WalkTo(4f);
                yield return JumpTo(10f);
                foreach (float x in new[] { 16f, 22f, 28f, 34f, 40f }) yield return JumpTo(x);
                Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
                yield return WalkTo(42.5f);
                yield return JumpDash();
                yield return FinishDash();
                yield return WaitFor(() => _player.CanAirDash, "Horizontal spore must refresh Dash.");
                VirtualInputBridge.TriggerDash();
                yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Refilled Dash must start.");
                yield return FinishDash();
                yield return LandAt(61f);
                yield return JumpDash();
                yield return FinishDash();
                yield return LandAt(71.5f);
                yield return JumpTo(78f);
                yield return JumpTo(84f);
                Assert.AreEqual(2, Gas.StageIndex);
                Assert.AreEqual(RisingGasPhase.Dormant, Gas.Phase);
                yield return LaunchCrownCatapult();
                yield return WalkTo(109.5f);
                yield return JumpDash();
                yield return FinishDash();
                yield return LandAt(118f);
                yield return WaitFor(() => GameProgression.IsEggRescued(EggType.PurpleEgg), "The route must rescue the Purple Egg.");
                yield return WalkTo(121f);
                VirtualInputBridge.MoveVector = Vector2.right;
                yield return WaitFor(() => Exit.IsCompleted, "Rescue must open the Boss 3 exit.");
                Assert.AreEqual(0, deaths);
            }
            finally { _player.OnRespawned -= onRespawn; ClearInput(); }
        }

        private IEnumerator LaunchCrownCatapult()
        {
            yield return WalkTo(88.2f);
            yield return JumpTo(89.7f);
            var lever = GameObject.Find("Crown_Catapult").GetComponent<SeesawPlatform2D>();
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.16f);
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => lever.IsArmed, "Real Pound must arm the launch end.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.GroundPound, "Pound must finish.");
            VirtualInputBridge.JumpHeld = true;
            VirtualInputBridge.MoveVector = Vector2.right;
            yield return WaitFor(() => _player.Rigidbody.position.y > 16f, "Running to the raised tip must launch Alma.");
            yield return WalkTo(101f);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 20f,
                "Catapult must reach the crown.");
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator JumpTo(float x)
        {
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.28f);
            VirtualInputBridge.TriggerJump();
            yield return WalkTo(x);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Land on branch at " + x + "; position=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(0.14f);
        }

        private IEnumerator JumpDash()
        {
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.34f);
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(0.3f);
            VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType == PlayerStateEnum.Dash, "Dash must start.");
        }

        private IEnumerator FinishDash() => WaitFor(() => _player.StateMachine.CurrentStateType != PlayerStateEnum.Dash, "Dash must finish.");

        private IEnumerator LandAt(float x)
        {
            yield return WalkTo(x);
            VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Landing must be safe at " + x);
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
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x - x), 0.3f, "Cannot reach " + x + "; position=" + _player.Rigidbody.position);
            yield return new WaitForSeconds(0.14f);
        }

        private static IEnumerator WaitFor(Func<bool> condition, string message, float timeout = 6f)
        {
            float deadline = Time.time + timeout;
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
