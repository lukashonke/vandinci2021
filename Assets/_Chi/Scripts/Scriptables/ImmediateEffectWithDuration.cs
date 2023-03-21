using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ImmediateEffectWithDuration : ImmediateEffect
    {
        public float duration;
        
        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (!ApplyEffect(target, sourceEntity, sourceItem, sourceModule))
            {
                return false;
            }

            ScheduleRemove(Time.time + duration, target, sourceEntity, sourceItem, sourceModule);

            return true;
        }

        protected void ScheduleRemove(float time, Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule)
        {
            Gamesystem.instance.Schedule(time, () => RemoveEffect(target, sourceEntity, sourceItem, sourceModule));
        }

        public abstract bool ApplyEffect(Entity target, Entity source, Item sourceItem, Module sourceModule);
        
        public abstract void RemoveEffect(Entity target, Entity source, Item sourceItem, Module sourceModule);
    }
}