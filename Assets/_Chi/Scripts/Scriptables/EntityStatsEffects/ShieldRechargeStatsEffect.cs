using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.EntityStatsEffects
{
    [CreateAssetMenu(fileName = "Shield Recharge Delay", menuName = "Gama/Stats Effect/Shield Recharge Delay")]
    public class ShieldRechargeStatsEffect : EntityStatsEffect
    {
        public override bool Apply(Entity target, object source, int level)
        {
            if (target is Player player)
            {
                player.stats.singleShieldRechargeDelay.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Entity target, object source)
        {
            if (target is Player player)
            {
                player.stats.singleShieldRechargeDelay.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                ("Shield Recharge", $"{AddLevelValueUI(value, level)}"),
            };
        }
    }
}