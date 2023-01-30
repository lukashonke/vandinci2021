using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.OdinInspector;

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
                foreach (var statModifier in modifiers)
                {
                    switch (statModifier.type)
                    {
                        case StatModifierType.Set:
                            value = statModifier.value;
                            break;
                        case StatModifierType.Add:
                            value += statModifier.value;
                            break;
                        case StatModifierType.Mul:
                            value *= statModifier.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            isDirty = false;
        }

        public void AddModifier(StatModifier modifier)
        {
            if (modifiers == null) modifiers = new();
            modifiers.Add(modifier);
            modifier.orderIndex = modifiers.IndexOf(modifier);
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
            modifiers = modifiers
                .OrderBy(m => m.order)
                .ThenBy(m => m.orderIndex)
                .ToList();
        }

        public void SetBaseValue(float val)
        {
            this.baseValue = val;
            isDirty = true;
        }
    }

    public class StatModifier
    {
        public short order; // defined order to apply this modifier

        public int orderIndex; // order in which module was added

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
        Mul
    }

    public enum StatOrders
    {
        Base = 1,
        PassiveModule = 10,
        ImmediateEffect = 20
    }
}