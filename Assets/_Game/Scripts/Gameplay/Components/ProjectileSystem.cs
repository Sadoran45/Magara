using System;
using System.Linq;
using _Game.Scripts.Gameplay.Core;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public class ProjectileSystem : BaseCastableHitter
    {
        [SerializeField] private AnimationCurve speedMultiplierOverTime = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField] private float hitBoxRadius = 0.2f;
        [SerializeField] private float maxLifetime = 7f;
        [SerializeField] private float baseSpeed = 12f;
        
        [SerializeField] private ParticleSystem muzzleEffect;
        [SerializeField] private ParticleSystem impactEffect;

        protected override bool AllowMultipleHits => false;

        private bool _isLaunched = false;
        private float _time;

        public override void Launch(IBaseDamageProvider baseDamageProvider, Vector3 direction, params GameObject[] ignoreColliders)
        {
            base.Launch(baseDamageProvider, direction, ignoreColliders);
            
            
            transform.forward = direction.normalized;
            _isLaunched = true;
            _time = 0f;
            
            ReleaseAndPlayEffect(muzzleEffect);
        }

        private void Update()
        {
            if (!_isLaunched) return;
            
            _time += Time.deltaTime;
            
            // If lifetime is over, destroy
            if (_time >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }
            
            
            var currentSpeed = baseSpeed * speedMultiplierOverTime.Evaluate(_time);
            transform.Translate(transform.forward * (currentSpeed * Time.deltaTime), Space.World);
            
            
            // Physics overlap sphere, todo: migrate to nonalloc
            var overlappedColliderCount = Physics
                .OverlapSphereNonAlloc(transform.position, hitBoxRadius, HitColliders);
            
            CheckHits(overlappedColliderCount);
            
        }

        public override void FireOnHit(Collider other)
        {
            base.FireOnHit(other);
            
            
            ReleaseAndPlayEffect(impactEffect);
            // Optionally destroy the projectile after hit
            // CONSIDER handling lifetime someway else
            Destroy(gameObject);
        }

        private void ReleaseAndPlayEffect(ParticleSystem effect)
        {
            effect.transform.localPosition = Vector3.zero;
            effect.transform.SetParent(null);
            effect.Play();
            
            Destroy(effect, 5f);
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