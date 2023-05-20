using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission.Events;
using _Chi.Scripts.Scriptables;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Mission
{
    public class SpawnAroundPlayer : SerializedMonoBehaviour, IMissionHandler
    {
        [HorizontalGroup("Main")]
        public string attacksName;
        [HorizontalGroup("Main")]
        public bool disable;
        
        public List<Spawn> spawns;
        
        [NonSerialized] private float nextCurvesUpdate = 0;
        [NonSerialized] public MissionEvent ev;
        
        [NonSerialized] public float fixedDuration;
        [NonSerialized] public float startAtTime;
        public float RelativeTime => (Time.time - startAtTime) / (fixedDuration);

        public void OnStart(MissionEvent ev, float fixedDuration)
        {
            this.fixedDuration = fixedDuration;
            this.startAtTime = Time.time;
            this.ev = ev;
            
            Debug.Log("start");
            
            foreach (var settings in spawns)
            {
                settings.Initialise();
                
                if (settings.baseCountPerMinute > 0)
                {
                    // fixed duration = 120
                    // 60 / 120 = 0.5

                    // fixed duration = 60
                    // 60 / 60 = 1
                    
                    // fixed duration = 30
                    // 60 / 30 = 2

                    settings.nextSpawnTime = (60 / fixedDuration / settings.baseCountPerMinute);
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
            if (disable)
            {
                return;
            }
            
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPosition = player.GetPosition();
            var relativeTime = RelativeTime;
            
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
                    settings.baseCountPerMinute = settings.countPerMinuteCurve.Evaluate(relativeTime);

                    settings.nextSpawnTime = settings.lastSpawnTime + (60 / fixedDuration / settings.GetCountPerMinute(relativeTime));
                }
                
                if (settings.nextSpawnTime > 0 && settings.nextSpawnTime < relativeTime)
                {
                    settings.lastSpawnTime = relativeTime;
                    settings.nextSpawnTime = settings.lastSpawnTime + (60 / settings.GetCountPerMinute(relativeTime));

                    Vector3 spawnPosition = Vector3.zero;
                    if (settings.spawnOutsideScreen)
                    {
                        spawnPosition = settings.GetSpawnPosition();
                    }
                    else
                    {
                        var distance = settings.GetDistanceFromPlayer(relativeTime);
                    
                        var relativePos = settings.relativePosition;
                        if (relativePos == SpawnRelativePosition.FrontOrBehindPlayer)
                        {
                            relativePos = Random.Range(0, 2) == 0 ? SpawnRelativePosition.FrontOfPlayer : SpawnRelativePosition.BehindPlayer;
                        }

                        switch (relativePos)
                        {
                            case SpawnRelativePosition.AroundPlayer:
                                var dir1 = (Vector3) Random.insideUnitCircle.normalized * distance;
                                spawnPosition = playerPosition + dir1;
                                break;
                            case SpawnRelativePosition.AroundMapCenter:
                                var dir4 = (Vector3) Random.insideUnitCircle.normalized * distance;
                                spawnPosition = Gamesystem.instance.missionManager.currentMission.center + dir4;
                                break;
                            case SpawnRelativePosition.FrontOfPlayer:
                                var dir2 = player.GetForwardVector().normalized * distance;
                                spawnPosition = playerPosition + dir2;
                                break;
                            case SpawnRelativePosition.BehindPlayer:
                                var dir3 = -player.GetForwardVector().normalized * distance;
                                spawnPosition = playerPosition + dir3;
                                break;
                        }
                    }

                    var spawnCount = settings.GetCountToSpawn(relativeTime);

                    if (spawnCount <= 2)
                    {
                        for (int i = 0; i < spawnCount; i++)
                        {
                            var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);
                        
                            var targetPosition = spawnPosition + (new Vector3(i*spread, 0, 0));
                        
                            var spawnPrefab = settings.GetRandomPrefab();

                            var dist = settings.despawnWhenOutsideScreen ? settings.despawnWhenOutsideScreenDist2 : settings.distanceFromPlayerToDespawn;
                            var condition = settings.despawnWhenOutsideScreen ? DespawnCondition.DistanceFromScreenBorder : DespawnCondition.DistanceFromPlayer;
                            
                            var spawned = spawnPrefab.SpawnOnPosition(targetPosition, playerPosition, dist, settings.despawnAfter, condition);

                            if (spawned != null)
                            {
                                if (settings.trackEntityForMission)
                                {
                                    ev.TrackAliveEntity(spawned);
                                    Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
                                }
                            }
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
                                
                                var spawnPrefab = settings.GetRandomPrefab();

                                var dist = settings.despawnWhenOutsideScreen ? settings.despawnWhenOutsideScreenDist2 : settings.distanceFromPlayerToDespawn;
                                var condition = settings.despawnWhenOutsideScreen ? DespawnCondition.DistanceFromScreenBorder : DespawnCondition.DistanceFromPlayer;
                                
                                var spawned = spawnPrefab.SpawnOnPosition(targetPosition, playerPosition, dist, settings.despawnAfter, condition);
                                
                                if (spawned != null)
                                {
                                    if (settings.trackEntityForMission)
                                    {
                                        ev.TrackAliveEntity(spawned);
                                        Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
                                    }
                                }
                                
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
        public float countPerMinuteRandomAddFrom = 0;
        public float countPerMinuteRandomAddTo = 0;
        public float countPerMinuteRandomMulFrom = 1;
        public float countPerMinuteRandomMulTo = 1;

        public AnimationCurve countPerMinuteRandomAddCurve = AnimationCurve.Constant(0, 1, 0);
        public AnimationCurve countPerMinuteRandomMulCurve = AnimationCurve.Constant(0, 1, 1);

        public AnimationCurve minSpawnCount = AnimationCurve.Constant(0, 1, 1);
        public AnimationCurve maxSpawnCount = AnimationCurve.Constant(0, 1, 1);


        public bool spawnOutsideScreen = true;
        public bool despawnWhenOutsideScreen = true;
        
        [ShowIf("despawnWhenOutsideScreen")]
        public float despawnWhenOutsideScreenDist2 = 2;
        
        [HideIf("spawnOutsideScreen")]
        public SpawnRelativePosition relativePosition;

        [ShowIf("spawnOutsideScreen")] 
        public float spawnBoxAroundPlayerWidth = 1.5f;
        
        [HideIf("despawnWhenOutsideScreen")]
        public float distanceFromPlayerToDespawn = 100;
        
        [HideIf("spawnOutsideScreen")]
        public AnimationCurve distanceFromPlayerCurve = AnimationCurve.Constant(0, 1, 12);
        
        public float spawnGroupSpreadMin = 1;
        public float spawnGroupSpreadMax = 1;
        
        
        public float despawnAfter = 0;
        
        public bool trackEntityForMission = false;
        
        // runtime
        [ReadOnly] public float nextSpawnTime;
        [ReadOnly] public float lastSpawnTime = 0;
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise()
        {
            prefabsByWeightValues = possiblePrefabs.ToWeights();
        }
        
        public void Recalculate(float time)
        {
            nextSpawnTime = lastSpawnTime + (60 / GetCountPerMinute(time));
            //Debug.Log(GetCountPerMinute(time));
        }
        
        public float GetCountPerMinute(float time)
        {
            //if(randomizeCount) return countPerMinute * CalcRandomMul(time) + (time) * randomizeIncreasePerSecond;

            return (baseCountPerMinute
                   * countPerMinuteRandomMulCurve.Evaluate(time)
                   * Random.Range(countPerMinuteRandomMulFrom, countPerMinuteRandomMulTo)
                   + Random.Range(countPerMinuteRandomAddFrom, countPerMinuteRandomAddTo)
                   + countPerMinuteRandomAddCurve.Evaluate(time)) * Gamesystem.instance.monsterCountMul;
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

        public Vector3 GetSpawnPosition()
        {
            if (spawnOutsideScreen)
            {
                var position = new Vector3();

                var playerMoveDirection = Gamesystem.instance.GetPlayerMoveDirection();

                const float boost = 0.3f;
                const float baseChance = 0.5f;
                float selectHorizontalChance = baseChance;
                
                if (playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Up) || playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Down))
                {
                    selectHorizontalChance -= boost;
                }
                if (playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Left) || playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Right))
                {
                    selectHorizontalChance += boost;
                }
                
                if (selectHorizontalChance > Random.value) // spawn on the left or right side
                {
                    float spawnRightChance = baseChance;
                    
                    if (playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Right))
                    {
                        spawnRightChance += boost;
                    }
                    if (playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Left))
                    {
                        spawnRightChance -= boost;
                    }
                    
                    float f = spawnRightChance > Random.value ? 1 : -1;
                    
                    position.x = f * Random.Range(Gamesystem.instance.HorizontalToBorderDistance, Gamesystem.instance.HorizontalToBorderDistance + spawnBoxAroundPlayerWidth);
                    position.y = Random.Range(-Gamesystem.instance.VerticalToBorderDistance, Gamesystem.instance.VerticalToBorderDistance);
                }
                else // spawn on the top or bottom side
                {
                    float spawnUpChance = baseChance;
                    
                    if (playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Up))
                    {
                        spawnUpChance += boost;
                    }
                    if (playerMoveDirection.HasFlag(Gamesystem.PlayerMoveDirection.Down))
                    {
                        spawnUpChance -= boost;
                    }
                    
                    float f = spawnUpChance > Random.value ? 1 : -1;
                    position.y = f * Random.Range(Gamesystem.instance.VerticalToBorderDistance, Gamesystem.instance.VerticalToBorderDistance + spawnBoxAroundPlayerWidth);
                    position.x = Random.Range(-Gamesystem.instance.HorizontalToBorderDistance, Gamesystem.instance.HorizontalToBorderDistance);
                }

                position += Gamesystem.instance.objects.currentPlayer.GetPosition();

                return position;
            }
            
            return Vector3.zero;
        }

        public int GetCountToSpawn(float time)
        {
            return (int)Math.Round(Random.Range(minSpawnCount.Evaluate(time), maxSpawnCount.Evaluate(time)));
        }
    }

    [Serializable]
    public class SpawnPrefab
    {
        public SpawnPrefabType type;
        
        //[ShowIf("type", SpawnPrefabType.Gameobject)]
        [EnableIf("@this.type == SpawnPrefabType.Gameobject || this.type == SpawnPrefabType.PoolableGo")]
        public GameObject prefab;
        
        [EnableIf("@this.type == SpawnPrefabType.PooledNpc || this.type == SpawnPrefabType.NonPooledNpc")]
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
        PoolableGo,
        Gameobject
    }

    public enum SpawnRelativePosition
    {
        AroundPlayer,
        FrontOfPlayer,
        BehindPlayer,
        FrontOrBehindPlayer,
        AroundMapCenter
    }

    public enum SpawnFormation
    {
        Grid,
        Circle,
        Arc,
        Line,
        Horde,
        ShightlyShiftedGrid,
        RandomPack,
        RandomAroundPlayer,
    }

    public enum SpawnBehavior
    {
        AttackPlayer,
        RoamRandomly,
        RoamTowardsPlayer,
        StandIdle,
    }
}