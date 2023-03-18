using System;
using _Chi.Scripts.Mono.Entities;
using Pathfinding;
using Pathfinding.RVO;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Chi.Scripts.Movement
{
    public class PathData
    {
	    public SimpleMoveToTargetJob job;
	    public bool jobDisposed;
	    
        private readonly Npc npc;
        
        //public RVODestinationCrowdedBehavior rvoDensityBehavior;

        public bool reachedEndOfPath;
        
        public Vector3 destination;

        public PathData(Npc npc)
        {
            this.npc = npc;
            //rvoDensityBehavior = new RVODestinationCrowdedBehavior(true, 0.5f, false);
            
            Initialise();
            InitialisePathJobData(true);
        }

        private void Initialise()
        {
	        /*if (seeker != null)
	        {
				seeker.pathCallback += OnPathComplete;
	        }*/
        }
        
        public void InitialisePathJobData(bool force = false)
        {
	        if (jobDisposed || force)
	        {
		        var hasRvo = npc.hasRvoController && npc.rvoController.enabled;
            
		        var data = new SimpleMoveToTargetJob()
		        {
			        hasRvo = hasRvo,
        
			        outputPositions = new NativeArray<float3>(4, Allocator.Persistent),
			        outputs = new NativeArray<bool>(6, Allocator.Persistent),
			        outputRotation = new NativeArray<quaternion>(1, Allocator.Persistent),
		        };

		        job = data;
		        jobDisposed = false;
	        }
        }

        public void DisposePathJob()
        {
	        if (!jobDisposed)
	        {
		        this.job.outputPositions.Dispose();
		        this.job.outputs.Dispose();
		        this.job.outputRotation.Dispose();
		        jobDisposed = true;
	        }
        }

        public void OnDestroy()
        {
	        DisposePathJob();
        }

        public void SetDestination(Vector3? destination)
        {
	        if (destination.HasValue)
	        {
				this.destination = destination.Value;
				//rvoDensityBehavior.OnDestinationChanged(destination.Value, ReachedDestination());
	        }
        }
    }
}