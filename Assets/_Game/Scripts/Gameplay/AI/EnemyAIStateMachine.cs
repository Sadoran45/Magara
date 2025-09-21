using System;
using System.Collections.Generic;
using System.Threading;
using _Game.Scripts.Gameplay.Characters;
using _Game.Scripts.Gameplay.Components;
using _Game.Scripts.Gameplay.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.Scripts.Gameplay.AI
{
    public enum EnemyState
    {
        None,
        AttackToCore,
        AttackToPlayer,
        Attacking,
        Dead
    }
    
    public class EnemyAIStateMachine : MonoBehaviour, IBaseDamageProvider, IHittable
    {
        [Header("Enemy Config")] [SerializeField]
        private float baseDamage = 10f;
        
        [Header("Detection Settings")]
        [SerializeField] private float playerDetectionRange = 8f;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float detectionUpdateInterval = 0.1f;
        
        [Header("Core Target")]
        [SerializeField] private Transform coreTarget;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 360f;
        
        [Header("Attack Settings")]
        [SerializeField] private float attackDuration = 2f;
        [SerializeField] private float attackCooldown = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        
        // State Management
        public EnemyState CurrentState { get; private set; } = EnemyState.None;
        public Transform CurrentTarget { get; private set; }
        public float BaseDamage => baseDamage;
        
        // Events
        public event Action<EnemyState, EnemyState> OnStateChanged;
        public event Action<Transform> OnTargetChanged;
        public event Action OnAttackStarted;
        public event Action OnAttackCompleted;
        
        // Private fields
        private Transform nearestPlayer;
        private Transform _currentHittable;
        private CancellationTokenSource stateMachineCTS;
        private CancellationTokenSource currentStateCTS;
        private List<Transform> playersInRange = new();
        
        // Components
        private Rigidbody rb;
        private Collider col;
        private EnemyCharacter enemyCharacter;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            enemyCharacter = GetComponent<EnemyCharacter>();
            
            // Set Rigidbody to kinematic for manual movement control
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Find core if not assigned
            if (coreTarget == null)
            {
                var core = GameObject.FindGameObjectWithTag("Core");
                if (core != null)
                    coreTarget = core.transform;
            }
        }
        
        private async void Start()
        {
            stateMachineCTS = new CancellationTokenSource();
            
            // Initialize state machine
            await StartStateMachine(stateMachineCTS.Token);
        }
        
        private async UniTask StartStateMachine(CancellationToken cancellationToken)
        {
            // Start player detection loop
            var detectionTask = PlayerDetectionLoop(cancellationToken);
            
            // Start with AttackToCore state
            await ChangeState(EnemyState.AttackToCore);
            
            await detectionTask;
        }
        
        private async UniTask PlayerDetectionLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdatePlayerDetection();
                await UniTask.Delay(TimeSpan.FromSeconds(detectionUpdateInterval), cancellationToken: cancellationToken);
            }
        }
        
        private void UpdatePlayerDetection()
        {
            playersInRange.Clear();
            nearestPlayer = null;
            float nearestDistance = float.MaxValue;
            
            // Find all players in detection range
            var players = FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None);
            
            foreach (var playerObj in players)
            {
                float distance = Vector3.Distance(transform.position, playerObj.transform.position);
                
                if (distance <= playerDetectionRange)
                {
                    playersInRange.Add(playerObj.transform);
                    
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPlayer = playerObj.transform;
                    }
                }
            }
            
            // Handle state transitions based on player detection
            HandlePlayerDetection();
        }
        
        private void HandlePlayerDetection()
        {
            switch (CurrentState)
            {
                case EnemyState.AttackToCore:
                    if (nearestPlayer != null)
                    {
                        _ = ChangeState(EnemyState.AttackToPlayer);
                    }
                    else if (IsCoreInAttackRange())
                    {
                        _currentHittable = coreTarget;
                        _ = ChangeState(EnemyState.Attacking);
                    }
                    break;
                    
                case EnemyState.AttackToPlayer:
                    if (nearestPlayer == null)
                    {
                        _ = ChangeState(EnemyState.AttackToCore);
                    }
                    else if (IsPlayerInAttackRange())
                    {
                        _currentHittable = nearestPlayer;
                        _ = ChangeState(EnemyState.Attacking);
                    }
                    break;
                    
                case EnemyState.Attacking:
                    if (!IsHittableTargetInAttackRange())
                    {
                        // Cancel current attack and return to appropriate state
                        if (nearestPlayer != null)
                            _ = ChangeState(EnemyState.AttackToPlayer);
                        else
                            _ = ChangeState(EnemyState.AttackToCore);
                    }
                    break;
            }
        }

        private bool IsHittableTargetInAttackRange()
        {
            return _currentHittable != null && Vector3.Distance(transform.position, _currentHittable.position) < attackRange;
        }
        private bool IsPlayerInAttackRange()
        {
            return nearestPlayer != null && 
                   Vector3.Distance(transform.position, nearestPlayer.position) <= attackRange;
        }
        
        private bool IsCoreInAttackRange()
        {
            return coreTarget != null && 
                   Vector3.Distance(transform.position, coreTarget.position) <= attackRange;
        }
        
        public async UniTask ChangeState(EnemyState newState)
        {
            if (CurrentState == newState) return;
            
            var oldState = CurrentState;
            CurrentState = newState;
            
            // Cancel current state operations
            currentStateCTS?.Cancel();
            currentStateCTS?.Dispose();
            currentStateCTS = new CancellationTokenSource();
            
            // Create combined cancellation token
            OnStateChanged?.Invoke(oldState, newState);
            Debug.Log($"Enemy state changed: {oldState} -> {newState}");
            
            // Execute new state
            try
            {
                switch (newState)
                {
                    case EnemyState.AttackToCore:
                        await ExecuteAttackToCoreState(currentStateCTS.Token);
                        break;
                        
                    case EnemyState.AttackToPlayer:
                        await ExecuteAttackToPlayerState(currentStateCTS.Token);
                        break;
                        
                    case EnemyState.Attacking:
                        await ExecuteAttackingState(currentStateCTS.Token);
                        break;
                        
                    case EnemyState.Dead:
                        await ExecuteDeadState(currentStateCTS.Token);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // State was cancelled, this is expected behavior
            }
            finally
            {
            }
        }
        
        private async UniTask ExecuteAttackToCoreState(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && CurrentState == EnemyState.AttackToCore)
            {
                Debug.Log("Core target: " + (coreTarget != null ? coreTarget.name : "null"));
                if (coreTarget != null)
                {
                    SetTarget(coreTarget);
                    await MoveToTarget(coreTarget.position, cancellationToken);
                }
                
                await UniTask.Yield(cancellationToken);
            }
        }
        
        private async UniTask ExecuteAttackToPlayerState(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && CurrentState == EnemyState.AttackToPlayer)
            {
                if (nearestPlayer != null)
                {
                    SetTarget(nearestPlayer);
                    await MoveToTarget(nearestPlayer.position, cancellationToken);
                }
                
                await UniTask.Yield(cancellationToken);
            }
        }
        
        private async UniTask ExecuteAttackingState(CancellationToken cancellationToken)
        {
            if (_currentHittable == null) return;
            
            SetTarget(_currentHittable);
            
            // Movement is already stopped since we're not calling MoveToTarget
            
            OnAttackStarted?.Invoke();

            try
            {
                // Just perform attack - no movement or rotation
                await PerformAttack(cancellationToken);

                // Attack completed successfully
                OnAttackCompleted?.Invoke();

                // Wait for cooldown
                await UniTask.Delay(TimeSpan.FromSeconds(attackCooldown), cancellationToken: cancellationToken);

            }
            catch (OperationCanceledException)
            {
                // Attack was cancelled
                Debug.Log("Attack cancelled");
                throw;
            }
            finally
            {
                
                // Return to appropriate state after attack
                if (nearestPlayer != null && Vector3.Distance(transform.position, nearestPlayer.position) <= playerDetectionRange)
                {
                    await ChangeState(EnemyState.AttackToPlayer);
                }
                else
                {
                    await ChangeState(EnemyState.AttackToCore);
                }
            }
        }
        private async UniTask ExecuteDeadState(CancellationToken cancellationToken)
        {
            // Movement is already stopped since we're not calling MoveToTarget
            
            SetTarget(null);
            
            // Dead state - do nothing until destroyed
            await UniTask.Delay(-1, cancellationToken: cancellationToken);
        }
        
        private async UniTask MoveToTarget(Vector3 targetPosition, CancellationToken cancellationToken)
        {
            Debug.Log("Moving to target: " + targetPosition);
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0; // Keep movement on horizontal plane
            
            // Kinematic movement - move transform directly
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            transform.position += movement;
            
            // Rotate towards target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
            
        }
        
        private async UniTask PerformAttack(CancellationToken cancellationToken)
        {
            Debug.Log("Attack animation started!");
            
            // Trigger attack animation here
            // Example: GetComponent<Animator>().SetTrigger("Attack");
            
            
            // Wait for attack duration (animation length)
            await UniTask.Delay(TimeSpan.FromSeconds(attackDuration), cancellationToken: cancellationToken);
            
            // Deal damage after duration
            if (IsHittableTargetInAttackRange())
            {
                Debug.Log("Dealing damage to hittable!");
                var hittable = _currentHittable.GetComponent<IHittable>();

                var hitData = new HittableHitData(transform, _currentHittable.gameObject, this,
                    _currentHittable.transform.position, _currentHittable.transform.rotation * Vector3.forward);
                hittable.OnReceivedHit(hitData);
            }
            else
            {
                Debug.Log("Player out of range, no damage dealt");
            }
        }
        
        private void SetTarget(Transform newTarget)
        {
            if (CurrentTarget != newTarget)
            {
                CurrentTarget = newTarget;
                OnTargetChanged?.Invoke(newTarget);
            }
        }
        
        // Public method to handle being hit by player (for re-targeting)
        public void OnHitByPlayer(Transform attackingPlayer)
        {
            // If not currently attacking, switch target to the attacking player
            if (CurrentState != EnemyState.Attacking)
            {
                nearestPlayer = attackingPlayer;
                
                if (CurrentState == EnemyState.AttackToCore)
                {
                    _ = ChangeState(EnemyState.AttackToPlayer);
                }
            }
        }
        
        public void Die()
        {
            _ = ChangeState(EnemyState.Dead);
        }
        
        // IHittable Implementation
        public void OnReceivedHit(HittableHitData data)
        {
            // If we have an EnemyCharacter component, delegate to it
            if (enemyCharacter != null)
            {
                enemyCharacter.OnReceivedHit(data);
            }
            
            // Handle AI response to being hit
            if (data.Source != null && data.Source.TryGetComponent<PlayerMotor>(out _))
            {
                var playerTransform = data.Target.transform;
                OnHitByPlayer(playerTransform);
            }
        }
        
        private void OnDestroy()
        {
            stateMachineCTS?.Cancel();
            stateMachineCTS?.Dispose();
            currentStateCTS?.Cancel();
            currentStateCTS?.Dispose();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;
            
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw line to current target
            if (CurrentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, CurrentTarget.position);
            }
            
            // Draw state info
            if (Application.isPlaying)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, CurrentState.ToString());
#endif
            }
        }

    }
}