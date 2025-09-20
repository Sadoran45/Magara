using _Game.Scripts.Gameplay.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public class AvengerCharacter : PlayerMotor
    {
        [SerializeField] private AutoAttackState.Config autoAttackConfig;
        
        
        public async UniTaskVoid AutoAttack()
        {
            
            var data = new AutoAttackState.Data(aimDirection);
            
            var state = new AutoAttackState(this, autoAttackConfig, data);

            await StartState(state);
        }
        
    }
}