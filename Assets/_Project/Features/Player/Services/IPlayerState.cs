using AlmaDino.Features.Player.Models;

namespace AlmaDino.Features.Player.Services
{
    public interface IPlayerState
    {
        PlayerStateEnum StateType { get; }
        void Enter();
        void Exit();
        void UpdateLogic(float deltaTime);
        void PhysicsUpdate(float fixedDeltaTime);
    }
}
