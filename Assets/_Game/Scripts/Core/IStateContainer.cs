using System.Collections.Generic;

namespace _Game.Scripts.Core
{
    public interface IStateContainer
    {
        void OnStateDisposed<TState>(TState state) where TState : class, IState;
        void CancelAllStates<TState>() where TState : class, IState;
        
        IEnumerable<TState> GetStates<TState>() where TState : class, IState;

        InterceptorHandle AddInterceptor<TInterceptor>(TInterceptor interceptor)
            where TInterceptor : BaseStateInterceptor;
        void RemoveInterceptor<TInterceptor>(TInterceptor interceptor)
            where TInterceptor : BaseStateInterceptor;
    }
}