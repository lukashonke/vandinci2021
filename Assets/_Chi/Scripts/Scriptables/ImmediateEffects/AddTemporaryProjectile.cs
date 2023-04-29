using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Temporary Projectile", menuName = "Gama/Immediate Effects/Temporary Projectile")]
    public class AddTemporaryProjectile : ImmediateEffect
    {
        public AddType addType;

        public int countBase;
        
        public bool staticCount;
        
        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (sourceModule is OffensiveModule module)
            {
                if (addType == AddType.ForNextShot)
                {
                    module.AddTemporaryProjectileUntilNextShot(staticCount ? countBase : (int) (countBase * strength));
                }
                else if (addType == AddType.ForCurrentOrNextMagazine)
                {
                    module.AddTemporaryProjectileForCurrentOrNextMagazine(staticCount ? countBase : (int) (countBase * strength));
                }
            }

            return true;
        }

    }

    public enum AddType
    {
        ForNextShot,
        ForCurrentOrNextMagazine
    }
}