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
            _player.HasDoubleJump = false;
            _player.ConsumeJumpBuffer();
            float force = _player.Config != null ? _player.Config.DoubleJumpForce : 12.0f;
            _player.SetVelocityY(force);
        }

        public void Exit() { }

        public void UpdateLogic(float deltaTime)
        {
            if (_player.Input.DashDown && _player.CanAirDash)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Dash);
                return;
            }

            if (_player.Input.GroundPoundDown && _player.Config != null && _player.Config.CanGroundPound)
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
            float targetSpeed = _player.Input.MoveVector.x * (_player.Config != null ? _player.Config.MoveSpeed : 8.5f);
            float airAccel = _player.Config != null ? _player.Config.MoveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime * 1.3f) : 90f;
            _player.AccelerateHorizontally(targetSpeed, airAccel);

            if (!_player.Input.JumpHeld && _player.Rigidbody.linearVelocity.y > 0f)
            {
                float cutMult = _player.Config != null ? _player.Config.JumpCutGravityMultiplier : 2.6f;
                float extraGravity = Physics2D.gravity.y * (cutMult - 1f) * fixedDeltaTime;
                _player.SetVelocityY(_player.Rigidbody.linearVelocity.y + extraGravity);
            }
        }
    }
}
