using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerRunState : IPlayerState
    {
        private readonly PlayerController _player;

        public PlayerStateEnum StateType => PlayerStateEnum.Run;

        public PlayerRunState(PlayerController player)
        {
            _player = player;
        }

        public void Enter() { }
        public void Exit() { }

        public void UpdateLogic(float deltaTime)
        {
            if (_player.JumpBufferTimer > 0f)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Jump);
                return;
            }

            if (_player.Input.DashDown && _player.IsDashUnlocked && (_player.Config == null || _player.Config.CanDash))
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Dash);
                return;
            }

            if (_player.Input.RoarDown && _player.IsRoarUnlocked && (_player.Config == null || _player.Config.CanRoar))
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Roar);
                return;
            }

            if (!_player.GroundDetector.IsGrounded)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
                return;
            }

            if (Mathf.Abs(_player.Input.MoveVector.x) <= 0.05f)
            {
                _player.StateMachine.ChangeState(PlayerStateEnum.Idle);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            float targetSpeed = _player.Input.MoveVector.x * (_player.Config != null ? _player.Config.MoveSpeed : 8.5f);
            float accelRate = _player.Config != null ? _player.Config.MoveSpeed / Mathf.Max(0.01f, _player.Config.AccelerationTime) : 120f;
            _player.AccelerateHorizontally(targetSpeed, accelRate);
        }
    }
}
