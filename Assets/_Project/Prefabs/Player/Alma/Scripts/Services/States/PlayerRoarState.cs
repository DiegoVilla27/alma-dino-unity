using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Player.Services;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services.States
{
    public class PlayerRoarState : IPlayerState
    {
        private readonly PlayerController _player;
        private float _timer;

        public PlayerStateEnum StateType => PlayerStateEnum.Roar;

        private readonly Collider2D[] _hitBuffer = new Collider2D[16];
        private readonly ContactFilter2D _contactFilter = new ContactFilter2D();

        public PlayerRoarState(PlayerController player)
        {
            _player = player;
            _contactFilter.useTriggers = true;
        }

        public void Enter()
        {
            _timer = 0f;
            _player.SetVelocityX(0f);

            // Retroalimentación háptica / screen shake por el rugido
            _player.RequestCameraShake(0.25f, 0.2f);

            // Onda de choque hacia adelante (Zero-alloc query)
            float radius = _player.Config != null ? _player.Config.RoarRadius : 3f;
            float resonanceRange = _player.Config != null ? _player.Config.RoarResonanceRange : 8f;
            Vector2 forwardOrigin = _player.Rigidbody.position;
            Vector2 direction = Vector2.right * (int)_player.FacingDirection;
            float halfAngle = _player.Config != null ? _player.Config.RoarHalfAngle : 45f;
            _player.EmitRoar(forwardOrigin, direction);
            int hitCount = Physics2D.OverlapCircle(forwardOrigin, Mathf.Max(radius, resonanceRange), _contactFilter, _hitBuffer);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null || col.attachedRigidbody == _player.Rigidbody) continue;
                float targetRange = col.TryGetComponent<IRangedRoarReactive2D>(out var ranged) ? Mathf.Min(resonanceRange, ranged.RoarRange) : radius;
                if (!RoarTargeting.Contains(forwardOrigin, col.bounds.center, direction, targetRange, halfAngle)) continue;
                if (col.TryGetComponent<IRoarReactive2D>(out var reactive))
                { reactive.ReceiveRoar(direction); continue; }

                if (col.attachedRigidbody != null && col.attachedRigidbody.bodyType == RigidbodyType2D.Dynamic)
                {
                    Vector2 impulse = new Vector2((int)_player.FacingDirection * 12f, 4f);
                    col.attachedRigidbody.linearVelocity = impulse;
                }
            }

            Debug.Log("[PlayerRoarState] ¡ROAAAR! Onda de choque emitida.");
        }

        public void Exit() { }

        public void UpdateLogic(float deltaTime)
        {
            _timer += deltaTime;
            float duration = _player.Config != null ? _player.Config.RoarDuration : 0.25f;

            if (_timer >= duration)
            {
                if (_player.GroundDetector.IsGrounded)
                    _player.StateMachine.ChangeState(PlayerStateEnum.Idle);
                else
                    _player.StateMachine.ChangeState(PlayerStateEnum.Fall);
            }
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            _player.SetVelocityX(0f);
        }
    }
}
