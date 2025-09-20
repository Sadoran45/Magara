using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Game.Scripts.Core;
using _Game.Scripts.Gameplay.Components;
using _Game.Scripts.Gameplay.Core;
using _Game.Scripts.Gameplay.States;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public abstract class PlayerMotor : MonoBehaviour, IStateContainer, IHittable
    {
        [SerializeField] private Animator animator;
        
        public Animator Animator => animator;
        public Rigidbody Rigidbody => rb;
        
        #region Locomotion

        [SerializeField] private Rigidbody rb;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float shootingAngle = 110f;
        public float rotationSpeed = 10f;

        [SerializeField] private ReceiveHitState.Config receiveHitConfig;
        
        private Vector3 _movementInput;
        protected Vector3 aimDirection;

        private bool _isMovingEnabled = true;
        
        private void Update()
        {
            // --- Animation parameter ---
        }

        private void FixedUpdate()
        {
            if (!_isMovingEnabled) return;
            
            // --- Move ---
            Vector3 velocity = _movementInput * movementSpeed;
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            
            var dot = Vector3.Dot(velocity.normalized, aimDirection.normalized);
            var dotAngle = dot * 180f;
            // Within rotate threshold
            
            var targetRot = Quaternion.LookRotation(aimDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

            var runningForward = dotAngle > shootingAngle;
            var normalizedSpeed = runningForward ? 1f : -1f;
            if (_movementInput == Vector3.zero)
                normalizedSpeed = 0;
            
            Animator.SetBool("Running", velocity != Vector3.zero);
            Animator.SetBool("RunningForward", runningForward);
            
        }
        
        public void SendMovementInput(Vector3 input)
        {
            _movementInput = input;
        }

        public void SetAimDirection(Vector3 direction)
        {
            aimDirection = direction;
        }

        public void InterceptMovement()
        {
            _isMovingEnabled = false;
        }
        // TODO: Refactor into using stackable interception
        public void LetMovement()
        {
            _isMovingEnabled = true;
        }
        
        public void OnProjectileHit(HittableHitData data)
        {
            var receiveHitData = new ReceiveHitState.Data(data.Source.BaseDamage);
            var receiveHitState = new ReceiveHitState(this, receiveHitConfig, receiveHitData);
            
            StartState(receiveHitState).Forget();
        }

        #endregion
        
        #region States
    
        private readonly List<StateRegistry> _states = new();
        private readonly List<BaseStateInterceptor> _interceptors = new();
        public async UniTask StartState<TState>(TState state) where TState : class, IState
        {
            var handle = new StateHandle(this, state);
            _states.Add(new StateRegistry(state, handle));
        
            // Interceptors
            var applicableInterceptors = _interceptors
                .Where(i => i.Matches(state)).ToList();
            if (applicableInterceptors.Count > 0)
            {
                // Chain interceptors
                Func<IState, CancellationToken, UniTask> chain = (s, c) => s.ExecuteAsync(c);
                foreach (var interceptor in applicableInterceptors.AsEnumerable().Reverse())
                {
                    var next = chain;
                    chain = (s, c) => interceptor.TryInterceptAsync(s, c, next);
                }

                await chain(state, handle.CancellationToken);
            }
            else
            {
                await state.ExecuteAsync(handle.CancellationToken);
            }
        }

        public void OnStateDisposed<TState>(TState state) where TState : class, IState
        {
            _states.RemoveAll(r => r.State == state);
        }
        public void CancelAllStates<TState>() where TState : class, IState
        {
            foreach (var registry in _states.Where(registry => registry.State is TState))
            {
                registry.Handle.Cancel();
            }
        }
        public IEnumerable<TState> GetStates<TState>() where TState : class, IState
        {
            foreach (var stateRegistry in _states)
            {
                if (stateRegistry.State is TState state)
                    yield return state;
            }
        }
    
        public InterceptorHandle AddInterceptor<TInterceptor>(TInterceptor interceptor)
            where TInterceptor : BaseStateInterceptor
        {
            _interceptors.Add(interceptor);
        
            return new InterceptorHandle(this, interceptor);
        }
        public void RemoveInterceptor<TInterceptor>(TInterceptor interceptor)
            where TInterceptor : BaseStateInterceptor
        {
            _interceptors.Remove(interceptor);
        }
    
        #endregion
    }
}
