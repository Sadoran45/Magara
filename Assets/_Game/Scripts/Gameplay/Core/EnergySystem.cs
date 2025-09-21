using System;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Core
{
    [Serializable]
    public class EnergySystem
    {
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float currentEnergy = 0f;
        
        public ReactiveProperty<float> Energy { get; } = new(0f);
        public ReactiveProperty<float> MaxEnergy { get; } = new(100f);
        
        // Computed properties
        public ReadOnlyReactiveProperty<float> EnergyPercentage { get; }
        public ReadOnlyReactiveProperty<bool> IsFullEnergy { get; }
        
        // Events
        public Observable<float> OnEnergyGained { get; }
        public Observable<float> OnEnergyConsumed { get; }
        public Observable<Unit> OnEnergyFullReached { get; }
        public Observable<Unit> OnEnergyEmpty { get; }
        
        private Subject<float> energyGainedSubject = new();
        private Subject<float> energyConsumedSubject = new();
        private Subject<Unit> energyFullSubject = new();
        private Subject<Unit> energyEmptySubject = new();
        
        private bool wasFullEnergy = false;
        private bool wasEmptyEnergy = true;
        
        public EnergySystem()
        {
            // Initialize values
            MaxEnergy.Value = maxEnergy;
            Energy.Value = currentEnergy;
            
            // Setup computed properties
            EnergyPercentage = Energy.CombineLatest(MaxEnergy, (energy, max) => max > 0 ? energy / max : 0f)
                .ToReadOnlyReactiveProperty();
            
            IsFullEnergy = Energy.CombineLatest(MaxEnergy, (energy, max) => energy >= max)
                .ToReadOnlyReactiveProperty();
            
            // Setup events
            OnEnergyGained = energyGainedSubject.AsObservable();
            OnEnergyConsumed = energyConsumedSubject.AsObservable();
            OnEnergyFullReached = energyFullSubject.AsObservable();
            OnEnergyEmpty = energyEmptySubject.AsObservable();
            
            // Handle full/empty events
            IsFullEnergy.Subscribe(isFull =>
            {
                if (!wasFullEnergy && isFull)
                {
                    energyFullSubject.OnNext(Unit.Default);
                }
                wasFullEnergy = isFull;
            });
            
            Energy.Subscribe(energy =>
            {
                bool isEmpty = energy <= 0f;
                if (!wasEmptyEnergy && isEmpty)
                {
                    energyEmptySubject.OnNext(Unit.Default);
                }
                wasEmptyEnergy = isEmpty;
            });
        }
        
        public void Initialize(float maxEnergy, float startingEnergy = 0f)
        {
            this.maxEnergy = maxEnergy;
            MaxEnergy.Value = maxEnergy;
            Energy.Value = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
            currentEnergy = Energy.Value;
        }
        
        public void GainEnergy(float amount)
        {
            if (amount <= 0f) return;
            
            float oldEnergy = Energy.Value;
            Energy.Value = Mathf.Clamp(Energy.Value + amount, 0f, MaxEnergy.Value);
            
            float actualGain = Energy.Value - oldEnergy;
            if (actualGain > 0f)
            {
                energyGainedSubject.OnNext(actualGain);
            }
        }
        
        public bool Consume(float amount)
        {
            if (amount <= 0f || Energy.Value < amount) return false;
            
            Energy.Value = Mathf.Clamp(Energy.Value - amount, 0f, MaxEnergy.Value);
            energyConsumedSubject.OnNext(amount);
            return true;
        }
        
        public bool Has(float amount)
        {
            return Energy.Value >= amount;
        }
        
        public void SetMaxEnergy(float newMaxEnergy)
        {
            if (newMaxEnergy <= 0f) return;
            
            float energyRatio = EnergyPercentage.CurrentValue;
            MaxEnergy.Value = newMaxEnergy;
            Energy.Value = newMaxEnergy * energyRatio;
        }
        
        public void SetEnergy(float amount)
        {
            Energy.Value = Mathf.Clamp(amount, 0f, MaxEnergy.Value);
        }
        
        public void FillEnergy()
        {
            float oldEnergy = Energy.Value;
            Energy.Value = MaxEnergy.Value;
            
            float actualGain = Energy.Value - oldEnergy;
            if (actualGain > 0f)
            {
                energyGainedSubject.OnNext(actualGain);
            }
        }
        
        public void EmptyEnergy()
        {
            float consumedAmount = Energy.Value;
            Energy.Value = 0f;
            
            if (consumedAmount > 0f)
            {
                energyConsumedSubject.OnNext(consumedAmount);
            }
        }
        
        public void Dispose()
        {
            energyGainedSubject?.Dispose();
            energyConsumedSubject?.Dispose();
            energyFullSubject?.Dispose();
            energyEmptySubject?.Dispose();
        }
    }
}