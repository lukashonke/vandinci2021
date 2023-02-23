using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission.Events;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Mission
{
    public class SpawnWawe : SerializedMonoBehaviour, IMissionHandler
    {
        [HorizontalGroup("Main")]
        public string waveName;
        [HorizontalGroup("Main")]
        public bool disable;
        
        public List<SpawnWaweData> spawns;

        [NonSerialized] private float nextCurvesUpdate = 0;
        [NonSerialized] public MissionEvent ev;

        private bool running;

        public void OnStart(MissionEvent ev)
        {
            this.ev = ev;
            Debug.Log("start wawe");

            foreach (var settings in spawns)
            {
                settings.Initialise(Time.time);
                settings.nextSpawnTime = Time.time + Random.Range(settings.spawnTimeMin, settings.spawnTimeMax);
            }

            running = true;

            StartCoroutine(UpdateRoutine());
        }

        public void OnStop()
        {
            running = false;
        }

        public bool IsFinished()
        {
            return spawns.All(s => s.finished);
        }

        public static void Spawn(SpawnWaweData settings, Vector3 playerPosition, float time, Entity relativeTo, MissionEvent ev)
        {
            settings.lastSpawnTime = time;
            settings.nextSpawnTime = 0;

            //SPAWN
            var spawnCount = settings.GetCountToSpawn(time);

            int squareSize = (int) Math.Ceiling(Math.Sqrt(spawnCount));

            var distance = settings.GetDistanceFromPlayer(time);
            
            var relativePos = settings.relativePosition;
            if (relativePos == SpawnRelativePosition.FrontOrBehindPlayer)
            {
                relativePos = Random.Range(0, 2) == 0 ? SpawnRelativePosition.FrontOfPlayer : SpawnRelativePosition.BehindPlayer;
            }

            Vector3 spawnPosition = Vector3.zero;
            switch (relativePos)
            {
                case SpawnRelativePosition.AroundPlayer:
                    var dir1 = (Vector3) Random.insideUnitCircle.normalized * distance;
                    spawnPosition = playerPosition + dir1;
                    break;
                case SpawnRelativePosition.FrontOfPlayer:
                    var dir2 = relativeTo.GetForwardVector().normalized * distance;
                    spawnPosition = playerPosition + dir2;
                    break;
                case SpawnRelativePosition.BehindPlayer:
                    var dir3 = -relativeTo.GetForwardVector().normalized * distance;
                    spawnPosition = playerPosition + dir3;
                    break;
            }

            Vector3? goToDirection = null;
            if (settings.behavior == SpawnBehavior.RoamRandomly)
            {
                goToDirection = (Vector3) Random.insideUnitCircle.normalized * settings.roamDistance;
                goToDirection += (Vector3) Random.insideUnitCircle * settings.roamRandomRadius;
            }
            else if(settings.behavior == SpawnBehavior.RoamTowardsPlayer)
            {
                goToDirection = (playerPosition - spawnPosition).normalized * settings.roamDistance;
                goToDirection += (Vector3) Random.insideUnitCircle * settings.roamRandomRadius;
            }

            if (spawnCount <= 2)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);
            
                    var targetPosition = spawnPosition + (new Vector3(i*spread, 0, 0));

                    var spawnPrefab = settings.GetRandomPrefab();

                    var spawned = spawnPrefab.SpawnOnPosition(targetPosition, playerPosition, settings.distanceFromPlayerToDespawn, settings.despawnAfter);
                    if (spawned != null)
                    {
                        ev?.TrackAliveEntity(spawned);
                        Gamesystem.instance.missionManager.TrackAliveEntity(spawned);

                        if (spawned is Npc npc)
                        {
                            if (settings.behavior == SpawnBehavior.StandIdle)
                            {
                                npc.SetCanMove(false);
                            }
                            else if (goToDirection.HasValue)
                            {
                                npc.SetFixedMoveTarget(targetPosition + goToDirection.Value, settings.stopWhenReachFixedMoveTarget, settings.dieWhenReachFixedMoveTarget);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int row = 0; row < squareSize; row++)
                {
                    for (int column = 0; column < squareSize; column++)
                    {
                        if (spawnCount == 0) break;

                        var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);

                        var targetPosition = spawnPosition + (new Vector3(column * spread, row * spread, 0));

                        var spawnPrefab = settings.GetRandomPrefab();
                        var spawned = spawnPrefab.SpawnOnPosition(targetPosition, playerPosition, settings.distanceFromPlayerToDespawn, settings.despawnAfter);

                        if (spawned != null)
                        {
                            ev?.TrackAliveEntity(spawned);
                            Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
                            
                            if (spawned is Npc npc)
                            {
                                if (settings.behavior == SpawnBehavior.StandIdle)
                                {
                                    npc.SetCanMove(false);
                                }
                                else if (goToDirection.HasValue)
                                {
                                    npc.SetFixedMoveTarget(targetPosition + goToDirection.Value, settings.stopWhenReachFixedMoveTarget, settings.dieWhenReachFixedMoveTarget);
                                }
                            }
                        }

                        spawnCount--;
                    }

                    if (spawnCount == 0) break;
                }
            }
            

            if (settings.repeatSpawn)
            {
                if (settings.repeatedCount < settings.repeatCount)
                {
                    settings.nextSpawnTime = settings.lastSpawnTime + Random.Range(settings.repeatSpawnIntervalMin, settings.repeatSpawnIntervalMax);
                    settings.repeatedCount++;
                }
                else
                {
                    settings.finished = true;
                }
            }
            else
            {
                settings.finished = true;
            }

            settings.firstSpawnDone = true;
        }

        private IEnumerator UpdateRoutine()
        {
            var waiter = new WaitForSeconds(0.2f);

            while (running)
            {
                yield return waiter;

                if (disable)
                {
                    continue;
                }

                var player = Gamesystem.instance.objects.currentPlayer;
                var playerPosition = player.GetPosition();
                var time = Time.time;

                foreach (var settings in spawns)
                {
                    if (!settings.finished && settings.nextSpawnTime < time)
                    {
                        Spawn(settings, playerPosition, time, player, ev);
                    }
                }
            }
        }
    }

    [Serializable]
    public class SpawnWaweData
    {
        public List<SpawnPrefab> possiblePrefabs;

        public SpawnRelativePosition relativePosition;
        
        public SpawnBehavior behavior;

        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public float roamDistance;
        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public float roamRandomRadius;

        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public bool dieWhenReachFixedMoveTarget;
        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public bool stopWhenReachFixedMoveTarget;

        public float spawnTimeMin;
        public float spawnTimeMax;

        public bool repeatSpawn;
        
        [ShowIf("repeatSpawn")]
        public float repeatSpawnIntervalMin;
        [ShowIf("repeatSpawn")]
        public float repeatSpawnIntervalMax;
        [ShowIf("repeatSpawn")]
        public int repeatCount;

        public int spawnCountMin;
        public int spawnCountMax;

        [FormerlySerializedAs("distanceFromPlayer")] public float distanceFromPlayerMin;
        [FormerlySerializedAs("distanceFromPlayer")] public float distanceFromPlayerMax;
        public float despawnAfter;

        public float spawnGroupSpreadMin = 1;
        public float spawnGroupSpreadMax = 1;
        
        public float distanceFromPlayerToDespawn = 100;
        
        // runtime
        [NonSerialized] public float nextSpawnTime;
        [NonSerialized] public float lastSpawnTime;
        [NonSerialized] public int repeatedCount = 0;
        [NonSerialized] public bool finished;
        [NonSerialized] public bool firstSpawnDone;
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise(float time)
        {
            firstSpawnDone = false;
            finished = false;

            prefabsByWeightValues = possiblePrefabs.ToWeights();
        }
        
        public float GetCountToSpawn(float time)
        {
            //if(randomizeCount) return countPerMinute * CalcRandomMul(time) + (time) * randomizeIncreasePerSecond;

            return Random.Range(spawnCountMin, spawnCountMax);
        }

        public SpawnPrefab GetRandomPrefab()
        {
            var random = (int) Random.Range(0, prefabsByWeightValues.Count);

            return prefabsByWeightValues[random];
        }

        public float GetDistanceFromPlayer(float time)
        {
            return Random.Range(distanceFromPlayerMin, distanceFromPlayerMax);
        }
    }
}