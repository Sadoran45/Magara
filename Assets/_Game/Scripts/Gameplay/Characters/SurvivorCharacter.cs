using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Game.Scripts.Gameplay.Core;
using _Game.Scripts.Gameplay.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public class SurvivorCharacter : PlayerMotor
    {
        [SerializeField] private RangeAutoAttackState.Config autoAttackConfig;
        [SerializeField] private DashState.Config dashConfig;
        [SerializeField] private ReceiveHitState.Config receiveHitConfig;


        public async UniTaskVoid AutoAttack()
        {
            
            var data = new RangeAutoAttackState.Data(Vector3.forward);
            
            var state = new RangeAutoAttackState(this, autoAttackConfig, data);

            await StartState(state);
        }

        public async UniTaskVoid ReceiveHit(ReceiveHitState.Data data)
        {
            var state = new ReceiveHitState(this, receiveHitConfig, data);
            
            await StartState(state);
        }

        public async UniTaskVoid Dash(Vector3 direction)
        {
            var data = new DashState.Data(direction);
            var state = new DashState(this, dashConfig, data);
            
            await StartState(state);
        }
    }
}