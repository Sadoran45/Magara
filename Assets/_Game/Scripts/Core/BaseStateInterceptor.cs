using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Game.Scripts.Core
{
    public abstract class BaseStateInterceptor
    {
        public abstract bool Matches(IState state);
        
        public abstract UniTask TryInterceptAsync(IState state, CancellationToken cancellation,
            Func<IState, CancellationToken, UniTask> next);
    }
    public abstract class BaseStateInterceptor<TState> : BaseStateInterceptor where TState : class, IState
    {
        public override bool Matches(IState state)
        {
            return state is TState;
        }
        
        public override UniTask TryInterceptAsync(IState state, CancellationToken cancellation,
            Func<IState, CancellationToken, UniTask> next)
        {
            return InterceptAsync(state as TState, cancellation, next);
        }

        protected abstract UniTask InterceptAsync(TState state, CancellationToken cancellation,
            Func<TState, CancellationToken, UniTask> next);
    }
}