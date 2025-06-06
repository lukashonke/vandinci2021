﻿using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using KNN;
using KNN.Jobs;
using QFSW.QC;
using Unity.Jobs;

namespace _Chi.Scripts.Mono.System
{
    public class DropManager : MonoBehaviour
    {
        public GameObject level1GoldPrefab;
        public int level1GoldAmount = 1;
        
        public GameObject level15GoldPrefab;
        public int level15GoldAmount = 5;
        
        public GameObject level2GoldPrefab;
        public int level2GoldAmount = 10;
        
        public GameObject level3GoldPrefab;
        public int level3GoldAmount = 100;
        
        public GameObject level1ExpPrefab;
        public int level1ExpAmount = 1;
        
        public GameObject level15ExpPrefab;
        public int level15ExpAmount = 5;
        
        public GameObject level2ExpPrefab;
        public int level2ExpAmount = 10;
        
        public GameObject level3ExpPrefab;
        public int level3ExpAmount = 100;

        public float pickupMoveSpeed = 2f;

        [NonSerialized] private int lastId = 0;
        [NonSerialized] private List<GameObject> beingPickedUp;
        
        [NonSerialized] private List<float3> pointsList;
        [NonSerialized] private List<GameObject> gameObjects;
        
        // BUG 2 - smaze se nejaka entita a pak blbne

        [NonSerialized] KnnContainer knnContainer;
        [NonSerialized] NativeArray<float3> points;
        
        private JobHandle rebuildHandle;
        private JobHandle queryJobHandle;
        
        private bool isQueryJobRunning = false;
        private NativeArray<int> queryResults;

        public float globalDropChance = 100f;

        private bool pickupCoroutineRunning = false;

        void Awake()
        {
            beingPickedUp = new();
            pointsList = new();
            gameObjects = new();
            
            points = new NativeArray<float3>(2048*4, Allocator.Persistent);
            knnContainer = new KnnContainer(points, false, Allocator.Persistent);
        }

        public void Start()
        {
        }

        public void OnDestroy()
        {
            points.Dispose();
            knnContainer.Dispose();
            queryResults.Dispose();
        }

        public void Update()
        {
            for (var index = 0; index < pointsList.Count; index++)
            {
                var point = pointsList[index];

                points[index] = point;
            }

            for (int index = pointsList.Count; index < points.Length; index++)
            {
                points[index] = new float3(1000000, 1000000, 1000000);
            }
            
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPos = player.GetPosition();
            var playerPosFloat3 = new float3(playerPos.x, playerPos.y, 0);
            const int neighbours = 1;
            
            if (isQueryJobRunning)
            {
                rebuildHandle.Complete();
                queryJobHandle.Complete();
                
                for (int i = 0; i < neighbours; i++)
                {
                    var index = queryResults[i];
                    var pos = points[index];
                
                    var dist = Utils.Dist2(playerPos, new Vector3(pos.x, pos.y, 0));
                    if (dist < player.stats.pickupAttractRange.GetValue())
                    {
                        var go = gameObjects[index];
                        beingPickedUp.Add(go);
                        RemoveDrop(index);
                    }
                }

                queryResults.Dispose();
            }
            
            var rebuild = new KnnRebuildJob(knnContainer);
            rebuildHandle = rebuild.Schedule();
            
            queryResults = new NativeArray<int>(neighbours, Allocator.TempJob);
            
            var queryJob = new QueryKNearestJob(knnContainer, playerPosFloat3, queryResults);
            queryJobHandle = queryJob.Schedule(rebuildHandle);
            isQueryJobRunning = true;

            if (Input.GetMouseButton(1))
            {
                Drop(DropType.Level1Exp, Utils.GetMousePosition(), true);
            }
        }
        
        public void LateUpdate()
        {
            /*const int neighbours = 1;

            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPos = player.GetPosition();
            var playerPosFloat3 = new float3(playerPos.x, playerPos.y, 0);*/

            /*if (isQueryJobRunning)
            {
                rebuildHandle.Complete();
                queryJobHandle.Complete();
                
                for (int i = 0; i < neighbours; i++)
                {
                    var index = queryResults[i];
                    var pos = points[index];
                
                    var dist = Utils.Dist2(playerPos, new Vector3(pos.x, pos.y, 0));
                    if (dist < player.stats.pickupAttractRange.GetValue())
                    {
                        var go = gameObjects[index];
                        beingPickedUp.Add(go);
                        RemoveDrop(index);
                    }
                }

                queryResults.Dispose();
            }*/
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

        public void Pickup(float maxRange)
        {
            StartCoroutine(PickupCoroutine(maxRange));
        }

        private IEnumerator PickupCoroutine(float maxRange)
        {
            float maxRange2 = maxRange * maxRange;
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPos = player.GetPosition();

            const int maxPicksUp = 300;

            for (var index = gameObjects.Count - 1; index >= 0; index--)
            {
                while (beingPickedUp.Count >= maxPicksUp)
                {
                    yield return null;
                }
                
                var go = gameObjects[index];
                var dist = Utils.Dist2(playerPos, go.transform.position);

                if (dist < maxRange2)
                {
                    beingPickedUp.Add(go);
                    RemoveDrop(index);
                }
            }
        }

        private bool CheckDropChance()
        {
            return UnityEngine.Random.Range(0f, 100f) <= globalDropChance;
        }
        
        public void Drop(DropType drop, Vector3 position, bool bypassDropChance = false)
        {
            if (!bypassDropChance && !CheckDropChance()) return;
            
            GameObject go;
            GameObject prefab;
            switch (drop)
            {
                case DropType.Level1Gold:
                    prefab = level1GoldPrefab;
                    break;
                case DropType.Level15Gold:
                    prefab = level15GoldPrefab;
                    break;
                case DropType.Level2Gold:
                    prefab = level2GoldPrefab;
                    break;
                case DropType.Level3Gold:
                    prefab = level3GoldPrefab;
                    break;
                case DropType.Level1Exp:
                    prefab = level1ExpPrefab;
                    break;
                case DropType.Level2Exp:
                    prefab = level2ExpPrefab;
                    break;
                case DropType.Level15Exp:
                    prefab = level15ExpPrefab;
                    break;
                case DropType.Level3Exp:
                    prefab = level3ExpPrefab;
                    break;                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(drop), drop, null);
            }
            
            go = Gamesystem.instance.poolSystem.Spawn(drop, prefab, 1000);
            
            go.transform.position = position;
            
            go.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360)); 
            
            go.name = drop.ToString();

            AddDrop(position, go);
        }

        private void AddDrop(Vector3 position, GameObject go)
        {
            pointsList.Add(position);
            gameObjects.Add(go);
        }
        
        private void RemoveDrop(int index)
        {
            pointsList.RemoveAt(index);
            gameObjects.RemoveAt(index);
        }

        public void Pickup(GameObject go)
        {
            var type = GetDropType(go);

            var amount = 0;

            switch (type)
            {
                case DropType.Level1Gold:
                    amount = level1GoldAmount;
                    break;
                case DropType.Level15Gold:
                    amount = level15GoldAmount;
                    break;
                case DropType.Level2Gold:
                    amount = level2GoldAmount;
                    break;
                case DropType.Level3Gold:
                    amount = level3GoldAmount;
                    break;
                case DropType.Level1Exp:
                    amount = level1ExpAmount;
                    break;
                case DropType.Level2Exp:
                    amount = level2ExpAmount;
                    break;
                case DropType.Level15Exp:
                    amount = level15ExpAmount;
                    break;
                case DropType.Level3Exp:
                    amount = level3ExpAmount;
                    break;   
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (type == DropType.Level1Gold || type == DropType.Level15Gold || type == DropType.Level2Gold ||
                type == DropType.Level3Gold)
            {
                Gamesystem.instance.progress.AddGold(amount);
                Gamesystem.instance.objects.currentPlayer.OnPickupGold(amount);
            }
            else if (type == DropType.Level1Exp || type == DropType.Level15Exp || type == DropType.Level2Exp ||
                     type == DropType.Level3Exp)
            {
                Gamesystem.instance.progress.AddExp(amount);
                Gamesystem.instance.objects.currentPlayer.OnPickupExp(amount);
            }
            
            Gamesystem.instance.poolSystem.Despawn(type, go);
        }

        private DropType GetDropType(GameObject go)
        {
            return Enum.Parse<DropType>(go.name);
        }
    }
}