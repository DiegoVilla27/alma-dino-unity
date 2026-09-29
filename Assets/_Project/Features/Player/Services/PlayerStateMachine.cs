using System;
using System.Collections.Generic;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Services
{
    public class PlayerStateMachine
    {
        private readonly Dictionary<PlayerStateEnum, IPlayerState> _states = new();
        private IPlayerState _currentState;

        public IPlayerState CurrentState => _currentState;
        public PlayerStateEnum CurrentStateType => _currentState != null ? _currentState.StateType : PlayerStateEnum.Idle;

        public event Action<PlayerStateEnum> OnStateChanged;

        public void RegisterState(IPlayerState state)
        {
            _states[state.StateType] = state;
        }

        public void Initialize(PlayerStateEnum startingState)
        {
            if (_states.TryGetValue(startingState, out var state))
            {
                _currentState = state;
                _currentState.Enter();
                OnStateChanged?.Invoke(startingState);
            }
            else
            {
                Debug.LogError($"[PlayerStateMachine] Estado inicial no registrado: {startingState}");
            }
        }

        public void ChangeState(PlayerStateEnum newStateType)
        {
            if (_currentState != null && _currentState.StateType == newStateType)
                return;

            if (_states.TryGetValue(newStateType, out var newState))
            {
                _currentState?.Exit();
                _currentState = newState;
                _currentState.Enter();
                OnStateChanged?.Invoke(newStateType);
            }
            else
            {
                Debug.LogError($"[PlayerStateMachine] Estado no encontrado: {newStateType}");
            }
        }

        public void UpdateLogic(float deltaTime)
        {
            _currentState?.UpdateLogic(deltaTime);
        }

        public void PhysicsUpdate(float fixedDeltaTime)
        {
            _currentState?.PhysicsUpdate(fixedDeltaTime);
        }
    }
}
