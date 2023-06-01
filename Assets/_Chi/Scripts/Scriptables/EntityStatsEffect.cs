using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Scriptables
{
    public abstract class EntityStatsEffect : SerializedScriptableObject
    {
        public StatModifierType modifier;

        public StatOrders order = StatOrders.Upgrade;
        
        public bool hasLevelScaledValue;
        
        [ShowIf("hasLevelScaledValue")]
        public List<float> levelScaledValue;

        [HideIf("hasLevelScaledValue")]
        public float value;

        public abstract bool Apply(Entity target, object source, int level);
        
        public abstract bool Remove(Entity target, object source);
        
        protected float AddLevelValue(float value, int level)
        {
            switch (modifier)
            {
                case StatModifierType.Set:
                    if (hasLevelScaledValue)
                    {
                        return GetLevelScaledValue(level);
                    }
                    return value;
                case StatModifierType.Add:
                case StatModifierType.BaseAdd:
                    if (hasLevelScaledValue)
                    {
                        return GetLevelScaledValue(level);
                    }
                    return value * level;
                case StatModifierType.Mul:
                case StatModifierType.BaseMul:
                case StatModifierType.OverallMul:
                    if (hasLevelScaledValue)
                    {
                        return GetLevelScaledValue(level);
                    }
                    return (value * level);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected string AddLevelValueUI(float value, int level)
        {
            switch (modifier)
            {
                case StatModifierType.Set:
                    if (hasLevelScaledValue)
                    {
                        return "set to " + GetLevelScaledValue(level);
                    }
                    return "set to " + value;
                case StatModifierType.Add:
                case StatModifierType.BaseAdd: // TODO zohlednit Base v popisku
                    if (hasLevelScaledValue)
                    {
                        var val = GetLevelScaledValue(level);
                        return (modifier == StatModifierType.BaseAdd ? "base " : "") + (val > 0 ? "+" : "") + val;
                    }
                    
                    var val3 = value * level;
                    return (modifier == StatModifierType.BaseAdd ? "base " : "") + (val3 > 0 ? "+" : "") + (value * level);
                case StatModifierType.Mul:
                case StatModifierType.OverallMul:
                case StatModifierType.BaseMul:
                    if (hasLevelScaledValue)
                    {
                        var val = (GetLevelScaledValue(level) * 100);
                        return (modifier == StatModifierType.BaseMul ? "base " : "") + (val > 0 ? "+" : "") + (val) + "%";
                    }

                    var val2 = value * level * 100;
                    return (modifier == StatModifierType.BaseMul ? "base " : "") + (val2 > 0 ? "+" : "") + (val2) + "%";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private float GetLevelScaledValue(int level)
        {
            if (!hasLevelScaledValue) return value;
            
            if ((level - 1) < levelScaledValue.Count)
            {
                return levelScaledValue[level-1];
            }

            return levelScaledValue[levelScaledValue.Count - 1];
        }
        
        public virtual List<(string title, string value)> GetUiStats(int level) => null;
    }
}