using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ModuleStatsEffect : SerializedScriptableObject
    {
        public StatModifierType modifier;

        public StatOrders order = StatOrders.PassiveModule;

        public bool hasLevelScaledValue;
        
        [ShowIf("hasLevelScaledValue")]
        public List<float> levelScaledValue;

        [HideIf("hasLevelScaledValue")]
        public float value;

        public abstract bool Apply(Module target, object source, int level);
        
        public abstract bool Remove(Module target, object source);

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
                    if (hasLevelScaledValue)
                    {
                        return GetLevelScaledValue(level);
                    }
                    return value * level;
                case StatModifierType.Mul:
                    if (hasLevelScaledValue)
                    {
                        return 1 + GetLevelScaledValue(level);
                    }
                    return 1 + (value * level);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetLevelScaledValue(int level)
        {
            if ((level - 1) < levelScaledValue.Count)
            {
                return levelScaledValue[level-1];
            }

            return levelScaledValue[levelScaledValue.Count - 1];
        }
    }
}