using UnityEngine;

namespace _Game.Scripts.Gameplay.Components
{
    public class ProjectileHitData
    {
        public GameObject Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        
        public ProjectileHitData(GameObject target, Vector3 hitPoint, Vector3 hitNormal)
        {
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }
}