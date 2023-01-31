using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace _Chi.Scripts.Mono.System
{
    public class PoolSystem : MonoBehaviour
    {
        [NonSerialized] public Dictionary<int, ObjectPool<Projectile>> projectilePools;
        
        [NonSerialized] public Dictionary<int, ObjectPool<Npc>> npcPools;
        
        [NonSerialized] public Dictionary<GameObject, ObjectPool<GameObject>> vfxPool;

        public bool collectionChecks = true;

        public void Awake()
        {
            projectilePools = new Dictionary<int, ObjectPool<Projectile>>();
            npcPools = new Dictionary<int, ObjectPool<Npc>>();
            vfxPool = new();
        }

        public GameObject SpawnVfx(GameObject prefab, int maxPoolSize = 100)
        {
            if (vfxPool.TryGetValue(prefab, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var newPool = new ObjectPool<GameObject>(() => CreatePooledItem(prefab), OnTakeFromPool,
                    OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                vfxPool.Add(prefab, newPool);

                return newPool.Get();
            }
        }

        public Npc Spawn(Npc prefabNpc, int maxPoolSize = 10000)
        {
            if (npcPools.TryGetValue(prefabNpc.poolId, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var newPool = new ObjectPool<Npc>(() => CreatePooledItem(prefabNpc), OnTakeFromPool,
                    OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                npcPools.Add(prefabNpc.poolId, newPool);

                return newPool.Get();
            }
        }

        public Projectile Spawn(Projectile prefabProjectile, int maxPoolSize = 500)
        {
            if (projectilePools.TryGetValue(prefabProjectile.poolId, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var newPool = new ObjectPool<Projectile>(() => CreatePooledItem(prefabProjectile), OnTakeFromPool,
                    OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                projectilePools.Add(prefabProjectile.poolId, newPool);

                return newPool.Get();
            }
        }
        
        public bool DespawnVfx(GameObject instance)
        {
            if (vfxPool.TryGetValue(instance, out var pool))
            {
                pool.Release(instance);
                return true;
            }

            return false;
        }
        
        public bool Despawn(Npc npcInstance)
        {
            if (npcPools.TryGetValue(npcInstance.poolId, out var pool))
            {
                pool.Release(npcInstance);
                return true;
            }

            return false;
        }

        public void Despawn(Projectile projectileInstance)
        {
            var hash = projectileInstance.GetHashCode();
            
            if (projectilePools.TryGetValue(projectileInstance.poolId, out var pool))
            {
                pool.Release(projectileInstance);
            }
        }

        Projectile CreatePooledItem(Projectile projectile)
        {
            var go = Instantiate(projectile.gameObject);

            var hash = go.GetHashCode();

            var newProjectile = go.GetComponent<Projectile>();

            newProjectile.poolId = projectile.poolId;

            var hash2 = newProjectile.GetHashCode();

            return newProjectile;
        }

        void OnReturnedToPool(Projectile projectile)
        {
            projectile.Cleanup();
        }

        void OnTakeFromPool(Projectile projectile)
        {
            
        }

        void OnDestroyPoolObject(Projectile projectile)
        {
            Destroy(projectile.gameObject);
        }
        
        Npc CreatePooledItem(Npc prefab)
        {
            var go = Instantiate(prefab.gameObject);

            var newProjectile = go.GetComponent<Npc>();

            newProjectile.poolId = prefab.poolId;

            return newProjectile;
        }

        void OnReturnedToPool(Npc npc)
        {
            npc.Cleanup();
        }

        void OnTakeFromPool(Npc npc)
        {
            
        }

        void OnDestroyPoolObject(Npc npc)
        {
            Destroy(npc.gameObject);
        }
        
        GameObject CreatePooledItem(GameObject prefab)
        {
            var go = Instantiate(prefab.gameObject);

            return go;
        }

        void OnReturnedToPool(GameObject go)
        {
            go.SetActive(false);
        }

        void OnTakeFromPool(GameObject go)
        {
            
        }

        void OnDestroyPoolObject(GameObject go)
        {
            Destroy(go);
        }
    }
}