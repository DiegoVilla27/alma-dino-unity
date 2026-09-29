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

        public void Enter() { }
        public void Exit() { }

        public void UpdateLogic(float deltaTime)
        {
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

            if (_player.GroundDetector.IsGrounded)
            {
                if (Mathf.Abs(_player.Input.MoveVector.x) > 0.05f)
                    _player.StateMachine.ChangeState(PlayerStateEnum.Run);
                else
                    _player.StateMachine.ChangeState(PlayerStateEnum.Idle);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            // Control horizontal en el aire
            float targetSpeed = _player.Input.MoveVector.x * (_player.Config != null ? _player.Config.MoveSpeed : 8.5f);
            float airAccel = _player.Config != null ? _player.Config.MoveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime * 1.3f) : 90f;
            _player.AccelerateHorizontally(targetSpeed, airAccel);

            // Gravedad de caída incrementada para feeling ágil tipo Celeste
            float fallMult = _player.Config != null ? _player.Config.FallGravityMultiplier : 1.9f;
            float extraGravity = Physics2D.gravity.y * (fallMult - 1f) * fixedDeltaTime;
            _player.SetVelocityY(_player.Rigidbody.linearVelocity.y + extraGravity);

            // Clamp a velocidad terminal
            float maxFall = _player.Config != null ? _player.Config.MaxFallSpeed : 20.0f;
            if (_player.Rigidbody.linearVelocity.y < -maxFall)
            {
                _player.SetVelocityY(-maxFall);
            }
        }
    }
}
