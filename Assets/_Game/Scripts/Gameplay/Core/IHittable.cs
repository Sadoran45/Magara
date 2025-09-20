using _Game.Scripts.Gameplay.Components;

namespace _Game.Scripts.Gameplay.Core
{
    public interface IHittable
    {
        void OnProjectileHit(ProjectileHitData data);
    }
}