using System.Collections;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Spawn", menuName = "Gama/Skills/Spawn")]
    public class SpawnSkill : Skill
    {
        public GameObject prefab;

        public float despawnAfter;
        
        public override bool Trigger(Entity entity, bool force = false)
        {
            if (!force && !CanTrigger(entity)) return false;
                
            DoSpawn(entity);
                
            entity.OnSkillUse();
            
            SpawnPrefabVfx(entity.GetPosition(), entity.transform.rotation, entity.transform);
                
            SetNextSkillUse(entity, GetReuseDelay(entity));
            return true;
        }

        private void DoSpawn(Entity entity)
        {
            var instance = Gamesystem.instance.poolSystem.SpawnGo(prefab);
            instance.transform.position = entity.GetPosition();
            
            if (despawnAfter > 0)
            {
                Gamesystem.instance.Schedule(Time.time + despawnAfter, () => Gamesystem.instance.poolSystem.DespawnGo(prefab, instance));
            }
        }

        public override SkillData CreateDefaultSkillData()
        {
            return new SpawnManySkillData();
        }
    }

    public class SpawnManySkillData : SkillData
    {
        
    }
}