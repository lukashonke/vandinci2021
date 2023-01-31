using System;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Movement;
using _Chi.Scripts.Statistics;
using BulletPro;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
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
        [NonSerialized] public SpriteRenderer renderer;
        
        public PathData pathData;

        public NpcStats stats;
        
        #endregion
        #region publics

        public float size = 1f;
        public bool goDirectlyToPlayer;
        [NonSerialized] public float distanceToPlayer;
        [NonSerialized] public float nextDamageTime;
        [NonSerialized] public Vector3 deathDirection;

        public float dissolveSpeed = 2f;
        [NonSerialized] public float currentDissolveProcess;
        [NonSerialized] public int poolId;
        [NonSerialized] public float? maxDistanceFromPlayerBeforeDespawn = null;

        #endregion
        #region privates

        private float nextPathSeekTime = 0f;
        
        private Func<Path> pathToFind = null;
        private bool resetPath;

        private bool isDissolving = false;

        private Material originalMaterial;

        private bool physicsActivated;

        private float nextPushTime;

        #endregion
        
        public override void Awake()
        {
            base.Awake();
            
            poolId = this.gameObject.GetInstanceID();
            
            seeker = GetComponent<Seeker>();
            hasSeeker = seeker != null;
            rvoController = GetComponent<RVOController>();
            hasRvoController = rvoController != null;
            renderer = GetComponent<SpriteRenderer>();
            originalMaterial = renderer.material;
            currentDissolveProcess = 1f;
            
            SetPhysicsActivated(false);

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
        
        public void Setup(Vector3 position, Quaternion rotation)
        {
            Register();

            var transform1 = transform;
            transform1.position = position;
            transform1.rotation = rotation;
            canMove = true;
            
            if (hasRvoController)
            {
                rvoController.enabled = true;
            }
            
            SetPhysicsActivated(false);
            
            currentDissolveProcess = 1f;
            
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
            renderer.material = originalMaterial;
            isDissolving = false;
            canMove = false;
            maxDistanceFromPlayerBeforeDespawn = null;
            
            gameObject.SetActive(false);
        }
        
        public void OnHitByBullet(Bullet bullet, Vector3 pos)
        {
            /*var damage = bullet.moduleParameters.GetFloat("_PowerLevel");
             
            //var val = bullet.dynamicSolver.SolveDynamicInt(p, 15198, ParameterOwner.Bullet);
            ReceiveDamage(damage, null);*/
        }

        public override void OnDie(DieCause cause)
        {
            if (cause == DieCause.Killed)
            {
                if (!isDissolving)
                {
                    Gamesystem.instance.OnKilled(this);

                    Gamesystem.instance.killEffectManager.StartDissolve(this);
            
                    isDissolving = true;
                    canMove = false;
                    if (hasRvoController)
                    {
                        rvoController.enabled = false;
                    }
                }
            }
            else if(cause == DieCause.Despawned)
            {
                if (!this.DeletePooledNpc())
                {
                    Destroy(this.gameObject);
                }
            }
        }
        
        public void OnFinishedDissolve()
        {
            if (!this.DeletePooledNpc())
            {
                Destroy(this.gameObject);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            pathData.OnDestroy();
        }
        
        public bool HasMoveTarget()
        {
            return pathToFind != null || pathData.IsPathReady();
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

        public void SetDistanceToPlayer(float dist2, Player player)
        {
            distanceToPlayer = dist2;

            if (maxDistanceFromPlayerBeforeDespawn.HasValue)
            {
                if (distanceToPlayer > maxDistanceFromPlayerBeforeDespawn.Value)
                {
                    if(physicsActivated)
                    {
                        player.RemoveNearbyEnemy(this);
                        SetPhysicsActivated(false);
                    }
                    
                    OnDie(DieCause.Despawned);
                    return;
                }
            }

            if (!physicsActivated && player.IsInNearbyDistance(dist2))
            {
                player.AddNearbyEnemy(this);
                SetPhysicsActivated(true);
            }
            else if(physicsActivated && !player.IsInNearbyDistance(dist2))
            {
                player.RemoveNearbyEnemy(this);
                SetPhysicsActivated(false);
            }
        }

        public void SetPhysicsActivated(bool b)
        {
            if (b == physicsActivated) return;

            physicsActivated = b;

            if (rb != null)
            {
                hasRb = physicsActivated;
                rb.simulated = physicsActivated;
            }
        }

        public override bool CanBePushed()
        {
            return nextPushTime < Time.time;
        }

        public override void SetCannotBePushed(float duration)
        {
            nextPushTime = Time.time + duration;
        }
    }
}
