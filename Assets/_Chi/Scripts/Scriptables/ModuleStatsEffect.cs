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
        
        public abstract bool Apply(Module target, object source);
        
        public abstract bool Remove(Module target, object source);
    }
}