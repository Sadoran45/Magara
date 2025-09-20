using System;
using System.Linq;
using _Game.Scripts.Gameplay.Core;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public class ProjectileSystem : MonoBehaviour, IBaseDamage
    {
        public Subject<ProjectileHitData> OnHit { get; private set; }
        
        [SerializeField] private AnimationCurve speedMultiplierOverTime = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField] private float hitBoxRadius = 0.2f;
        [SerializeField] private float maxLifetime = 7f;

        // FOR TEST PURPOSES
        public float BaseDamage => 50f;

        private float _startSpeed;
        private float _time;
        private GameObject[] _ignoreColliders;
        
        // Consider adding more properties like speed
        public void Launch(Vector3 direction, float speed, params GameObject[] ignoreColliders)
        {
            OnHit?.Dispose();
            OnHit = new Subject<ProjectileHitData>();
            
            // Implement projectile movement logic here
            // For simplicity, we just set the forward direction
            transform.forward = direction.normalized;
            
            _startSpeed = speed;
            _time = 0f;
            _ignoreColliders = ignoreColliders;
        }

        private void Update()
        {
            if (_startSpeed <= 0) return;
            
            _time += Time.deltaTime;
            
            // If lifetime is over, destroy
            if (_time >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }
            
            
            var currentSpeed = _startSpeed * speedMultiplierOverTime.Evaluate(_time);
            transform.Translate(transform.forward * (currentSpeed * Time.deltaTime), Space.World);
            
            
            // Physics overlap sphere, todo: migrate to nonalloc
            var overlappedCollider = Physics
                .OverlapSphere(transform.position, hitBoxRadius).FirstOrDefault(x => !_ignoreColliders.Contains(x.gameObject));
            if (overlappedCollider)
            {
                OnHitSomething(overlappedCollider);
            }
            
        }

        private void OnHitSomething(Collider other)
        {
            var hitData = new ProjectileHitData(
                target: other.gameObject,
                source: this,
                hitPoint: other.ClosestPoint(transform.position),
                hitNormal: other.transform.forward // Simplified normal calculation
            );
            
            OnHit.OnNext(hitData);
            
            // Optionally destroy the projectile after hit
            // CONSIDER handling lifetime someway else
            Destroy(gameObject);
        }
        
        
        // Gizmos for radius
        private void OnDrawGizmos()
        {
            // Show hit box radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, hitBoxRadius);
        }
    }
}