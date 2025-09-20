using System;
using System.Linq;
using _Game.Scripts.Gameplay.Characters;
using _Game.Scripts.Gameplay.Core;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public abstract class BaseCastableHitter : MonoBehaviour
    {
        public Subject<HittableHitData> OnHit { get; private set; }
        private IBaseDamageProvider _baseDamageProvider;
        protected GameObject[] ignoreColliders;
        
        protected Collider[] HitColliders { get; private set; } = new Collider[10];
        
        
        public virtual void Launch(IBaseDamageProvider baseDamageProvider, Vector3 direction, params GameObject[] ignoreColliders)
        {
            _baseDamageProvider = baseDamageProvider;
            this.ignoreColliders = ignoreColliders;

            OnHit?.Dispose();
            OnHit = new Subject<HittableHitData>();
        }

        protected abstract bool AllowMultipleHits { get; }

        protected virtual bool ShouldIgnore(Collider other)
        {
            return other.gameObject.TryGetComponent<PlayerMotor>(out _) || ignoreColliders.Contains(other.gameObject);
        }
        
        public virtual void CheckHits(int hitCount)
        {
            foreach (var hittableCollider in HitColliders.Take(hitCount).Where(x=>!ShouldIgnore(x)))
            {
                FireOnHit(hittableCollider);
                
                if(!AllowMultipleHits)
                    break;
            }
        }
        public virtual void FireOnHit(Collider other)
        {
            var hitData = new HittableHitData(
                target: other.gameObject,
                damageProvider: _baseDamageProvider,
                hitPoint: other.ClosestPoint(transform.position),
                hitNormal: other.transform.forward // Simplified normal calculation
            );
            
            OnHit.OnNext(hitData);
        }
        
        
    }
}