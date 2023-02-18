using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Movement;
using _Chi.Scripts.Scriptables;
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
        
        public PathData pathData;

        public NpcStats stats;
        
        #endregion
        #region publics

        public string currentVariant;
        [NonSerialized] public PrefabVariant currentVariantInstance;
        
        public float size = 1f;
        public bool goDirectlyToPlayer;
        public Vector3? fixedMoveTarget;
        public bool stopWhenReachFixedMoveTarget;
        public bool dieWhenReachFixedMoveTarget;
        [NonSerialized] public float dist2ToPlayer;
        [NonSerialized] public float nextDamageTime;
        [NonSerialized] public Vector3 deathDirection;

        public float dissolveSpeed = 2f;
        [NonSerialized] public float currentDissolveProcess;
        public int poolId;
        [NonSerialized] public float? maxDistanceFromPlayerBeforeDespawn = null;

        public float activateWhenCloseToPlayerDist2 = 0;
        
        [NonSerialized] public Dictionary<Skill, SkillData> skillDatas;

        public ParticleSystem spawnEffect;

        #endregion
        #region privates

        private float nextPathSeekTime = 0f;
        public Func<Path> pathToFind = null;
        private bool resetPath;
        private bool isDissolving = false;
        
        private bool physicsActivated;
        private float nextPushTime;

        private float originalSpeed;

        private bool isRegisteredWithSkills;

        #endregion
        
        public override void Awake()
        {
            base.Awake();
            
            seeker = GetComponent<Seeker>();
            hasSeeker = seeker != null;
            rvoController = GetComponent<RVOController>();
            hasRvoController = rvoController != null;
            
            currentDissolveProcess = 1f;

            originalSpeed = stats.speed;

            SetPhysicsActivated(false);

            pathData = new PathData(this);
        }

        public override void Start()
        {
            base.Start();

            if (currentVariant != null && currentVariant.Length > 0)
            {
                ApplyVariant(currentVariant);
            }
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
        
        public virtual void Setup(Vector3 position, Quaternion rotation)
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

            if (spawnEffect != null)
            {
                spawnEffect.Play();
            }
        }
        
        public virtual void Cleanup()
        {
            Unregister();
            
            entityStats.maxHpAdd = 0;
            entityStats.maxHpMul = 1;
            this.Heal();
            pathData.SetPath(null);
            pathData.SetDestination(null);
            if (hasRenderer)
            {
                renderer.material = originalMaterial;
            }
            isDissolving = false;
            canMove = false;
            maxDistanceFromPlayerBeforeDespawn = null;
            
            SetImmobilizedUntil(0);
            
            currentEffects.Clear();

            if (vfx.Any())
            {
                foreach (var vfx in vfx)
                {
                    Gamesystem.instance.poolSystem.DespawnGo(vfx.Key, vfx.Value);
                }
                
                vfx.Clear();
            }

            immobilizedCounter = 0;
            stats.speed = originalSpeed;
            
            gameObject.SetActive(false);

            if (isRegisteredWithSkills)
            {
                Gamesystem.instance.objects.UnregisterNpcWithSkills(this);
                skillDatas?.Clear();
                isRegisteredWithSkills = false;
            }

            fixedMoveTarget = null;
            stopWhenReachFixedMoveTarget = false;
            dieWhenReachFixedMoveTarget = false;
            hasRvoController = rvoController != null;
            
            if (hasRvoController)
            {
                rvoController.layer = (RVOLayer) 1;
                rvoController.collidesWith = (RVOLayer) (1);
            }
        }

        public virtual void ApplyVariant(string variant)
        {
            if (variant == null)
            {
                Debug.LogError("Cannot set null variant.");
                return;
            }

            var variantInstance = Gamesystem.instance.prefabDatabase.GetVariant(variant);
            if (variantInstance == null)
            {
                Debug.LogError($"Variant {variant} is not defined.");
                return;
            }

            currentVariant = variant;
            currentVariantInstance = variantInstance;
            entityStats.CopyFrom(variantInstance.entityStats);
            stats.CopyFrom(variantInstance.npcStats);

            if (hasRenderer)
            {
                renderer.sprite = variantInstance.sprite;
                renderer.material = variantInstance.spriteMaterial;
            }

            if (hasAnimator)
            {
                if (variantInstance.animatorController != null)
                {
                    animator.enabled = true;
                    animatorSetup = true;
                    animator.runtimeAnimatorController = variantInstance.animatorController;
                    animator.SetFloat("MovementSpeed", stats.speed);
                }
                else
                {
                    animatorSetup = false;
                    animator.enabled = false;
                }
            }

            if (hasRvoController && variantInstance.parameters != null)
            {
                rvoController.priority = variantInstance.parameters.rvoPriority;
                if (variantInstance.parameters.setRvoLayers)
                {
                    rvoController.layer = variantInstance.parameters.rvoLayer;
                    rvoController.collidesWith = variantInstance.parameters.rvoCollidesWith;
                }

                if (variantInstance.parameters.disableRvoCollision)
                {
                    rvoController.layer = RVOLayer.Layer30;
                    rvoController.collidesWith = (RVOLayer) (0);
                }
            }

            if (variantInstance.skills != null && variantInstance.skills.Count > 0)
            {
                skillDatas = new();
                Gamesystem.instance.objects.RegisterNpcWithSkills(this);
                isRegisteredWithSkills = true;
            }
            else if(isRegisteredWithSkills)
            {   
                skillDatas?.Clear();
                Gamesystem.instance.objects.UnregisterNpcWithSkills(this);
                isRegisteredWithSkills = false;
            }
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
                    if (currentVariantInstance?.skillOnDie != null)
                    {
                        currentVariantInstance.skillOnDie.Trigger(this, force: true);
                    }
                    
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
                if (hasRvoController)
                {
                    rvoController.enabled = false;
                }
                canMove = false;
                
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
            
            if (isRegisteredWithSkills)
            {   
                skillDatas?.Clear();
                Gamesystem.instance.objects.UnregisterNpcWithSkills(this);
                isRegisteredWithSkills = false;
            }
        }
        
        public bool HasMoveTarget()
        {
            return pathToFind != null || pathData.IsPathReady();
        }

        public void SetFixedMoveTarget(Vector3? target, bool stopWhenReachFixedTarget = false, bool dieWhenReachFixedTarget = false)
        {
            if (target == null && dieWhenReachFixedMoveTarget)
            {
                OnDie(DieCause.Despawned);
                return;
            }
            else if (target == null && stopWhenReachFixedMoveTarget)
            {
                return;
            }
            
            fixedMoveTarget = target;
            pathData.reachedEndOfPath = false;
            dieWhenReachFixedMoveTarget = dieWhenReachFixedTarget;
            stopWhenReachFixedMoveTarget = stopWhenReachFixedTarget;
        }
        
        public void SetMovePath(Path path)
        {
            if (path != null)
            {
                pathToFind = () => path;
            }
            else
            {
                resetPath = true;
                pathToFind = null;
                SetRotationTarget(null);
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

        public bool CanGoDirectlyToPlayer()
        {
            if (activateWhenCloseToPlayerDist2 > 0)
            {
                if (dist2ToPlayer < activateWhenCloseToPlayerDist2)
                {
                    return goDirectlyToPlayer;
                }

                return false;
            }

            return goDirectlyToPlayer;
        }

        public void SetDistanceToPlayer(float dist2, Player player)
        {
            dist2ToPlayer = dist2;

            if (maxDistanceFromPlayerBeforeDespawn.HasValue)
            {
                if (dist2ToPlayer > maxDistanceFromPlayerBeforeDespawn.Value)
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
            
            var distToDamage = player.stats.maxDistanceToReceiveContactDamage.GetValue();
            if (dist2ToPlayer < distToDamage && nextDamageTime < Time.time && isAlive)
            {
                player.ReceiveDamageByContact(this, false);
            }
        }

        public void SetPhysicsActivated(bool b)
        {
            if (b == physicsActivated)
            {
                return;
            }

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

        public void AddToSpeed(float val)
        {
            stats.speed += val;
        }

        public override void UpdateImmobilized()
        {
            if (!hasRvoController) return;
            
            if (immobilizedCounter > 0)
            {
                rvoController.enabled = false;
            }
            else
            {
                rvoController.enabled = true;
            }
        }
        
        public override SkillData GetSkillData(Skill skill)
        {
            if (skillDatas == null) return null;
            
            return skillDatas.TryGetValue(skill, out var data) ? data : null;
        }
    }
}
