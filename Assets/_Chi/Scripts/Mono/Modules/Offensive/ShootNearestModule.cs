using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Utilities;
using BulletPro;
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
            yield return new WaitForSeconds(Random.Range(0.05f, 0.5f));
            
            float nextTargetUpdate = Time.time + targetUpdateInterval;

            reloadProgress = 0f;

            //float nextFireRate = Time.time + stats.fireRate;

            //var prefabProjectile = prefab.GetComponent<Projectile>();
            
            while (activated && parent.CanShoot())
            {
                yield return null;

                if (startReloadAtTime < Time.time)
                {
                    reloadProgress = Mathf.Min(1, reloadProgress + Time.deltaTime / GetFireRate());
                }

                RefreshStatusbarReload();

                if (nextTargetUpdate <= Time.time)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval + Random.Range(0.1f, 0.2f);
                    
                    if (targetType == ShootNearestTargetType.NoRotation)
                    {
                        //emitter.Play();
                    }
                    else
                    {
                        var nearest = ((Player) parent).GetNearestEnemy(GetPosition(), nearestTargetFilterFunc);

                        if (nearest != null && Utils.Dist2(nearest.GetPosition(), GetPosition()) < Mathf.Pow(stats.targetRange.GetValue(), 2))
                        {
                            currentTarget = nearest.transform;
                            //emitter.Play();
                        }
                        else
                        {
                            currentTarget = null;
                            //emitter.Pause();
                        }
                    }
                }
                
                //emitter.ApplyParams(stats, parent);
                
                if (currentTarget != null)
                {
                    if (reloadProgress >= 1)
                    {
                        reloadProgress = 0;
                        emitter.applyBulletParamsAction = () =>
                        {
                            emitter.ApplyParams(stats, parent, this);
                        };
                        emitter.Play();
                        
                        startReloadAtTime = Time.time + Math.Max(0, stats.shotsPerShot.GetValue()) * stats.projectileDelayBetweenConsecutiveShots.GetValue();
                    }
                    
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

        public override void OnShootInstruction(object source)
        {
            base.OnShootInstruction(source);
            
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