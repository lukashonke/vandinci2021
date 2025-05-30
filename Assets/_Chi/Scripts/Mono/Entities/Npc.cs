using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Movement;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using BulletPro;
using Pathfinding;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
using UnityEngine;

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
        
        public bool addTargetToUi;
        [ShowIf("addTargetToUi")]
        public LocationTargetType addTargetToUiType;

        [Button]
        [HideInPlayMode]
        public void RandomPoolId()
        {
            poolId = GetInstanceID();
        }
        
        [NonSerialized] public float? maxDistanceFromPlayerBeforeDespawn = null;
        [NonSerialized] public DespawnCondition despawnCondition;
        [NonSerialized] public float? despawnTime = 0;

        public float activateWhenCloseToPlayerDist2 = 0;
        
        [NonSerialized] public Dictionary<Skill, SkillData> skillDatas;

        public ParticleSystem spawnEffect;

        [NonSerialized] public bool despawned;

        public bool takesUpPositionInPositionManager = false;

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
        
        private GameObject prefabVariantChild;

        #endregion
        
        public override void Awake()
        {
            base.Awake();
            
            rvoController = GetComponent<RVOController>();
            hasRvoController = rvoController != null;
            
            currentDissolveProcess = 1f;

            originalSpeed = stats.speed;

            SetPhysicsActivated(false);

            pathData = new PathData(this);
            
            despawnTime = null;
            despawned = false;
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

            /*if (hasSeeker)
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
            }*/

            if (despawnTime.HasValue)
            {
                if(despawnTime.Value < Time.time)
                {
                    OnDie(DieCause.Despawned);
                    despawnTime = null;
                }
            }
        }
        
        public virtual void Setup(Vector3 position, Quaternion rotation)
        {
            Register();

            try
            {
                var transform1 = transform;
                transform1.position = position;
                transform1.rotation = rotation;
                SetCanMove(true);
            
                if (hasRvoController)
                {
                    rvoController.enabled = true;
                }
            
                SetPhysicsActivated(false);
            
                currentDissolveProcess = 1f;
            
                gameObject.SetActive(true);
                
                SetFearing(false);

                if (spawnEffect != null)
                {
                    spawnEffect.Play();
                }
            
                if (addTargetToUi)
                {
                    Gamesystem.instance.locationManager.AddTarget(position, this.gameObject, addTargetToUiType);
                }
                
                despawned = false;
            }
            catch (Exception e)
            {
                Debug.LogError(name);
                throw;
            }
        }
        
        public virtual void Cleanup()
        {
            Unregister();
            
            Gamesystem.instance.positionManager.RemovePosition(this);

            if (prefabVariantChild != null)
            {
                Destroy(prefabVariantChild);
            }
            
            entityStats.maxHpAdd = 0;
            entityStats.maxHpMul = 1;
            this.Heal();
            //pathData.SetPath(null);
            pathData.SetDestination(null);
            if (hasRenderer)
            {
                renderer.material = originalMaterial;
            }
            isDissolving = false;
            SetCanMove(false);
            maxDistanceFromPlayerBeforeDespawn = null;
            despawnCondition = DespawnCondition.DistanceFromPlayer;
            
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
            stunnedCounter = 0;
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
            
            SetFearing(false);
            
            if (hasRvoController)
            {
                rvoController.layer = (RVOLayer) 1;
                rvoController.collidesWith = (RVOLayer) (1);
            }

            despawnTime = null;
            
            if (addTargetToUi)
            {
                Gamesystem.instance.locationManager.RemoveTarget(this.gameObject);
            }
            
            despawned = true;
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
            
            if (prefabVariantChild != null)
            {
                Destroy(prefabVariantChild);
                prefabVariantChild = null;
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
                    animator.SetFloat("MovementSpeed", canMove ? GetMoveSpeed() : 0);
                }
                else
                {
                    animatorSetup = false;
                    animator.enabled = false;
                }
            }

            if (variantInstance.parameters != null)
            {
                if (hasRvoController)
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
                    
                    if (variantInstance.parameters.addChildren)
                    {
                        var transform1 = transform;
                        prefabVariantChild = Instantiate(variantInstance.parameters.addChildren, transform1.position, Quaternion.identity, transform1);
                    }
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
            if (isDissolving || despawned)
            {
                return;
            }
            
            if (cause == DieCause.Killed)
            {
                if (!isDissolving)
                {
                    if (currentVariantInstance?.skillsOnDie != null)
                    {
                        foreach (var skill in currentVariantInstance.skillsOnDie)
                        {
                            skill.Trigger(this, force: true);    
                        }
                    }
                    
                    Gamesystem.instance.OnKilled(this);

                    Gamesystem.instance.killEffectManager.StartDissolve(this);
            
                    isDissolving = true;
                    SetCanMove(false);
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
                SetCanMove(false);
                
                if (!this.DeletePooledNpc())
                {
                    try
                    {
                        throw new Exception();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("2 destroying npc " + name);
                        Debug.LogError(e);
                    }
                    
                    Destroy(this.gameObject);
                }
            }
        }
        
        public bool OnFinishedDissolve()
        {
            if (!this.DeletePooledNpc())
            {
                try
                {
                    throw new Exception();
                }
                catch (Exception e)
                {
                    Debug.LogError("1 destroying npc " + name);
                    Debug.LogError(e);
                }
                
                Destroy(this.gameObject);

                return false;
            }

            return true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                Debug.LogError("destroying npc " + name);
                Debug.LogError(e);
            }
            
            pathData.OnDestroy();
            
            if (isRegisteredWithSkills)
            {   
                skillDatas?.Clear();
                Gamesystem.instance.objects.UnregisterNpcWithSkills(this);
                isRegisteredWithSkills = false;
            }
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
                var doDespawn = false;
                
                if (despawnCondition == DespawnCondition.DistanceFromScreenBorder)
                {
                    //TODO optimise
                    var position = GetPosition();
                    
                    var distX = Math.Abs(player.position.x - position.x) - Gamesystem.instance.HorizontalToBorderDistance;
                    if (distX > maxDistanceFromPlayerBeforeDespawn.Value)
                    {
                        doDespawn = true;
                    }
                    
                    var distY = Math.Abs(player.position.y - position.y) - Gamesystem.instance.VerticalToBorderDistance;
                    if (distY > maxDistanceFromPlayerBeforeDespawn.Value)
                    {
                        doDespawn = true;
                    }
                }
                else
                {
                    if (dist2ToPlayer > maxDistanceFromPlayerBeforeDespawn.Value)
                    {
                        doDespawn = true;
                    }
                }

                if (doDespawn)
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


            /*var visible = player.stats.visibilityRange.GetValue() > dist2ToPlayer;
            if (hasRenderer)
            {
                renderer.enabled = visible;
            }*/

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
        
        public override void UpdateStunned()
        {
            /*if (!hasRvoController) return;
            
            if (stunnedCounter > 0)
            {
                rvoController.enabled = false;
            }
            else
            {
                rvoController.enabled = true;
            }*/
        }

        public override float GetMoveSpeed()
        {
            if (stunnedCounter > 0)
            {
                return Math.Min(stats.speed, Gamesystem.instance.miscSettings.maxStunnedSpeed);
            }
            
            return stats.speed;
        }

        public override SkillData GetSkillData(Skill skill)
        {
            if (skillDatas == null) return null;
            
            return skillDatas.TryGetValue(skill, out var data) ? data : null;
        }

        public override void SetCanMove(bool b)
        {
            base.SetCanMove(b);

            if (hasAnimator && animator.gameObject.activeSelf && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat("MovementSpeed", canMove ? GetMoveSpeed() : 0);
            }
        }

        public Vector3 TransformMoveDestination(Vector3 destination)
        {
            if (isFearing)
            {
                var angle = fearEscapeAngle;
                var dir = (GetPosition() - destination).normalized;
                var rot = Quaternion.AngleAxis(angle, Vector3.forward);
                Debug.DrawLine(destination, destination + rot * dir, Color.red, 1);
                destination = GetPosition() + rot * dir;
                //destination = GetPosition() + (GetPosition() - destination);
            }
            return destination;
        }
    }
}
