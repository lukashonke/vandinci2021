using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Mission
{
    //TODO wawe spawn
    //TODO initial delay to activate
    //TODo repeated wawe, repeat interval, repeat count
    //TODO set direction from player
    //TODO possibility to track

    public class SpawnWawe : SerializedMonoBehaviour, IMissionHandler
    {
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
                settings.nextSpawnTime = Time.time + settings.spawnTime;
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

        private IEnumerator UpdateRoutine()
        {
            var waiter = new WaitForSeconds(0.2f);

            while (running)
            {
                yield return waiter;

                var player = Gamesystem.instance.objects.currentPlayer;
                var playerPosition = player.GetPosition();
                var time = Time.time;

                foreach (var settings in spawns)
                {
                    if (!settings.finished && settings.nextSpawnTime < time)
                    {
                        settings.lastSpawnTime = time;
                        settings.nextSpawnTime = 0;

                        //SPAWN
                        var spawnCount = settings.GetCountToSpawn(time);

                        int squareSize = (int) Math.Ceiling(Math.Sqrt(spawnCount));

                        var distance = settings.GetDistanceFromPlayer(time);
                        var dir = (Vector3) Random.insideUnitCircle.normalized * distance;
                        var spawnPosition = playerPosition + dir;

                        if (spawnCount <= 2)
                        {
                            for (int i = 0; i < spawnCount; i++)
                            {
                                var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);
                        
                                var targetPosition = spawnPosition + (new Vector3(i*spread, 0, 0));
                        
                                var spawned = settings.SpawnOnPosition(targetPosition, settings.GetRandomPrefab(), playerPosition);
                                ev.TrackAliveEntity(spawned);
                                Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
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

                                    var spawned = settings.SpawnOnPosition(targetPosition, settings.GetRandomPrefab(),
                                        playerPosition);
                                    ev.TrackAliveEntity(spawned);
                                    Gamesystem.instance.missionManager.TrackAliveEntity(spawned);

                                    spawnCount--;
                                }

                                if (spawnCount == 0) break;
                            }
                        }
                        

                        if (settings.repeatSpawn)
                        {
                            if (settings.repeatedCount < settings.repeatCount)
                            {
                                settings.nextSpawnTime = settings.lastSpawnTime + settings.repeatSpawnInterval;
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
                }
            }
        }
    }

    [Serializable]
    public class SpawnWaweData
    {
        public List<SpawnPrefab> possiblePrefabs;

        public float spawnTime;

        public bool repeatSpawn;
        
        [ShowIf("repeatSpawn")]
        public float repeatSpawnInterval;
        [ShowIf("repeatSpawn")]
        public int repeatCount;

        public int spawnCountMin;
        public int spawnCountMax;

        public float distanceFromPlayer;

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
            
            prefabsByWeightValues = new Dictionary<int, SpawnPrefab>();

            int index = 0;
            foreach (var pp in possiblePrefabs)
            {
                for (int i = 0; i < pp.weight; i++)
                {
                    prefabsByWeightValues.Add(index++, pp);
                }
            }
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
            return distanceFromPlayer;
        }

        public Entity SpawnOnPosition(Vector3 position, SpawnPrefab prefab, Vector3 attackTarget)
        {
            Quaternion rotation;

            if (prefab.rotateTowardsPlayer)
            {
                rotation = Quaternion.LookRotation(position - attackTarget, Vector3.forward);
                rotation.x = 0;
                rotation.y = 0;
            }
            else if (prefab.noRotation)
            {
                rotation = Quaternion.identity;
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            }
            
            switch (prefab.type)
            {
                case SpawnPrefabType.PooledNpc:
                    var npc = prefab.prefabNpc.SpawnPooledNpc(position, rotation);
                    Gamesystem.instance.prefabDatabase.ApplyPrefabVariant(npc, prefab.prefabVariant);
                    npc.maxDistanceFromPlayerBeforeDespawn = distanceFromPlayerToDespawn * distanceFromPlayerToDespawn;
                    return npc;
                case SpawnPrefabType.NonPooledNpc:
                    var go = GameObject.Instantiate(prefab.prefabNpc.gameObject, position, rotation);

                    var npc2 = go.GetComponent<Npc>();
                    Gamesystem.instance.prefabDatabase.ApplyPrefabVariant(npc2, prefab.prefabVariant);
                    npc2.maxDistanceFromPlayerBeforeDespawn = distanceFromPlayerToDespawn * distanceFromPlayerToDespawn;
                    return npc2;
                case SpawnPrefabType.Gameobject:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}