using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Core.Helpers;
using _Game.Scripts.Gameplay.Characters;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.States
{
    public class DashState : IState
    {
        [Serializable]
        public class Config
        {
            public CancellableAnimation dashAnimation;
            public float dashDistance = 5f;
            
            
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
        
        public DashState(PlayerMotor owner, Config config, Data data)
        {
            _owner = owner;
            _config = config;
            StateData = data;
        }
        
        public UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}