using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using Sirenix.OdinInspector;
using UnityEngine;
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
            
                    var spawned = settings.SpawnOnPosition(targetPosition, settings.GetRandomPrefab(), position);

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

                        var spawned = settings.SpawnOnPosition(targetPosition, settings.GetRandomPrefab(), position);
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

        public float distanceFromPlayer;

        public float spawnGroupSpreadMin = 1;
        public float spawnGroupSpreadMax = 1;
        
        public float distanceFromPlayerToDespawn = 100;
        
        // runtime
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise(float time)
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
                    var go = Object.Instantiate(prefab.prefabNpc.gameObject, position, rotation);

                    var npc2 = go.GetComponent<Npc>();
                    Gamesystem.instance.prefabDatabase.ApplyPrefabVariant(npc2, prefab.prefabVariant);
                    npc2.maxDistanceFromPlayerBeforeDespawn = distanceFromPlayerToDespawn * distanceFromPlayerToDespawn;
                    return npc2;
                case SpawnPrefabType.Gameobject:
                    var go2 = Object.Instantiate(prefab.prefab);
                    go2.transform.position = position;
                    go2.transform.rotation = rotation;
                    return null;
                case SpawnPrefabType.PoolableGo:
                    var poolable = Gamesystem.instance.poolSystem.SpawnPoolable(prefab.prefab);
                    poolable.MoveTo(position);
                    poolable.Rotate(rotation);
                    poolable.Run();
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }
    }
}