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
            public float dashSpeed = 10f;
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
        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _owner.InterceptMovement();
            _config.dashAnimation.PlayAsync(_owner.Animator, cancellationToken).Forget();

            var velocity = StateData.Direction * _config.dashSpeed;
            var movedDistance = 0f;
            while (movedDistance < _config.dashDistance)
            {
                var movement = velocity * Time.fixedDeltaTime;
                movedDistance += movement.magnitude;
                _owner.Rigidbody.MovePosition(_owner.Rigidbody.position + movement);
                await UniTask.WaitForFixedUpdate();
            }
            
            _owner.LetMovement();
            // REFACTOR -> AnimationState class instead of cancellable
            _config.dashAnimation.Cancel(_owner.Animator);
        }
    }
}