using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay.Characters;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.States
{
    public class ReceiveHitState : IState
    {
        [Serializable]
        public class Config
        {
            public GameObject hitEffectPrefab;
            
        }
        public class Data
        {
            public float Damage { get; }
            public float DamageReductionRate { get; set; }
            
            public Data(float damage)
            {
                Damage = damage;
                DamageReductionRate = 0f;
            }
        }

        public Data StateData { get; }

        private readonly PlayerMotor _owner;
        private readonly Config _config;
        
        public ReceiveHitState(PlayerMotor owner, Config config, Data data)
        {
            _owner = owner;
            _config = config;
            StateData = data;
        }

        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // Simulate processing the hit, e.g., playing an animation or applying effects
            await UniTask.Delay(500, cancellationToken: cancellationToken);
            // Here you would typically reduce the character's health by data.Damage
            Debug.Log("Received hit with damage: " + StateData.Damage * (1 - StateData.DamageReductionRate));
            
            
        }
    }
}