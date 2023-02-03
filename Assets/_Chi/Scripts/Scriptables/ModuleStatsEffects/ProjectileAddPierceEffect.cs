using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Projectile Add Pierced", menuName = "Gama/Module Stats Effect/Projectile Add Pierced")]
    public class ProjectileAddPierceEffect : ModuleStatsEffect
    {
        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.projectilePierceCount.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.projectilePierceCount.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
    }
}