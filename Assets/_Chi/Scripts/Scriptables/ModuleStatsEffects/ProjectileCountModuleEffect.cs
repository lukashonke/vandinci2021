using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Projectile Count", menuName = "Gama/Module Stats Effect/Projectile Count")]
    public class ProjectileCountModuleEffect : ModuleStatsEffect
    {
        public float value;

        public override bool Apply(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.projectileCount.AddModifier(new StatModifier(source, value, modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.projectileCount.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
    }
}