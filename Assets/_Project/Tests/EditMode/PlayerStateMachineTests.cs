using AlmaDino.Features.Player.Models;
using AlmaDino.Features.Player.Services;
using NUnit.Framework;

namespace AlmaDino.Tests.EditMode
{
    [TestFixture]
    public class PlayerStateMachineTests
    {
        private class MockPlayerState : IPlayerState
        {
            public PlayerStateEnum StateType { get; }
            public int EnterCount { get; private set; }
            public int ExitCount { get; private set; }
            public int LogicUpdateCount { get; private set; }
            public int PhysicsUpdateCount { get; private set; }

            public MockPlayerState(PlayerStateEnum stateType)
            {
                StateType = stateType;
            }

            public void Enter() => EnterCount++;
            public void Exit() => ExitCount++;
            public void UpdateLogic(float deltaTime) => LogicUpdateCount++;
            public void PhysicsUpdate(float fixedDeltaTime) => PhysicsUpdateCount++;
        }

        [Test]
        public void Initialize_SetsStartingState_AndCallsEnter()
        {
            var sm = new PlayerStateMachine();
            var idleState = new MockPlayerState(PlayerStateEnum.Idle);
            sm.RegisterState(idleState);

            sm.Initialize(PlayerStateEnum.Idle);

            Assert.AreEqual(PlayerStateEnum.Idle, sm.CurrentStateType);
            Assert.AreEqual(1, idleState.EnterCount);
        }

        [Test]
        public void ChangeState_CallsExitOnOldState_AndEnterOnNewState()
        {
            var sm = new PlayerStateMachine();
            var idleState = new MockPlayerState(PlayerStateEnum.Idle);
            var runState = new MockPlayerState(PlayerStateEnum.Run);

            sm.RegisterState(idleState);
            sm.RegisterState(runState);
            sm.Initialize(PlayerStateEnum.Idle);

            sm.ChangeState(PlayerStateEnum.Run);

            Assert.AreEqual(PlayerStateEnum.Run, sm.CurrentStateType);
            Assert.AreEqual(1, idleState.ExitCount);
            Assert.AreEqual(1, runState.EnterCount);
        }

        [Test]
        public void ChangeState_ToSameState_DoesNotReEnter()
        {
            var sm = new PlayerStateMachine();
            var idleState = new MockPlayerState(PlayerStateEnum.Idle);

            sm.RegisterState(idleState);
            sm.Initialize(PlayerStateEnum.Idle);

            sm.ChangeState(PlayerStateEnum.Idle);

            Assert.AreEqual(1, idleState.EnterCount);
            Assert.AreEqual(0, idleState.ExitCount);
        }

        [Test]
        public void UpdateLogic_And_PhysicsUpdate_ForwardToActiveState()
        {
            var sm = new PlayerStateMachine();
            var jumpState = new MockPlayerState(PlayerStateEnum.Jump);

            sm.RegisterState(jumpState);
            sm.Initialize(PlayerStateEnum.Jump);

            sm.UpdateLogic(0.016f);
            sm.PhysicsUpdate(0.02f);

            Assert.AreEqual(1, jumpState.LogicUpdateCount);
            Assert.AreEqual(1, jumpState.PhysicsUpdateCount);
        }
    }
}
