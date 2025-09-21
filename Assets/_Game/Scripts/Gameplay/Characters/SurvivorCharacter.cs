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
        [SerializeField] private AudioSource audioSource;
        
        
        public async UniTaskVoid AutoAttack()
        {
            
            var data = new AutoAttackState.Data(aimDirection);
            
            var state = new AutoAttackState(this, autoAttackConfig, data, audioSource);

            await StartState(state);
        }

        public async UniTaskVoid Dash()
        {
            var data = new DashState.Data(aimDirection);
            var state = new DashState(this, dashConfig, data, audioSource);
            
            await StartState(state);
        }

        public async UniTaskVoid LaserAttack()
        {
            var state = new SurvivorLaserAttackState(this, laserAttackConfig, new SurvivorLaserAttackState.Data(), audioSource);
            
            await StartState(state);
        }
    }
}