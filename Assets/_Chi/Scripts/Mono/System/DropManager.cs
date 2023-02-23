using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Utilities;
using ProjectDawn.Collections;
using ProjectDawn.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using BurstGridSearch;
using KNN;
using KNN.Jobs;
using Unity.Jobs;

namespace _Chi.Scripts.Mono.System
{
    public class DropManager : MonoBehaviour
    {
        public GameObject level1GoldPrefab;
        public int level1GoldAmount = 1;
        
        public GameObject level2GoldPrefab;
        public int level2GoldAmount = 1;
        
        public GameObject level3GoldPrefab;
        public int level3GoldAmount = 1;

        public float pickupMoveSpeed = 2f;

        [NonSerialized] private int lastId = 0;
        
        // BUG 1 - drops se nepickupují nekdy
        // BUG 2 - smaze se nejaka entita a pak blbne

        [NonSerialized] private Dictionary<int, GameObject> drops;
        [NonSerialized] private UnsafeKdTree<float3, TreeComparer> tree;

        [NonSerialized] private List<float3> positionsList;
        [NonSerialized] private NativeArray<float3> positions;
        [NonSerialized] private List<GameObject> gameObjects;

        [NonSerialized] private List<GameObject> beingPickedUp;
        [NonSerialized] GridSearchBurst gsb;
        
        [NonSerialized] KnnContainer knnContainer;

        void Awake()
        {
            tree = new UnsafeKdTree<float3, TreeComparer>(1, Allocator.Persistent, new TreeComparer());

            drops = new();
            beingPickedUp = new();
            
            positionsList = new();
            gameObjects = new();
            
            knnContainer = new KnnContainer(positions, false, Allocator.TempJob);
        }

        public void OnDestroy()
        {
            tree.Dispose();
        }

        public void FixedUpdate()
        {
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPos = player.GetPosition();
            
            for (var index = beingPickedUp.Count - 1; index >= 0; index--)
            {
                var go = beingPickedUp[index];

                var goPosition = go.transform.position;
                goPosition = (playerPos - goPosition).normalized * (pickupMoveSpeed * Time.fixedDeltaTime) + goPosition;
                
                go.transform.position = goPosition;
                
                if (Utils.Dist2(goPosition, playerPos) < player.stats.pickupRange.GetValue())
                {
                    beingPickedUp.RemoveAt(index);
                    Pickup(go);
                }
            }
        }

        public void Update()
        {
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPos = player.GetPosition();
            
            var rebuildJob = new KnnRebuildJob(knnContainer);
            rebuildJob.Schedule().Complete();
            
            gsb.initGrid(positions);
            
            //TODO jobify and burst

            if (!tree.IsEmpty)
            {
                UnsafeKdTree<float3, TreeComparer>.Handle handle = tree.FindNearest(new float3(playerPos.x, playerPos.y, 0), out _);

                if (handle.Valid)
                {
                    var foundPos = tree[handle];
                    var dist = Utils.Dist2(playerPos, new Vector3(foundPos.x, foundPos.y, 0));
                    
                    Debug.DrawLine(player.GetPosition(), new Vector3(foundPos.x, foundPos.y, 0), Color.red);
                    if (dist < player.stats.pickupAttractRange.GetValue())
                    {
                        beingPickedUp.Add(drops[(int) foundPos.z]);
                        drops.Remove((int) foundPos.z);
                        tree.RemoveAt(handle);
                    }
                }
            }

            if (Input.GetMouseButton(0))
            {
                Drop(DropType.Level1Gold, Utils.GetMousePosition());
            }
        }
        
        struct TreeComparer : IKdTreeComparer<float3>
        {
            public int Compare(float3 x, float3 y, int depth)
            {
                int axis = depth % 2;
                return x[axis].CompareTo(y[axis]);
            }

            public float DistanceSq(float3 x, float3 y)
            {
                var x2 = new float2(x.x, x.y);
                var y2 = new float2(y.x, y.y);
                
                return math.distancesq(x2, y2);
            }

            public float DistanceToSplitSq(float3 x, float3 y, int depth)
            {
                int axis = depth % 2;
                return (x[axis] - y[axis]) * (x[axis] - y[axis]);
            }
        }
        
        public void Drop(DropType drop, Vector3 position)
        {
            GameObject go;
            GameObject prefab;
            switch (drop)
            {
                case DropType.Level1Gold:
                    prefab = level1GoldPrefab;
                    break;
                case DropType.Level2Gold:
                    prefab = level2GoldPrefab;
                    break;
                case DropType.Level3Gold:
                    prefab = level3GoldPrefab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(drop), drop, null);
            }
            
            go = Gamesystem.instance.poolSystem.Spawn(drop, prefab, 1000);
            
            go.transform.position = position;
            
            go.name = drop.ToString();

            int id = lastId++;
            drops[id] = go;
            
            //tree.Add(new float3(position.x, position.y, id));
            
            positionsList.Add(position);
            gameObjects.Add(go);
            
            //TODO dat na vic vlaken
            //TOD BG jobs - dokud neni prepocitano, zadny pickupy se nekonaj
            //https://github.com/ArthurBrussee/KNN
            
            positions = new NativeArray<float3>(positionsList.ToArray(), Allocator.Persistent);
        }

        public void Pickup(GameObject go)
        {
            var type = GetDropType(go);

            switch (type)
            {
                case DropType.Level1Gold:
                    Gamesystem.instance.progress.AddGold(level1GoldAmount);
                    break;
                case DropType.Level2Gold:
                    Gamesystem.instance.progress.AddGold(level2GoldAmount);
                    break;
                case DropType.Level3Gold:
                    Gamesystem.instance.progress.AddGold(level3GoldAmount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Gamesystem.instance.poolSystem.Despawn(type, go);
        }

        private DropType GetDropType(GameObject go)
        {
            return Enum.Parse<DropType>(go.name);
        }
    }
}