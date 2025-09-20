using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Core.Components
{
    public class OegAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        public Animator Animator => animator;
        
        // TODO: Handle receiving animation state machine events and dispatching them to interested parties.
        
        public UniTask WaitForAnimationEnd(int stateHash)
        {
            // Handle task completion 
            var tcs = new UniTaskCompletionSource();
            void OnStateExit()
            {
                tcs.TrySetResult();
                // Unsubscribe from the event to avoid memory leaks
                // animator.OnStateExit -= OnStateExit;
            }
            // Subscribe to the event
            // animator.OnStateExit += OnStateExit;
            return tcs.Task;
        }
    }
}