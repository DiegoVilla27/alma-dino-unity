using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerJumpState : IPlayerState
    {
        private readonly PlayerController _player;

        public PlayerStateEnum StateType => PlayerStateEnum.Jump;

        public PlayerJumpState(PlayerController player)
        {
            _player = player;
        }

        public void Enter()
        {
            _player.ConsumeJumpBuffer();
            _player.ConsumeCoyoteTime();
            float jumpForce = _player.Config != null ? _player.Config.JumpForce : 14.0f;
            _player.SetVelocityY(jumpForce);
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

            if (_player.Input.JumpDown && _player.HasDoubleJump)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.DoubleJump);
                return;
            }

            if (_player.Rigidbody.linearVelocity.y <= 0f)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            // Control horizontal en el aire
            float targetSpeed = _player.Input.MoveVector.x * (_player.Config != null ? _player.Config.MoveSpeed : 8.5f);
            float airAccel = _player.Config != null ? _player.Config.MoveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime * 1.3f) : 90f;
            _player.AccelerateHorizontally(targetSpeed, airAccel);

            // Jump Cut: si se suelta el botón de salto antes de la cima, se aplica gravedad incrementada
            if (!_player.Input.JumpHeld && _player.Rigidbody.linearVelocity.y > 0f)
            {
                float cutMult = _player.Config != null ? _player.Config.JumpCutGravityMultiplier : 2.6f;
                float extraGravity = Physics2D.gravity.y * (cutMult - 1f) * fixedDeltaTime;
                _player.SetVelocityY(_player.Rigidbody.linearVelocity.y + extraGravity);
            }
        }
    }
}
