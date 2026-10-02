using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using AlmaDino.Features.Player.Services.States;
using UnityEngine;

namespace AlmaDino.Features.Player.Services
{
    public sealed class PlayerCollisionService2D
    {
        private readonly PlayerController _player;
        private readonly Collider2D[] _seismicHits = new Collider2D[32];
        public PlayerCollisionService2D(PlayerController player) => _player = player;

        public void HandleTrigger(Collider2D other)
        {
            if (_player.StateMachine.CurrentStateType == PlayerStateEnum.Dash && !_player.GroundDetector.IsGrounded
                && other.TryGetComponent<IDashStrikeReceiver2D>(out var receiver)
                && receiver.TryReceiveAirDash(_player.Rigidbody.linearVelocity))
            {
                _player.ApplyBounce(receiver.BounceVelocity, true);
                _player.SetVelocityX(0f);
                _player.RefreshAirDash();
                return;
            }
            if (other.TryGetComponent<IHazard2D>(out var hazard) && IsDangerous(hazard))
            {
                hazard.OnHazardTouch();
                _player.KillAndRespawn();
            }
        }

        public void HandleCollision(Collision2D collision)
        {
            var pound = _player.StateMachine.CurrentState as PlayerGroundPoundState;
            bool seismicLanding = pound != null && pound.IsDiving && LandedFromAbove(collision);
            if (seismicLanding) EmitSeismicShock();
            if (collision.collider.TryGetComponent<IHazard2D>(out var hazard) && IsDangerous(hazard))
            {
                hazard.OnHazardTouch();
                _player.KillAndRespawn();
                return;
            }
            if (seismicLanding)
            {
                if (collision.collider.TryGetComponent<IBreakable2D>(out var breakable))
                {
                    breakable.Break();
                    _player.GroundDetector.ResetGroundState();
                    _player.SetVelocityY(-(_player.Config != null ? _player.Config.GroundPoundSpeed : 22f));
                    _player.RequestCameraShake(0.3f, 0.15f);
                }
                else
                {
                    if (collision.collider.TryGetComponent<IGroundPoundReceiver2D>(out var receiver))
                        receiver.ReceiveGroundPound(_player.Rigidbody.position);
                    pound.RegisterLanding();
                }
            }
            else if (_player.StateMachine.CurrentStateType == PlayerStateEnum.Dash
                && collision.collider.TryGetComponent<IDashBreakable2D>(out var barrier))
                barrier.BreakWithDash();
        }

        private void EmitSeismicShock()
        {
            float radius = _player.Config != null ? _player.Config.GroundPoundShockRadius : 2f;
            var filter = new ContactFilter2D { useTriggers = true };
            int count = Physics2D.OverlapCircle(_player.Rigidbody.position, radius, filter, _seismicHits);
            for (int i = 0; i < count; i++)
                _seismicHits[i].GetComponentInParent<ISeismicReactive2D>()?.ReceiveSeismicShock();
        }

        private static bool IsDangerous(IHazard2D hazard)
            => !(hazard is IConditionalHazard2D conditional) || conditional.IsDangerous;

        private static bool LandedFromAbove(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y > 0.5f) return true;
            return false;
        }
    }
}
