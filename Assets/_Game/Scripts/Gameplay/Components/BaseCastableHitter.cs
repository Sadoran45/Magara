using System;
using System.Linq;
using _Game.Scripts.Gameplay.Core;
using R3;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public abstract class BaseCastableHitter : MonoBehaviour
    {
        public Subject<HittableHitData> OnHit { get; private set; }
        private IBaseDamageProvider _baseDamageProvider;
        private GameObject[] _ignoreColliders;
        
        protected Collider[] HitColliders { get; private set; } = new Collider[4];
        
        
        public virtual void Launch(IBaseDamageProvider baseDamageProvider, Vector3 direction, params GameObject[] ignoreColliders)
        {
            _baseDamageProvider = baseDamageProvider;
            _ignoreColliders = ignoreColliders;

            OnHit?.Dispose();
            OnHit = new Subject<HittableHitData>();
        }

        protected abstract bool AllowMultipleHits { get; }
        
        public virtual void CheckHits(int hitCount)
        {
            foreach (var hittableCollider in HitColliders.Take(hitCount).Where(x => !_ignoreColliders.Contains(x.gameObject)))
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