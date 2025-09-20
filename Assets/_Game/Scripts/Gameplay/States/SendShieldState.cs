using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay.Characters;
using Cysharp.Threading.Tasks;

namespace _Game.Scripts.Gameplay.States
{
    public class SendShieldState : IState
    {
        [Serializable]
        public class Config
        {
            
        }
        public class Data
        {
            public PlayerMotor Target { get; }
            
            public Data(PlayerMotor target)
            {
                Target = target;
            }
        }
        
        private readonly PlayerMotor _owner;
        private readonly Config _config;
        public Data StateData { get; }
        public SendShieldState(PlayerMotor owner, Config config, Data data)
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