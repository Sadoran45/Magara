using _Game.Scripts.Gameplay.Core;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public class HittableHitData
    {
        public Transform Source { get; }
        public GameObject Target { get; }
        public IBaseDamageProvider DamageProvider { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        
        public HittableHitData(Transform source, GameObject target, IBaseDamageProvider damageProvider, Vector3 hitPoint, Vector3 hitNormal)
        {
            Source = source;
            Target = target;
            DamageProvider = damageProvider;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }
}