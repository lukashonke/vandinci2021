using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ImmediateEffectWithDuration : ImmediateEffect
    {
        public float duration;
        
        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            if (!ApplyEffect(target, sourceEntity, sourceItem, sourceModule))
            {
                return false;
            }
            
            Gamesystem.instance.Schedule(Time.time + duration, () => RemoveEffect(target, sourceEntity, sourceItem, sourceModule));

            return true;
        }

        public abstract bool ApplyEffect(Entity target, Entity source, Item sourceItem, Module sourceModule);
        
        public abstract void RemoveEffect(Entity target, Entity source, Item sourceItem, Module sourceModule);
    }
}