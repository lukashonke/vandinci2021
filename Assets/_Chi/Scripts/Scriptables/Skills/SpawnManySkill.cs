using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Spawn Many (NPC)", menuName = "Gama/Skills/Spawn Many (NPC)")]
    public class SpawnManySkill : Skill
    {
        public Npc npc;

        public string variant;

        public float pushForceMin, pushForceMax;
        
        public float angleMin, angleMax;

        public int spawnCount;

        public float jumpToAliveDuration = 1f;

        public override bool Trigger(Entity entity, bool force = false)
        {
            bool usedExtraCharge = false;            
            if (!force && !CanTrigger(entity, out usedExtraCharge)) return false;

            entity.StartCoroutine(DoSpawn(entity));
                
            entity.OnSkillUse();
            
            SpawnPrefabVfx(entity.GetPosition(), entity.transform.rotation, entity.transform);

            if (!usedExtraCharge)
            {
                SetNextSkillUse(entity, GetReuseDelay(entity));
            }
            return true;
        }

        private IEnumerator DoSpawn(Entity entity)
        {
            List<Npc> spawnedNpcs = ListPool<Npc>.Get();

            var player = Gamesystem.instance.objects.currentPlayer;
            var dist = Utils.Dist2(player.GetPosition(), entity.GetPosition());

            for (int i = 0; i < spawnCount; i++)
            {
                var randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                var spawned = npc.SpawnPooledNpc(entity.GetPosition(), randomRotation);
                Gamesystem.instance.prefabDatabase.ApplyPrefabVariant(spawned, variant);
                
                spawned.SetImmobilizedUntil(Time.time + jumpToAliveDuration);
                spawned.SetDistanceToPlayer(dist, player);
                
                var pushForce = Random.Range(pushForceMin, pushForceMax);
                var pushDirection = entity.GetForwardVector(Random.Range(angleMin, angleMax)) * pushForce;
                
                spawned.rb.AddForce(pushDirection, ForceMode2D.Impulse);
                
                spawnedNpcs.Add(spawned);
            }

            yield return new WaitForSeconds(jumpToAliveDuration);
            
            foreach (var spawned in spawnedNpcs)
            {
                spawned.rb.velocity = Vector2.zero;
            }
            
            ListPool<Npc>.Release(spawnedNpcs);
        }

        public override SkillData CreateDefaultSkillData()
        {
            return new SpawnSkillData();
        }
    }

    public class SpawnSkillData : SkillData
    {
        
    }
}