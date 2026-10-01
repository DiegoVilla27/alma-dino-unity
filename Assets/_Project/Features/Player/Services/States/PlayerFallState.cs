using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerFallState : IPlayerState
    {
        private readonly PlayerController _player;

        public PlayerStateEnum StateType => PlayerStateEnum.Fall;

        public PlayerFallState(PlayerController player)
        {
            _player = player;
        }

        public void Enter()
        {
            _player.ClearBouncing();
        }
        public void Exit() => _player.ResetGravityScale();

        public void UpdateLogic(float deltaTime)
        {
            if (_player.GroundDetector.IsGrounded)
            {
                if (_player.JumpBufferTimer > 0f)
                    _player.StateMachine.ChangeState(PlayerStateEnum.Jump);
                else if (Mathf.Abs(_player.Input.MoveVector.x) > 0.05f)
                    _player.StateMachine.ChangeState(PlayerStateEnum.Run);
                else
                    _player.StateMachine.ChangeState(PlayerStateEnum.Idle);
                return;
            }
            // Coyote Time Jump
            if (_player.CoyoteTimer > 0f && _player.JumpBufferTimer > 0f)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Jump);
                return;
            }

            if (_player.Input.JumpDown && _player.HasDoubleJump)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.DoubleJump);
                return;
            }

            if (_player.Input.RoarDown && _player.IsRoarUnlocked && (_player.Config == null || _player.Config.CanRoar))
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Roar);
                return;
            }

            if (_player.Input.DashDown && _player.CanAirDash)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Dash);
                return;
            }

            if (_player.Input.GroundPoundDown && _player.IsGroundPoundUnlocked && (_player.Config == null || _player.Config.CanGroundPound))
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.GroundPound);
                return;
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            // Control horizontal en el aire
            float moveSpeed = _player.Config != null ? _player.Config.MoveSpeed : 7.0f;
            float targetSpeed = _player.Input.MoveVector.x * moveSpeed;
            float airAccel = _player.Config != null ? moveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime * 1.3f) : 70f;
            _player.AccelerateHorizontally(targetSpeed, airAccel);

            float fallMultiplier = _player.Config != null ? _player.Config.FallGravityMultiplier : 1.8f;
            float baseGravity = _player.Config != null ? _player.Config.GravityScale : 2.2f;
            float maxFallSpeed = _player.Config != null ? _player.Config.MaxFallSpeed : 20f;
            float velocityY = Mathf.Max(_player.Rigidbody.linearVelocity.y, -maxFallSpeed);
            _player.SetVelocityY(velocityY);

            // Reduce the final gravity step so integration cannot exceed terminal speed.
            float remainingSpeed = Mathf.Max(0f, maxFallSpeed + velocityY);
            float gravityStep = Mathf.Max(0.0001f, -Physics2D.gravity.y * fixedDeltaTime);
            _player.Rigidbody.gravityScale = Mathf.Min(baseGravity * fallMultiplier, remainingSpeed / gravityStep);
        }
    }
}
