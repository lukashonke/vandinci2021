using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Projectile Range", menuName = "Gama/Module Stats Effect/Projectile Range")]
    public class ProjectileRangeEffect : ModuleStatsEffect
    {
        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.projectileRange.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.stats.projectileRange.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                ("Projectile Range", $"{AddLevelValueUI(value, level)}"),
            };
        }
    }
}