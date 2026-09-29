using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerGroundPoundState : IPlayerState
    {
        private readonly PlayerController _player;
        private float _timer;
        private bool _isDiving;

        public PlayerStateEnum StateType => PlayerStateEnum.GroundPound;

        public PlayerGroundPoundState(PlayerController player)
        {
            _player = player;
        }

        public void Enter()
        {
            _timer = 0f;
            _isDiving = false;
            _player.SetVelocity(Vector2.zero);
            _player.Rigidbody.gravityScale = 0f;
        }

        public void Exit()
        {
            _player.Rigidbody.gravityScale = 1f;
        }

        public void UpdateLogic(float deltaTime)
        {
            _timer += deltaTime;
            float windup = _player.Config != null ? _player.Config.GroundPoundWindup : 0.1f;

            if (!_isDiving && _timer >= windup)
            {
                _isDiving = true;
                float speed = _player.Config != null ? _player.Config.GroundPoundSpeed : 22.0f;
                _player.SetVelocity(new Vector2(0f, -speed));
            }

            if (_isDiving && _player.GroundDetector.IsGrounded)
            {
                // Impacto sísmico en el suelo: sacudida de pantalla fuerte
                _player.RequestCameraShake(0.45f, 0.25f);
                _player.StateMachine.ChangeState(PlayerStateEnum.Idle);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            if (!_isDiving)
            {
                _player.SetVelocity(Vector2.zero);
            }
            else
            {
                float speed = _player.Config != null ? _player.Config.GroundPoundSpeed : 22.0f;
                _player.SetVelocity(new Vector2(0f, -speed));
            }
        }
    }
}
