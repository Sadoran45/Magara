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
    public class SurvivorLaserAttackState : IState, IBaseDamageProvider
    {
        [Serializable]
        public class Config
        {
            public Transform muzzleTransform;
            public CancellableAnimation attackAnimation;
            public BaseCastableHitter hitter;
            
            // FOR TEST
            public float hitDelay = 0.6f;
            public float baseDamage = 5f;
        }
        public class Data
        {
        }

        public float BaseDamage => _config.baseDamage;

        private readonly PlayerMotor _owner;
        private readonly Config _config;
        public Data StateData { get; }
        
        public SurvivorLaserAttackState(PlayerMotor owner, Config config, Data data)
        {
            _owner = owner;
            _config = config;
            StateData = data;
        }
        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _owner.InterceptMovement();
            
            try
            {
                // Do not wait for any callback
                _config.attackAnimation.PlayAsync(_owner.Animator, cancellationToken).Forget();
                
                // TODO: Wait for event callback
                await UniTask.Delay( (int)(1000 * _config.hitDelay), cancellationToken: cancellationToken);

                var direction = _owner.transform.forward;
                var hitter = Object.Instantiate(_config.hitter, _config.muzzleTransform.position, Quaternion.LookRotation(direction));

                hitter.Launch(this, direction, ignoreColliders: _owner.gameObject);
                hitter.OnHit.Take(1).TakeUntil(hitter.destroyCancellationToken).Subscribe(hitData =>
                {
                    // Deal damage to the target if it's a hittable
                    var hittable = hitData.Target.GetComponent<IHittable>();

                    hittable?.OnReceivedHit(hitData);
                });
                
                _config.attackAnimation.Cancel(_owner.Animator);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Projectile is canceled because of destruction");
            }
            finally
            {
                _owner.LetMovement();
            }
            
        }
    }
}