using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;
using UnityEngine.Pool;

namespace _Chi.Scripts.Mono.System
{
    public class PoolSystem : MonoBehaviour
    {
        [NonSerialized] public Dictionary<int, ObjectPool<Projectile>> projectilePools;
        
        [NonSerialized] public Dictionary<int, ObjectPool<Npc>> npcPools;
        
        [NonSerialized] public Dictionary<GameObject, ObjectPool<GameObject>> goPool;
        
        [NonSerialized] public Dictionary<GameObject, ObjectPool<IPoolable>> poolablePool;
        
        [NonSerialized] public Dictionary<DropType, ObjectPool<GameObject>> dropPool;

        public bool collectionChecks = true;

        public void Awake()
        {
            projectilePools = new Dictionary<int, ObjectPool<Projectile>>();
            npcPools = new Dictionary<int, ObjectPool<Npc>>();
            goPool = new();
            poolablePool = new();
            dropPool = new();
        }
        
        public GameObject Spawn(DropType drop, GameObject prefab, int maxPoolSize = 100)
        {
            if (dropPool.TryGetValue(drop, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var newPool = new ObjectPool<GameObject>(() => CreatePoolableDropItem(prefab), (a) => OnTakeDropFromPool(a),
                    OnReturnedDropToPool, OnDestroyDropPoolObject, collectionChecks, maxPoolSize);
                dropPool.Add(drop, newPool);

                return newPool.Get();
            }
        }
        
        public IPoolable SpawnPoolable(GameObject prefab, int maxPoolSize = 100)
        {
            if (poolablePool.TryGetValue(prefab, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var newPool = new ObjectPool<IPoolable>(() => CreatePoolableItem(prefab), (a) => OnTakeFromPool(a, prefab),
                    OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                poolablePool.Add(prefab, newPool);

                return newPool.Get();
            }
        }

        public GameObject SpawnGo(GameObject prefab, int maxPoolSize = 100)
        {
            if (goPool.TryGetValue(prefab, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var newPool = new ObjectPool<GameObject>(() => CreatePooledItem(prefab), OnTakeFromPool,
                    OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                goPool.Add(prefab, newPool);

                return newPool.Get();
            }
        }

        public Npc Spawn(Npc prefabNpc, int maxPoolSize = 10000)
        {
            if (prefabNpc.poolId == 0)
            {
                Debug.LogError("Cannot spawn NPC with no pool id! Set Pool id on the prefab.");
            }
            
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
        
        public bool Despawn(GameObject prefab, IPoolable instance)
        {
            if (poolablePool.TryGetValue(prefab, out var pool))
            {
                pool.Release(instance);
                return true;
            }

            return false;
        }
        
        public bool DespawnGo(GameObject prefab, GameObject instance)
        {
            if (goPool.TryGetValue(prefab, out var pool))
            {
                pool.Release(instance);
                return true;
            }

            return false;
        }
        
        public bool Despawn(Npc npcInstance)
        {
            if (npcInstance.despawned)
            {
                return false;
            }
            
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
        
        public void Despawn(DropType drop, GameObject instance)
        {
            if (dropPool.TryGetValue(drop, out var pool))
            {
                pool.Release(instance);
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

            var newNpc = go.GetComponent<Npc>();

            //newProjectile.poolId = prefab.poolId;

            return newNpc;
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
        
        IPoolable CreatePoolableItem(GameObject prefab)
        {
            var go = Instantiate(prefab.gameObject);

            return go.GetComponent<IPoolable>();
        }

        void OnReturnedToPool(IPoolable go)
        {
            go.Reset();
        }

        void OnTakeFromPool(IPoolable go, GameObject prefab)
        {
            go.Setup(prefab);
        }

        void OnDestroyPoolObject(IPoolable go)
        {
            go.Destroy();
        }
        
        GameObject CreatePoolableDropItem(GameObject prefab)
        {
            var go = Instantiate(prefab.gameObject);

            return go;
        }

        void OnReturnedDropToPool(GameObject go)
        {
            go.SetActive(false);
        }

        void OnTakeDropFromPool(GameObject go)
        {
            go.SetActive(true);
        }

        void OnDestroyDropPoolObject(GameObject go)
        {
            Destroy(go);
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
            go.SetActive(true);
        }

        void OnDestroyPoolObject(GameObject go)
        {
            Destroy(go);
        }
    }
}