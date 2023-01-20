using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ImmediateEffectWithDuration : ImmediateEffect
    {
        public float duration;
        
        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem)
        {
            if (!ApplyEffect(target, sourceEntity, sourceItem))
            {
                return false;
            }
            
            Gamesystem.instance.Schedule(Time.time + duration, () => RemoveEffect(target, sourceEntity, sourceItem));

            return true;
        }

        public abstract bool ApplyEffect(Entity target, Entity source, Item sourceItem);
        
        public abstract void RemoveEffect(Entity target, Entity source, Item sourceItem);
    }
}