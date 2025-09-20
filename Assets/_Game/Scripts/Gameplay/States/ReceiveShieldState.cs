using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay.Characters;
using _Game.Scripts.Gameplay.Interceptors;
using Cysharp.Threading.Tasks;

namespace _Game.Scripts.Gameplay.States
{
    public class ReceiveShieldState : IState
    {
        public class Data
        {
            public SurvivorCharacter ReceivingCharacter { get; }
            public float ShieldTime { get; }
            
            public Data(SurvivorCharacter receivingCharacter, float shieldTime)
            {
                ReceivingCharacter = receivingCharacter;
                ShieldTime = shieldTime;
            }
        }
        
        public Data StateData { get; }
        public ReceiveShieldState(Data data)
        {
            StateData = data;
        }

        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Show VFX
            
            // Add block damage effect to the character
            var interceptor = new ReceiveHitReduceDamageInterceptor();
            var interceptorHandle = StateData.ReceivingCharacter.AddInterceptor(interceptor);

            try
            {
                // Wait for the shield duration
                await UniTask.Delay((int)(StateData.ShieldTime * 1000), cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // State was cancelled, proceed to remove the effect
            }
            finally
            {
                // Remove block damage effect from the character
                interceptorHandle.Remove();
                
            }
            
        }
    }
}