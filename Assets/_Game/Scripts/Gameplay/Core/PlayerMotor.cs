using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Game.Scripts.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Core
{
    public abstract class PlayerMotor : MonoBehaviour, IStateContainer
    {
        #region States
    
        private readonly List<StateRegistry> _states = new();
        private readonly List<BaseStateInterceptor> _interceptors = new();
        public async UniTask StartState<TState>(TState state) where TState : class, IState
        {
            var handle = new StateHandle(this, state);
            _states.Add(new StateRegistry(state, handle));
        
            // Interceptors
            var applicableInterceptors = _interceptors
                .Where(i => i.Matches(state)).ToList();
            if (applicableInterceptors.Count > 0)
            {
                // Chain interceptors
                Func<IState, CancellationToken, UniTask> chain = (s, c) => s.ExecuteAsync(c);
                foreach (var interceptor in applicableInterceptors.AsEnumerable().Reverse())
                {
                    var next = chain;
                    chain = (s, c) => interceptor.TryInterceptAsync(s, c, next);
                }

                await chain(state, handle.CancellationToken);
            }
            else
            {
                await state.ExecuteAsync(handle.CancellationToken);
            }
        }

        public void OnStateDisposed<TState>(TState state) where TState : class, IState
        {
            _states.RemoveAll(r => r.State == state);
        }
        public void CancelAllStates<TState>() where TState : class, IState
        {
            foreach (var registry in _states.Where(registry => registry.State is TState))
            {
                registry.Handle.Cancel();
            }
        }
        public IEnumerable<TState> GetStates<TState>() where TState : class, IState
        {
            foreach (var stateRegistry in _states)
            {
                if (stateRegistry.State is TState state)
                    yield return state;
            }
        }
    
        public InterceptorHandle AddInterceptor<TInterceptor>(TInterceptor interceptor)
            where TInterceptor : BaseStateInterceptor
        {
            _interceptors.Add(interceptor);
        
            return new InterceptorHandle(this, interceptor);
        }
        public void RemoveInterceptor<TInterceptor>(TInterceptor interceptor)
            where TInterceptor : BaseStateInterceptor
        {
            _interceptors.Remove(interceptor);
        }
    
        #endregion
    }
}
