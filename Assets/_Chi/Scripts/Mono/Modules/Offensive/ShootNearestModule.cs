using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
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
            yield return new WaitForSeconds(Random.Range(0.05f, 0.5f));
            
            Vector3 lastPosition = transform.position;

            float nextTargetUpdate = Time.time + targetUpdateInterval;
            
            float lastFire = 0;

            reloadProgress = 0f;

            //float nextFireRate = Time.time + stats.fireRate;

            //var prefabProjectile = prefab.GetComponent<Projectile>();
            
            while (activated && parent.CanShoot())
            {
                yield return null;
                
                var currentPosition = transform.position;

                float boost = 1.0f;

                if (parent is Player player)
                {
                    if (player.IsMoving())
                    {
                        boost = stats.movingReloadDurationBoost.GetValue();    
                    }
                    else
                    {
                        boost = stats.stationaryReloadDurationBoost.GetValue();
                    }
                }
                
                if (startReloadAtTime < Time.time && isReloading)
                {
                    reloadProgress = Mathf.Min(1, reloadProgress + ((Time.deltaTime * boost) / GetReloadDuration()));
                }
                
                lastPosition = currentPosition;

                RefreshStatusbarReload();
                
                bool canFire = false;
                bool startReload = false;

                var magazineSize = stats.magazineSize.GetValueInt();
                
                if (reloadProgress >= 1)
                {
                    reloadProgress = 0;
                    
                    if (magazineSize > 0)
                    {
                        currentMagazine = magazineSize + temporaryProjectilesForNextMagazine;
                        temporaryProjectilesForNextMagazine = 0;
                    }

                    canFire = true;

                    if (magazineSize == 0)
                    {
                        startReload = true;
                    }

                    isReloading = false;
                    
                    OnMagazineReload();
                }

                if (magazineSize > 0 && !isReloading)
                {
                    if (currentMagazine > 0)
                    {
                        canFire = lastFire + GetFireRate() < Time.time;
                    }

                    if (currentMagazine <= 0)
                    {
                        startReload = true;
                    }
                }
                
                if(magazineSize == 0 && !isReloading)
                {
                    canFire = lastFire + GetFireRate() < Time.time;
                }
                
                if (nextTargetUpdate <= Time.time)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval + Random.Range(0.1f, 0.2f);
                    
                    if (targetType == ShootNearestTargetType.NoRotation)
                    {
                        
                    }
                    else if (targetType == ShootNearestTargetType.RandomEnemy)
                    {
                        var nearest = ((Player) parent).GetRandomEnemy(GetPosition(), nearestTargetFilterFunc, Mathf.Pow(stats.targetRange.GetValue(), 2));

                        if (nearest != null)
                        {
                            currentTarget = nearest.transform;
                        }
                        else
                        {
                            currentTarget = null;
                        }
                    }
                    else
                    {
                        var nearest = ((Player) parent).GetNearestEnemy(GetPosition(), nearestTargetFilterFunc);

                        if (nearest != null && Utils.Dist2(nearest.GetPosition(), GetPosition()) < Mathf.Pow(stats.targetRange.GetValue(), 2))
                        {
                            currentTarget = nearest.transform;
                        }
                        else
                        {
                            currentTarget = null;
                        }
                    }
                }
                
                //emitter.ApplyParams(stats, parent);
                
                if (currentTarget != null)
                {
                    if (canFire)
                    {
                        lastFire = Time.time;
                        emitter.applyBulletParamsAction = () =>
                        {
                            emitter.ApplyParams(stats, parent, this);
                        };
                        emitter.Play();
                        
                        currentMagazine--;
                        
                        if (currentMagazine <= 0)
                        {
                            startReload = true;
                        }

                        if (magazineSize == 0)
                        {
                            startReload = true;
                        }
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
                
                if (startReload)
                {
                    if (Random.value < stats.instantReloadChance.GetValue())
                    {
                        Gamesystem.instance.prefabDatabase.selfEffect.Spawn(transform.position, "Reloaded!");
                        
                        reloadProgress = 1;
                        startReloadAtTime = Time.time;
                        isReloading = true;
                    }
                    else
                    {
                        reloadProgress = 0;
                        startReloadAtTime = Time.time + Math.Max(0, stats.shotsPerShot.GetValue()) * stats.projectileDelayBetweenConsecutiveShots.GetValue();
                        isReloading = true;
                    }
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
                ("Shoot Interval", $"{stats.reloadDuration.GetValue()}"),
                ("Projectiles", $"{stats.projectileCount.GetValue()}"),
            };
        }
    }

    public enum ShootNearestTargetType
    {
        Any,
        InFront,
        NoRotation,
        RandomEnemy
    }
}