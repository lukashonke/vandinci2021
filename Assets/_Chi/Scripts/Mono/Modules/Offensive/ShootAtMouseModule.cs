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
                emitter.Play();
                return true;
            }
            return false;
        }

        public override bool DeactivateEffects()
        {
            if (base.DeactivateEffects())
            {
                emitter.Stop();
                return true;
            }
            return false;
        }

        public override IEnumerator UpdateLoop()  
        {
            var waiter = new WaitForFixedUpdate();

            float nextTargetUpdate = Time.time + targetUpdateInterval;

            while (activated && parent.CanShoot())
            {
                yield return waiter;

                if (nextTargetUpdate > Time.time)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval + Random.Range(0.1f, 0.2f);

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

                RotateTowards(Utils.GetMousePosition());
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
}