using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Modules.Offensive  
{
    public class ShootAtMouseModule : OffensiveModule
    {
        public override void Awake()
        {
            base.Awake();
        }

        public override bool ActivateEffects()
        {
            if (base.ActivateEffects())
            {
                //emitter.Play();
                return true;
            }
            return false;
        }

        public override bool DeactivateEffects()
        {
            if (base.DeactivateEffects())
            {
                //emitter.Stop();
                return true;
            }
            return false;
        }

        public override IEnumerator UpdateLoop()  
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.5f));
            
            Vector3 lastPosition = transform.position;
            
            var waiter = new WaitForFixedUpdate();

            float lastFire = 0;

            reloadProgress = 0f;

            while (activated && parent.CanShoot())
            {
                yield return waiter;
                
                var currentPosition = transform.position;

                float boost = 1.0f;
                
                var velocity = (currentPosition - lastPosition).magnitude / Time.deltaTime;
                if (velocity > 1)
                {
                    boost = stats.movingFireRateBoost.GetValue();
                }
                else if (velocity < 0.05f)
                {
                    boost = stats.stationaryFireRateBoost.GetValue();
                }
                
                if (startReloadAtTime < Time.time && isReloading)
                {
                    reloadProgress = Mathf.Min(1, reloadProgress + (Time.deltaTime * boost) / GetFireRate());
                }
                
                lastPosition = currentPosition;
                
                RefreshStatusbarReload();

                bool doFire = false;
                bool startReload = false;

                var magazineSize = stats.magazineSize.GetValueInt();
                
                if (reloadProgress >= 1)
                {
                    reloadProgress = 0;
                    
                    if (magazineSize > 0)
                    {
                        currentMagazine = magazineSize;
                    }

                    doFire = true;

                    if (magazineSize == 0)
                    {
                        startReload = true;
                    }

                    isReloading = false;
                }

                if (magazineSize > 0 && !isReloading)
                {
                    if (currentMagazine > 0)
                    {
                        doFire = lastFire + stats.fireRate.GetValue() < Time.time;

                        if (doFire)
                        {
                            currentMagazine--;
                        }
                    }

                    if (currentMagazine <= 0)
                    {
                        startReload = true;
                    }
                }
                
                if(magazineSize == 0 && !isReloading)
                {
                    doFire = lastFire + stats.fireRate.GetValue() < Time.time;
                }

                if (doFire)
                {
                    lastFire = Time.time;
                    emitter.applyBulletParamsAction = () =>
                    {
                        emitter.ApplyParams(stats, parent, this);
                    };
                    emitter.Play();
                    
                    if (magazineSize == 0)
                    {
                        startReload = true;
                    }
                }

                if (startReload)
                {
                    reloadProgress = 0;
                    startReloadAtTime = Time.time + Math.Max(0, stats.shotsPerShot.GetValue()) * stats.projectileDelayBetweenConsecutiveShots.GetValue();
                    isReloading = true;
                }

                RotateTowards(Utils.GetMousePosition());
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
}