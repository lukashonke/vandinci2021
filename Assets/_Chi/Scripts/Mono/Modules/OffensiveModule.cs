using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using BulletPro;
using Com.LuisPedroFonseca.ProCamera2D;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils = _Chi.Scripts.Utilities.Utils;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module, IBulletEmitterEntityParameters
    {
        [NonSerialized] public BulletEmitter emitter;
        [NonSerialized] public bool hasEmitter;
        
        public float reloadProgress;
        [NonSerialized] public float startReloadAtTime;

        public List<Transform> muzzles;
        private int lastMuzzle;

        public OffensiveModuleStats stats;
        
        public List<ImmediateEffect> effects;
        
        [FormerlySerializedAs("beforeBulletDestroyEffects")] public List<ImmediateEffect> onBulletDestroyEffects;

        [ReadOnly] public List<(object, ImmediateEffect)> additionalEffects;
        
        [ReadOnly] public List<(object, ImmediateEffect)> additionalOnBulletDestroyEffects;
        
        [ReadOnly] public List<(object, ImmediateEffect)> disabledEffects;

        [ReadOnly] public List<(object, Func<Entity, bool>)> targetAffectConditions;
        
        public ParticleSystem shootVfx;

        [FormerlySerializedAs("shootEffectSelfs")] public List<ImmediateEffect> shootEffectsSelf;
        
        [ReadOnly] public List<(object, ImmediateEffect)> additionalShootEffectsSelf;

        public TargetType affectType;

        public TrailParameters trailParameters;
        
        public SpriteEffectParameters spriteEffectParameters;

        public BulletBehaviorType bulletBehavior = BulletBehaviorType.Default;

        protected bool activated;

        [ReadOnly] public int currentMagazine;
        [ReadOnly] public bool isReloading;
        
        public int temporaryProjectilesUntilNextShot = 0;
        [FormerlySerializedAs("temporaryProjectilesForCurrentOrNextMagazine")] public int temporaryProjectilesForNextMagazine = 0;

        public ShakePreset fireCameraShake;

        public override void Awake()
        {
            base.Awake();

            additionalEffects = new();
            additionalOnBulletDestroyEffects = new();
            additionalShootEffectsSelf = new();
            targetAffectConditions = new();
            disabledEffects = new();
            emitter = GetComponent<BulletEmitter>();
            subEmitters = new();
            hasEmitter = emitter != null;
        }

        public override void Start()
        {
            base.Start();
        }

        public override bool ActivateEffects()
        {
            if (!base.ActivateEffects() || activated) return false;

            if (hasEmitter)
            {
                emitter.shootInstruction.AddListener(OnEmitterShootInstruction);
            }
            
            activated = true;
            StartCoroutine(UpdateLoop());
            
            return true;
        }

        public override bool DeactivateEffects()
        {
            if (!base.DeactivateEffects() || !activated) return false;

            if (hasEmitter)
            {
                emitter.shootInstruction.RemoveListener(OnEmitterShootInstruction);
            }
            activated = false;
            return true;
        }

        public abstract IEnumerator UpdateLoop();
        
        public void ShootEffect()
        {
            if (shootVfx != null)
            {
                if (muzzles != null && muzzles.Count > 0)
                {
                    lastMuzzle++;
                    if (lastMuzzle >= muzzles.Count) lastMuzzle = 0;
                    shootVfx.transform.position = muzzles[lastMuzzle].position;
                }
                
                shootVfx.Play();
            }

            if (shootEffectsSelf != null)
            {
                foreach (var eff in shootEffectsSelf)
                {
                    var data = Gamesystem.instance.poolSystem.GetEffectData();
                    data.target = parent;
                    data.targetPosition = parent.GetPosition();
                    data.sourceEntity = parent;
                    data.sourceModule = this;
                    
                    eff.ApplyWithChanceCheck(data, 1f, new ImmediateEffectParams());
                    
                    Gamesystem.instance.poolSystem.ReturnEffectData(data);
                }
            }
            
            if(additionalShootEffectsSelf != null)
            {
                foreach (var (source, eff) in additionalShootEffectsSelf)
                {
                    var data = Gamesystem.instance.poolSystem.GetEffectData();
                    data.target = parent;
                    data.targetPosition = parent.GetPosition();
                    data.sourceEntity = parent;
                    data.sourceModule = this;
                    
                    eff.ApplyWithChanceCheck(data, 1f, new ImmediateEffectParams());
                    
                    Gamesystem.instance.poolSystem.ReturnEffectData(data);
                }
            }
            
            if(fireCameraShake != null)
            {
                ProCamera2DShake.Instance.Shake(fireCameraShake);
            }
        }

        public virtual void OnEmitterShootInstruction()
        {
            OnShootInstruction(emitter);
        }

        public virtual void OnShootInstruction(object source)
        {
            foreach (var subEmitter in subEmitters.Values)
            {
                foreach (var su in subEmitter)
                {
                    su.OnParentShoot(source);
                }
            }
        }

        public virtual void OnBulletDeath(Bullet bullet, BulletBehavior behavior)
        {
            if (subEmitters != null)
            {
                foreach (var kp in subEmitters)
                {
                    foreach (var subEmitter in kp.Value)
                    {
                        subEmitter.OnBulletDeath(this, bullet, behavior);
                    }
                }
            }
        }

        public virtual bool OnBulletBeforeDeactivated(Bullet bullet, BulletBehavior behavior)
        {
            if (bulletBehavior.HasFlag(BulletBehaviorType.OnDieGoToPlayer))
            {
                for (int i = 0; i < behavior.collidedWith.Length; i++)
                {
                    behavior.collidedWith[i] = null;
                }
                
                behavior.piercedEnemies = 0;
                behavior.piercedDeadEnemies = 0;
                
                bullet.moduleMovement.Rotate(bullet.moduleMovement.GetAngleTo(this.transform));
                return false;
            }
            return true;
        }

        public virtual void OnBulletEffectGiven(Bullet bullet, BulletBehavior behavior, bool bulletWillDie)
        {
            var retargetForward = bulletBehavior.HasFlag(BulletBehaviorType.RetargetOnCollisionForward);
            var retargetNoRestriction = bulletBehavior.HasFlag(BulletBehaviorType.RetargetWithoutRestrictions);
            if (!bulletWillDie && (retargetForward || retargetNoRestriction))
            {
                var nearest = ((Player) parent).GetNearestEnemy(bullet.self.position, (e) =>
                {
                    if (behavior.lastAffectedEnemy == e) return false;

                    for (var index = 0; index < behavior.collidedWith.Length; index++)
                    {
                        var bulletReceiver = behavior.collidedWith[index];
                        if (bulletReceiver == null) continue;
                        if (e.hasBulletReceiver && e.bulletReceiver == bulletReceiver)
                        {
                            if (retargetNoRestriction && Utils.Dist2(e.transform.position, bullet.self.position) > 0.81f)
                            {
                                behavior.collidedWith[index] = null;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    
                    var angle = bullet.moduleMovement.GetAngleTo(e.transform);

                    if (!retargetNoRestriction && Math.Abs(angle) > 75) return false;
                    
                    return Utils.Dist2(e.GetPosition(), bullet.self.position) < Math.Pow(stats.projectileRange.GetValue(), 2);
                });

                if (nearest != null)
                {
                    bullet.moduleMovement.Rotate(bullet.moduleMovement.GetAngleTo(nearest.transform));
                }
            }
        }

        public float GetReloadDuration()
        {
            var retValue = stats.reloadDuration.GetValue();
            if (parent is Player player)
            {
                retValue *= player.stats.moduleReloadDurationMul.GetValue();

                /*if (player.IsMoving())
                {
                    retValue *= stats.movingReloadDurationBoost.GetValue();
                }
                else
                {
                    retValue *= stats.stationaryReloadDurationBoost.GetValue();
                }*/
            }

            return retValue;
        }
        
        public float GetFireRate()
        {
            var retValue = stats.fireRate.GetValue();
            if (parent is Player player)
            {
                retValue *= player.stats.moduleFireRateMul.GetValue();

                if (player.IsMoving())
                {
                    retValue *= stats.movingFireRateBoost.GetValue();
                }
                else
                {
                    retValue *= stats.stationaryFireRateBoost.GetValue();
                }
            }

            return retValue;
        }

        public void AddRechargeFireProgressPercent(float percent)
        {
            if (!isReloading) return;
            reloadProgress = Mathf.Min(1, reloadProgress + percent);
        }

        public void ReplenishMagazine()
        {
            currentMagazine = stats.magazineSize.GetValueInt();
        }
        
        public void AddRechargeFireProgressTime(float time)
        {
            if (!isReloading) return;
            reloadProgress = Mathf.Min(1, reloadProgress + (time) / GetReloadDuration());
        }
        
        public void AddTemporaryProjectileUntilNextShot(int projectiles)
        {
            temporaryProjectilesUntilNextShot += projectiles;
        }

        public void AddTemporaryProjectileForCurrentOrNextMagazine(int projectiles)
        {
            if (isReloading)
            {
                temporaryProjectilesForNextMagazine += projectiles;   
            }
            else
            {
                currentMagazine += projectiles;
            }        
        }

        protected void RefreshStatusbarReload()
        {
            if (statusbar != null)
            {
                statusbar.value = reloadProgress;
                statusbar.maxValue = 1;
                statusbar.Recalculate();
            }
        }

        public override void OnAfterSkillUse(Skill skill)
        {
            base.OnAfterSkillUse(skill);

            if (stats.shootOnSkillUse.GetValueInt() > 0)
            {
                var shots = stats.shootOnSkillUse.GetValueInt();
                for (int i = 0; i < shots; i++)
                {
                    emitter.applyBulletParamsAction = () =>
                    {
                        emitter.ApplyParams(stats, parent, this);
                    };
                    
                    var nearest = ((Player) parent).GetNearestEnemy(GetPosition(), null);
                    if (nearest != null)
                    {
                        RotateTowards(nearest.GetPosition(), instantRotation);
                    }
                    
                    emitter.Play();
                    //emitter.gameObject.transform.rotation = origRotation;
                }
            }
        }
        
        private Quaternion RotateBackwards(Quaternion rotation)
        {
            return Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z + 180);
        }
    }

    [Serializable]
    public class TrailParameters
    {
        public bool useTrail;

        public Material material;

        public float trailLengthTime;
    }

    [Serializable]
    public class SpriteEffectParameters
    {
        public bool enabled;
        
        public Material material;
        public Sprite sprite;

        public float rotation;

        public Vector3 scale = Vector3.one;
        [FormerlySerializedAs("position")] public Vector3 offset = Vector3.zero;
    }

    [Flags]
    public enum BulletBehaviorType
    {
        Default = 0,
        RetargetOnCollisionForward = 1 << 0,
        RetargetWithoutRestrictions = 1 << 1,
        OnDieGoToPlayer = 1 << 2,
    }
}