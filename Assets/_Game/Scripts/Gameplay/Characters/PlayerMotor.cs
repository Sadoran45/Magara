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
        
        #region Locomotion

        [SerializeField] private Rigidbody rb;
        [SerializeField] private float movementSpeed = 5f;
        public float rotationSpeed = 10f;

        [SerializeField] private ReceiveHitState.Config receiveHitConfig;
        
        private Vector3 _movementInput;
        
        private void FixedUpdate()
        {
            var movement = _movementInput * (movementSpeed * Time.fixedDeltaTime);
            
            rb.MovePosition(rb.position + movement);
            
            var normalizedSpeed = _movementInput.magnitude > 0.1f ? 1f : 0f;
            
            animator.SetFloat("NormalizedSpeed", normalizedSpeed);
            
            // Rotate only if moving
            if (_movementInput.sqrMagnitude > 0.01f)
            {
                var targetRot = Quaternion.LookRotation(_movementInput, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
            }
        }

        private void Update()
        {
            // TODO
        }

        public void SendMovementInput(Vector3 input)
        {
            _movementInput = input;
        }

        public void OnProjectileHit(ProjectileHitData data)
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
