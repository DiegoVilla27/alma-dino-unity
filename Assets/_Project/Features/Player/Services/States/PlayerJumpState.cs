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
            if (!_player.IsBouncing)
            {
                float jumpForce = _player.Config != null ? _player.Config.JumpForce : 8.2f;
                _player.SetVelocityY(jumpForce);
            }
        }

        public void Exit()
        {
            _player.ClearBouncing();
        }

        public void UpdateLogic(float deltaTime)
        {
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
            float moveSpeed = _player.Config != null ? _player.Config.MoveSpeed : 7.0f;
            float targetSpeed = _player.Input.MoveVector.x * moveSpeed;
            float airAccel = _player.Config != null ? moveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime * 1.3f) : 70f;
            _player.AccelerateHorizontally(targetSpeed, airAccel);

            // Jump Cut: solo aplica a saltos manuales del jugador, NUNCA cuando es un impulso de hongo/rebote
            if (!_player.IsBouncing && !_player.Input.JumpHeld && _player.Rigidbody.linearVelocity.y > 0f)
            {
                float cutMult = _player.Config != null ? _player.Config.JumpCutGravityMultiplier : 2.4f;
                float baseGravScale = _player.Config != null ? _player.Config.GravityScale : 2.2f;
                float extraGravity = Physics2D.gravity.y * baseGravScale * (cutMult - 1f) * fixedDeltaTime;
                _player.SetVelocityY(_player.Rigidbody.linearVelocity.y + extraGravity);
            }
        }
    }
}
