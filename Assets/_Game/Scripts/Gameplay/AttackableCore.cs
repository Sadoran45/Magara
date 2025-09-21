using _Game.Scripts.Gameplay.Components;
using _Game.Scripts.Gameplay.Core;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    public class AttackableCore : MonoBehaviour, IHittable
    {
        [SerializeField] private HealthSystem healthSystem;

        public static AttackableCore Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void OnReceivedHit(HittableHitData data)
        {
            healthSystem.TakeDamage(data.DamageProvider.BaseDamage);
        }
    }
}