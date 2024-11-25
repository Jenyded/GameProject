using System;
using System.Collections.Generic;
using Core;
using UniRx;

namespace StateMachine
{
    public class GameMachine : DisposableClass
    {
        private readonly Dictionary<Type, IGameState> _availableStates = new();
        private IGameState _activeState;
        
        public void AddState(IGameState state)
        {
            _availableStates.Add(state.GetType(), state);
        }

        public void ChangeState<T>(object data)
        {
            _activeState?.Deinit();
            _activeState = _availableStates[typeof(T)];
            _activeState.Init(data);
        }

        protected override void OnInit()
        {
            base.OnInit();

            Disposable
                .Create(OnDisposed)
                .AddTo(Disposables);
        }
        
        private void OnDisposed()
        {
            _activeState?.Deinit();
        }
    }
}