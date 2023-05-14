using _Chi.Scripts.Mono.Common;
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
        
        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (data.sourceModule is OffensiveModule module)
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