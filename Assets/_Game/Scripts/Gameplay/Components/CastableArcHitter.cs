using System;
using System.Collections.Generic;
using _Game.Scripts.Gameplay.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace _Game.Scripts.Gameplay.Components
{
    public class CastableArcHitter : BaseCastableHitter, IHittable
    {
        [SerializeField] private float arcAngularLength = 120f;
        [SerializeField] private float radius = 1.8f;
        [SerializeField] private HealthSystem healthSystem = new();
        
        protected override bool AllowMultipleHits => true;
        private List<GameObject> _alreadyHitObjects = new();

        private void Awake()
        {
            // Initialize health system with default values
            healthSystem.Initialize(100f); // You can adjust this value as needed
            
            // Subscribe to death event
            healthSystem.OnDeath.Subscribe(_ => HandleDeath());
        }

        private void Update()
        {
            // Handle health regeneration
            healthSystem.UpdateRegeneration(Time.deltaTime);
        }

        // IHittable Implementation
        public void OnReceivedHit(HittableHitData data)
        {
            healthSystem.TakeDamage(data.DamageProvider.BaseDamage);
        }

        private void HandleDeath()
        {
            // Handle death - destroy the GameObject
            Destroy(gameObject);
        }

        public override void Launch(IBaseDamageProvider baseDamageProvider, Vector3 direction, params GameObject[] ignoreColliders)
        {
            base.Launch(baseDamageProvider, direction, ignoreColliders);
            
            // look towards direction
            transform.rotation = Quaternion.LookRotation(direction);

            LaunchCore().Forget();
        }

        protected override bool ShouldIgnore(Collider other)
        {
            if (_alreadyHitObjects.Contains(other.gameObject)) return true;
            
            var result = base.ShouldIgnore(other);
            if (result) return true;
            
            Vector3 toTarget = (other.transform.position - transform.position).normalized;
            float angle = Mathf.Abs(Vector3.Angle(transform.forward, toTarget));

            return angle > arcAngularLength * 0.5f;
        }

        private async UniTaskVoid LaunchCore()
        {
            await UniTask.Yield(); // optional delay before hit check

            // Perform an overlap sphere in front
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius, // define radius in BaseCastableHitter?
                HitColliders
            );

            CheckHits(hitCount);
            
        }

        public override void FireOnHit(Collider other)
        {
            _alreadyHitObjects.Add(other.gameObject);
            
            base.FireOnHit(other);
        }
        
        // Draw gizmos
        
        private void OnDrawGizmos()
        {
            // Show hit box radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, radius);

            // Arc boundaries
            Quaternion leftRot = Quaternion.AngleAxis(-arcAngularLength * 0.5f, Vector3.up);
            Quaternion rightRot = Quaternion.AngleAxis(arcAngularLength * 0.5f, Vector3.up);

            Vector3 leftDir = leftRot * transform.forward;
            Vector3 rightDir = rightRot * transform.forward;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, leftDir * radius);
            Gizmos.DrawRay(transform.position, rightDir * radius);
        }
    }
}