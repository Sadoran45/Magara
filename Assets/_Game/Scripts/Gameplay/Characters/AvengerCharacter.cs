using _Game.Scripts.Gameplay.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public class AvengerCharacter : PlayerMotor
    {   
        public AudioSource audioSource;
        [SerializeField] private AutoAttackState.Config autoAttackConfig;
        [SerializeField] private ReceiveShieldState.Config receiveShieldConfig;
        
        public async UniTaskVoid AutoAttack()
        {
            
            var data = new AutoAttackState.Data(aimDirection);
            
            var state = new AutoAttackState(this, autoAttackConfig, data, audioSource);

            await StartState(state);
        }
        
        public async UniTaskVoid ReceiveShield(float shieldTime)
        {
            var data = new ReceiveShieldState.Data(this, shieldTime);
            var state = new ReceiveShieldState(receiveShieldConfig, data);
            
            await StartState(state);
        }
        
    }
}