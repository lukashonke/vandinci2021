using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.EntityStatsEffects
{
    [CreateAssetMenu(fileName = "Generic Stats Effect", menuName = "Gama/Stats Effect/Generic Stats Effect")]
    public class GenericEntityStatsEffect : EntityStatsEffect
    {
        public EntityStatsEffectType effectType;
        
        public override bool Apply(Entity target, object source, int level)
        {
            if (target is Player player)
            {
                switch (effectType)
                {
                    
                }
                
                player.stats.criticalDamageMul.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Entity target, object source)
        {
            if (target is Player player)
            {
                switch (effectType)
                {
                    
                }
                
                player.stats.criticalDamageMul.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            (string, string) tuple = default;

            switch (effectType)
            {
                
            }
            
            tuple = ("Critical Damage", $"{AddLevelValueUI(value, level)}");
            
            return new List<(string title, string value)>()
            {
                tuple
            };
        }
    }
    
    public enum EntityStatsEffectType
    {
        
    }
}