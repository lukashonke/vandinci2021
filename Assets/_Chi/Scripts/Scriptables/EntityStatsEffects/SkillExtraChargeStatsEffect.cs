using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.EntityStatsEffects
{
    [CreateAssetMenu(fileName = "Skill Extra Charge", menuName = "Gama/Stats Effect/Skill Extra Charge")]
    public class SkillExtraChargeStatsEffect : EntityStatsEffect
    {
        public override bool Apply(Entity target, object source, int level)
        {
            if (target is Player player)
            {
                player.stats.skillExtraChargeCount.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Entity target, object source)
        {
            if (target is Player player)
            {
                player.stats.skillExtraChargeCount.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                ("Skill Extra Charges", $"{AddLevelValueUI(value, level)}"),
            };
        }
    }
}