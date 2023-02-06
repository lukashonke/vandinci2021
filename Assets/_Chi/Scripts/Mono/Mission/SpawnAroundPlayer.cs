using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission.Events;
using _Chi.Scripts.Scriptables;
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
        [NonSerialized] public MissionEvent ev;

        public void OnStart(MissionEvent ev)
        {
            this.ev = ev;
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

        public bool IsFinished()
        {
            return true;
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

                    var distance = settings.GetDistanceFromPlayer(time);
                    var dir = (Vector3) Random.insideUnitCircle.normalized * distance;
                    var spawnPosition = playerPosition + dir;
                    
                    var spawnCount = settings.GetCountToSpawn(time);

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
                        int squareSize = (int)Math.Ceiling(Math.Sqrt(spawnCount));

                        for (int row = 0; row < squareSize; row++)
                        {
                            for (int column = 0; column < squareSize; column++)
                            {
                                if (spawnCount == 0) break;
                                
                                var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);
                                    
                                var targetPosition = spawnPosition + (new Vector3(column*spread, row*spread, 0));
                                
                                var spawned = settings.SpawnOnPosition(targetPosition, settings.GetRandomPrefab(), playerPosition);
                                ev.TrackAliveEntity(spawned);
                                Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
                                
                                spawnCount--;
                            }
                            
                            if (spawnCount == 0) break;
                        }
                    }
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
        
        public float distanceFromPlayerToDespawn = 100;
        
        // runtime
        [ReadOnly] public float nextSpawnTime;
        [ReadOnly] public float startAtTime = 0;
        [ReadOnly] public float lastSpawnTime = 0;
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise()
        {
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

        public int GetCountToSpawn(float time)
        {
            return (int)Math.Round(Random.Range(minSpawnCount.Evaluate(time), maxSpawnCount.Evaluate(time)));
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
                case SpawnPrefabType.Gameobject:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }

    [Serializable]
    public class SpawnPrefab
    {
        public SpawnPrefabType type;
        
        [ShowIf("type", SpawnPrefabType.Gameobject)]
        public GameObject prefab;
        
        [HideIf("type", SpawnPrefabType.Gameobject)]
        public Npc prefabNpc;
        
        [Min(1)]
        public int weight;

        public bool rotateTowardsPlayer = true;
        public bool noRotation = false;

        [Required]
        public string prefabVariant;

        private static string[] variants()
        {
            return Gamesystem.instance.prefabDatabase.variants.Select(v => v.variant).ToArray();
        }
    }

    public enum SpawnPrefabType
    {
        PooledNpc,
        NonPooledNpc,
        Gameobject
    }
}