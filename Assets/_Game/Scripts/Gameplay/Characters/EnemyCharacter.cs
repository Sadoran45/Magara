using UnityEngine;
using _Game.Scripts.Gameplay.Core;
using _Game.Scripts.Gameplay.Components;
using System;

namespace _Game.Scripts.Gameplay.Characters
{
    public class EnemyCharacter : MonoBehaviour, IHittable
    {
        [Header("Enemy Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float deathDelay = 2f;

        [Header("Visual Effects")]
        [SerializeField] private GameObject damageEffect;
        [SerializeField] private GameObject deathEffect;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip damageSound;
        [SerializeField] private AudioClip deathSound;

        [Header("Death Particle System")]
        [SerializeField] private GameObject deathParticlePrefab; // Particle prefab to instantiate on death
        [SerializeField] private Vector3 particleOffset = Vector3.up; // Offset from enemy position
        [SerializeField] private float particleDuration = 3f; // How long to wait before destroying both particle and enemy

        [Header("Debug")]
        [SerializeField] private bool showHealthInConsole = true;

        // Health System - Simple implementation without UniRx
        private float currentHealth;
        private bool isAlive = true;
        
        // Components
        private Animator animator;
        private Collider enemyCollider;
        private Rigidbody enemyRigidbody;
        
        // Events
        public event Action<float> OnDamageReceived;
        public event Action OnEnemyDeath;
        
        // Properties
        public bool IsAlive => isAlive;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;

        private void Awake()
        {
            // Get components
            animator = GetComponent<Animator>();
            enemyCollider = GetComponent<Collider>();
            enemyRigidbody = GetComponent<Rigidbody>();
            
            // Initialize health
            currentHealth = maxHealth;
            isAlive = true;
        }

        private void Start()
        {
            // Initialization complete
        }



        public void OnReceivedHit(HittableHitData data)
        {
            if (!IsAlive) return;

            // Take damage based on the damage provider's base damage
            float damageAmount = data.DamageProvider.BaseDamage;
            TakeDamage(damageAmount);

            // Optional: Apply knockback or other hit effects here
            ApplyHitEffects(data);
        }

        private void ApplyHitEffects(HittableHitData data)
        {
            // You can add knockback, status effects, etc. here
            // Example: Apply small knockback
            if (enemyRigidbody != null)
            {
                Vector3 knockbackDirection = (transform.position - data.HitPoint).normalized;
                knockbackDirection.y = 0; // Keep on ground
                enemyRigidbody.AddForce(knockbackDirection * 5f, ForceMode.Impulse);
            }
        }

        private void PlayDamageEffects()
        {
            // Play damage visual effect
            if (damageEffect != null)
            {
                GameObject effect = Instantiate(damageEffect, transform.position, transform.rotation);
                Destroy(effect, 2f);
            }

            // Play damage sound
            if (audioSource != null && damageSound != null)
            {
                audioSource.PlayOneShot(damageSound);
            }
        }

        private void HandleDeath()
        {
            // Disable collision
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            // Stop movement
            if (enemyRigidbody != null)
            {
                enemyRigidbody.linearVelocity = Vector3.zero;
                enemyRigidbody.isKinematic = true;
            }

            // Play death animation
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }

            // Play death effects
            PlayDeathEffects();

            // Handle death particle effect
            HandleDeathParticleEffect();

            // Disable AI if it exists
            var aiStateMachine = GetComponent<_Game.Scripts.Gameplay.AI.EnemyAIStateMachine>();
            if (aiStateMachine != null)
            {
                aiStateMachine.Die();
            }

            // Note: Destruction is now handled by HandleDeathParticleEffect method
            // The enemy will be destroyed along with the particle effect
        }

        private void PlayDeathEffects()
        {
            // Play death visual effect
            if (deathEffect != null)
            {
                GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
                Destroy(effect, 5f);
            }

            // Play death sound
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
        }

        private void HandleDeathParticleEffect()
        {
            // Check if we have a particle prefab assigned
            if (deathParticlePrefab == null) return;

            // Calculate particle spawn position
            Vector3 particlePosition = transform.position + particleOffset;

            // Instantiate the particle prefab
            GameObject instantiatedParticle = Instantiate(deathParticlePrefab, particlePosition, transform.rotation);
            
            // Start the destruction countdown for both particle and enemy
            StartDestruction(instantiatedParticle);
        }

        private void StartDestruction(GameObject particleObject)
        {
            // Destroy the particle after specified duration
            if (particleObject != null)
            {
                Destroy(particleObject, particleDuration);
            }
            
            // Destroy this enemy gameobject after the same duration
            Destroy(gameObject, particleDuration);
        }

        private void DisableEnemy()
        {
            gameObject.SetActive(false);
        }

        // Public methods for external use
        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0) return;

            currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
            
            if (showHealthInConsole)
            {
                Debug.Log($"{gameObject.name} took {damage} damage. Health: {CurrentHealth}/{MaxHealth}");
            }
            
            OnDamageReceived?.Invoke(damage);
            PlayDamageEffects();
            
            // Trigger damage animation if available
            if (animator != null)
            {
                animator.SetTrigger("TakeDamage");
            }
            
            // Check for death
            if (currentHealth <= 0)
            {
                isAlive = false;
                
                if (showHealthInConsole)
                {
                    Debug.Log($"{gameObject.name} has died!");
                }
                
                OnEnemyDeath?.Invoke();
                HandleDeath();
            }
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            if (newMaxHealth <= 0f) return;
            
            float healthRatio = HealthPercentage;
            maxHealth = newMaxHealth;
            currentHealth = newMaxHealth * healthRatio;
        }

        public void HealToFull()
        {
            currentHealth = maxHealth;
            if (!isAlive)
            {
                isAlive = true;
            }
        }

        private void OnDestroy()
        {
            // Cleanup if needed
        }

        // Gizmos for debugging
        private void OnDrawGizmosSelected()
        {
            if (IsAlive)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
    }
}