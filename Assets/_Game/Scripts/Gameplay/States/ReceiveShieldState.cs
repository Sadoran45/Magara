using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay.Characters;
using _Game.Scripts.Gameplay.Interceptors;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.Scripts.Gameplay.States
{
    public class ReceiveShieldState : IState
    {
        [Serializable]
        public class Config
        {
            public GameObject receiveShieldEffectPrefab;
        }
        public class Data
        {
            public AvengerCharacter ReceivingCharacter { get; }
            public float ShieldTime { get; }
            
            public Data(AvengerCharacter receivingCharacter, float shieldTime)
            {
                ReceivingCharacter = receivingCharacter;
                ShieldTime = shieldTime;
            }
        }

        private Config _config;
        public Data StateData { get; }
        public ReceiveShieldState(Config config, Data data)
        {
            _config = config;
            StateData = data;
        }

        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Show VFX
            var spawnedEffect = Object.Instantiate(_config.receiveShieldEffectPrefab,
                StateData.ReceivingCharacter.transform.position + Vector3.up * 0.5f,
                Quaternion.identity);
            
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
                
                // Clean up VFX
                if (spawnedEffect != null)
                {
                    Object.Destroy(spawnedEffect);
                }
            }
            
        }
    }
}