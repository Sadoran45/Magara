using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay.Core;
using _Game.Scripts.Gameplay.States;
using Cysharp.Threading.Tasks;
using R3;

namespace _Game.Scripts.Gameplay.Interceptors
{
    public class ReceiveHitReduceDamageInterceptor : BaseStateInterceptor<ReceiveHitState>
    {
        public Subject<ReceiveHitState> OnReceiveHitStateIntercepted { get; } = new();
        
        protected override UniTask InterceptAsync(ReceiveHitState state, CancellationToken cancellation, Func<ReceiveHitState, CancellationToken, UniTask> next)
        {
            state.StateData.DamageReductionRate = 1f;
            
            OnReceiveHitStateIntercepted.OnNext(state);
            return next(state, cancellation);
        }
    }
}