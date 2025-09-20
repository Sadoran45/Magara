using System;
using System.Threading;

namespace _Game.Scripts.Core
{
    public class StateHandle : IDisposable
    {
        private readonly IStateContainer _stateContainer;
        private readonly IState _state;
        private readonly CancellationTokenSource _cts = new();
        private bool _isDisposed;

        public CancellationToken CancellationToken => _cts.Token;
        
        public StateHandle(IStateContainer stateContainer, IState state)
        {
            _stateContainer = stateContainer;
            _state = state;
            _isDisposed = false;
        }

        public void Cancel()
        {
            if (_isDisposed) return;
            
            _cts.Cancel();
        }
        public void Dispose()
        {
            if (_isDisposed) return;

            _cts.Dispose();
            _stateContainer.OnStateDisposed(_state);
            _isDisposed = true;
        }
    }
}