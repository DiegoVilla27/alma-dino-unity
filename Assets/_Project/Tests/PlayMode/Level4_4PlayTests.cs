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
    public class Level4_4PlayTests
    {
        private PlayerController _player;
        private static TempleTrialGates2D Trials=>GameObject.Find("Temple_Trial_Gates").GetComponent<TempleTrialGates2D>();
        private static GreenEggRescue2D Egg=>GameObject.Find("Red_Egg").GetComponent<GreenEggRescue2D>();
        private static RedEggSanctuary2D Sanctuary=>GameObject.Find("Red_Egg_Sanctuary").GetComponent<RedEggSanctuary2D>();
        private static LevelExit2D Exit=>UnityEngine.Object.FindAnyObjectByType<LevelExit2D>(FindObjectsInactive.Include);
        private static PushableBoulder2D Boulder=>GameObject.Find("Basalt_Alignment_Boulder").GetComponent<PushableBoulder2D>();
        private static MagmaSalamander2D Enemy(int i)=>GameObject.Find("Magma_Salamander_"+i).GetComponent<MagmaSalamander2D>();
        private static LavaGeyser2D Fire=>GameObject.Find("Timed_Fire_Current").GetComponent<LavaGeyser2D>();
        [UnitySetUp] public IEnumerator SetUp()
        {
            ClearInput();GameProgression.ResetProgression();yield return SceneManager.LoadSceneAsync("Level_4_4");yield return null;
            _player=UnityEngine.Object.FindAnyObjectByType<PlayerController>();yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Safe entrance.");
        }
        [TearDown] public void TearDown() {ClearInput();GameProgression.ResetProgression();}
        private static void ClearInput() {VirtualInputBridge.MoveVector=Vector2.zero;VirtualInputBridge.ReleaseJump();VirtualInputBridge.ConsumeFrameTriggers();}
        private static IEnumerator WaitFor(Func<bool> condition,string message,float timeout=6f)
        {
            float end=Time.time+timeout;while(!condition()&&Time.time<end)yield return null;Assert.IsTrue(condition(),message);
        }
        [Test] public void DirectEntryHasFourAbilitiesCameraSixAndLockedFinalExit()
        {
            Assert.IsTrue(_player.IsDoubleJumpUnlocked);Assert.IsTrue(_player.IsGroundPoundUnlocked);Assert.IsTrue(_player.IsDashUnlocked);Assert.IsTrue(_player.IsRoarUnlocked);
            Assert.AreEqual(6f,Camera.main.orthographicSize);Assert.IsFalse(Exit.gameObject.activeSelf);Assert.AreEqual("Boss_Final",Exit.NextSceneName);
            Assert.IsFalse(Egg.IsRescued);Assert.IsFalse(GameProgression.IsWorldCompleted(4));Assert.AreEqual(0,GameProgression.RescuedEggCount);
        }
        [UnityTest] public IEnumerator RearWallPreventsFallingBehindEntrance()
        {
            int deaths=0;Action<Vector2> count=_=>deaths++;_player.OnRespawned+=count;
            VirtualInputBridge.MoveVector=Vector2.left;yield return new WaitForSeconds(2f);_player.OnRespawned-=count;
            Assert.AreEqual(0,deaths);Assert.Greater(_player.Rigidbody.position.x,-5f);
        }
        [UnityTest] public IEnumerator FirstColumnRequiresDoubleJumpWithRealInputs()
        {
            _player.RespawnAt(new Vector2(5.6f,.7f));yield return new WaitForSeconds(.15f);
            VirtualInputBridge.MoveVector=Vector2.right;VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.8f);
            Assert.Less(_player.Rigidbody.position.x,7f);VirtualInputBridge.MoveVector=Vector2.zero;VirtualInputBridge.ReleaseJump();
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Recover after the single jump.");
            yield return JumpTo(9.5f);Assert.Greater(_player.Rigidbody.position.y,2.7f);
        }
        [UnityTest] public IEnumerator OrdinaryLandingAndRoarDoNotBreakPillars()
        {
            _player.RespawnAt(new Vector2(27f,.7f));yield return new WaitForSeconds(.2f);VirtualInputBridge.TriggerRoar();yield return new WaitForSeconds(.3f);
            Assert.IsFalse(Trials.IsPoundGateOpen);Assert.IsTrue(GameObject.Find("Seismic_Pillar_0").activeSelf);Assert.IsTrue(GameObject.Find("Seismic_Pillar_1").activeSelf);
        }
        [UnityTest] public IEnumerator OnePoundBreaksBothPillarsAndStunsLowerGuardian()
        {
            _player.RespawnAt(new Vector2(27f,.7f));yield return Pound();
            Assert.IsTrue(Trials.IsPoundGateOpen);Assert.Less(_player.Rigidbody.position.y,-1.1f);Assert.IsTrue(Enemy(1).IsStunned);
        }
        [UnityTest] public IEnumerator DeathRestoresPillarsBeforeCheckpoint35()
        {
            var first=GameObject.Find("Seismic_Pillar_0").GetComponent<BreakableGround2D>();var second=GameObject.Find("Seismic_Pillar_1").GetComponent<BreakableGround2D>();
            _player.RespawnAt(new Vector2(27f,.7f));yield return Pound();_player.KillAndRespawn();yield return new WaitForFixedUpdate();
            Assert.IsFalse(first.IsBroken);Assert.IsFalse(second.IsBroken);Assert.IsFalse(Trials.IsPoundGateOpen);
        }
        [UnityTest] public IEnumerator Checkpoint35PreservesPoundTrialAfterDeath()
        {
            _player.RespawnAt(new Vector2(27f,.7f));yield return Pound();_player.RespawnAt(new Vector2(35f,.7f));yield return new WaitForSeconds(.15f);
            _player.KillAndRespawn();yield return new WaitForFixedUpdate();Assert.IsTrue(Trials.IsPoundGateOpen);Assert.AreEqual(35f,_player.Rigidbody.position.x,.3f);
        }
        [UnityTest] public IEnumerator AirDashBreaksGridAndDeathResetsUpcomingTrial()
        {
            var grid=GameObject.Find("Air_Dash_Trial_Grid").GetComponent<DashBreakableBarrier2D>();
            _player.RespawnAt(new Vector2(38.2f,.7f));yield return new WaitForSeconds(.15f);yield return StunGuard(2);yield return WalkTo(43.4f);yield return FireWindow();yield return JumpDash();yield return WalkTo(53.6f);
            Assert.IsTrue(grid.IsBroken);_player.SetCheckpoint(new Vector2(35f,.7f));_player.KillAndRespawn();yield return new WaitForFixedUpdate();Assert.IsFalse(grid.IsBroken);
        }
        [UnityTest] public IEnumerator ActiveFireDamagesAlmaDuringDash()
        {
            _player.RespawnAt(new Vector2(43.4f,.7f));yield return WaitFor(()=>Fire.IsErupting,"Wait for an active current.");
            int deaths=0;Action<Vector2> count=_=>deaths++;_player.OnRespawned+=count;
            _player.Rigidbody.position=new Vector2(43.5f,3.1f);_player.GroundDetector.ResetGroundState();VirtualInputBridge.MoveVector=Vector2.right;VirtualInputBridge.TriggerDash();
            yield return WaitFor(()=>_player.StateMachine.CurrentStateType==PlayerStateEnum.Dash,"Dash starts outside the active current.",.2f);
            yield return WaitFor(()=>deaths>0,"Active fire must damage even during Dash.",.5f);_player.OnRespawned-=count;
        }
        [UnityTest] public IEnumerator RoarAlignsBasaltAndCheckpoint75PreservesIt()
        {
            _player.RespawnAt(new Vector2(53.6f,.7f));yield return FaceRight();VirtualInputBridge.TriggerRoar();yield return WaitFor(()=>Boulder.IsSolidified,"Roar must align the bridge.");
            Assert.AreEqual(61f,Boulder.transform.position.x,.05f);
            _player.RespawnAt(new Vector2(75f,.7f));yield return new WaitForSeconds(.2f);_player.KillAndRespawn();yield return new WaitForFixedUpdate();
            Assert.IsTrue(Boulder.IsSolidified);Assert.AreEqual(75f,_player.Rigidbody.position.x,.3f);Assert.IsFalse(Egg.IsRescued);
        }
        [UnityTest] public IEnumerator RedRescueCalmsCombatShowsFourEggsAndPersistsWithoutCompletingWorld()
        {
            SeedPreviousEggs();_player.RespawnAt(new Vector2(90f,.7f));yield return WaitFor(()=>Egg.IsRescued,"Physical contact rescues the Red Egg.");
            Assert.AreEqual(4,GameProgression.RescuedEggCount);Assert.IsTrue(Sanctuary.IsCalm);Assert.IsTrue(Exit.gameObject.activeSelf);
            Assert.IsFalse(GameObject.Find("--- LEVEL ---").transform.Find("Temple_Combat").gameObject.activeSelf);
            Assert.IsFalse(GameObject.Find("--- LEVEL ---").transform.Find("Temple_Fire_Currents").gameObject.activeSelf);
            yield return WaitFor(()=>Sanctuary.GuardianRevealed,"The Thief King emerges after the quiet rescue beat.");
            for(int i=1;i<=4;i++)Assert.IsTrue(GameObject.Find("Carried_Egg_"+i).activeInHierarchy);
            _player.KillAndRespawn();Assert.AreEqual(90f,_player.Rigidbody.position.x,.3f);Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            yield return SceneManager.LoadSceneAsync("Level_4_4");yield return null;Assert.IsTrue(Egg.IsRescued);Assert.IsTrue(Sanctuary.IsCalm);Assert.AreEqual(4,GameProgression.RescuedEggCount);
        }
        [UnityTest] public IEnumerator PreviousLevelExitLoadsTheAntechamber()
        {
            yield return SceneManager.LoadSceneAsync("Level_4_3");yield return null;_player=UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            _player.RespawnAt(new Vector2(110f,5.3f));yield return WaitFor(()=>SceneManager.GetActiveScene().name=="Level_4_4","4-3 exit loads 4-4.");
        }
        [UnityTest] public IEnumerator CompleteFourTrialRouteRescuesRedEggWithoutDeaths()
        {
            SeedPreviousEggs();int deaths=0;Action<Vector2> count=_=>deaths++;_player.OnRespawned+=count;
            try
            {
                yield return WalkTo(5.6f);yield return JumpTo(9.5f);yield return WalkTo(11.2f);yield return JumpTo(15.5f);
                yield return WalkTo(16.4f);yield return JumpTo(18f,false);yield return StunGuard(0);
                yield return WalkTo(27f);yield return Pound();yield return WalkTo(29.3f);yield return JumpTo(33.4f);yield return WalkTo(35f);
                yield return WalkTo(38.2f);yield return StunGuard(2);yield return WalkTo(43.4f);yield return FireWindow();yield return JumpDash();yield return WalkTo(53.6f);
                VirtualInputBridge.ReleaseJump();yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Land beyond the fire current.");yield return FaceRight();VirtualInputBridge.TriggerRoar();
                yield return WaitFor(()=>Boulder.IsSolidified,"Align the basalt support.");yield return WalkTo(57f);yield return JumpTo(61f);yield return WalkTo(62.5f);
                yield return JumpDash();yield return WalkTo(75f);VirtualInputBridge.ReleaseJump();yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Reach checkpoint 75.");
                yield return WalkTo(77.4f);yield return JumpDash();yield return WalkTo(88f);VirtualInputBridge.ReleaseJump();yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Land on the pedestal island.");
                yield return WalkTo(90f);yield return WaitFor(()=>Egg.IsRescued,"Complete the fourth rescue.");
                Assert.AreEqual(0,deaths);Assert.AreEqual(4,GameProgression.RescuedEggCount);Assert.IsTrue(Trials.IsPoundGateOpen);Assert.IsTrue(Boulder.IsSolidified);Assert.IsFalse(GameProgression.IsWorldCompleted(4));
            }
            finally{_player.OnRespawned-=count;ClearInput();}
        }
        private static void SeedPreviousEggs(){GameProgression.RescueEgg(EggType.GreenEgg);GameProgression.RescueEgg(EggType.BlueEgg);GameProgression.RescueEgg(EggType.PurpleEgg);}
        private IEnumerator FaceRight(){VirtualInputBridge.MoveVector=Vector2.right;yield return new WaitForFixedUpdate();yield return null;VirtualInputBridge.MoveVector=Vector2.zero;}
        private IEnumerator StunGuard(int i){yield return FaceRight();VirtualInputBridge.TriggerRoar();yield return WaitFor(()=>Enemy(i).IsStunned,"Roar must clear guard "+i);yield return new WaitForSeconds(.27f);}
        private IEnumerator Pound()
        {
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Stand above the pillars.");VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.18f);VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(()=>_player.GroundDetector.IsGrounded&&_player.Rigidbody.position.y<-1.1f,"One Pound must cross both pillars.");VirtualInputBridge.ReleaseJump();
        }
        private static IEnumerator FireWindow()
        {
            yield return WaitFor(()=>Fire.Phase!=GeyserPhase.Dormant,"Wait for the current fire cycle.");yield return WaitFor(()=>Fire.Phase==GeyserPhase.Dormant,"Dash during a full safe window.");
        }
        private IEnumerator WalkTo(float x)
        {
            float end=Time.time+6f;while(Mathf.Abs(_player.Rigidbody.position.x-x)>.2f&&Time.time<end){VirtualInputBridge.MoveVector=Vector2.right*Mathf.Sign(x-_player.Rigidbody.position.x);yield return null;}
            VirtualInputBridge.MoveVector=Vector2.zero;Assert.Less(Mathf.Abs(_player.Rigidbody.position.x-x),.35f,"Cannot reach "+x+"; player="+_player.Rigidbody.position);yield return new WaitForSeconds(.12f);
        }
        private IEnumerator JumpTo(float x,bool doubleJump=true)
        {
            VirtualInputBridge.MoveVector=Vector2.right;VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.28f);if(doubleJump)VirtualInputBridge.TriggerJump();
            yield return WalkTo(x);VirtualInputBridge.ReleaseJump();yield return WaitFor(()=>_player.GroundDetector.IsGrounded,"Land at "+x+"; player="+_player.Rigidbody.position);
        }
        private IEnumerator JumpDash()
        {
            VirtualInputBridge.MoveVector=Vector2.right;VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.34f);VirtualInputBridge.TriggerJump();yield return new WaitForSeconds(.3f);VirtualInputBridge.TriggerDash();
            yield return WaitFor(()=>_player.StateMachine.CurrentStateType==PlayerStateEnum.Dash,"Air Dash starts.");yield return WaitFor(()=>_player.StateMachine.CurrentStateType!=PlayerStateEnum.Dash,"Air Dash ends.");
        }
    }
}
