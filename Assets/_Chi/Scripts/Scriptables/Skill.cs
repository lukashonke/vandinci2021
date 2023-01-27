using _Chi.Scripts.Mono.Entities;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class Skill : SerializedScriptableObject
    {
        public abstract bool Trigger(Entity entity);
        
        public T GetSkillData<T>(Entity entity) where T : SkillData
        {
            if (entity is Player player)
            {
                return player.GetSkillData(this) as T;
            }

            return null;
        }

        public virtual bool CanTrigger(Entity entity)
        {
            SkillData skillData = GetSkillData<SkillData>(entity);

            if (skillData == null) return false;

            return skillData.nextPossibleUse <= Time.time;
        }

        public virtual void SetNextSkillUse(Entity entity, float delay)
        {
            SkillData skillData = GetSkillData<SkillData>(entity);

            if (skillData == null) return;

            skillData.nextPossibleUse = Time.time + delay;
        }

        public abstract SkillData CreateDefaultSkillData();
    }

    public abstract class SkillData
    {
        public float nextPossibleUse;
    }
}