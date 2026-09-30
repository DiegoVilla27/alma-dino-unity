using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerDashState : IPlayerState
    {
        private readonly PlayerController _player;
        private float _timer;

        public PlayerStateEnum StateType => PlayerStateEnum.Dash;

        public PlayerDashState(PlayerController player)
        {
            _player = player;
        }

        public void Enter()
        {
            _player.ConsumeAirDash();
            _timer = 0f;

            _player.Rigidbody.gravityScale = 0f;

            float distance = _player.Config != null ? _player.Config.DashDistance : 6.0f;
            float duration = _player.Config != null ? _player.Config.DashDuration : 0.2f;
            float dashSpeed = distance / Mathf.Max(0.01f, duration);

            int direction = (int)_player.FacingDirection;
            _player.SetVelocity(new Vector2(direction * dashSpeed, 0f));
        }

        public void Exit()
        {
            _player.ResetGravityScale();
        }

        public void UpdateLogic(float deltaTime)
        {
            _timer += deltaTime;
            float duration = _player.Config != null ? _player.Config.DashDuration : 0.2f;

            if (_timer >= duration)
            {
                if (_player.GroundDetector.IsGrounded)
                    _player.StateMachine.ChangeState(PlayerStateEnum.Run);
                else
                    _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            // Mantener velocidad y gravedad congelada en Y durante el dash
            float distance = _player.Config != null ? _player.Config.DashDistance : 6.0f;
            float duration = _player.Config != null ? _player.Config.DashDuration : 0.2f;
            float dashSpeed = distance / Mathf.Max(0.01f, duration);
            int direction = (int)_player.FacingDirection;
            _player.SetVelocity(new Vector2(direction * dashSpeed, 0f));
        }
    }
}
