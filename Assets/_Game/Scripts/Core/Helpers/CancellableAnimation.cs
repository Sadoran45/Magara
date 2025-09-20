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
        
        [SerializeField] private string animatorPropertyName;
        [SerializeField] private string animatorStateName;
        
        private int _animatorPropertyHash;
        private int AnimatorPropertyHash => _animatorPropertyHash != 0 ? _animatorPropertyHash : (_animatorPropertyHash = Animator.StringToHash(animatorPropertyName));
        
        private int _animatorStateHash;
        private int AnimatorStateHash => _animatorStateHash != 0 ? _animatorStateHash : (_animatorStateHash = Animator.StringToHash(animatorStateName));
        

        public async UniTask PlayAsync(OegAnimator animator, CancellationToken cancellationToken = default)
        {
            animator.Animator.SetBool(_animatorPropertyHash, true);
            var waitForAnimationEnd = animator.WaitForAnimationEnd(_animatorStateHash);
            try
            {
                await UniTask.WhenAny(waitForAnimationEnd, UniTask.WaitUntilCanceled(cancellationToken));
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
            }
            finally
            {
                animator.Animator.SetBool(_animatorPropertyHash, false);
            }
        }
    }
}