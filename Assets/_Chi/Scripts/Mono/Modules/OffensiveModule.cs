using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using _Chi.Scripts.Utilities;
using BulletPro;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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

        public ParticleSystem shootVfx;

        [FormerlySerializedAs("shootEffectSelfs")] public List<ImmediateEffect> shootEffectsSelf;
        
        [ReadOnly] public List<(object, ImmediateEffect)> additionalShootEffectsSelf;

        public TargetType affectType;

        public TrailParameters trailParameters;

        public BulletBehaviorType bulletBehavior = BulletBehaviorType.Default;

        protected bool activated;
        
        public int temporaryProjectilesUntilNextShot = 0;

        public override void Awake()
        {
            base.Awake();

            additionalEffects = new();
            additionalOnBulletDestroyEffects = new();
            additionalShootEffectsSelf = new();
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
                    eff.Apply(parent, parent.GetPosition(), parent, null, this, 1f, new ImmediateEffectParams());
                }
            }
            
            if(additionalShootEffectsSelf != null)
            {
                foreach (var (source, eff) in additionalShootEffectsSelf)
                {
                    eff.Apply(parent, parent.GetPosition(), parent, null, this, 1f, new ImmediateEffectParams());
                }
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

            temporaryProjectilesUntilNextShot = 0;
        }

        public virtual void OnBulletEffectGiven(Bullet bullet, BulletBehavior behavior, bool bulletWillDie)
        {
            if (!bulletWillDie && bulletBehavior.HasFlag(BulletBehaviorType.RetargetOnCollision))
            {
                var nearest = ((Player) parent).GetNearestEnemy(bullet.self.position, (e) =>
                {
                    if (behavior.lastAffectedEnemy == e) return false;

                    for (var index = 0; index < behavior.collidedWith.Length; index++)
                    {
                        var bulletReceiver = behavior.collidedWith[index];
                        if (bulletReceiver == null) break;
                        if(e.hasBulletReceiver && e.bulletReceiver == bulletReceiver) return false;
                    }
                    
                    var angle = bullet.moduleMovement.GetAngleTo(e.transform);
                    
                    return Math.Abs(angle) < 75 && Utils.Dist2(e.GetPosition(), bullet.self.position) < Math.Pow(stats.projectileRange.GetValue(), 2);
                });

                if (nearest != null)
                {
                    bullet.moduleMovement.Rotate(bullet.moduleMovement.GetAngleTo(nearest.transform));
                }
            }
        }

        public float GetFireRate()
        {
            var retValue = stats.fireRate.GetValue();
            if (parent is Player player)
            {
                retValue *= player.stats.moduleFireRateMul.GetValue();
            }

            return retValue;
        }

        public void AddRechargeFireProgressPercent(float percent)
        {
            reloadProgress = Mathf.Min(1, reloadProgress + percent);
        }
        
        public void AddRechargeFireProgressTime(float time)
        {
            reloadProgress = Mathf.Min(1, reloadProgress + (time) / GetFireRate());
        }

        public void AddTemporaryProjectileUntilNextShot(int projectiles)
        {
            temporaryProjectilesUntilNextShot += projectiles;
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

    [Flags]
    public enum BulletBehaviorType
    {
        Default = 0,
        RetargetOnCollision = 1
    }
}