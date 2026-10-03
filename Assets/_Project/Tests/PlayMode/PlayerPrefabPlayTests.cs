#if UNITY_EDITOR
using System.Collections;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class PlayerPrefabPlayTests
    {
        private Scene _scene;
        private Scene _previousScene;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.ReleaseJump();
            VirtualInputBridge.ConsumeFrameTriggers();
            GameProgression.ResetProgression();
            if (_previousScene.IsValid()) SceneManager.SetActiveScene(_previousScene);
            if (_scene.IsValid()) yield return SceneManager.UnloadSceneAsync(_scene);
        }

        [UnityTest]
        public IEnumerator StandalonePrefabKeepsDashPoundAndRoarDistinct()
        {
            GameProgression.ResetProgression();
            _previousScene = SceneManager.GetActiveScene();
            _scene = SceneManager.CreateScene("Player_Prefab_Test");
            SceneManager.SetActiveScene(_scene);
            var floor = new GameObject("Floor");
            floor.layer = 0; // Existing levels use the Default layer for solid ground.
            floor.transform.position = new Vector3(0, -1, 0);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/Alma/Player_Alma.prefab");
            Assert.IsNotNull(asset);
            var instance = Object.Instantiate(asset, new Vector3(0, 2, 0), Quaternion.identity);
            var player = instance.GetComponent<PlayerController>();
            player.UnlockAbility(AbilityType.DoubleJump);
            player.UnlockAbility(AbilityType.Dash);
            player.UnlockAbility(AbilityType.GroundPound);
            player.UnlockAbility(AbilityType.Roar);
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(player.GroundDetector.IsGrounded);
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(.12f);
            Assert.Greater(player.Rigidbody.linearVelocity.y, 0);
            VirtualInputBridge.ReleaseJump();
            yield return null;
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(.04f);
            Assert.AreEqual(PlayerStateEnum.DoubleJump, player.StateMachine.CurrentStateType);
            VirtualInputBridge.ReleaseJump();
            VirtualInputBridge.TriggerDash();
            yield return new WaitForSeconds(.06f);
            Assert.AreEqual(PlayerStateEnum.Dash, player.StateMachine.CurrentStateType);
            Assert.Greater(Mathf.Abs(player.Rigidbody.linearVelocity.x), 20);
            Assert.AreEqual(0, player.Rigidbody.linearVelocity.y, .1f);
            yield return new WaitForSeconds(.25f);
            VirtualInputBridge.TriggerGroundPound();
            yield return new WaitForSeconds(.04f);
            Assert.AreEqual(PlayerStateEnum.GroundPound, player.StateMachine.CurrentStateType);
            yield return new WaitForSeconds(.8f);
            int roars = 0;
            player.OnRoared += (_, _) => roars++;
            VirtualInputBridge.TriggerRoar();
            yield return new WaitForSeconds(.05f);
            Assert.AreEqual(PlayerStateEnum.Roar, player.StateMachine.CurrentStateType);
            Assert.AreEqual(1, roars);
        }
    }
}
#endif
