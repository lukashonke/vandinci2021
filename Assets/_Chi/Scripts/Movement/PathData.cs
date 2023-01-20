using System;
using _Chi.Scripts.Mono.Entities;
using Pathfinding;
using Pathfinding.RVO;
using Pathfinding.Util;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _Chi.Scripts.Movement
{
    public class PathData
    {
	    public MoveToTargetJob job;
	    public bool jobDisposed;
	    
        private readonly Npc npc;
        private readonly Seeker seeker;
        private Path path;
        
        //public RVODestinationCrowdedBehavior rvoDensityBehavior;
        
        public float endReachedDistance = 0.2f;
        
        public bool isPathReady;
        
        public int currentWaypoint = 0;
        public bool reachedEndOfPath;
        public NativeArray<float3> pathWaypoints;
        public bool waitingForPathCalculation = false;
        
        public Vector3 destination;
        
        protected PathInterpolator.Cursor interpolator;
        private PathInterpolator interpolatorPath = new PathInterpolator();
        
        public Vector3 position => npc.GetPosition();

        public PathData(Npc npc)
        {
            this.npc = npc;
            this.seeker = npc.seeker;
            //rvoDensityBehavior = new RVODestinationCrowdedBehavior(true, 0.5f, false);

            jobDisposed = true;

            Initialise();
        }

        private void Initialise()
        {
	        if (seeker != null)
	        {
				seeker.pathCallback += OnPathComplete;
	        }
        }

        public bool IsPathReady()
        {
	        return isPathReady;
        }
        
        protected void CancelCurrentPathRequest () 
        {
	        waitingForPathCalculation = false;
	        // Abort calculation of the current path
	        if (seeker != null) seeker.CancelCurrentPathRequest();
        }
        
        public void SetPath (Path path) 
        {
	        if (path == null) 
	        {
		        CancelCurrentPathRequest();
		        ClearPath();
	        } 
	        else if (path.PipelineState == PathState.Created) 
	        {
		        waitingForPathCalculation = true;
		        seeker.CancelCurrentPathRequest();
		        seeker.StartPath(path);
	        } 
	        else if (path.PipelineState == PathState.Returned) 
	        {
		        // Path has already been calculated

		        // We might be calculating another path at the same time, and we don't want that path to override this one. So cancel it.
		        if (seeker.GetCurrentPath() != path)
		        {
			        seeker.CancelCurrentPathRequest();
		        }
		        else throw new System.ArgumentException("If you calculate the path using seeker.StartPath then this script will pick up the calculated path anyway as it listens for all paths the Seeker finishes calculating. You should not call SetPath in that case.");

		        OnPathComplete(path);
	        } 
	        else 
	        {
		        // Path calculation has been started, but it is not yet complete. Cannot really handle this.
		        throw new System.ArgumentException("You must call the SetPath method with a path that either has been completely calculated or one whose path calculation has not been started at all. It looks like the path calculation for the path you tried to use has been started, but is not yet finished.");
	        }
        }

        public Path Path
        {
	        get
	        {
		        return path;
	        }
	        set
	        {
		        path = value;
		        if (path == null)
		        {
			        SetPathReady(false);
		        }
	        }
        }
        
        private NativeArray<float3> dummyArray = new NativeArray<float3>(1, Allocator.Persistent);

        public void InitialisePathJob()
        {
	        var hasRvo = npc.hasRvoController && npc.rvoController.enabled;
            var doPath = true; // TODO can root but keep rotation here
            
            var data = new MoveToTargetJob()
            {
                waypoints = doPath ? npc.pathData.pathWaypoints : dummyArray,
        
                hasRvo = hasRvo,
        
                outputPositions = new NativeArray<float3>(3, Allocator.Persistent),
                outputs = new NativeArray<bool>(6, Allocator.Persistent),
                outputWaypoints = new NativeArray<int>(1, Allocator.Persistent),
                outputRotation = new NativeArray<quaternion>(1, Allocator.Persistent),
            };

            job = data;
            jobDisposed = false;
        }

        public void DisposePathJob()
        {
	        if (!jobDisposed)
	        {
		        this.job.outputPositions.Dispose();
		        this.job.outputs.Dispose();
		        this.job.outputWaypoints.Dispose();
		        this.job.outputRotation.Dispose();
		        jobDisposed = true;
	        }
        }
        
        protected void OnPathComplete (Path newPath) 
        {
			ABPath p = newPath as ABPath;

			if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

			waitingForPathCalculation = false;
			
			currentWaypoint = 0;

			// Increase the reference count on the new path.
			// This is used for object pooling to reduce allocations.
			p.Claim(this);

			// Path couldn't be calculated of some reason.
			// More info in p.errorLog (debug string)
			if (p.error) {
				p.Release(this);
				SetPath(null);
				return;
			}

			// Release the previous path.
			if (path != null)
			{
				path.Release(this);
				pathWaypoints.Dispose();
			}

			// Replace the old path
			path = p;
			
			pathWaypoints = new NativeArray<float3>(path.vectorPath.Count, Allocator.Persistent);
			for (int i = 0; i < path.vectorPath.Count; i++)
				pathWaypoints[i] = path.vectorPath[i];
			
			// The RandomPath and MultiTargetPath do not have a well defined destination that could have been
			// set before the paths were calculated. So we instead set the destination here so that some properties
			// like #reachedDestination and #remainingDistance work correctly.
			if (path is RandomPath rpath) {
				destination = rpath.originalEndPoint;
			} else if (path is MultiTargetPath mpath) {
				destination = mpath.originalEndPoint;
			}

			// Make sure the path contains at least 2 points
			if (path.vectorPath.Count == 1) path.vectorPath.Add(path.vectorPath[0]);
			interpolatorPath.SetPath(path.vectorPath);
			interpolator = interpolatorPath.start;

			// Reset some variables
			reachedEndOfPath = false;

			// Simulate movement from the point where the path was requested
			// to where we are right now. This reduces the risk that the agent
			// gets confused because the first point in the path is far away
			// from the current position (possibly behind it which could cause
			// the agent to turn around, and that looks pretty bad).
			interpolator.MoveToLocallyClosestPoint((position + p.originalStartPoint) * 0.5f);
			interpolator.MoveToLocallyClosestPoint(position);

			// Update which point we are moving towards.
			// Note that we need to do this here because otherwise the remainingDistance field might be incorrect for 1 frame.
			// (due to interpolator.remainingDistance being incorrect).
			//interpolator.MoveToCircleIntersection2D(position, pickNextWaypointDist, movementPlane);
			
			//currentWaypoint = interpolator.segmentIndex;

			var distanceToEnd = remainingDistance;

			if (distanceToEnd <= endReachedDistance) {
				reachedEndOfPath = true;
				SetPathReady(false);
				npc.OnTargetReached();
			}
			else
			{
				SetPathReady(true);
			}
		}

        public void SetPathReady(bool b)
        {
	        isPathReady = b;
	        if (isPathReady)
	        {
		        InitialisePathJob();
	        }
	        else
	        {
		        DisposePathJob();
	        }
        }

		public void ClearPath () 
		{
			CancelCurrentPathRequest();
			if (Path != null)
			{
				Path = null;
				pathWaypoints.Dispose();
			}
			interpolatorPath.SetPath(null);
			reachedEndOfPath = false;
			SetPathReady(false);
		}
    
        public bool ReachedDestination()
        {
            if (!reachedEndOfPath) return false;
            
            //TODO funguje?
            if (remainingDistance + (destination - interpolator.endPoint).magnitude > endReachedDistance) return false;

            return true;
        }

        public void SetDestination(Vector3? destination)
        {
	        if (destination.HasValue)
	        {
				this.destination = destination.Value;
				//rvoDensityBehavior.OnDestinationChanged(destination.Value, ReachedDestination());
	        }
        }
    
        public float remainingDistance 
        {
            get {
                return interpolator.valid ? interpolator.remainingDistance + ToPlane(interpolator.position - position).magnitude : float.PositiveInfinity;
            }
        }
        
        public Vector3 endOfPath {
	        get {
		        return interpolator.valid ? interpolator.endPoint : destination;
	        }
        }
    
        Vector2 ToPlane(Vector3 point)
        {
            return new Vector2(point.x, point.y);
        }
    }
}