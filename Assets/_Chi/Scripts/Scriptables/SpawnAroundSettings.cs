using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Spawn Settings", menuName = "Gama/Configuration/Spawn Settings")]
    public class SpawnAroundSettings : SerializedScriptableObject
    {
        public List<SpawnAroundSettingsData> spawns;

        private Dictionary<string, SpawnAroundSettingsData> spawnsByName;

        public void Initialise()
        {
            foreach (var spawn in spawns)
            {
                spawn.Initialise(0);
            }
            
            spawnsByName = spawns.ToDictionary(s => s.name);
        }

        public void Spawn(string groupName, Vector3 position)
        {
            var settings = spawnsByName[groupName];
            var time = Time.time;
            
            var spawnCount = settings.GetCountToSpawn(time);

            int squareSize = (int) Math.Ceiling(Math.Sqrt(spawnCount));

            var distance = settings.GetDistanceFromPlayer(time);
            var dir = (Vector3) Random.insideUnitCircle.normalized * distance;
            var spawnPosition = position + dir;

            if (spawnCount <= 2)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);
            
                    var targetPosition = spawnPosition + (new Vector3(i*spread, 0, 0));
                    
                    var spawnPrefab = settings.GetRandomPrefab();
                    var spawned = spawnPrefab.SpawnOnPosition(targetPosition, position, settings.distanceFromPlayerToDespawn);
            
                    if (spawned != null)
                    {
                        Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
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
                        var spawned = spawnPrefab.SpawnOnPosition(targetPosition, position, settings.distanceFromPlayerToDespawn);
                        
                        Gamesystem.instance.missionManager.TrackAliveEntity(spawned);

                        spawnCount--;
                    }

                    if (spawnCount == 0) break;
                }
            }
        }
    }

    [Serializable]
    public class SpawnAroundSettingsData
    {
        public string name;
        
        public List<SpawnPrefab> possiblePrefabs;

        public int spawnCountMin;
        public int spawnCountMax;

        [FormerlySerializedAs("distanceFromPlayer")] public float distanceFromPlayerMin;
        [FormerlySerializedAs("distanceFromPlayer")] public float distanceFromPlayerMax;

        public float spawnGroupSpreadMin = 1;
        public float spawnGroupSpreadMax = 1;
        
        public float distanceFromPlayerToDespawn = 100;
        
        // runtime
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise(float time)
        {
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