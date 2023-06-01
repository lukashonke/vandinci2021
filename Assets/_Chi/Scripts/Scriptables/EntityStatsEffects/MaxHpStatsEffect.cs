using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.EntityStatsEffects
{
    [CreateAssetMenu(fileName = "Max HP", menuName = "Gama/Stats Effect/Max HP")]
    public class MaxHpStatsEffect : EntityStatsEffect
    {
        public override bool Apply(Entity target, object source, int level)
        {
            if (modifier == StatModifierType.Add || modifier == StatModifierType.BaseAdd)
            {
                target.entityStats.maxHpAdd += AddLevelValue(value, level);
            }
            else if (modifier == StatModifierType.Mul || modifier == StatModifierType.BaseMul || modifier == StatModifierType.OverallMul)
            {
                target.entityStats.maxHpMul += value;
            }
            else
            {
                Debug.LogError("No support for setting max HP directly.");
                return false;
            }
            
            target.Heal();

            return true;
        }

        public override bool Remove(Entity target, object source)
        {
            if (modifier == StatModifierType.Add || modifier == StatModifierType.BaseAdd)
            {
                target.entityStats.maxHpAdd -= value;
            }
            else if (modifier == StatModifierType.Mul || modifier == StatModifierType.BaseMul || modifier == StatModifierType.OverallMul)
            {
                target.entityStats.maxHpMul -= value;
            }
            else
            {
                Debug.LogError("No support for setting max HP directly.");
                return false;
            }
            
            target.Heal();

            return true;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                ("Max HP", $"{AddLevelValueUI(value, level)}"),
            };
        }
    }
}