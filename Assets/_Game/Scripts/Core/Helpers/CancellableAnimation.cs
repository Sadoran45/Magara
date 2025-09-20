using System;
using System.Threading;
using _Game.Scripts.Core.Components;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Core.Helpers
{
    [Serializable]
    public class CancellableAnimation
    {
        
        
        [SerializeField] private string triggerPropertyName;
        [SerializeField] private string cancelPropertyName;
        [SerializeField] private string animatorStateName;
        
        private int _triggerPropertyHash;
        private int TriggerPropertyHash => _triggerPropertyHash != 0 ? _triggerPropertyHash : (_triggerPropertyHash = Animator.StringToHash(triggerPropertyName));
        private int _cancelPropertyHash;
        private int CancelPropertyHash => _cancelPropertyHash != 0 ? _cancelPropertyHash : (_cancelPropertyHash = Animator.StringToHash(cancelPropertyName));
        
        
        private int _animatorStateHash;
        private int AnimatorStateHash => _animatorStateHash != 0 ? _animatorStateHash : (_animatorStateHash = Animator.StringToHash(animatorStateName));
        

        public async UniTask PlayAsync(Animator animator, CancellationToken cancellationToken = default)
        {
            animator.SetTrigger(TriggerPropertyHash);
            
            try
            {
                await animator.WaitForStateExit(AnimatorStateHash, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
                animator.SetTrigger(CancelPropertyHash);
            }
            finally
            {
            }
        }
    }
}