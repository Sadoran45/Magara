using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Core.Helpers;
using _Game.Scripts.Gameplay.Components;
using _Game.Scripts.Gameplay.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.States
{
    public class RangeAutoAttackState : IState
    {
        [Serializable]
        public class Config
        {
            public float baseFireRate;
            public CancellableAnimation attackAnimation;
            public ProjectileSystem projectile;
            public float projectileSpeed = 5f;
        }
        public class Data
        {
            public Vector3 Direction { get; }
           
            public Data(Vector3 direction)
            {
                Direction = direction;
            }
        }

        private readonly PlayerMotor _owner;
        private readonly Config _config;
        public Data StateData { get; }
        
        public RangeAutoAttackState(PlayerMotor owner, Config config, Data data)
        {
            _owner = owner;
            _config = config;
            StateData = data;
        }
        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            
        }
    }
}