using System;
using System.Collections.Generic;
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
        
        public virtual void SetActivated(Entity entity, bool activated)
        {
            SkillData skillData = GetSkillData<SkillData>(entity);

            if (skillData == null) return;

            skillData.activated = activated;
        }

        public virtual bool CanTrigger(Entity entity, out bool consumedSkillCharge)
        {
            consumedSkillCharge = false;
            
            SkillData skillData = GetSkillData<SkillData>(entity);

            if (skillData == null) return false;
            
            if(skillData.activated) return false;
            
            if(entity is Player player && player.extraSkillCharges[this] > 0)
            {
                consumedSkillCharge = true;
                player.RemoveExtraSkillCharges(this, 1);
                return true;
            }
            
            var skillReady = skillData.nextPossibleUse <= Time.time;
            return skillReady;
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

        public GameObject SpawnPrefabVfx(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (vfx == null) return null;
            
            var instance = Gamesystem.instance.poolSystem.SpawnGo(vfx);

            instance.transform.position = position;
            //instance.transform.rotation = rotation;
            instance.transform.parent = parent;

            if (vfxDespawnAfter > 0)
            {
                Gamesystem.instance.Schedule(Time.time + vfxDespawnAfter, () => Gamesystem.instance.poolSystem.DespawnGo(vfx, instance));
            }

            return instance;
        }
        
        public List<ImmediateEffect> GetAdditionalEffects(Player player)
        {
            List<ImmediateEffect> retValue = null;
            foreach (var upgradeItem in player.skillUpgradeItems)
            {
                if (upgradeItem.additionalEffects != null)
                {
                    if (retValue == null)retValue = new();
                    
                    retValue.AddRange(upgradeItem.additionalEffects);
                }
            }

            return retValue;
        }

        public abstract SkillData CreateDefaultSkillData();
    }

    public abstract class SkillData
    {
        public bool activated;
        
        public float nextPossibleUse;

        public float lastUse = -100f;
    }
}