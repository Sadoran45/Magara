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
    public class RangeAutoAttackState : IState
    {
        [Serializable]
        public class Config
        {
            public float baseFireRate;
            public Transform muzzleTransform;
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
            // Do not wait for any callback
            _config.attackAnimation.PlayAsync(_owner.Animator, cancellationToken).Forget();
            
            
            var projectile = Object.Instantiate(_config.projectile, _config.muzzleTransform.position, Quaternion.LookRotation(StateData.Direction));

            try
            {
                projectile.Launch(StateData.Direction, _config.projectileSpeed, ignoreColliders: _owner.gameObject);
                var projectileHitTask = projectile.OnHit.FirstAsync(projectile.destroyCancellationToken);
            
                var hitData = await projectileHitTask;
            
                // Deal damage to the target if it's a hittable
                var hittable = hitData.Target.GetComponent<IHittable>();

                hittable?.OnProjectileHit(hitData);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Projectile is canceled because of destruction");
            }
            
        }
    }
}