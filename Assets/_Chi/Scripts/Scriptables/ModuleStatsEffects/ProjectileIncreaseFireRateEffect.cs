using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Projectile Fire Rate", menuName = "Gama/Module Stats Effect/Projectile Fire Rate")]
    public class ProjectileIncreaseFireRateEffect : ModuleStatsEffect
    {
        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.fireRate.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.fireRate.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
    }
}