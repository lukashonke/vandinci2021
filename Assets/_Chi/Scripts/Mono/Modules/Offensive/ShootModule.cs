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
    public class ShootModule : OffensiveModule
    {
        public ShootTargetType targetType;

        private Func<Entity, bool> nearestTargetFilterFunc;

        public override void Awake()
        {
            base.Awake();

            switch (targetType)
            {
                case ShootTargetType.InFront:
                    nearestTargetFilterFunc = entity => ProjectileExtensions.CanFire(entity.GetPosition(), slot.transform.rotation * originalRotation, transform.position, 90);
                    break;
                case ShootTargetType.NoRotation:
                    nearestTargetFilterFunc = entity => ProjectileExtensions.CanFire(entity.GetPosition(), slot.transform.rotation * originalRotation, transform.position, 15);
                    break;
            }
        }

        public override IEnumerator UpdateLoop()
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.5f));

            float nextTargetUpdate = Time.time + targetUpdateInterval;
            
            float lastFire = -100;
            
            if (stats.magazineSize.GetValueInt() > 0)
            {
                currentMagazine = stats.magazineSize.GetValueInt();
                temporaryProjectilesForNextMagazine = 0;
            }

            reloadProgress = 1f;

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

                RefreshStatusbarReload();
                
                bool canFire = false;
                bool startReload = false;

                var magazineSize = stats.magazineSize.GetValueInt();
                
                if (reloadProgress >= 1 && isReloading)
                {
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
                
                bool canSeekNewTarget = (targetType != ShootTargetType.NoRotation && targetType != ShootTargetType.AtMouse && targetType != ShootTargetType.AtMouseWhenMouseDown) 
                                       && 
                                       (nextTargetUpdate <= Time.time || (currentTargetEntity != null && !currentTargetEntity.isAlive));
                
                if (canSeekNewTarget)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval + Random.Range(0.1f, 0.2f);
                    
                    if (targetType == ShootTargetType.RandomEnemy)
                    {
                        var nearest = ((Player) parent).GetRandomEnemy(GetPosition(), nearestTargetFilterFunc, Mathf.Pow(stats.targetRange.GetValue(), 2));

                        if (nearest != null)
                        {
                            currentTarget = nearest.transform;
                            currentTargetEntity = nearest;
                        }
                        else
                        {
                            currentTarget = null;
                            currentTargetEntity = null;
                        }
                    }
                    else
                    {
                        var nearest = ((Player) parent).GetNearestEnemy(GetPosition(), nearestTargetFilterFunc);

                        if (nearest != null && Utils.Dist2(nearest.GetPosition(), GetPosition()) < Mathf.Pow(stats.targetRange.GetValue(), 2))
                        {
                            currentTarget = nearest.transform;
                            currentTargetEntity = nearest;
                        }
                        else
                        {
                            currentTarget = null;
                            currentTargetEntity = null;
                        }
                    }
                }
                
                //emitter.ApplyParams(stats, parent);
                
                var isFireRequested = currentTarget != null 
                                      || targetType == ShootTargetType.AtMouse 
                                      || targetType == ShootTargetType.AtMouseWhenMouseDown && parent is Player && ((Player) parent).IsFireRequested(this);

                bool hasFired = false;
                
                if (isFireRequested)
                {
                    if (canFire)
                    {
                        hasFired = true;
                        
                        lastFire = Time.time;
                        emitter.applyBulletParamsAction = () =>
                        {
                            emitter.ApplyParams(stats, parent, this);
                        };
                        emitter.Play();
                        
                        OnModuleFire();

                        bool consumeNoAmmo = false;
                        
                        if (parent is Player player2)
                        {
                            if (Random.value < stats.consumeNoAmmoChance.GetValue())
                            {
                                consumeNoAmmo = true;
                            }
                            else if (player2.IsMoving() && Random.value < stats.movingConsumeNoAmmoChance.GetValue())
                            {
                                consumeNoAmmo = true;
                            }
                            else if(!player2.IsMoving() && Random.value < stats.standingConsumeNoAmmoChance.GetValue())
                            {
                                consumeNoAmmo = true;
                            }
                        }

                        if (!consumeNoAmmo)
                        {
                            currentMagazine--;
                        }
                        
                        if (currentMagazine <= 0)
                        {
                            startReload = true;
                        }

                        if (magazineSize == 0)
                        {
                            startReload = true;
                        }
                    }
                    
                    if (targetType != ShootTargetType.NoRotation)
                    {
                        if (targetType == ShootTargetType.AtMouse || targetType == ShootTargetType.AtMouseWhenMouseDown)
                        {
                            RotateTowards(Utils.GetMousePosition());
                        }
                        else
                        {
                            RotateTowards(currentTarget.position, instantRotation);
                        }
                    }
                }
                else
                {
                    this.transform.localRotation = originalRotation;
                }
                
                if (startReload && hasFired)
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
                    
                    OnMagazineStartReload();
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

    public enum ShootTargetType
    {
        Any,
        InFront,
        NoRotation,
        RandomEnemy,
        AtMouse,
        AtMouseWhenMouseDown
    }
}