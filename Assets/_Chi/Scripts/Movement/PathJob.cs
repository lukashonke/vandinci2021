using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _Chi.Scripts.Movement
{
    public class PathJob
    {
        private readonly GameobjectHolder holder;

        public PathJob(GameobjectHolder holder)
        {
            this.holder = holder;
        }
        
        public void OnFixedUpdate()
        {
            List<Npc> toMove = holder.npcEntitiesList;
            int movablesCount = toMove.Count;

            var playerPosition = Gamesystem.instance.objects.currentPlayer.GetPosition();

            foreach (var npc in toMove)
            {
                if (npc.fixedMoveTarget.HasValue)
                {
                    var target = npc.TransformMoveDestination(npc.fixedMoveTarget.Value);
                    npc.pathData.SetDestination(target);
                    npc.SetRotationTarget(target);
                }
                else if (npc.goDirectlyToPlayer)
                {
                    var target = npc.TransformMoveDestination(playerPosition);
                    npc.pathData.SetDestination(target);
                    npc.SetRotationTarget(target);
                }
            }
            
            (NativeArray<JobHandle> jobHandles, SimpleMoveToTargetJob[] movementJobs) result = CreateJobs(toMove, movablesCount);
            
            JobHandle.CompleteAll(result.jobHandles);
            
            movablesCount = toMove.Count;

            ProcessJobs(toMove, movablesCount, result.jobHandles, result.movementJobs);
            
            result.jobHandles.Dispose();
        }
        
        /*static readonly ProfilerMarker createJob1 = new ProfilerMarker(ProfilerCategory.Ai, "VanDinci.CreateJob.1");
        static readonly ProfilerMarker createJob2 = new ProfilerMarker(ProfilerCategory.Ai, "VanDinci.CreateJob.2");
        static readonly ProfilerMarker createJob3 = new ProfilerMarker(ProfilerCategory.Ai, "VanDinci.CreateJob.3");*/

        private (NativeArray<JobHandle> jobHandles,  SimpleMoveToTargetJob[] movementJobs) CreateJobs(List<Npc> toMove, int movablesCount)
        {
            SimpleMoveToTargetJob[] movementJobs = new SimpleMoveToTargetJob[movablesCount];
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(movablesCount, Allocator.TempJob);
            
            bool playerExists = Gamesystem.instance.objects.currentPlayer != null;
            Vector3 playerPosition = playerExists ? Gamesystem.instance.objects.currentPlayer.GetPosition() : Vector3.zero;
            
            for (var index = 0; index < movablesCount; index++)
            {
                var npc = toMove[index];
                if ((npc.goDirectlyToPlayer || npc.fixedMoveTarget.HasValue) && npc.CanMove())
                {
                    float3 rotateTarget = float3.zero;

                    var doPath = true; // TODO can root but keep rotation here

                    var rotateTargetVector = npc.GetRotationTarget();

                    bool doRotate = false;
                    if (rotateTargetVector.HasValue)
                    {
                        doRotate = true;
                        rotateTarget = rotateTargetVector.Value;
                    }

                    npc.pathData.job.deltaTime = Time.fixedDeltaTime;
                    npc.pathData.job.destination = npc.pathData.destination;

                    npc.pathData.job.rotate = doRotate;
                    npc.pathData.job.rotateTarget = rotateTarget;
                    npc.pathData.job.rotateSpeed = npc.stats.rotationSpeed;
                    npc.pathData.job.position = npc.GetPosition();
                    npc.pathData.job.currentRotation = npc.transform.rotation;

                    npc.pathData.job.computePlayerPosition = playerExists;
                    npc.pathData.job.playerPosition = playerPosition;

                    npc.pathData.job.movePath = doPath;
                    npc.pathData.job.maxSpeed = doPath ? npc.stats.speed : 0;
                    npc.pathData.job.reachedEndOfPath = false; //doPath ? npc.pathData.reachedEndOfPath : false;

                    try
                    {
                        npc.pathData.job.rvoCalculatedSpeed = npc.pathData.job.hasRvo ? npc.rvoController.rvoAgent.CalculatedSpeed : 0;
                        npc.pathData.job.rvoCalculatedTargetPoint = npc.pathData.job.hasRvo
                            ? (float3) npc.rvoController.rvoAgent.CalculatedTargetPoint
                            : float3.zero;
                    }
                    catch (Exception)
                    {
                        npc.pathData.job.rvoCalculatedSpeed = 0;
                        npc.pathData.job.rvoCalculatedTargetPoint = float3.zero;
                    }
                    
                    movementJobs[index] = npc.pathData.job;
                    jobHandles[index] = npc.pathData.job.Schedule();
                }
            }

            return (jobHandles, movementJobs);
        }

        private void ProcessJobs(List<Npc> toMove, int movablesCount, NativeArray<JobHandle> jobHandles, SimpleMoveToTargetJob[] movementJobs)
        {
            var player = Gamesystem.instance.objects.currentPlayer;
            var playerPosition = player.GetPosition();
            
            for (var index = movablesCount - 1; index >= 0; index--)
            {
                var npc = toMove[index];
                if (!npc.pathData.jobDisposed && (npc.goDirectlyToPlayer || npc.fixedMoveTarget.HasValue) && npc.CanMove())
                {
                    var data = movementJobs[index];

                    if (data.movePath)
                    {
                        bool doMove = npc.CanGoDirectlyToPlayer();
                        
                        npc.pathData.reachedEndOfPath = data.reachedEndOfPath;

                        //bool stop = false;
                        
                        //character.pathData.rvoDensityBehavior.Update(character.rvoController.enabled, character.pathData.ReachedDestination(), ref stop, ref character.rvoController.priorityMultiplier, ref character.rvoController.flowFollowingStrength, character.GetPosition());
                        
                        //if (!stop) // TODO solve better, propably
                        if(doMove)
                        {
                            npc.MoveTo((Vector3)data.outputPositions[0]);
                            npc.SetMoving(npc.stats.speed);
                        }
                        else
                        {
                            npc.SetMoving(0);
                        }

                        if (data.computePlayerPosition)
                        {
                            npc.SetDistanceToPlayer(data.outputPositions[3].x, player);
                        }
                        
                        /*if (data.outputs[3] && character.CanRotate())
                        {
                            character.SetRotationTarget(data.outputPositions[1]);
                        }*/
                        
                        /*if (data.outputs[1])
                        {
                            //character.OnTargetReached();
                        }*/
    
                        if (data.outputs[2])
                        {
                            npc.pathData.reachedEndOfPath = true;
                            
                            if (npc.fixedMoveTarget.HasValue)
                            {
                                npc.SetFixedMoveTarget(null);
                            }
                        }
    
                        if (data.outputs[4])
                        {
                            if (npc.hasRvoController)
                            {
                                if (doMove)
                                {
                                    npc.rvoController.SetTarget(data.outputPositions[2], npc.stats.speed, npc.stats.speed * 1.2f, npc.pathData.destination);
                                }
                                else
                                {
                                    npc.rvoController.SetTarget(data.outputPositions[2], npc.stats.speed, npc.stats.speed * 1.2f, playerPosition);
                                }
                            }
                        }
                        
                        //character.SetBlockedInCrowd(data.outputs[5]);
                    }

                    if (data.rotate)
                    {
                        //character.rb.MoveRotation(data.outputRotation[0]);
                        //character.rb.transform.rotation = data.outputRotation[0];
                        
                        npc.SetRotation(data.outputRotation[0]);
                    }
                }
            }
        }
    }

    [BurstCompile]
    public struct SimpleMoveToTargetJob : IJob
    {
        [ReadOnly] public float deltaTime;
        
        [ReadOnly] public bool movePath;
        
        [ReadOnly] public float3 position;
        [ReadOnly] public float maxSpeed;
        
        [ReadOnly] public float3 destination;
        
        [ReadOnly] public bool rotate;
        [ReadOnly] public quaternion currentRotation;
        [ReadOnly] public float3 rotateTarget;
        [ReadOnly] public int rotateSpeed;
        
        [ReadOnly] public bool reachedEndOfPath;
        
        [ReadOnly] public bool hasRvo;
        [ReadOnly] public float3 rvoCalculatedTargetPoint;
        [ReadOnly] public float rvoCalculatedSpeed;

        [ReadOnly] public bool computePlayerPosition;
        [ReadOnly] public float3 playerPosition;

        [WriteOnly] public NativeArray<float3> outputPositions;
        [WriteOnly] public NativeArray<bool> outputs;
        [WriteOnly] public NativeArray<quaternion> outputRotation;

        public void Execute()
        {
            outputPositions[0] = 0;
            outputPositions[1] = 0;
            outputPositions[2] = 0;
            outputPositions[3] = 0;
            outputs[0] = false;
            outputs[1] = false;
            outputs[2] = false;
            outputs[3] = false;
            outputs[4] = false;
            outputs[5] = false;
            outputRotation[0] = quaternion.identity;
            
            float distanceToDestination;
            var prevTargetReached = reachedEndOfPath;

            if (computePlayerPosition)
            {
                outputPositions[3] = new float3(math.distancesq(position, playerPosition), 0, 0);
            }

            if (rotate && !movePath)
            {
                var targetRotation = quaternion.LookRotationSafe(position - rotateTarget, new float3(0, 0, 1));
                targetRotation.value.x = 0;
                targetRotation.value.y = 0;
                outputRotation[0] = MMExtensions.RotateTowards(currentRotation, targetRotation, rotateSpeed * deltaTime);
            }

            if (movePath)
            {
                distanceToDestination = math.distancesq(position, destination);
                
                if (distanceToDestination < 0.04f)
                {
                    // Set a status variable to indicate that the agent has reached the end of the path.
                    // You can use this to trigger some special code if your game requires that.
                    outputs[2] = true;
                    reachedEndOfPath = true;
                }

                if (!reachedEndOfPath)
                {
                    float3 desiredVelocity = math.normalize(destination - position) * maxSpeed;
                    float3 desiredPosition = position + desiredVelocity;
                    
                    float3 moveDelta;

                    if (hasRvo)
                    {
                        var delta = (rvoCalculatedTargetPoint - position);

                        moveDelta = ClampMagnitude(delta, rvoCalculatedSpeed * deltaTime);

                        var difference = math.abs(desiredPosition - rvoCalculatedTargetPoint);
                        
                        // the difference between character target and the target chosen by RVO controller is more than 0.6 * 0.6
                        // -> mark him as stuck in crowd (crowd is affecting where he moves)
                        outputs[5] = math.lengthsq(difference) > 0.36f;
                    }
                    else
                    {
                        moveDelta = desiredVelocity * deltaTime;
                    }
                    
                    if (rotate)
                    {
                        var targetRotation = quaternion.LookRotationSafe(desiredVelocity * -1, new float3(0, 0, 1));
                        targetRotation.value.x = 0;
                        targetRotation.value.y = 0;
                        outputRotation[0] = MMExtensions.RotateTowards(currentRotation, targetRotation, rotateSpeed * deltaTime);
                    }
                    
                    outputPositions[0] = position + moveDelta;
                    outputPositions[1] = desiredPosition;
                    outputPositions[2] = desiredPosition;
                    outputs[3] = true;
                    outputs[4] = true; // set use rvo
                }
                else
                {
                    if (distanceToDestination > 2f)
                    {
                        outputs[0] = true;
                        outputPositions[0] = position;
                        return;
                    }
                    
                    var diff = distanceToDestination / 0.04f;
                    var slowdown = CalculateSlowdown(diff);
                    
                    if (!prevTargetReached)
                    {
                        outputs[1] = true;
                    }

                    if (slowdown > 0.01f)
                    {
                        float desiredSpeed = maxSpeed * slowdown;
                        float3 desiredVelocity = math.normalize(destination - position) * desiredSpeed;
                        float3 desiredPosition = position + desiredVelocity;

                        float3 moveDelta;
                        if (hasRvo)
                        {
                            var delta = (rvoCalculatedTargetPoint - position);

                            moveDelta = ClampMagnitude(delta, rvoCalculatedSpeed * deltaTime);
                        }
                        else
                        {
                            moveDelta = desiredVelocity * deltaTime;
                        }
                        
                        /*var remainingDistance = math.length((waypoint - translation.Value)) + math.length((waypoint - p2));
                        for (int i = pathfinder.wp; i < vectorPath.Length - 1; i++) pathfinder.remainingDistance +=
                            math.length(To2DIn3D(vectorPath[i + 1].Value - vectorPath[i].Value));*/

                        var rvoTarget = position + ClampMagnitude(desiredVelocity, math.sqrt(distanceToDestination));
                        
                        outputPositions[0] = position + moveDelta;
                        outputPositions[1] = desiredPosition;
                        outputPositions[2] = rvoTarget;
                        outputs[3] = true;
                        outputs[4] = true; // set use rvo
                    }
                    else
                    {
                        outputs[0] = true;
                        outputPositions[0] = position;
                    }
                }
            }
        }

        private float CalculateSlowdown(float diff)
        {
            var x = Clamp01(diff);

            return (float) (math.abs(x - 1) < 0.0001f ? 1 : 1 - math.pow(2, -10 * x));
        }

        private float Clamp01(float value)
        {
            if ((float) value < 0.0)
                return 0.0f;
            return (float) value > 1.0 ? 1f : value;
        }
        
        private float3 ClampMagnitude(float3 vector, float maxLength)
        {
            float sqrMagnitude = SqrMagnitude(vector);
            if ((double) sqrMagnitude <= (double) maxLength * (double) maxLength)
                return vector;
            float num1 = (float) math.sqrt((double) sqrMagnitude);
            float num2 = vector.x / num1;
            float num3 = vector.y / num1;
            return new float3(num2 * maxLength, num3 * maxLength, 0);
        }
        
        private float SqrMagnitude(float3 a) => (float) ((double) a.x * (double) a.x + (double) a.y * (double) a.y);
    }
    
    /*[BurstCompile]
    public struct MoveToTargetJob : IJob
    {
        [ReadOnly] public float deltaTime;
        
        [ReadOnly] public bool movePath;
        
        [ReadOnly] public float3 position;
        [ReadOnly] public float maxSpeed;
        
        [ReadOnly] public NativeArray<float3> waypoints;
        [ReadOnly] public int currentWaypoint;

        [ReadOnly] public bool rotate;
        [ReadOnly] public quaternion currentRotation;
        [ReadOnly] public float3 rotateTarget;
        [ReadOnly] public int rotateSpeed;
        
        [ReadOnly] public bool reachedEndOfPath;
        
        [ReadOnly] public bool hasRvo;
        [ReadOnly] public float3 rvoCalculatedTargetPoint;
        [ReadOnly] public float rvoCalculatedSpeed;

        [ReadOnly] public bool computePlayerPosition;
        [ReadOnly] public float3 playerPosition;

        [WriteOnly] public NativeArray<float3> outputPositions;
        [WriteOnly] public NativeArray<bool> outputs;
        [WriteOnly] public NativeArray<int> outputWaypoints;
        [WriteOnly] public NativeArray<quaternion> outputRotation;

        public void Execute()
        {
            outputPositions[0] = 0;
            outputPositions[1] = 0;
            outputPositions[2] = 0;
            outputPositions[3] = 0;
            outputs[0] = false;
            outputs[1] = false;
            outputs[2] = false;
            outputs[3] = false;
            outputs[4] = false;
            outputs[5] = false;
            outputWaypoints[0] = 0;
            outputRotation[0] = quaternion.identity;
            
            float distanceToWaypoint;
            var prevTargetReached = reachedEndOfPath;

            if (computePlayerPosition)
            {
                outputPositions[3] = new float3(math.distancesq(position, playerPosition), 0, 0);
            }

            if (rotate && !movePath)
            {
                var targetRotation = quaternion.LookRotationSafe(position - rotateTarget, new float3(0, 0, 1));
                targetRotation.value.x = 0;
                targetRotation.value.y = 0;
                outputRotation[0] = MMExtensions.RotateTowards(currentRotation, targetRotation, rotateSpeed * deltaTime);
            }

            if (movePath)
            {
                float3 nextWaypoint;
                while (true)
                {
                    nextWaypoint = waypoints[currentWaypoint];
                    distanceToWaypoint = math.distancesq(position, nextWaypoint);
                    
                    if (distanceToWaypoint < 0.04f)
                    {
                        // Check if there is another waypoint or if we have reached the end of the path
                        if (currentWaypoint + 1 < waypoints.Length) 
                        {
                            currentWaypoint++;
                        } 
                        else 
                        {
                            if (distanceToWaypoint < 0.04f)
                            {
                                // Set a status variable to indicate that the agent has reached the end of the path.
                                // You can use this to trigger some special code if your game requires that.
                                outputs[2] = true;
                                reachedEndOfPath = true;
                            }
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                outputWaypoints[0] = currentWaypoint;

                if (!reachedEndOfPath)
                {
                    float3 desiredVelocity = math.normalize(nextWaypoint - position) * maxSpeed;
                    float3 desiredPosition = position + desiredVelocity;
                    
                    float3 moveDelta;

                    if (hasRvo)
                    {
                        var delta = (rvoCalculatedTargetPoint - position);

                        moveDelta = ClampMagnitude(delta, rvoCalculatedSpeed * deltaTime);

                        var difference = math.abs(desiredPosition - rvoCalculatedTargetPoint);
                        
                        // the difference between character target and the target chosen by RVO controller is more than 0.6 * 0.6
                        // -> mark him as stuck in crowd (crowd is affecting where he moves)
                        outputs[5] = math.lengthsq(difference) > 0.36f;
                    }
                    else
                    {
                        moveDelta = desiredVelocity * deltaTime;
                    }
                    
                    if (rotate)
                    {
                        var targetRotation = quaternion.LookRotationSafe(desiredVelocity * -1, new float3(0, 0, 1));
                        targetRotation.value.x = 0;
                        targetRotation.value.y = 0;
                        outputRotation[0] = MMExtensions.RotateTowards(currentRotation, targetRotation, rotateSpeed * deltaTime);
                    }
                    
                    outputPositions[0] = position + moveDelta;
                    outputPositions[1] = desiredPosition;
                    outputPositions[2] = desiredPosition;
                    outputs[3] = true;
                    outputs[4] = true; // set use rvo
                }
                else
                {
                    if (distanceToWaypoint > 2f)
                    {
                        outputs[0] = true;
                        outputPositions[0] = position;
                        return;
                    }
                    
                    var diff = distanceToWaypoint / 0.04f;
                    var slowdown = CalculateSlowdown(diff);
                    
                    if (!prevTargetReached)
                    {
                        outputs[1] = true;
                    }

                    if (slowdown > 0.01f)
                    {
                        float desiredSpeed = maxSpeed * slowdown;
                        float3 desiredVelocity = math.normalize(nextWaypoint - position) * desiredSpeed;
                        float3 desiredPosition = position + desiredVelocity;

                        float3 moveDelta;
                        if (hasRvo)
                        {
                            var delta = (rvoCalculatedTargetPoint - position);

                            moveDelta = ClampMagnitude(delta, rvoCalculatedSpeed * deltaTime);
                        }
                        else
                        {
                            moveDelta = desiredVelocity * deltaTime;
                        }
                        
                        /*var remainingDistance = math.length((waypoint - translation.Value)) + math.length((waypoint - p2));
                        for (int i = pathfinder.wp; i < vectorPath.Length - 1; i++) pathfinder.remainingDistance +=
                            math.length(To2DIn3D(vectorPath[i + 1].Value - vectorPath[i].Value));#1#

                        var rvoTarget = position + ClampMagnitude(desiredVelocity, math.sqrt(distanceToWaypoint));
                        
                        outputPositions[0] = position + moveDelta;
                        outputPositions[1] = desiredPosition;
                        outputPositions[2] = rvoTarget;
                        outputs[3] = true;
                        outputs[4] = true; // set use rvo
                    }
                    else
                    {
                        outputs[0] = true;
                        outputPositions[0] = position;
                    }
                }
            }
        }

        private float CalculateSlowdown(float diff)
        {
            var x = Clamp01(diff);

            return (float) (math.abs(x - 1) < 0.0001f ? 1 : 1 - math.pow(2, -10 * x));
        }

        private float Clamp01(float value)
        {
            if ((float) value < 0.0)
                return 0.0f;
            return (float) value > 1.0 ? 1f : value;
        }
        
        private float3 ClampMagnitude(float3 vector, float maxLength)
        {
            float sqrMagnitude = SqrMagnitude(vector);
            if ((double) sqrMagnitude <= (double) maxLength * (double) maxLength)
                return vector;
            float num1 = (float) math.sqrt((double) sqrMagnitude);
            float num2 = vector.x / num1;
            float num3 = vector.y / num1;
            return new float3(num2 * maxLength, num3 * maxLength, 0);
        }
        
        private float SqrMagnitude(float3 a) => (float) ((double) a.x * (double) a.x + (double) a.y * (double) a.y);

    }*/
}