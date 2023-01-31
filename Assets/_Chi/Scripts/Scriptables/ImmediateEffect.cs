using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ImmediateEffect : SerializedScriptableObject
    {
        public StatOrders order = StatOrders.ImmediateEffect;
        
        public abstract bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength);
    }
}