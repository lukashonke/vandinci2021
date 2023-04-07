using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class MonsterStructure : MonsterPortal
    {
        public List<MonsterStructureSpawnItem> damageBasedSpawns;
        
        private List<MonsterStructureSpawnItem> spawnsRepeated;
        private List<MonsterStructureSpawnItem> spawnsOnce;

        public override void Awake()
        {
            base.Awake();

            foreach (var spawn in damageBasedSpawns)
            {
                spawn.Initialise();
            }

            spawnsRepeated = damageBasedSpawns.Where(s => s.spawnAtHpPercentIsRepeated).OrderByDescending(s => s.spawnAtHpPercent).ToList();
            spawnsOnce = damageBasedSpawns.Where(s => !s.spawnAtHpPercentIsRepeated).ToList();

            var hpPercent = entityStats.hp / entityStats.maxHp;
            foreach (var spawn in spawnsRepeated)
            {
                spawn.nextSpawnHpPercentAt = hpPercent - spawn.spawnAtHpPercent;
            }
        }

        protected override void UpdaterAction()
        {
            base.UpdaterAction();

            if (!damageBasedSpawns.Any()) return;

            var currentHpPercent = entityStats.hp / entityStats.maxHp;

            if (spawnsOnce.Count > 0)
            {
                for (var index = 0; index < spawnsOnce.Count; index++)
                {
                    var attackSpawnGroup = spawnsOnce[index];
                    if (currentHpPercent > attackSpawnGroup.spawnAtHpPercent)
                    {
                        break;
                    }

                    SpawnGroup(attackSpawnGroup);
                    
                    spawnsOnce.RemoveAt(index);

                    break;
                }
            }

            if (spawnsRepeated.Count > 0)
            {
                foreach (var attackSpawnGroup in spawnsRepeated)
                {
                    if (currentHpPercent <= attackSpawnGroup.nextSpawnHpPercentAt)
                    {
                        SpawnGroup(attackSpawnGroup);
                        attackSpawnGroup.nextSpawnHpPercentAt = currentHpPercent - attackSpawnGroup.spawnAtHpPercent;
                    }
                }
            }
        }
        
        private void SpawnGroup(MonsterStructureSpawnItem group)
        {
            var pos = GetPosition();
            var playerPos = Gamesystem.instance.objects.currentPlayer.GetPosition();
            
            int count = Random.Range(group.spawnCountMin, group.spawnCountMax);
            for (int i = 0; i < count; i++)
            {
                var prefab = group.GetRandomPrefab();
                        
                var targetPosition = pos + (new Vector3(Random.Range(-group.spawnSpread, group.spawnSpread), Random.Range(-group.spawnSpread, group.spawnSpread), 0));
                        
                prefab.SpawnOnPosition(targetPosition, playerPos, distanceBeforeDespawn, 0f, DespawnCondition.DistanceFromScreenBorder);
            }
        }
    }
    
    [Serializable]
    public class MonsterStructureSpawnItem
    {
        public bool spawnAtHpPercentIsRepeated;
        public float spawnAtHpPercent;
        [NonSerialized] public float nextSpawnHpPercentAt;
        
        public List<SpawnPrefab> prefabsToSpawn;

        public int spawnCountMin = 1;
        public int spawnCountMax = 1;
        public float spawnSpread = 1.5f;
        
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise()
        {
            prefabsByWeightValues = prefabsToSpawn.ToWeights();
        }
        
        public SpawnPrefab GetRandomPrefab()
        {
            var random = (int) Random.Range(0, prefabsByWeightValues.Count);

            return prefabsByWeightValues[random];
        }
    }
}