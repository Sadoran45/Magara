using _Game.Scripts.Gameplay.Core;
using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public class ProjectileHitData
    {
        public GameObject Target { get; }
        public IBaseDamage Source { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        
        public ProjectileHitData(GameObject target, IBaseDamage source, Vector3 hitPoint, Vector3 hitNormal)
        {
            Target = target;
            Source = source;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }
}