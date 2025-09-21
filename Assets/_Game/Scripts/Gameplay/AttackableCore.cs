using _Game.Scripts.Gameplay.Components;
using _Game.Scripts.Gameplay.Core;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    public class AttackableCore : MonoBehaviour, IHittable
    {
        [SerializeField] private HealthSystem healthSystem;


        public void OnReceivedHit(HittableHitData data)
        {
            healthSystem.TakeDamage(data.DamageProvider.BaseDamage);
        }
    }
}