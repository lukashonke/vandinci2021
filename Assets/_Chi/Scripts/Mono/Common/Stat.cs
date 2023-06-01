using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Common
{
    [Serializable]
    public class Stat
    {
        [NonSerialized] public bool isDirty;

        [Button]
        public void ForceValue(float val)
        {
            this.baseValue = val;
            isDirty = true;
        }
        
        public float baseValue;
        [Sirenix.OdinInspector.ReadOnly] public float value;

        private List<StatModifier> modifiers;

        public Stat()
        {
            isDirty = true;
        }

        public IReadOnlyList<StatModifier> Modifiers => modifiers;
        
        public int GetValueInt()
        {
            return (int) GetValue();
        }

        public float GetValue()
        {
            if (isDirty)
            {
                Recalculate();
            }

            return value;
        }

        private void Recalculate()
        {
            value = baseValue;

            if (modifiers != null)
            {
                if (Gamesystem.instance.miscSettings.upgradesApplyOneAfterAnother)
                {
                    foreach (var stat in modifiers.Where(m => m.type == StatModifierType.BaseMul))
                    {
                        value *= (1 + stat.value);
                    }
                    
                    foreach (var stat in modifiers.Where(m => m.type == StatModifierType.BaseAdd))
                    {
                        value += stat.value;
                    }
                    
                    foreach (var stat in modifiers.Where(m => m.type == StatModifierType.Mul))
                    {
                        value *= (1 + stat.value);
                    }
                    
                    foreach (var stat in modifiers.Where(m => m.type == StatModifierType.Add))
                    {
                        value += stat.value;
                    }
                    
                    foreach (var stat in modifiers.Where(m => m.type == StatModifierType.OverallMul))
                    {
                        value *= (1 + stat.value);
                    }
                    
                    foreach (var stat in modifiers.Where(m => m.type == StatModifierType.Set))
                    {
                        value = stat.value;
                    }
                }
                else
                {
                    foreach (var groupModifiers in modifiers.GroupBy(m => m.order).OrderBy(m => m.Key))
                    {
                        float addValue = 0f;
                    
                        foreach (var statModifier in groupModifiers)
                        {
                            switch (statModifier.type)
                            {
                                case StatModifierType.Add:
                                case StatModifierType.BaseAdd:
                                    addValue += statModifier.value;
                                    break;
                            }
                        }

                        float mulValue = 1f;
                    
                        foreach (var statModifier in groupModifiers)
                        {
                            switch (statModifier.type)
                            {
                                case StatModifierType.Mul:
                                case StatModifierType.OverallMul:
                                case StatModifierType.BaseMul:
                                    mulValue += statModifier.value;
                                    break;
                            }
                        }

                        mulValue = Mathf.Max(mulValue, 0f);
                    
                        value += addValue;
                        value *= mulValue;

                        foreach (var statModifier in groupModifiers)
                        {
                            switch (statModifier.type)
                            {
                                case StatModifierType.Set:
                                    value = statModifier.value;
                                    break;
                            }
                        }
                    }
                }
            }
            
            isDirty = false;
        }

        public void AddModifier(StatModifier modifier)
        {
            if (modifiers == null) modifiers = new();
            modifiers.Add(modifier);
            //modifier.orderIndex = modifiers.IndexOf(modifier);
            isDirty = true;
            SortModifiers();
        }

        public void RemoveModifier(StatModifier modifier)
        {
            if (modifiers == null) return;
            modifiers.Remove(modifier);
            isDirty = true;
            SortModifiers();
        }
        
        public void RemoveModifiersBySource(object source)
        {
            if (modifiers == null) return;
            
            int removed = modifiers.RemoveAll(m => m.source == source);
            if (removed > 0)
            {
                isDirty = true;
                SortModifiers();
            }
        }

        private void SortModifiers()
        {
            /*modifiers = modifiers
                .OrderBy(m => m.order)
                .ThenBy(m => m.orderIndex)
                .ToList();*/
        }

        public void SetBaseValue(float val)
        {
            this.baseValue = val;
            isDirty = true;
        }
    }

    public class StatModifier
    {
        public int order; // defined order to apply this modifier

        public float value;

        public StatModifierType type;

        public object source;

        public StatModifier(object source, float value, StatModifierType type, short order)
        {
            this.value = value;
            this.type = type;
            this.order = order;
            this.source = source;
        }
    }

    public enum StatModifierType
    {
        Set,
        Add,
        Mul,
        BaseAdd,
        BaseMul,
        OverallMul
    }

    public enum StatOrders
    {
        Base = 1,
        Upgrade = 10,
        PostUpgrade = 15,
        ImmediateEffect = 20,
        PostImmediateEffect = 25,
        EnvironmentEffect = 30
    }
}