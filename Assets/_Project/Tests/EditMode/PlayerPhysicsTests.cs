using System.Collections.Generic;
using System.Reflection;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using AlmaDino.Features.Player.ScriptableObjects;
using AlmaDino.Shared.Data;
using NUnit.Framework;
using UnityEngine;

namespace AlmaDino.Tests.EditMode
{
    public class PlayerPhysicsTests
    {
        private GameObject _playerObject;
        private PlayerController _player;
        private PlayerInputReader _reader;
        private AlmaPhysicsConfigSO _config;
        private SimulationMode2D _previousSimulationMode;
        private Vector2 _previousGravity;
        private const float Step = 0.02f;

        [SetUp]
        public void SetUp()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            _previousSimulationMode = Physics2D.simulationMode;
            _previousGravity = Physics2D.gravity;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = new Vector2(0f, -9.81f);
            _config = ScriptableObject.CreateInstance<AlmaPhysicsConfigSO>();
            _playerObject = new GameObject("PhysicsTestPlayer");
            _playerObject.transform.position = new Vector3(0f, 10f, 0f);
            _player = _playerObject.AddComponent<PlayerController>();
            _reader = _playerObject.GetComponent<PlayerInputReader>();
            SetField(_player, "_config", _config);
            SetField(_player, "_rigidbody", _playerObject.GetComponent<Rigidbody2D>());
            SetField(_player, "_inputReader", _reader);
            SetField(_player, "_groundDetector", _playerObject.GetComponent<GroundDetector2D>());
            SetField(_player, "_visualRoot", _playerObject.transform);
            SetField(_player, "_doubleJumpUnlocked", true);
            SetField(_player, "_dashUnlocked", true);
            Invoke(_player.GroundDetector, "Awake");
            Invoke(_player, "ConfigurePhysics");
            Invoke(_player, "InitializeStateMachine");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_config);
            Physics2D.simulationMode = _previousSimulationMode;
            Physics2D.gravity = _previousGravity;
        }

        [Test]
        public void JumpPress_BetweenPhysicsSteps_IsRetainedAndAppliedOnce()
        {
            _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            SetField(_player, "_coyoteTimer", _config.CoyoteTime);
            RenderInput(new PlayerFrameInput { JumpDown = true, JumpHeld = true });
            RenderInput(new PlayerFrameInput { JumpHeld = true });
            Assert.AreEqual(0f, _player.Rigidbody.linearVelocity.y,
                "Render updates must not apply a physical impulse.");
            Invoke(_player, "FixedUpdate");
            Assert.AreEqual(_config.JumpForce, _player.Rigidbody.linearVelocity.y, 0.001f);
            Invoke(_player, "FixedUpdate");
            Assert.AreEqual(PlayerStateEnum.Jump, _player.StateMachine.CurrentStateType,
                "One press must not also consume the double jump.");
        }

        [Test]
        public void DoubleJump_DuringAscent_DoesNotStackAnUnboundedImpulse()
        {
            _player.SetVelocityY(_config.JumpForce);
            _player.StateMachine.ChangeState(PlayerStateEnum.DoubleJump);
            Assert.AreEqual(_config.JumpForce, _player.Rigidbody.linearVelocity.y, 0.001f);
        }

        [Test]
        public void Dash_CoversConfiguredDistance_AndKeepsItsInitialDirection()
        {
            _player.StateMachine.ChangeState(PlayerStateEnum.Dash);
            SetField(_player, "_facingDirection", FacingDirection2D.Left);
            float startX = _player.Rigidbody.position.x;
            for (int i = 0; i < 10; i++)
            {
                _player.StateMachine.PhysicsUpdate(Step);
                Physics2D.Simulate(Step);
                _player.StateMachine.UpdateLogic(Step);
            }
            Assert.AreEqual(_config.DashDistance, _player.Rigidbody.position.x - startX, 0.01f);
            Assert.AreEqual(PlayerStateEnum.Fall, _player.StateMachine.CurrentStateType);
        }

        [Test]
        public void DashRefill_RestoresDoubleJumpAsWellAsDash()
        {
            _player.HasDoubleJump = false;
            _player.ConsumeAirDash();
            _player.RefreshAirDash();
            Assert.IsTrue(_player.HasDoubleJump);
            Assert.IsTrue(_player.CanAirDash);
        }

        [Test]
        public void FullJump_AtTopSpeed_HasExpectedHeightAndRange()
        {
            RenderInput(new PlayerFrameInput { MoveVector = Vector2.right, JumpHeld = true });
            _player.SetVelocityX(_config.MoveSpeed);
            _player.StateMachine.ChangeState(PlayerStateEnum.Jump);
            Vector2 start = _player.Rigidbody.position;
            float peak = start.y;
            for (int i = 0; i < 100; i++)
            {
                Invoke(_player, "FixedUpdate");
                Physics2D.Simulate(Step);
                peak = Mathf.Max(peak, _player.Rigidbody.position.y);
                if (_player.Rigidbody.position.y < start.y) break;
            }
            Assert.That(peak - start.y, Is.InRange(1.4f, 1.6f));
            Assert.That(_player.Rigidbody.position.x - start.x, Is.InRange(4.3f, 4.8f));
        }

        [Test]
        public void DoubleJump_AtApex_ClearsTheNormalJumpRange()
        {
            RenderInput(new PlayerFrameInput { MoveVector = Vector2.right, JumpHeld = true });
            _player.SetVelocityX(_config.MoveSpeed);
            _player.StateMachine.ChangeState(PlayerStateEnum.Jump);
            Vector2 start = _player.Rigidbody.position;
            float peak = start.y;
            bool usedDoubleJump = false;
            for (int i = 0; i < 100; i++)
            {
                if (!usedDoubleJump && _player.Rigidbody.linearVelocity.y <= 0f)
                {
                    _player.StateMachine.ChangeState(PlayerStateEnum.DoubleJump);
                    usedDoubleJump = true;
                }
                Invoke(_player, "FixedUpdate");
                Physics2D.Simulate(Step);
                peak = Mathf.Max(peak, _player.Rigidbody.position.y);
                if (_player.Rigidbody.position.y < start.y) break;
            }
            Assert.That(peak - start.y, Is.InRange(2.6f, 2.9f));
            Assert.That(_player.Rigidbody.position.x - start.x, Is.InRange(7.5f, 8.0f));
        }

        [Test]
        public void Fall_AtTerminalSpeed_DoesNotAcceleratePastTheLimit()
        {
            _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            _player.SetVelocityY(-_config.MaxFallSpeed);
            Invoke(_player, "FixedUpdate");
            Physics2D.Simulate(Step);
            Assert.AreEqual(-_config.MaxFallSpeed, _player.Rigidbody.linearVelocity.y, 0.001f);
        }

        [Test]
        public void DoubleJumpThenDash_CrossesThePlannedElevenMetreTutorialGap()
        {
            RenderInput(new PlayerFrameInput { MoveVector = Vector2.right, JumpHeld = true });
            _player.SetVelocityX(_config.MoveSpeed);
            _player.StateMachine.ChangeState(PlayerStateEnum.Jump);
            Vector2 start = _player.Rigidbody.position;
            bool usedDoubleJump = false;
            bool usedDash = false;
            for (int i = 0; i < 100; i++)
            {
                if (_player.Rigidbody.linearVelocity.y <= 0f && !usedDoubleJump)
                {
                    _player.StateMachine.ChangeState(PlayerStateEnum.DoubleJump);
                    usedDoubleJump = true;
                }
                else if (_player.Rigidbody.linearVelocity.y <= 0f && !usedDash)
                {
                    _player.StateMachine.ChangeState(PlayerStateEnum.Dash);
                    usedDash = true;
                }
                Invoke(_player, "FixedUpdate");
                Physics2D.Simulate(Step);
                if (_player.Rigidbody.position.y < start.y) break;
            }
            Assert.That(_player.Rigidbody.position.x - start.x, Is.InRange(13.0f, 14.3f));
        }

        [Test]
        public void AirborneRoar_IsAvailableForTheDocumentedPuzzles()
        {
            SetField(_player, "_roarUnlocked", true);
            _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            RenderInput(new PlayerFrameInput { RoarDown = true });
            Invoke(_player, "FixedUpdate");
            Assert.AreEqual(PlayerStateEnum.Roar, _player.StateMachine.CurrentStateType);
        }

        [Test]
        public void Wind_UsesAccelerationIndependentlyForEachMassAndGravityScale()
        {
            var windObject = new GameObject("TestWind");
            var firstObject = new GameObject("FirstWindBody");
            var secondObject = new GameObject("SecondWindBody");
            try
            {
                windObject.AddComponent<BoxCollider2D>();
                var wind = windObject.AddComponent<WindCurrentZone2D>();
                var first = firstObject.AddComponent<Rigidbody2D>();
                var second = secondObject.AddComponent<Rigidbody2D>();
                first.mass = 1f;
                first.gravityScale = 1f;
                second.mass = 3f;
                second.gravityScale = 2f;
                var bodies = (List<Rigidbody2D>)typeof(WindCurrentZone2D)
                    .GetField("_affectedBodies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wind);
                bodies.Add(first);
                bodies.Add(second);
                Invoke(wind, "FixedUpdate");
                Physics2D.Simulate(Step);
                Assert.AreEqual((22f - 9.81f * 0.5f) * Step, first.linearVelocity.y, 0.001f);
                Assert.AreEqual((22f - 9.81f) * Step, second.linearVelocity.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(windObject);
                Object.DestroyImmediate(firstObject);
                Object.DestroyImmediate(secondObject);
            }
        }

        private void RenderInput(PlayerFrameInput input)
        {
            SetField(_reader, "_currentInput", input);
            Invoke(_player, "Update");
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static void Invoke(object target, string name)
        {
            target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }
    }
}
