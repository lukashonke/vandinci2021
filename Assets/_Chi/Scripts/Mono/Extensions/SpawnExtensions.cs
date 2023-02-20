using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class SpawnExtensions
    {
        public static Entity SpawnOnPosition(this SpawnPrefab prefab, Vector3 position, Vector3 attackTarget, float distanceFromPlayerToDespawn, float despawnAfter = 0)
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
                if (prefab.noRotation)
                {
                    rotation = Quaternion.identity;
                }
                else
                {
                    rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                }
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
                    if (despawnAfter > 0)
                    {
                        Object.Destroy(go2, despawnAfter);
                    }
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

        public static Dictionary<int, SpawnPrefab> ToWeights(this List<SpawnPrefab> prefabs)
        {
            var prefabsByWeightValues = new Dictionary<int, SpawnPrefab>();

            int index = 0;
            foreach (var pp in prefabs)
            {
                for (int i = 0; i < pp.weight; i++)
                {
                    prefabsByWeightValues.Add(index++, pp);
                }
            }

            return prefabsByWeightValues;
        }
    }
}