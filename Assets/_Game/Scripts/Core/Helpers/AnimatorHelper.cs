using System.Threading;
using _Game.Scripts.Core.Components;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;

namespace _Game.Scripts.Core.Helpers
{
    public static class AnimatorHelper
    {
        public static UniTask WaitForStateExit(this Animator animator, int stateHash,
            CancellationToken cancellationToken = default)
        {
            var behaviour = animator.GetBehaviour<ObservableStateMachineTrigger>();

            return behaviour
                .OnStateExitAsObservable()
                .FirstAsync(x => x.StateInfo.fullPathHash == stateHash, cancellationToken).AsUniTask();
        }
    }
}