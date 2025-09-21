using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Game.Scripts.Gameplay.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public class SurvivorCharacter : PlayerMotor
    {
        [SerializeField] private AutoAttackState.Config autoAttackConfig;
        [SerializeField] private DashState.Config dashConfig;
        [SerializeField] private SurvivorLaserAttackState.Config laserAttackConfig;
        [SerializeField] private SendEffectState.Config sendShieldEffectConfig;
        
        
        public async UniTaskVoid AutoAttack()
        {
            
            var data = new AutoAttackState.Data(aimDirection);
            
            var state = new AutoAttackState(this, autoAttackConfig, data);

            await StartState(state);
        }

        public async UniTaskVoid Dash()
        {
            var data = new DashState.Data(aimDirection);
            var state = new DashState(this, dashConfig, data);
            
            await StartState(state);
        }

        public async UniTaskVoid LaserAttack()
        {
            var state = new SurvivorLaserAttackState(this, laserAttackConfig, new SurvivorLaserAttackState.Data());
            
            await StartState(state);
        }

        public async UniTaskVoid SendShieldEffect()
        {
            var playerMotors = FindObjectsByType<PlayerMotor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var otherPlayerMotor = playerMotors.FirstOrDefault(x => x != this);
            if (!otherPlayerMotor)
            {
                Debug.Log("Couldn't find other player motor");
                return;
            }

            var data = new SendEffectState.Data(otherPlayerMotor);
            var state = new SendEffectState(this, sendShieldEffectConfig, data);
            
            await StartState(state);
        }
    }
}