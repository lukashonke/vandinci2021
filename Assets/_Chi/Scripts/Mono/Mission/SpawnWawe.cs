using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission.Events;
using _Chi.Scripts.Utilities;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Mission
{
    public class SpawnWawe : SerializedMonoBehaviour, IMissionHandler
    {
        [Button]
        public void Convert(float waveDurationSeconds)
        {
            if(waveDurationSeconds == 0)
            {
                Debug.LogError("fixedDuration is 0");
                return;
            }
            
            foreach (var spawn in spawns)
            {
                // round to 2 decimals
                spawn.spawnTimeMin = spawn.spawnTimeMin / waveDurationSeconds;
                spawn.spawnTimeMax = spawn.spawnTimeMax / waveDurationSeconds;
                if (spawn.repeatSpawn)
                {
                    spawn.repeatSpawnIntervalMin = spawn.repeatSpawnIntervalMin / waveDurationSeconds;
                    spawn.repeatSpawnIntervalMax = spawn.repeatSpawnIntervalMax / waveDurationSeconds;
                }
            }
        }
        
        [HorizontalGroup("Main")]
        public string waveName;
        [HorizontalGroup("Main")]
        public bool disable;
        
        public List<SpawnWaweData> spawns;

        [NonSerialized] private float nextCurvesUpdate = 0;
        [NonSerialized] public MissionEvent ev;
        
        [NonSerialized] public float fixedDuration;
        [NonSerialized] public float startAtTime;
        public float RelativeTime => (Time.time - startAtTime) / (fixedDuration);

        private bool running;

        public void OnStart(MissionEvent ev, float fixedDuration)
        {
            this.fixedDuration = fixedDuration;
            this.startAtTime = Time.time;
            
            this.ev = ev;
            Debug.Log("start wawe");

            foreach (var settings in spawns)
            {
                settings.Initialise();
                settings.nextSpawnTime = Random.Range(settings.spawnTimeMin, settings.spawnTimeMax);
            }

            running = true;

            StartCoroutine(UpdateRoutine());
        }
        
        private IEnumerator UpdateRoutine()
        {
            var waiter = new WaitForSeconds(0.2f);

            while (running)
            {
                yield return waiter;

                if (disable)
                {
                    continue;
                }

                var player = Gamesystem.instance.objects.currentPlayer;
                var playerPosition = player.GetPosition();
                var time = RelativeTime;

                foreach (var settings in spawns)
                {
                    if (!settings.finished && settings.nextSpawnTime < time)
                    {
                        Spawn(settings, playerPosition, time, player, ev);
                    }
                }
            }
        }

        public void OnStop()
        {
            running = false;
        }

        public bool IsFinished()
        {
            return spawns.All(s => s.finished);
        }

        public static void Spawn(SpawnWaweData settings, Vector3 playerPosition, float relativeTime, Entity relativeTo, MissionEvent ev)
        {
            settings.lastSpawnTime = relativeTime;
            settings.nextSpawnTime = 0;

            //SPAWN
            var spawnCount = settings.GetCountToSpawn(relativeTime);

            //int squareSize = (int) Math.Ceiling(Math.Sqrt(spawnCount));
            Vector3 spawnPosition = Vector3.zero;

            if (settings.spawnAtFixedPositions)
            {
                // set up later
                spawnPosition = Vector3.zero;
            }
            else if (settings.spawnOutsideScreen)
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
                        var dir2 = relativeTo.GetForwardVector().normalized * distance;
                        spawnPosition = playerPosition + dir2;
                        break;
                    case SpawnRelativePosition.BehindPlayer:
                        var dir3 = -relativeTo.GetForwardVector().normalized * distance;
                        spawnPosition = playerPosition + dir3;
                        break;
                }    
            }

            Vector3? goToDirection = null;
            if (settings.behavior == SpawnBehavior.RoamRandomly)
            {
                goToDirection = (Vector3) Random.insideUnitCircle.normalized * settings.roamDistance;
                goToDirection += (Vector3) Random.insideUnitCircle * settings.roamRandomRadius;
            }
            else if(settings.behavior == SpawnBehavior.RoamTowardsPlayer)
            {
                goToDirection = (playerPosition - spawnPosition).normalized * settings.roamDistance;
                goToDirection += (Vector3) Random.insideUnitCircle * settings.roamRandomRadius;
            }

            int rows = 1;
            float theta = 0;

            switch (settings.formation)
            {
                case SpawnFormation.Grid:
                case SpawnFormation.ShightlyShiftedGrid:
                    rows = FormationsUtils.GetGridRows((int) spawnCount);
                    break;
                case SpawnFormation.Horde:
                    rows = FormationsUtils.GetHordeRows((int) spawnCount);
                    break;
                case SpawnFormation.Arc:
                    theta = FormationsUtils.GetArcTheta((int) spawnCount);
                    break;
                case SpawnFormation.Circle:
                    theta = FormationsUtils.GetCircleTheta((int) spawnCount);
                    break;
                case SpawnFormation.Line:
                case SpawnFormation.RandomAroundPlayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int attempts;

            List<Vector3> fixedRandomPositionsCopy = null;
            if (settings.spawnAtFixedPositions)
            {
                fixedRandomPositionsCopy = settings.fixedSpawnPositions.positions.ToList();
                fixedRandomPositionsCopy.Shuffle();
            }
            
            for (int i = 0; i < spawnCount; i++)
            {
                attempts = 100;

                Vector3 targetPosition = playerPosition;
                while (--attempts > 0)
                {
                    var spread = Random.Range(settings.spawnGroupSpreadMin, settings.spawnGroupSpreadMax);

                    switch (settings.formation)
                    {
                        case SpawnFormation.Grid:
                            targetPosition = FormationsUtils.GetGridTargetPosition(spawnPosition, Quaternion.identity, i, settings.formationLookAhead, rows, new Vector2(spread, spread));
                            break;
                        case SpawnFormation.RandomAroundPlayer:
                            targetPosition = Utils.GetRandomPositionAround(playerPosition, settings.distanceFromPlayerMin, settings.distanceFromPlayerMax);
                            break;
                        case SpawnFormation.ShightlyShiftedGrid:
                            targetPosition = FormationsUtils.GetGridTargetPosition(spawnPosition, Quaternion.identity, i, settings.formationLookAhead, rows, new Vector2(spread, spread), 0.2f);
                            break;
                        case SpawnFormation.Horde:
                            targetPosition = FormationsUtils.GetHordeTargetPosition(spawnPosition, Quaternion.identity, i, settings.formationLookAhead, rows, new Vector2(spread, spread), Random.Range(0, 1f), 0.1f);
                            break;
                        case SpawnFormation.RandomPack:
                            targetPosition = FormationsUtils.GetHordeTargetPosition(spawnPosition, Quaternion.identity, i, settings.formationLookAhead, rows, new Vector2(spread, spread));
                            break;
                        case SpawnFormation.Arc:
                            targetPosition = FormationsUtils.GetArcPosition(spawnPosition, Quaternion.identity, i, theta, spread, settings.formationLookAhead, true);
                            break;
                        case SpawnFormation.Circle:
                            targetPosition = FormationsUtils.GetCirclePosition(spawnPosition, Quaternion.identity, i, theta, spread, settings.formationLookAhead);
                            break;
                        case SpawnFormation.Line:
                            targetPosition = FormationsUtils.GetLinePosition(spawnPosition, Quaternion.identity, i, spread, settings.formationLookAhead, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (!settings.onlyOnPlayerAccessibleArea || Gamesystem.instance.CanBeAccessed(targetPosition))
                    {
                        break;
                    }
                }
                
                if(attempts == 0)
                {
                    Debug.LogError("Could not find a valid spawn position");
                }

                var spawnPrefab = settings.GetRandomPrefab();
                
                var dist = settings.despawnWhenOutsideScreen ? settings.despawnWhenOutsideScreenDist2 : settings.distanceFromPlayerToDespawn;
                var condition = settings.despawnWhenOutsideScreen ? DespawnCondition.DistanceFromScreenBorder : DespawnCondition.DistanceFromPlayer;

                if (settings.spawnAtFixedPositions)
                {
                    targetPosition = fixedRandomPositionsCopy[i % fixedRandomPositionsCopy.Count];
                }

                var spawned = spawnPrefab.SpawnOnPosition(targetPosition, playerPosition, dist, settings.despawnAfter, condition);
                if (spawned != null)
                {
                    if (settings.trackEntityForMission)
                    {
                        ev?.TrackAliveEntity(spawned);
                        Gamesystem.instance.missionManager.TrackAliveEntity(spawned);
                    }

                    if (spawned is Npc npc)
                    {
                        if (settings.behavior == SpawnBehavior.StandIdle)
                        {
                            npc.SetCanMove(false);
                        }
                        else if (goToDirection.HasValue)
                        {
                            npc.SetFixedMoveTarget(targetPosition + goToDirection.Value, settings.stopWhenReachFixedMoveTarget, settings.dieWhenReachFixedMoveTarget);
                        }
                    }
                }
            }

            if (settings.repeatSpawn)
            {
                if (settings.repeatedCount < settings.repeatCount)
                {
                    settings.nextSpawnTime = settings.lastSpawnTime + Random.Range(settings.repeatSpawnIntervalMin, settings.repeatSpawnIntervalMax);
                    settings.repeatedCount++;
                }
                else
                {
                    settings.finished = true;
                }
            }
            else
            {
                settings.finished = true;
            }

            settings.firstSpawnDone = true;
        }
    }

    [Serializable]
    public class SpawnWaweData
    {
        public List<SpawnPrefab> possiblePrefabs;

        public SpawnFormation formation;

        public float formationLookAhead = 2f;

        public float formationRadius = 5f;
        
        public SpawnBehavior behavior;

        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public float roamDistance;
        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public float roamRandomRadius;

        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public bool dieWhenReachFixedMoveTarget;
        [HideIf("behavior", SpawnBehavior.AttackPlayer)]
        public bool stopWhenReachFixedMoveTarget;

        public float spawnTimeMin;
        public float spawnTimeMax;

        public bool repeatSpawn;
        
        [ShowIf("repeatSpawn")]
        public float repeatSpawnIntervalMin;
        [ShowIf("repeatSpawn")]
        public float repeatSpawnIntervalMax;
        [ShowIf("repeatSpawn")]
        public int repeatCount;

        public int spawnCountMin;
        public int spawnCountMax;

        [HideIf("spawnOutsideScreen")] public float distanceFromPlayerMin;
        [HideIf("spawnOutsideScreen")] public float distanceFromPlayerMax;
        
        public float despawnAfter;

        public float spawnGroupSpreadMin = 1;
        public float spawnGroupSpreadMax = 1;
        
        public bool spawnOutsideScreen = false;
        public bool spawnAtFixedPositions = false;
        
        [FormerlySerializedAs("spawnPositions")] [ShowIf("spawnAtFixedPositions")]
        public SpawnPositions fixedSpawnPositions;
        
        public bool despawnWhenOutsideScreen = true;
        
        [HideIf("spawnOutsideScreen")]
        public SpawnRelativePosition relativePosition;
        
        [ShowIf("despawnWhenOutsideScreen")]
        public float despawnWhenOutsideScreenDist2 = 2;

        [ShowIf("spawnOutsideScreen")] 
        public float spawnBoxAroundPlayerWidth = 1.5f;
        
        [HideIf("despawnWhenOutsideScreen")]
        public float distanceFromPlayerToDespawn = 100;

        public bool trackEntityForMission = false;
        
        public bool onlyOnPlayerAccessibleArea;
        
        // runtime
        [NonSerialized] public float nextSpawnTime;
        [NonSerialized] public float lastSpawnTime;
        [NonSerialized] public int repeatedCount = 0;
        [NonSerialized] public bool finished;
        [NonSerialized] public bool firstSpawnDone;
        [NonSerialized] private Dictionary<int, SpawnPrefab> prefabsByWeightValues;

        public void Initialise()
        {
            firstSpawnDone = false;
            finished = false;

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
        
        public Vector3 GetSpawnPosition()
        {
            if (spawnOutsideScreen)
            {
                var position = new Vector3();
                
                float f = Random.value > 0.5f ? 1 : -1;
                if (Random.value > 0.5f)
                {
                    position.x = f * Random.Range(Gamesystem.instance.HorizontalToBorderDistance, Gamesystem.instance.HorizontalToBorderDistance + spawnBoxAroundPlayerWidth);
                    position.y = Random.Range(-Gamesystem.instance.VerticalToBorderDistance, Gamesystem.instance.VerticalToBorderDistance);
                }
                else
                {
                    position.y = f * Random.Range(Gamesystem.instance.VerticalToBorderDistance, Gamesystem.instance.VerticalToBorderDistance + spawnBoxAroundPlayerWidth);
                    position.x = Random.Range(-Gamesystem.instance.HorizontalToBorderDistance, Gamesystem.instance.HorizontalToBorderDistance);
                }

                position += Gamesystem.instance.objects.currentPlayer.GetPosition();

                return position;
            }
            
            return Vector3.zero;
        }
    }
}