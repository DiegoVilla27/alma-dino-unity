using System;
using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Enemies.Controllers;
using AlmaDino.Features.Environment;
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
    public class Level4_3PlayTests
    {
        private PlayerController _player;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            ClearInput(); GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Level_4_3"); yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, "Spawn must be safe.");
        }
        [TearDown] public void TearDown() { ClearInput(); GameProgression.ResetProgression(); }
        private static void ClearInput() { VirtualInputBridge.MoveVector = Vector2.zero; VirtualInputBridge.ReleaseJump(); VirtualInputBridge.ConsumeFrameTriggers(); }
        private static MeteorImpactGate2D Gate(int i) => GameObject.Find("Meteor_Gate_" + i).GetComponent<MeteorImpactGate2D>();
        private static FractureEruption2D Eruption => GameObject.Find("Fracture_Eruption").GetComponent<FractureEruption2D>();
        private static MagmaSalamander2D Enemy(int i) => GameObject.Find("Magma_Salamander_" + i).GetComponent<MagmaSalamander2D>();
        private static IEnumerator WaitFor(Func<bool> condition, string message, float timeout = 6f)
        {
            float end = Time.time + timeout;
            while (!condition() && Time.time < end) yield return null;
            Assert.IsTrue(condition(), message);
        }
        [Test]
        public void DirectEntryHasAllAbilitiesAndCameraSix()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked); Assert.IsTrue(_player.IsGroundPoundUnlocked);
            Assert.IsTrue(_player.IsDashUnlocked); Assert.IsTrue(_player.IsRoarUnlocked);
            Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.AreEqual("Level_4_4", GameObject.Find("Portal_Exit_To_4_4").GetComponent<LevelExit2D>().NextSceneName);
            Assert.IsFalse(GameProgression.IsWorldCompleted(4)); Assert.AreEqual(3, UnityEngine.Object.FindObjectsByType<MagmaSalamander2D>(FindObjectsSortMode.None).Length);
        }
        [UnityTest]
        public IEnumerator RearBoundaryPreventsFallingBehindSpawn()
        {
            int deaths=0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            VirtualInputBridge.MoveVector=Vector2.left; yield return new WaitForSeconds(2f);
            _player.OnRespawned -= count; Assert.AreEqual(0,deaths); Assert.Greater(_player.Rigidbody.position.x,-5f);
        }
        [UnityTest]
        public IEnumerator LaunchCollapsesAndDeathRestoresItImmediately()
        {
            _player.RespawnAt(new Vector2(11f,.7f));
            var launch=GameObject.Find("Collapsing_Launch_0").GetComponent<Collider2D>();
            yield return WaitFor(() => !launch.enabled,"Launch must collapse after standing on it.",1.5f);
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate(); Assert.IsTrue(launch.enabled);
        }
        [UnityTest]
        public IEnumerator OrdinaryLandingDoesNotBreakFloorOrSpawnMeteor()
        {
            _player.RespawnAt(new Vector2(22f,.7f)); yield return new WaitForSeconds(.4f);
            Assert.IsFalse(Gate(0).IsOpen); Assert.IsFalse(Gate(0).Meteor.IsFlying); Assert.AreEqual(0,Gate(0).MeteorsLaunched);
        }
        [UnityTest]
        public IEnumerator JetWarnsBeforeFiringAndLowerRefugeIsSafe()
        {
            _player.RespawnAt(new Vector2(22f,.7f));
            var jet=GameObject.Find("Landing_Flame_Jet_0").GetComponent<FractureFlameJet2D>();
            yield return WaitFor(() => jet.IsWarning,"Jet must telegraph the attack."); Assert.IsFalse(jet.IsDangerous);
            yield return PoundFromStanding();
            yield return WaitFor(() => jet.IsDangerous,"Jet must erupt after its warning.");
            Assert.Less(_player.Rigidbody.position.y,-.25f); Assert.AreEqual(22f,_player.Rigidbody.position.x,.5f);
        }
        [UnityTest]
        public IEnumerator ReflectedMeteorOpensGateAndOrdinaryRoarCannotOpenIt()
        {
            _player.RespawnAt(new Vector2(25f,.7f)); yield return FaceRight(); VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.3f);
            Assert.IsFalse(Gate(0).IsOpen);
            _player.RespawnAt(new Vector2(22f,.7f)); yield return PoundFromStanding(); yield return Reflect(0); Assert.IsTrue(Gate(0).IsOpen);
        }
        [UnityTest]
        public IEnumerator UnreflectedMeteorKillsAndRestoresPoundFloor()
        {
            var ground = GameObject.Find("Pound_Landing_0").GetComponent<BreakableGround2D>();
            _player.RespawnAt(new Vector2(22f,.7f)); yield return PoundFromStanding();
            int deaths=0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            yield return WaitFor(() => deaths>0,"Ignoring a meteor must cause damage."); _player.OnRespawned -= count;
            Assert.IsFalse(Gate(0).IsOpen); Assert.IsFalse(Gate(0).Meteor.IsFlying); Assert.AreEqual(0,Gate(0).MeteorsLaunched); Assert.IsFalse(ground.IsBroken);
        }
        [UnityTest]
        public IEnumerator SalamandersAimBothDirectionsAndRoarStunsAndPushes()
        {
            var enemy=Enemy(0); _player.RespawnAt(new Vector2(33f,.7f));
            yield return WaitFor(() => enemy.ShotsFired>0,"Salamander must attack Alma on its left."); Assert.Less(enemy.LastShotDirection.x,0f);
            _player.RespawnAt(new Vector2(41f,.7f)); yield return WaitFor(() => enemy.ShotsFired>0,"Salamander must attack Alma on its right."); Assert.Greater(enemy.LastShotDirection.x,0f);
            _player.RespawnAt(new Vector2(34f,.7f)); yield return FaceRight();
            enemy.ReceiveRoar(Vector2.right); Vector2 start=enemy.transform.position;
            Assert.IsTrue(enemy.IsStunned); Assert.IsFalse(enemy.IsDangerous);
            yield return new WaitForSeconds(.2f); Assert.Greater(enemy.transform.position.x,start.x+.5f);
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate(); Assert.IsFalse(enemy.IsStunned); Assert.AreEqual(0,enemy.ShotsFired);
        }
        [UnityTest]
        public IEnumerator CheckpointKeepsCompletedGateAndResetsNextChamber()
        {
            _player.RespawnAt(new Vector2(22f,.7f)); yield return PoundFromStanding(); yield return Reflect(0);
            _player.RespawnAt(new Vector2(32f,.7f)); yield return new WaitForSeconds(.2f);
            Assert.IsTrue(Gate(0).IsOpen); Assert.AreEqual(RisingGasPhase.Dormant,Eruption.Phase);
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate();
            Assert.IsTrue(Gate(0).IsOpen); Assert.AreEqual(RisingGasPhase.Dormant,Eruption.Phase); Assert.AreEqual(32f,_player.Rigidbody.position.x,.3f);
        }
        [UnityTest]
        public IEnumerator WrongFacingRoarDoesNotReflectMeteor()
        {
            _player.RespawnAt(new Vector2(22f,.7f)); yield return PoundFromStanding();
            yield return WaitFor(() => Gate(0).Meteor.IsFlying,"Wait for the incoming meteor.");
            VirtualInputBridge.MoveVector=Vector2.left; yield return new WaitForFixedUpdate(); yield return null;
            VirtualInputBridge.MoveVector=Vector2.zero; VirtualInputBridge.TriggerRoar(); yield return new WaitForSeconds(.27f);
            Assert.IsTrue(Gate(0).Meteor.IsDangerous); Assert.IsFalse(Gate(0).IsOpen);
        }
        [UnityTest]
        public IEnumerator DeathAtFinalChamberRestoresFloorAndReturnsToCheckpoint68()
        {
            _player.RespawnAt(new Vector2(68f,.7f)); yield return new WaitForSeconds(.2f);
            var ground=GameObject.Find("Final_Eruption_Seal").GetComponent<BreakableGround2D>();
            // Place Alma in the final room without moving the active checkpoint.
            _player.Rigidbody.position=new Vector2(72f,.7f); yield return new WaitForSeconds(.1f);
            yield return PoundFromStanding(); Assert.IsTrue(ground.IsBroken);
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(ground.IsBroken); Assert.AreEqual(RisingGasPhase.Dormant,Eruption.Phase); Assert.IsFalse(Eruption.IsDangerous);
            Assert.AreEqual(68f,_player.Rigidbody.position.x,.3f);
        }
        [UnityTest]
        public IEnumerator Level42PortalLoadsTheFracture()
        {
            yield return SceneManager.LoadSceneAsync("Level_4_2"); yield return null;
            _player=UnityEngine.Object.FindAnyObjectByType<PlayerController>(); _player.RespawnAt(new Vector2(118f,1.5f));
            yield return WaitFor(() => SceneManager.GetActiveScene().name=="Level_4_3","Previous portal must load 4-3.");
        }
        [UnityTest]
        public IEnumerator UpperRouteAndEruptionEscapeCompleteWithoutDeaths()
        {
            int deaths=0; Action<Vector2> count = _ => deaths++; _player.OnRespawned += count;
            try
            {
                yield return Chain(0,11.4f,26f,30f); yield return WalkTo(32f); yield return PassEnemy(0,40.5f);
                yield return UpperRoute(); yield return WalkTo(68f); yield return Escape();
                VirtualInputBridge.MoveVector=Vector2.right;
                yield return WaitFor(() => GameObject.Find("Portal_Exit_To_4_4").GetComponent<LevelExit2D>().IsCompleted,"Exit must complete the level.");
                Assert.AreEqual(0,deaths,"Four-ability route must be achievable.");
                Assert.IsTrue(Gate(0).IsOpen); Assert.AreEqual(RisingGasPhase.Stopped,Eruption.Phase);
                Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            }
            finally { _player.OnRespawned -= count; ClearInput(); }
        }
        [UnityTest]
        public IEnumerator LowerRouteProvidesASecondSafePathToCheckpoint68()
        {
            _player.RespawnAt(new Vector2(40.5f,.7f)); yield return WaitFor(() => _player.GroundDetector.IsGrounded,"Stand at the fork.");
            int deaths=0; Action<Vector2> count=_=>deaths++; _player.OnRespawned+=count;
            try { yield return LowerRoute(); yield return WalkTo(68f); Assert.AreEqual(0,deaths); }
            finally { _player.OnRespawned-=count; ClearInput(); }
        }
        [UnityTest]
        public IEnumerator BreakingSealStartsActualRisingLavaAndDeathResetsIt()
        {
            _player.RespawnAt(new Vector2(72f,.7f)); yield return PoundFromStanding();
            Assert.AreEqual(RisingGasPhase.Warning,Eruption.Phase); float initial=Eruption.SurfaceY;
            yield return WaitFor(() => Eruption.Phase==RisingGasPhase.Rising,"Seal must cause an eruption.");
            yield return new WaitForSeconds(.4f); Assert.Greater(Eruption.SurfaceY,initial+.15f); Assert.IsTrue(Eruption.IsDangerous);
            _player.KillAndRespawn(); yield return new WaitForFixedUpdate();
            Assert.AreEqual(RisingGasPhase.Dormant,Eruption.Phase); Assert.AreEqual(initial,Eruption.SurfaceY,.01f);
        }
        [Test]
        public void LevelHasOneMeteorPuzzleAndTwoDifferentMiddleRoutes()
        {
            Assert.AreEqual(1,UnityEngine.Object.FindObjectsByType<MeteorImpactGate2D>(FindObjectsSortMode.None).Length);
            Assert.IsNotNull(GameObject.Find("Upper_Crumbling_0")); Assert.IsNotNull(GameObject.Find("Lower_Stone_0"));
            Assert.IsNotNull(GameObject.Find("Final_Eruption_Seal"));
        }
        private IEnumerator UpperRoute()
        {
            yield return JumpTo(45f); yield return WalkTo(47.2f); yield return JumpTo(53f);
            yield return WalkTo(54.2f); yield return JumpTo(60f); yield return WalkTo(61.2f);
            yield return JumpTo(65f,false);
        }
        private IEnumerator LowerRoute()
        {
            yield return JumpTo(45f,false); yield return SteamWindow(47f); yield return WalkTo(49.4f);
            yield return JumpTo(53.4f,false); yield return FaceRight(); VirtualInputBridge.TriggerRoar();
            yield return WaitFor(() => Enemy(1).IsStunned,"Roar must clear the lower route guard.");
            yield return new WaitForSeconds(.27f); yield return WalkTo(55.9f); yield return SteamWindow(57.5f);
            yield return WalkTo(58.4f); yield return JumpTo(61.7f,false); yield return WalkTo(63.3f);
            yield return JumpTo(65f,false);
        }
        private static IEnumerator SteamWindow(float x)
        {
            var geyser=GameObject.Find("Branch_Steam_"+x).GetComponent<LavaGeyser2D>();
            yield return WaitFor(() => geyser.Phase==GeyserPhase.Dormant,"Wait for a safe steam window.");
        }
        private IEnumerator Escape()
        {
            yield return WalkTo(72f); yield return PoundFromStanding(); yield return JumpTo(76.4f);
            yield return WalkTo(78.2f); yield return JumpTo(82.8f); yield return WalkTo(84.2f);
            yield return JumpTo(87.6f); yield return FaceRight(); VirtualInputBridge.TriggerRoar();
            yield return WaitFor(() => Enemy(2).IsStunned,"Roar must clear the climbing escape guard.");
            yield return new WaitForSeconds(.27f); yield return WalkTo(90.2f); yield return JumpTo(94.4f); yield return WalkTo(96.4f);
            VirtualInputBridge.MoveVector=Vector2.right; VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.34f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.3f); VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType==PlayerStateEnum.Dash,"Last Dash must start.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType!=PlayerStateEnum.Dash,"Last Dash must finish.");
            yield return WalkTo(107f); VirtualInputBridge.ReleaseJump(); yield return WaitFor(() => _player.GroundDetector.IsGrounded,"Reach the final sanctuary.");
        }
        private IEnumerator JumpTo(float x,bool doubleJump=true)
        {
            VirtualInputBridge.MoveVector=Vector2.right; VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.26f);
            if(doubleJump) VirtualInputBridge.TriggerJump();
            yield return WalkTo(x); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded,"Land at "+x+"; player="+_player.Rigidbody.position);
        }
        private IEnumerator Chain(int index,float launch,float lowerEnd,float upperLanding)
        {
            yield return WalkTo(launch); VirtualInputBridge.MoveVector=Vector2.right; VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(.34f); VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.3f); VirtualInputBridge.TriggerDash();
            yield return WaitFor(() => _player.StateMachine.CurrentStateType==PlayerStateEnum.Dash,"Dash must start.");
            yield return WaitFor(() => _player.StateMachine.CurrentStateType!=PlayerStateEnum.Dash,"Dash must finish.");
            VirtualInputBridge.MoveVector=Vector2.zero; VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y<-.25f,"Pound must reach the lower refuge.");
            VirtualInputBridge.ReleaseJump(); yield return Reflect(index); yield return WalkTo(lowerEnd);
            VirtualInputBridge.MoveVector=Vector2.right; VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.22f);
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.3f); yield return WalkTo(upperLanding); VirtualInputBridge.ReleaseJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded,"Return to solid ground.");
        }
        private IEnumerator PoundFromStanding()
        {
            yield return WaitFor(() => _player.GroundDetector.IsGrounded,"Stand on cracked floor.");
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.18f); VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y<-.25f,"Pound through the floor.");
            VirtualInputBridge.ReleaseJump();
        }
        private IEnumerator Reflect(int i)
        {
            yield return FaceRight(); yield return WaitFor(() => Gate(i).Meteor.IsFlying,"Pound must release a meteor.");
            VirtualInputBridge.TriggerRoar(); yield return WaitFor(() => Gate(i).IsOpen,"Reflected meteor must hit its gate.");
        }
        private IEnumerator PassEnemy(int i,float destination)
        {
            var enemy=Enemy(i); float end=Time.time+3f;
            while(enemy.transform.position.x-_player.Rigidbody.position.x>2.2f && Time.time<end)
            { VirtualInputBridge.MoveVector=Vector2.right; yield return null; }
            VirtualInputBridge.MoveVector=Vector2.zero; VirtualInputBridge.TriggerRoar();
            yield return WaitFor(() => enemy.IsStunned,"Actual Roar must stun the salamander.");
            yield return new WaitForSeconds(.27f); VirtualInputBridge.MoveVector=Vector2.right;
            VirtualInputBridge.TriggerJump(); yield return new WaitForSeconds(.24f); VirtualInputBridge.TriggerJump();
            yield return WalkTo(destination); VirtualInputBridge.ReleaseJump(); yield return WaitFor(() => _player.GroundDetector.IsGrounded,"Land beyond the climbing wall.");
        }
        private IEnumerator FaceRight()
        {
            VirtualInputBridge.MoveVector=Vector2.right; yield return new WaitForFixedUpdate(); yield return null; VirtualInputBridge.MoveVector=Vector2.zero;
        }
        private IEnumerator WalkTo(float x)
        {
            float end=Time.time+6f;
            while(Mathf.Abs(_player.Rigidbody.position.x-x)>.2f && Time.time<end)
            { VirtualInputBridge.MoveVector=Vector2.right*Mathf.Sign(x-_player.Rigidbody.position.x); yield return null; }
            VirtualInputBridge.MoveVector=Vector2.zero;
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x-x),.35f,"Cannot reach "+x+"; player="+_player.Rigidbody.position);
            yield return new WaitForSeconds(.12f);
        }
    }
}
