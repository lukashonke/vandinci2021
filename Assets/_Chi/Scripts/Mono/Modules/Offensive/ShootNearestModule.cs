using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Modules.Offensive  
{
    public class ShootNearestModule : OffensiveModule
    {
        public ShootNearestTargetType targetType;

        private Func<Entity, bool> nearestTargetFilterFunc;

        public override void Awake()
        {
            base.Awake();

            switch (targetType)
            {
                case ShootNearestTargetType.InFront:
                    nearestTargetFilterFunc = entity => ProjectileExtensions.CanFire(entity.GetPosition(), slot.transform.rotation * originalRotation, transform.position, 90);
                    break;
                case ShootNearestTargetType.NoRotation:
                    nearestTargetFilterFunc = entity => ProjectileExtensions.CanFire(entity.GetPosition(), slot.transform.rotation * originalRotation, transform.position, 15);
                    break;
            }
        }
        
        public override IEnumerator UpdateLoop()  
        {
            var waiter = new WaitForFixedUpdate();

            float nextTargetUpdate = Time.time + targetUpdateInterval;

            //float nextFireRate = Time.time + stats.fireRate;

            //var prefabProjectile = prefab.GetComponent<Projectile>();
            
            while (activated && parent.CanShoot())
            {
                yield return waiter;

                if (nextTargetUpdate > Time.time)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval + Random.Range(0.1f, 0.2f);
                    
                    if (targetType == ShootNearestTargetType.NoRotation)
                    {
                        emitter.Play();
                    }
                    else
                    {
                        var nearest = ((Player) parent).GetNearestEnemy(GetPosition(), nearestTargetFilterFunc);

                        if (nearest != null && Utils.Dist2(nearest.GetPosition(), GetPosition()) < Mathf.Pow(stats.targetRange.GetValue(), 2))
                        {
                            currentTarget = nearest.transform;
                            emitter.Play();
                        }
                        else
                        {
                            currentTarget = null;
                            emitter.Pause();
                        }
                    }
                }
                
                ApplyParams();

                if (currentTarget != null)
                {
                    /*if (nextFireRate < Time.time && ProjectileExtensions.CanFire(currentTarget.position, transform.rotation, transform.position, 5f))
                    {
                        nextFireRate = Time.time + stats.fireRate;
                    
                        var projectile = prefabProjectile.SpawnProjectile(this);

                        var hash = projectile.GetHashCode();
                        
                        //TODO zakomponovat pocet projektilů
                    
                        var tuple = projectile.RotateAndDirectTowards(currentTarget.position, 0);
                        
                        Debug.Log(this.stats.projectileCount.GetValueInt());

                        projectile.rb.velocity = tuple.direction * stats.projectileSpeed;
                        projectile.transform.rotation = tuple.rotation;
                    
                        projectile.ScheduleUnspawn(DamageExtensions.CalculateProjectileLifetime(stats.projectileLifetime, this));
                    }*/

                    if (targetType != ShootNearestTargetType.NoRotation)
                    {
                        RotateTowards(currentTarget.position, instantRotation);
                    }
                }
                else
                {
                    this.transform.localRotation = originalRotation;
                }
            }
        }

        private void ApplyParams()
        {
            if (emitter.rootBullet != null)
            {
                emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, stats.projectileCount.GetValueInt() * stats.projectileMultiplier.GetValueInt());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpeed, stats.projectileSpeed.GetValue());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.WaitDuration, GetFireRate());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpread, stats.projectileSpreadAngle.GetValue());

                if (stats.projectileLifetime.GetValue() > 0)
                {
                    emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileLifetime, stats.projectileLifetime.GetValue());
                }

                if (stats.projectileRange.GetValue() > 0)
                {
                    emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileRange, stats.projectileRange.GetValue());
                }
            }
        }

        public override void OnShootInstruction()
        {
            base.OnShootInstruction();
            
            ShootEffect();
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                ("Damage", $"{stats.projectileDamage.GetValue()}"),
                ("Shoot Interval", $"{stats.fireRate.GetValue()}"),
                ("Projectiles", $"{stats.projectileCount.GetValue()}"),
            };
        }
    }

    public enum ShootNearestTargetType
    {
        Any,
        InFront,
        NoRotation
    }
}