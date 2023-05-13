using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ImmediateEffectWithDuration : ImmediateEffect
    {
        public float duration;

        public bool ignoreDuplicateEffects;

        public bool stackDuration;
        
        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if ((ignoreDuplicateEffects || stackDuration) && !target.AddImmediateEffect(this, duration, stackDuration))
            {
                return false;
            }
            
            if (!ApplyEffect(target, sourceEntity, sourceItem, sourceModule))
            {
                return false;
            }

            ScheduleRemove(Time.time + duration, target, sourceEntity, sourceItem, sourceModule);

            return true;
        }

        protected void ScheduleRemove(float time, Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule)
        {
            Gamesystem.instance.Schedule(time, () => DoRemoveEffect(target, sourceEntity, sourceItem, sourceModule));
        }
        
        protected void DoRemoveEffect(Entity target, Entity source, Item sourceItem, Module sourceModule)
        {
            bool canRemove = false;

            if (ignoreDuplicateEffects || stackDuration)
            {
                var rescheduledUntil = target.TryRemoveImmediateEffect(this);
                if (rescheduledUntil > 0)
                {
                    ScheduleRemove(rescheduledUntil, target, source, sourceItem, sourceModule);
                }
                else if (rescheduledUntil > -1)
                {
                    canRemove = true;
                }
            }
            else
            {
                canRemove = true;
            }

            if (canRemove)
            {
                RemoveEffect(target, source, sourceItem, sourceModule);   
            }
        }

        public abstract bool ApplyEffect(Entity target, Entity source, Item sourceItem, Module sourceModule);
        
        public abstract void RemoveEffect(Entity target, Entity source, Item sourceItem, Module sourceModule);
    }
}