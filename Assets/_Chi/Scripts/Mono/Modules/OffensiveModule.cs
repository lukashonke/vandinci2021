using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using _Chi.Scripts.Utilities;
using BulletPro;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module, IBulletEmitterEntityParameters
    {
        [NonSerialized] public BulletEmitter emitter;
        [NonSerialized] public bool hasEmitter;

        public List<Transform> muzzles;
        private int lastMuzzle;

        public OffensiveModuleStats stats;
        
        public List<ImmediateEffect> effects;

        [ReadOnly] public List<(object, ImmediateEffect)> additionalEffects;

        public ParticleSystem shootVfx;

        public TargetType affectType;

        public TrailParameters trailParameters;

        public BulletBehaviorType bulletBehavior = BulletBehaviorType.Default;

        protected bool activated;

        public override void Awake()
        {
            base.Awake();

            additionalEffects = new();
            emitter = GetComponent<BulletEmitter>();
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
                emitter.shootInstruction.AddListener(OnShootInstruction);
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
                emitter.shootInstruction.RemoveListener(OnShootInstruction);
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
        }

        public virtual void OnShootInstruction()
        {
            
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