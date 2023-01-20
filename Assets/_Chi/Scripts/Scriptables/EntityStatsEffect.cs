using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Scriptables
{
    public abstract class EntityStatsEffect : SerializedScriptableObject
    {
        public StatModifierType modifier;

        public StatOrders order = StatOrders.PassiveModule;
        
        public abstract bool Apply(Entity target, object source);
        
        public abstract bool Remove(Entity target, object source);
    }
}