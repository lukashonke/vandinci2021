using _Chi.Scripts.Mono.Common;
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
        
        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if ((ignoreDuplicateEffects || stackDuration) && !data.target.AddImmediateEffect(this, duration, stackDuration))
            {
                return false;
            }

            var dataCopy = Gamesystem.instance.poolSystem.GetEffectData();
            dataCopy.Copy(data);
            
            if (!ApplyEffect(dataCopy))
            {
                return false;
            }

            ScheduleRemove(Time.time + duration, dataCopy);

            return true;
        }

        protected void ScheduleRemove(float time, EffectSourceData data)
        {
            Gamesystem.instance.Schedule(time, () => DoRemoveEffect(data));
        }
        
        protected void DoRemoveEffect(EffectSourceData data)
        {
            bool canRemove = false;

            if (ignoreDuplicateEffects || stackDuration)
            {
                var rescheduledUntil = data.target.TryRemoveImmediateEffect(this);
                if (rescheduledUntil > 0)
                {
                    ScheduleRemove(rescheduledUntil, data);
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
                RemoveEffect(data);
                Gamesystem.instance.poolSystem.ReturnEffectData(data);
            }
            
            //TODO cannot release this data as its over multiple frames
        }

        public abstract bool ApplyEffect(EffectSourceData data);
        
        public abstract void RemoveEffect(EffectSourceData data);
    }
}