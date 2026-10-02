using System;
using System.Collections;
using System.Linq;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Boss.Services;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class Boss3PlayTests
    {
        private PlayerController _player;
        private PterodactylBoss2D _boss;
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GameProgression.ResetProgression();
            ClearInput();
            yield return SceneManager.LoadSceneAsync("Boss_3");
            yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            _boss = UnityEngine.Object.FindAnyObjectByType<PterodactylBoss2D>();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, 2f, "Spawn must land safely.");
        }
        [TearDown]
        public void TearDown() { ClearInput(); Time.timeScale = 1f; GameProgression.ResetProgression(); }
        private static void ClearInput() { VirtualInputBridge.MoveVector = Vector2.zero; VirtualInputBridge.ReleaseJump(); }
        private static IEnumerator WaitFor(Func<bool> condition, float timeout, string message)
        {
            float end = Time.time + timeout;
            while (!condition() && Time.time < end) yield return null;
            Assert.IsTrue(condition(), message);
        }
        [UnityTest]
        public IEnumerator ArenaHasFourBranchesAndSharedCameraSize()
        {
            Assert.AreEqual(4, SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>()).Count(t => t.name.StartsWith("Arena_Branch_")));
            Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.IsTrue(_player.IsDashUnlocked);
            Assert.IsFalse(GameProgression.IsWorldCompleted(3));
            Assert.IsFalse(UnityEngine.Object.FindObjectsByType<LevelExit2D>(FindObjectsInactive.Include)[0].gameObject.activeSelf);
            yield return null;
        }
        [UnityTest]
        public IEnumerator DoubleJumpCanReachAllFourBranches()
        {
            _boss.enabled = false;
            foreach (float x in new[] { -6f, 0f, 6f, 12f })
            {
                float side = Mathf.Sign(x - _player.Rigidbody.position.x);
                VirtualInputBridge.MoveVector = Vector2.right * side;
                VirtualInputBridge.TriggerJump();
                yield return new WaitForSeconds(.34f);
                VirtualInputBridge.TriggerJump();
                yield return WaitFor(() => Mathf.Abs(_player.Rigidbody.position.x - x) < .25f, 2f, "Jump must reach branch " + x);
                VirtualInputBridge.MoveVector = Vector2.zero;
                yield return WaitFor(() => _player.GroundDetector.IsGrounded, 2f, "Land on branch " + x);
                VirtualInputBridge.ReleaseJump();
                yield return new WaitForSeconds(.15f);
            }
        }
        [UnityTest]
        public IEnumerator BodyContactKillsWithoutDamagingBoss()
        {
            _boss.enabled = false;
            _boss.Fight.BeginWarning(); _boss.Fight.BeginDive();
            _boss.GetComponent<Rigidbody2D>().position = new Vector2(-2.2f, .7f);
            int deaths = 0; Action<Vector2> count = _ => deaths++;
            _player.OnRespawned += count;
            yield return WaitFor(() => deaths > 0, 1f, "Body must deal contact damage.");
            _player.OnRespawned -= count;
            Assert.AreEqual(0, _boss.Fight.Hits);
        }
        [UnityTest]
        public IEnumerator Level34PortalLoadsBoss3AfterPurpleEggRescue()
        {
            yield return SceneManager.LoadSceneAsync("Level_3_4");
            yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            _player.RespawnAt(new Vector2(118f, 20.2f));
            yield return WaitFor(() => GameProgression.IsEggRescued(EggType.PurpleEgg), 1f, "Rescue the Purple Egg by contact.");
            _player.RespawnAt(new Vector2(123f, 21f));
            yield return WaitFor(() => SceneManager.GetActiveScene().name == "Boss_3", 5f, "The 3-4 exit must load the new boss arena.");
            Assert.IsNotNull(UnityEngine.Object.FindAnyObjectByType<PterodactylBoss2D>());
        }
        [UnityTest]
        public IEnumerator OrdinaryHeadContactKillsWithoutDamagingBoss()
        {
            _boss.enabled = false;
            _boss.Fight.BeginWarning(); _boss.Fight.BeginDive();
            _boss.GetComponent<Rigidbody2D>().position = new Vector2(0f, .7f);
            int deaths = 0; Action<Vector2> count = _ => deaths++;
            _player.OnRespawned += count;
            yield return WaitFor(() => deaths > 0, 1f, "Unprotected head contact must deal damage.");
            _player.OnRespawned -= count;
            Assert.AreEqual(0, _boss.Fight.Hits);
        }
        [UnityTest]
        public IEnumerator GroundDashCannotDamageHead()
        {
            _boss.enabled = false;
            _boss.Fight.BeginWarning(); _boss.Fight.BeginDive();
            _boss.GetComponent<Rigidbody2D>().position = new Vector2(2.5f, .7f);
            VirtualInputBridge.MoveVector = Vector2.right;
            VirtualInputBridge.TriggerDash();
            yield return new WaitForSeconds(.4f);
            Assert.AreEqual(0, _boss.Fight.Hits);
        }
        [UnityTest]
        public IEnumerator SingleJumpDashCannotReachTheDiveHead()
        {
            yield return WaitNearCenterUntilWarning();
            yield return WaitFor(() => _boss.IsDiving, 2f, "Wait for the actual dive.");
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(.34f);
            VirtualInputBridge.MoveVector = Vector2.right * -_boss.DiveDirection;
            yield return new WaitForFixedUpdate();
            VirtualInputBridge.TriggerDash();
            yield return new WaitForSeconds(.4f);
            Assert.AreEqual(0, _boss.Fight.Hits, "The counter height must require Double Jump.");
        }
        [UnityTest]
        public IEnumerator ThreeRealAirDashCountersWinAndPreserveHitsAfterDeath()
        {
            int deaths = 0; Action<Vector2> count = _ => deaths++;
            _player.OnRespawned += count;
            try
            {
                for (int hit = 1; hit <= 3; hit++)
                {
                    yield return WaitNearCenterUntilWarning();
                    yield return WaitFor(() => _boss.IsDiving, 2f, "Warning must lead into a real dive.");
                    float attackSide = -_boss.DiveDirection;
                    VirtualInputBridge.TriggerJump();
                    yield return new WaitForSeconds(.34f);
                    VirtualInputBridge.TriggerJump();
                    yield return new WaitForSeconds(.20f);
                    VirtualInputBridge.MoveVector = Vector2.right * attackSide;
                    yield return new WaitForFixedUpdate();
                    VirtualInputBridge.TriggerDash();
                    yield return WaitFor(() => _boss.Fight.Hits == hit, 1f,
                        "Air Dash must strike head " + hit + "; player=" + _player.Rigidbody.position + " boss=" + _boss.transform.position);
                    ClearInput();
                    Assert.AreEqual(hit, Enumerable.Range(1, 3).Count(i => !GameObject.Find("--- LEVEL ---").transform.Find("Arena_Branch_" + i).gameObject.activeSelf));
                    if (hit < 3)
                    {
                        yield return ReturnToCenter();
                        if (hit == 1)
                        {
                            _player.KillAndRespawn();
                            yield return new WaitForFixedUpdate();
                            Assert.AreEqual(1, _boss.Fight.Hits);
                            Assert.AreEqual(PterodactylPhase.Wind, _boss.Fight.Phase);
                        }
                    }
                }
                Assert.AreEqual(1, deaths, "Only the explicit reset between successful counters may kill Alma.");
                Assert.IsTrue(GameProgression.IsWorldCompleted(3));
                var exit = UnityEngine.Object.FindObjectsByType<LevelExit2D>(FindObjectsInactive.Include)[0];
                Assert.IsTrue(exit.gameObject.activeSelf);
                Assert.AreEqual("Level_4_1", exit.NextSceneName);
                yield return ReturnToCenter();
                yield return WaitFor(() => exit.IsCompleted, 2f, "Victory portal must accept physical contact.");
            }
            finally { _player.OnRespawned -= count; }
        }
        private IEnumerator WaitNearCenterUntilWarning()
        {
            float deadline = Time.time + 8f;
            while (_boss.Fight.Phase != PterodactylPhase.Warning && Time.time < deadline)
            {
                float x = _player.Rigidbody.position.x;
                VirtualInputBridge.MoveVector = Mathf.Abs(x) > .15f ? Vector2.right * -Mathf.Sign(x) : Vector2.zero;
                yield return null;
            }
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.AreEqual(PterodactylPhase.Warning, _boss.Fight.Phase);
        }
        private IEnumerator ReturnToCenter()
        {
            float deadline = Time.time + 3f;
            while ((!_player.GroundDetector.IsGrounded || Mathf.Abs(_player.Rigidbody.position.x) > .2f) && Time.time < deadline)
            {
                float x = _player.Rigidbody.position.x;
                VirtualInputBridge.MoveVector = Mathf.Abs(x) > .15f ? Vector2.right * -Mathf.Sign(x) : Vector2.zero;
                yield return null;
            }
            ClearInput();
            Assert.Less(Mathf.Abs(_player.Rigidbody.position.x), .3f);
            Assert.IsTrue(_player.GroundDetector.IsGrounded);
        }
    }
}
