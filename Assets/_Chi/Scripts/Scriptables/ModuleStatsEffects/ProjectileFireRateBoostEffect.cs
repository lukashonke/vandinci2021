using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Projectile Fire Rate Boost", menuName = "Gama/Module Stats Effect/Projectile Fire Rate Boost")]
    public class ProjectileFireRateBoostEffect : ModuleStatsEffect
    {
        public ProjectileFireRateBoostType boostType;
        
        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                switch (boostType)
                {
                    case ProjectileFireRateBoostType.Stationary:
                        offensiveModule.stats.stationaryReloadDurationBoost.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ProjectileFireRateBoostType.Moving:
                        offensiveModule.stats.movingReloadDurationBoost.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                switch (boostType)
                {
                    case ProjectileFireRateBoostType.Stationary:
                        offensiveModule.stats.stationaryReloadDurationBoost.RemoveModifiersBySource(source);
                        break;
                    case ProjectileFireRateBoostType.Moving:
                        offensiveModule.stats.movingReloadDurationBoost.RemoveModifiersBySource(source);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            switch (boostType)
            {
                case ProjectileFireRateBoostType.Stationary:
                    return new List<(string title, string value)>()
                    {
                        ("Standing Fire Rate", $"{AddLevelValueUI(value, level)}"),
                    };
                case ProjectileFireRateBoostType.Moving:
                    return new List<(string title, string value)>()
                    {
                        ("Moving Fire rATE", $"{AddLevelValueUI(value, level)}"),
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public enum ProjectileFireRateBoostType
    {
        Stationary,
        Moving
    }
}