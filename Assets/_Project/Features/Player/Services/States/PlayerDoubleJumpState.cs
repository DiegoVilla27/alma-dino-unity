using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerDoubleJumpState : IPlayerState
    {
        private readonly PlayerController _player;

        public PlayerStateEnum StateType => PlayerStateEnum.DoubleJump;

        public PlayerDoubleJumpState(PlayerController player)
        {
            _player = player;
        }

        public void Enter()
        {
            _player.ResetGravityScale();
            _player.HasDoubleJump = false;
            _player.ConsumeJumpBuffer();
            float force = _player.Config != null ? _player.Config.DoubleJumpForce : 7.6f;
            float currentY = _player.Rigidbody.linearVelocity.y;
            float newVelocityY = Mathf.Max(currentY, force);
            _player.SetVelocityY(newVelocityY);
        }

        public void Exit() { }

        public void UpdateLogic(float deltaTime)
        {
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

            if (_player.Rigidbody.linearVelocity.y <= 0f)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            float moveSpeed = _player.Config != null ? _player.Config.MoveSpeed : 7.0f;
            float targetSpeed = _player.Input.MoveVector.x * moveSpeed;
            float airAccel = _player.Config != null ? moveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime * 1.3f) : 70f;
            _player.AccelerateHorizontally(targetSpeed, airAccel);

            float baseGravity = _player.Config != null ? _player.Config.GravityScale : 2.2f;
            float cutMultiplier = _player.Config != null ? _player.Config.JumpCutGravityMultiplier : 2.4f;
            _player.Rigidbody.gravityScale = !_player.Input.JumpHeld
                && _player.Rigidbody.linearVelocity.y > 0f ? baseGravity * cutMultiplier : baseGravity;
        }
    }
}
