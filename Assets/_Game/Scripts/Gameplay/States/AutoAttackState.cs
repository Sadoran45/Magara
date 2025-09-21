using System;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Core.Helpers;
using _Game.Scripts.Core.Features;
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
            
            [Header("Sound Effects")]
            public SoundEffectAsset attackSound;
            public SoundEffectAsset hitSound;
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
        private readonly AudioSource _audioSource;
        public Data StateData { get; }
        
        public AutoAttackState(PlayerMotor owner, Config config, Data data, AudioSource audioSource)
        {
            _owner = owner;
            _config = config;
            _audioSource = audioSource;
            StateData = data;
        }
        
        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // Play attack sound when attack starts
            if (_config.attackSound != null)
            {
                _config.attackSound.PlaySound(_audioSource, cancellationToken).Forget();
            }
            
            // Do not wait for any callback
            _config.attackAnimation.PlayAsync(_owner.Animator, cancellationToken).Forget();
            
            var hitter = Object.Instantiate(_config.hitter, _config.muzzleTransform.position, Quaternion.LookRotation(StateData.Direction));

            try
            {
                hitter.SetOwner(_owner.transform);
                hitter.Launch(this, StateData.Direction, ignoreColliders: _owner.gameObject);
                var hitTask = hitter.OnHit.FirstAsync(hitter.destroyCancellationToken);

                var hitData = await hitTask;

                // Deal damage to the target if it's a hittable
                var hittable = hitData.Target.GetComponent<IHittable>();

                hittable?.OnReceivedHit(hitData);
                
                // Play hit sound when hit occurs
                if (hittable != null && _config.hitSound != null)
                {
                    _config.hitSound.PlaySound(_audioSource, cancellationToken).Forget();
                }
                
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Projectile is canceled because of destruction");
            }
            
        }
    }
}