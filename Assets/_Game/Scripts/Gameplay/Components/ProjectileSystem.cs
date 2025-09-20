using System;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public class ProjectileSystem : MonoBehaviour
    {
        public readonly Subject<ProjectileHitData> OnHit = new();
        
        [SerializeField] private Rigidbody rb;
        [SerializeField] private AnimationCurve speedMultiplierOverTime = AnimationCurve.Linear(0, 1, 1, 1);

        private float _startSpeed;
        private float _time;
        
        // Consider adding more properties like speed
        public void Launch(Vector3 direction, float speed)
        {
            // Implement projectile movement logic here
            // For simplicity, we just set the forward direction
            transform.forward = direction.normalized;
            
            _startSpeed = speed;
            _time = 0f;
        }

        private void Update()
        {
            if (_startSpeed <= 0) return;
            
            _time += Time.deltaTime;
            var currentSpeed = _startSpeed * speedMultiplierOverTime.Evaluate(_time);
            rb.linearVelocity = transform.forward * currentSpeed;
        }

        private void OnTriggerEnter(Collider other)
        {
            var hitData = new ProjectileHitData(
                target: other.gameObject,
                hitPoint: other.ClosestPoint(transform.position),
                hitNormal: other.transform.forward // Simplified normal calculation
            );
            
            OnHit.OnNext(hitData);
            
            // Optionally destroy the projectile after hit
            // CONSIDER handling lifetime someway else
            Destroy(gameObject);
        }
    }
}