using System;
using System.Collections;
using AlmaDino.Core.Utilities;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class Boss2PlayTests
    {
        private PlayerController _player;
        private PrehistoricArmadilloBoss2D _boss;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            VirtualInputBridge.MoveVector = Vector2.zero;
            yield return SceneManager.LoadSceneAsync("Boss_2");
            yield return null;
            _player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            _boss = UnityEngine.Object.FindAnyObjectByType<PrehistoricArmadilloBoss2D>();
        }

        [TearDown]
        public void TearDown()
        {
            VirtualInputBridge.MoveVector = Vector2.zero;
            VirtualInputBridge.ReleaseJump();
        }

        [UnityTest]
        public IEnumerator LeftRefugeIsReachableAndAllowsPoundAtLeftPillar()
            => ClimbAndPoundFromRefuge(-1f);

        [UnityTest]
        public IEnumerator RightRefugeIsReachableAndAllowsPoundAtRightPillar()
            => ClimbAndPoundFromRefuge(1f);

        private IEnumerator ClimbAndPoundFromRefuge(float side)
        {
            // Isolate the route from the attack cycle so both pillars can be checked independently.
            _boss.enabled = false;
            _player.RespawnAt(new Vector2(side * 5f, .7f));
            yield return WaitFor(() => _player.GroundDetector.IsGrounded, 1f, "Alma must land on the arena floor.");
            VirtualInputBridge.TriggerJump();
            yield return new WaitForSeconds(.18f);
            VirtualInputBridge.TriggerJump();
            yield return WaitFor(() => _player.GroundDetector.IsGrounded && _player.Rigidbody.position.y > 2.5f
                    && _player.StateMachine.CurrentStateType == PlayerStateEnum.Idle
                    && Mathf.Abs(_player.Rigidbody.linearVelocity.y) < .1f,
                2f, "Double jump must reach the refuge from the floor.");
            VirtualInputBridge.ReleaseJump();
            yield return new WaitForFixedUpdate();
            yield return null;
            float bossX = side * 8f;
            _boss.GetComponent<Rigidbody2D>().position = new Vector2(bossX, 1f);
            StunBoss();
            _boss.enabled = true;
            VirtualInputBridge.MoveVector = Vector2.right * side;
            VirtualInputBridge.TriggerJump();
            yield return WaitFor(() => Mathf.Abs(_player.Rigidbody.position.x - bossX) < .25f,
                1f, "Alma must jump beyond the refuge towards the exposed crown.");
            VirtualInputBridge.MoveVector = Vector2.zero;
            Assert.That(_player.GroundDetector.IsGrounded, Is.False,
                "Attack jump must stay airborne above the exposed crown: " + _player.Rigidbody.position);
            VirtualInputBridge.TriggerGroundPound();
            yield return WaitFor(() => _boss.Fight.Hits == 1, 1f,
                "Pound must reach the crown: player=" + _player.Rigidbody.position
                    + " state=" + _player.StateMachine.CurrentStateType + " boss=" + _boss.GetComponent<Rigidbody2D>().position
                    + " stunned=" + _boss.Fight.IsStunned);
        }

        private static IEnumerator WaitFor(Func<bool> condition, float timeout, string message)
        {
            float deadline = Time.time + timeout;
            while (!condition() && Time.time < deadline) yield return null;
            Assert.That(condition(), Is.True, message);
        }

        private void StunBoss()
        {
            _boss.Fight.BeginRoll();
            for (int i = 0; i < 3; i++) _boss.Fight.HitPillar();
        }

        [UnityTest]
        public IEnumerator ActualPoundCollisionDamagesStunnedCrown()
        {
            _player.RespawnAt(new Vector2(0f, 4f));
            StunBoss();
            float airborneDeadline = Time.time + .5f;
            while (_player.StateMachine.CurrentStateType != PlayerStateEnum.Fall && Time.time < airborneDeadline)
                yield return null;
            Assert.That(_player.StateMachine.CurrentStateType, Is.EqualTo(PlayerStateEnum.Fall));
            VirtualInputBridge.TriggerGroundPound();
            float deadline = Time.time + 2f;
            while (_boss.Fight.Hits == 0 && Time.time < deadline) yield return null;
            Assert.That(_boss.Fight.Hits, Is.EqualTo(1), "Physical Pound must reach boss receiver.");
        }

        [UnityTest]
        public IEnumerator OrdinaryLandingDoesNotDamageAndRespawnClearsCycle()
        {
            _player.RespawnAt(new Vector2(0f, 3f));
            StunBoss();
            yield return new WaitForSeconds(.5f);
            Assert.That(_boss.Fight.Hits, Is.Zero);
            _player.KillAndRespawn();
            yield return new WaitForFixedUpdate();
            Assert.That(_boss.Fight.IsStunned, Is.False);
            Assert.That(_boss.Fight.Bounces, Is.Zero);
            Assert.That(_boss.transform.position.x, Is.EqualTo(0f).Within(.01f));
        }
    }
}
