using _Chi.Scripts.Mono.Entities;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class Skill : SerializedScriptableObject
    {
        public float reuseDelay = 1f;

        public GameObject vfx;
        [ShowIf("vfx")]
        public float vfxDespawnAfter;
        
        public abstract bool Trigger(Entity entity, bool force = false);
        
        public T GetSkillData<T>(Entity entity) where T : SkillData
        {
            return entity.GetSkillData(this) as T;
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
            skillData.lastUse = Time.time;
        }

        private float GetReuseDelay(Player player)
        {
            return reuseDelay * player.stats.skillReuseMul.GetValue();
        }
        
        public float GetReuseDelay(Entity entity)
        {
            if (entity is Player player)
            {
                return GetReuseDelay(player);
            }
            return reuseDelay;
        }

        public void SpawnPrefabVfx(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (vfx == null) return;
            
            var instance = Gamesystem.instance.poolSystem.SpawnGo(vfx);

            instance.transform.position = position;
            //instance.transform.rotation = rotation;
            instance.transform.parent = parent;

            if (vfxDespawnAfter > 0)
            {
                Gamesystem.instance.Schedule(Time.time + vfxDespawnAfter, () => Gamesystem.instance.poolSystem.DespawnGo(vfx, instance));
            }
        }

        public abstract SkillData CreateDefaultSkillData();
    }

    public abstract class SkillData
    {
        public float nextPossibleUse;

        public float lastUse;
    }
}