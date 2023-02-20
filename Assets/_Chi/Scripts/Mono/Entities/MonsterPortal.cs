using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class MonsterPortal : DestructibleStructure
    {
        public List<SpawnPrefab> continuousPrefabsToSpawn;
        
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public float continuousSpawnIntervalMin = 0.25f;
        public float continuousSpawnIntervalMax = 0.75f;
        public float continuousSpawnWhenPlayerNearbyDist2 = 81f;
        public int continuousSpawnAtOnceCount = 1;
        public float continuousSpawnSpread = 1.5f;
        public float distanceBeforeDespawn = 100f;

        public float despawnAfterTime = 0;

        public override void Start()
        {
            base.Start();
            
            prefabsByWeightValues = continuousPrefabsToSpawn.ToWeights();

            StartCoroutine(Updater());
        }

        private IEnumerator Updater()
        {
            var waiter = new WaitForSeconds(0.05f);

            float nextSpawn = 0;
            
            var despawn = despawnAfterTime > 0 ? Time.time + despawnAfterTime : 0;

            while (isAlive)
            {
                yield return waiter;
                
                if (despawn > 0 && despawn < Time.time)
                {
                    OnDie(DieCause.Despawned);
                    yield break;
                }
                
                UpdaterAction();
                
                if (continuousPrefabsToSpawn.Any() && dist2ToPlayer <= continuousSpawnWhenPlayerNearbyDist2 && nextSpawn < Time.time)
                {
                    nextSpawn = Time.time + Random.Range(continuousSpawnIntervalMin, continuousSpawnIntervalMax);

                    var pos = GetPosition();
                    var playerPos = Gamesystem.instance.objects.currentPlayer.GetPosition();

                    for (int i = 0; i < continuousSpawnAtOnceCount; i++)
                    {
                        var prefab = GetRandomPrefab();
                        
                        var targetPosition = pos + (new Vector3(Random.Range(-continuousSpawnSpread, continuousSpawnSpread), Random.Range(-continuousSpawnSpread, continuousSpawnSpread), 0));
                        
                        prefab.SpawnOnPosition(targetPosition, playerPos, distanceBeforeDespawn);
                    }
                }
                
            }
        }

        protected virtual void UpdaterAction()
        {
            
        }
        
        private SpawnPrefab GetRandomPrefab()
        {
            var random = (int) Random.Range(0, prefabsByWeightValues.Count);

            return prefabsByWeightValues[random];
        }
    }
}