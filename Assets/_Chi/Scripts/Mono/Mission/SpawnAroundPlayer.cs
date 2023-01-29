using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Mission
{
    public class SpawnAroundPlayer : SerializedMonoBehaviour, IMissionHandler
    {
        public List<Spawn> spawns;
        
        [NonSerialized] private float nextCurvesUpdate = 0;
        
        public void OnStart()
        {
            Debug.Log("start");
            
            foreach (var settings in spawns)
            {
                settings.Initialise();
                
                settings.startAtTime = Time.time;
                
                if (settings.baseCountPerMinute > 0)
                {
                    settings.nextSpawnTime = Time.time + (60 / settings.baseCountPerMinute);
                }
                else
                {
                    settings.nextSpawnTime = -1;
                }
            }
        }

        public void OnStop()
        {
            
        }

        public void Update()
        {
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPosition = player.GetPosition();
            var time = Time.time;
            
            bool updateCurves = false;
            if (Time.time > nextCurvesUpdate)
            {
                nextCurvesUpdate = Time.time + .25f;
                updateCurves = true;
            }

            foreach (var settings in spawns)
            {
                if (updateCurves)
                {
                    settings.baseCountPerMinute = settings.countPerMinuteCurve.Evaluate(time-settings.startAtTime);
                    settings.Recalculate(Time.time - settings.startAtTime);
                }
                
                if (settings.nextSpawnTime > 0 && settings.nextSpawnTime < Time.time)
                {
                    settings.lastSpawnTime = Time.time;
                    settings.nextSpawnTime = settings.lastSpawnTime + (60 / settings.GetCountPerMinute(time-settings.startAtTime));

                    var prefab = settings.GetRandomPrefab();

                    var distance = settings.GetDistanceFromPlayer(time);
                    var dir = (Vector3) Random.insideUnitCircle.normalized * distance;
                    var spawnPosition = playerPosition + dir;
                    
                    settings.SpawnOnPosition(spawnPosition, prefab, playerPosition);

                    //TODO spawn

                    /*List<Transform> spawnPositions;
                    bool doSpawnOnAllSpawns = false;

                    if (kp.SpawnsInScene != null && kp.SpawnsInScene.Any())
                    {
                        spawnPositions = kp.SpawnsInScene;
                        doSpawnOnAllSpawns = kp.spawnOnAllSpawns;
                    }
                    else
                    {
                        spawnPositions = DefaultSpawnsInScene;
                        doSpawnOnAllSpawns = spawnOnAllSpawns;
                    }

                    if (Gamesystem.Instance.Status.CanSpawnNewMonsters() && !isPaused)
                    {
                        if (doSpawnOnAllSpawns)
                        {
                            foreach (var spawn in spawnPositions)
                            {
                                if (spawn.gameObject.GetEntity() is ZombieHole hole)
                                {
                                    if (hole.enabled)
                                    {
                                        SpawnSingle(spawn, kp, 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            SpawnSingle(spawnPositions[Random.Range(0, spawnPositions.Count)], kp, 1);
                        }
                    }*/
                }
            }
        }
    }

    [Serializable]
    public class Spawn
    {
        public List<SpawnPrefab> possiblePrefabs;

        public AnimationCurve countPerMinuteCurve;
        
        [FormerlySerializedAs("countPerMinute")] [ReadOnly] public float baseCountPerMinute;
        public float countPerMinuteRandomAdd = 0;
        public float countPerMinuteRandomMulFrom = 1;
        public float countPerMinuteRandomMulTo = 1;

        public AnimationCurve countPerMinuteRandomAddCurve = AnimationCurve.Constant(0, 1, 0);
        public AnimationCurve countPerMinuteRandomMulCurve = AnimationCurve.Constant(0, 1, 1);

        public AnimationCurve minSpawnCount = AnimationCurve.Constant(0, 1, 1);
        public AnimationCurve maxSpawnCount = AnimationCurve.Constant(0, 1, 1);

        public AnimationCurve distanceFromPlayerCurve = AnimationCurve.Constant(0, 1, 12);
        
        public float spawnGroupSpreadMin = 1;
        public float spawnGroupSpreadMax = 1;
        
        public float distanceFromPlayerToDespawn = 100; // TODO
        
        // runtime
        [ReadOnly] public float nextSpawnTime;
        [ReadOnly] public float startAtTime = 0;
        [ReadOnly] public float lastSpawnTime = 0;
        [NonSerialized] private float sumPossiblePrefabsWeights;
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise()
        {
            sumPossiblePrefabsWeights = possiblePrefabs.Sum(p => p.weight);

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
        
        public void Recalculate(float time)
        {
            nextSpawnTime = lastSpawnTime + (60 / GetCountPerMinute(time));
            //Debug.Log(GetCountPerMinute(time));
        }
        
        public float GetCountPerMinute(float time)
        {
            //if(randomizeCount) return countPerMinute * CalcRandomMul(time) + (time) * randomizeIncreasePerSecond;

            return baseCountPerMinute
                   * countPerMinuteRandomMulCurve.Evaluate(time)
                   * Random.Range(countPerMinuteRandomMulFrom, countPerMinuteRandomMulTo)
                   + Random.Range(-countPerMinuteRandomAdd, countPerMinuteRandomAdd)
                   + countPerMinuteRandomAddCurve.Evaluate(time);
        }

        public SpawnPrefab GetRandomPrefab()
        {
            var random = (int) Random.Range(0, prefabsByWeightValues.Count);

            return prefabsByWeightValues[random];
        }

        public float GetDistanceFromPlayer(float time)
        {
            return distanceFromPlayerCurve.Evaluate(time);
        }

        public void SpawnOnPosition(Vector3 position, SpawnPrefab prefab, Vector3 attackTarget)
        {
            Quaternion rotation;

            if (prefab.rotateTowardsPlayer)
            {
                rotation = Quaternion.LookRotation(position - attackTarget, Vector3.forward);
                rotation.x = 0;
                rotation.y = 0;
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            }
            
            switch (prefab.type)
            {
                case SpawnPrefabType.Npc:
                    var npc = prefab.prefabNpc.SpawnPooledNpc(position, rotation);
                    npc.maxDistanceFromPlayerBeforeDespawn = distanceFromPlayerToDespawn * distanceFromPlayerToDespawn;
                    break;
                case SpawnPrefabType.Gameobject:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [Serializable]
    public class SpawnPrefab
    {
        public SpawnPrefabType type;
        
        [ShowIf("type", SpawnPrefabType.Gameobject)]
        public GameObject prefab;
        
        [ShowIf("type", SpawnPrefabType.Npc)]
        public Npc prefabNpc;
        
        public int weight;

        public bool rotateTowardsPlayer = true;
    }

    public enum SpawnPrefabType
    {
        Npc,
        Gameobject
    }
}