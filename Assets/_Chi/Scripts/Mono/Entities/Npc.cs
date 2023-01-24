using System;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Movement;
using _Chi.Scripts.Statistics;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class Npc : Entity
    {
        #region references

        [HideInInspector] public Seeker seeker;
        [NonSerialized] public bool hasSeeker;
        [HideInInspector] public RVOController rvoController;
        [NonSerialized] public bool hasRvoController;
        
        public PathData pathData;
        
        public NpcStats stats;
        
        #endregion
        #region publics

        [NonSerialized] public int poolId;

        #endregion
        #region privates

        private float nextPathSeekTime = 0f;
        
        private Func<Path> pathToFind = null;
        private bool resetPath;

        #endregion
        
        public override void Awake()
        {
            base.Awake();
            
            poolId = this.gameObject.GetInstanceID();
            
            seeker = GetComponent<Seeker>();
            hasSeeker = seeker != null;
            rvoController = GetComponent<RVOController>();
            hasRvoController = rvoController != null;

            pathData = new PathData(this);
        }

        public override void DoUpdate()
        {
            if (!isAlive)
            {
                return;
            }

            if (hasSeeker)
            {
                if (pathData.Path == null
                    || Time.time > nextPathSeekTime
                    || resetPath)
                {
                    if (resetPath) // we want to reset the path
                    {
                        pathData.SetPath(null);
                        resetPath = false;
                        pathData.SetDestination(null);
                    
                        //TODO path claiming? for horde movement
                    }
                    else if (pathToFind != null) // we have a new path to find
                    {
                        nextPathSeekTime = Time.time + Random.Range(miscSettingsReference.minSeekPathPeriod, miscSettingsReference.maxSeekPathPeriod);

                        pathData.SetPath(pathToFind());

                        pathToFind = null;
                    }
                }
            }
        }
        
        public bool HasMoveTarget()
        {
            return pathToFind != null || pathData.IsPathReady();
        }

        public void Setup(Vector3 position, Quaternion rotation)
        {
            Register();

            var transform1 = transform;
            transform1.position = position;
            transform1.rotation = rotation;
            
            gameObject.SetActive(true);
        }
        
        public void Cleanup()
        {
            Unregister();
            
            entityStats.maxHpAdd = 0;
            entityStats.maxHpMul = 1;
            this.Heal();
            pathData.SetPath(null);
            pathData.SetDestination(null);
            
            gameObject.SetActive(false);
        }

        public override void OnDie()
        {
            Gamesystem.instance.OnKilled(this);
            
            if (!this.DeletePooledNpc())
            {
                base.OnDie();
            }
        }

        public void SetMoveTarget(Func<Vector3> target)
        {
            if (target != null)
            {
                pathToFind = () =>
                {
                    var targetPos = target();
                    var p = ABPath.Construct(GetPosition(), targetPos, null);

                    pathData.SetDestination(targetPos);
                    SetRotationTarget(targetPos);
                    resetPath = false;
                    
                    return p;
                };
            }
            else
            {
                resetPath = true;
                pathToFind = null;
                SetRotationTarget(null);
            }
        }
    
        public void OnTargetReached()
        {
            //currentPath = null;
        }

        public void Deactivate()
        {
            
        }
    }
}
