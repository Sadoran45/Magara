using System;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Core
{
    [Serializable]
    public class HealthSystem
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float regenerationRate = 1f; // Health per second
        [SerializeField] private float regenerationDelay = 3f; // Seconds after damage before regen starts
        
        public ReactiveProperty<float> Health { get; } = new(0f);
        public ReactiveProperty<float> MaxHealth { get; } = new(100f);
        
        // Computed properties
        public ReadOnlyReactiveProperty<float> HealthPercentage { get; }
        public ReadOnlyReactiveProperty<bool> IsAlive { get; }
        
        // Events
        public Observable<float> OnDamageDealt { get; }
        public Observable<Unit> OnDeath { get; }
        
        private Subject<float> damageSubject = new();
        private Subject<Unit> deathSubject = new();
        private float lastDamageTime;
        private bool wasAlive = true;
        
        public HealthSystem()
        {
            // Initialize MaxHealth
            MaxHealth.Value = maxHealth;
            Health.Value = maxHealth;
            
            // Setup computed properties
            HealthPercentage = Health.CombineLatest(MaxHealth, (health, max) => max > 0 ? health / max : 0f)
                .ToReadOnlyReactiveProperty();
            
            IsAlive = Health.Select(h => h > 0f).ToReadOnlyReactiveProperty();
            
            // Setup events
            OnDamageDealt = damageSubject.AsObservable();
            OnDeath = deathSubject.AsObservable();
            
            // Handle death
            IsAlive.Subscribe(isAlive =>
            {
                if (wasAlive && !isAlive)
                {
                    deathSubject.OnNext(Unit.Default);
                }
                wasAlive = isAlive;
            });
        }
        
        public void Initialize(float maxHealth)
        {
            this.maxHealth = maxHealth;
            MaxHealth.Value = maxHealth;
            Health.Value = maxHealth;
        }
        
        public void UpdateRegeneration(float deltaTime)
        {
            if (!IsAlive.CurrentValue || Health.Value >= MaxHealth.Value) return;
            
            // Check if enough time passed since last damage
            if (Time.time - lastDamageTime >= regenerationDelay)
            {
                var newHealth = Mathf.Clamp(Health.Value + regenerationRate * deltaTime, 0f, MaxHealth.Value);
                Health.Value = newHealth;
            }
        }
        
        public bool TakeDamage(float damage)
        {
            if (damage <= 0f || !IsAlive.CurrentValue) return false;
            
            lastDamageTime = Time.time;
            var newHealth = Mathf.Clamp(Health.Value - damage, 0f, MaxHealth.Value);
            Health.Value = newHealth;
            
            damageSubject.OnNext(damage);
            return true;
        }
        
        public void SetMaxHealth(float newMaxHealth)
        {
            if (newMaxHealth <= 0f) return;
            
            var healthRatio = HealthPercentage.CurrentValue;
            MaxHealth.Value = newMaxHealth;
            Health.Value = newMaxHealth * healthRatio;
        }
        
        public void Dispose()
        {
            damageSubject?.Dispose();
            deathSubject?.Dispose();
        }
    }
}