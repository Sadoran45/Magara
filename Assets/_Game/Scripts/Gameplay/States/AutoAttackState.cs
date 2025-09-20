using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Core.Helpers;
using _Game.Scripts.Gameplay.Characters;
using _Game.Scripts.Gameplay.Components;
using _Game.Scripts.Gameplay.Core;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.Scripts.Gameplay.States
{
    public class AutoAttackState : IState, IBaseDamageProvider
    {
        [Serializable]
        public class Config
        {
            public Transform muzzleTransform;
            public CancellableAnimation attackAnimation;
            public BaseCastableHitter hitter;
            
            public float baseDamage = 5f;
        }
        public class Data
        {
            public Vector3 Direction { get; }
           
            public Data(Vector3 direction)
            {
                Direction = direction;
            }
        }

        public float BaseDamage => _config.baseDamage;

        private readonly PlayerMotor _owner;
        private readonly Config _config;
        public Data StateData { get; }
        
        public AutoAttackState(PlayerMotor owner, Config config, Data data)
        {
            _owner = owner;
            _config = config;
            StateData = data;
        }
        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // Do not wait for any callback
            _config.attackAnimation.PlayAsync(_owner.Animator, cancellationToken).Forget();
            
            
            var hitter = Object.Instantiate(_config.hitter, _config.muzzleTransform.position, Quaternion.LookRotation(StateData.Direction));

            try
            {
                hitter.Launch(this, StateData.Direction, ignoreColliders: _owner.gameObject);
                var hitTask = hitter.OnHit.FirstAsync(hitter.destroyCancellationToken);
            
                var hitData = await hitTask;
            
                // Deal damage to the target if it's a hittable
                var hittable = hitData.Target.GetComponent<IHittable>();

                hittable?.OnReceivedHit(hitData);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Projectile is canceled because of destruction");
            }
            
        }
    }
}