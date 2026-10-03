using System.Collections;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Boss.Models;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class WorldOnePrefabPlayTests
    {
        [TearDown]
        public void Cleanup()
        {
            VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.ReleaseJump();
            VirtualInputBridge.ConsumeFrameTriggers();
            GameProgression.ResetProgression();
        }

        [UnityTest] public IEnumerator Level11LoadsAndMoves() => CheckLevel("Level_1_1");
        [UnityTest] public IEnumerator Level12LoadsAndMoves() => CheckLevel("Level_1_2");
        [UnityTest] public IEnumerator Level13LoadsAndMoves() => CheckLevel("Level_1_3");
        [UnityTest] public IEnumerator Level14LoadsAndMoves() => CheckLevel("Level_1_4");

        private static IEnumerator CheckLevel(string scene)
        {
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync(scene);
            yield return new WaitForSeconds(.3f);
            var player = Object.FindAnyObjectByType<PlayerController>();
            Assert.IsNotNull(player);
            Assert.AreEqual(6f, Camera.main.orthographicSize);
            Assert.Greater(Object.FindObjectsByType<Checkpoint2D>(FindObjectsSortMode.None).Length, 0);
            Assert.IsNotNull(Object.FindAnyObjectByType<LevelExit2D>());
            float start = player.transform.position.x;
            VirtualInputBridge.MoveVector = Vector2.right;
            yield return new WaitForSeconds(.25f);
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.Greater(player.transform.position.x, start + .1f);
            Assert.AreEqual(0, Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
        }

        [UnityTest]
        public IEnumerator MonkeyPrefabRetainsHeadHitsAndVictoryPortal()
        {
            GameProgression.ResetProgression();
            yield return SceneManager.LoadSceneAsync("Boss_1");
            yield return new WaitForSeconds(.2f);
            var boss = Object.FindAnyObjectByType<GiantMonkeyBoss2D>();
            Assert.IsNotNull(boss);
            var exit = Object.FindAnyObjectByType<LevelExit2D>(FindObjectsInactive.Include);
            Assert.IsNotNull(exit);
            for (int i = 0; i < boss.MaxHits; i++)
            {
                boss.TransitionToState(BossStateEnum.TiredDescent);
                boss.OnHeadStomped();
            }
            Assert.IsTrue(boss.IsDefeated);
            yield return new WaitForSeconds(3f);
            Assert.IsTrue(exit.gameObject.activeSelf);
        }
    }
}
