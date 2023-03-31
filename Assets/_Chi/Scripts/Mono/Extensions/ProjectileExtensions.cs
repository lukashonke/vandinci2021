using System;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Statistics;
using _Chi.Scripts.Utilities;
using BulletPro;
using UnityEngine;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class ProjectileExtensions
    {
        public static Projectile SpawnProjectile(this Projectile prefab, Entity entity)
        {
            var projectile = Gamesystem.instance.poolSystem.Spawn(prefab);

            var hash = projectile.GetHashCode();

            projectile.Setup(entity);

            return projectile;
        }
        
        public static Projectile SpawnProjectile(this Projectile prefab, Module module)
        {
            var projectile = Gamesystem.instance.poolSystem.Spawn(prefab);

            var hash = projectile.GetHashCode();

            projectile.Setup(module);

            return projectile;
        }
        
        public static void ScheduleUnspawn(this Projectile projectile, float duration)
        {
            var hash = projectile.GetHashCode();
            
            Gamesystem.instance.Schedule(Time.time + duration, () => Gamesystem.instance.poolSystem.Despawn(projectile));
        }

        public static (Vector3 direction, Quaternion rotation) RotateAndDirectTowards(this Projectile projectile, Vector3 target, float angleAdd)
        {
            Vector3 pos = projectile.transform.position;
            Vector3 direction;
            
            var projectileRotation = Quaternion.Euler(new Vector3(0, 0, angleAdd));

            Quaternion newRotation = Utils.GetRotationTowards(pos, target);
            direction = (projectileRotation * (target - pos).normalized);
            
            return (direction, newRotation * projectileRotation);
        }
        
        public static bool CanFire(Vector3 target, Quaternion rotation, Vector3 position, float maxAngleToAim)
        {
            var angleToAttackTarget = Math.Abs(Utils.AngleToTarget(rotation, position, target));

            if (angleToAttackTarget > maxAngleToAim)
            {
                //Debug.Log($"Cannot fire! Angle {angleToAttackTarget}");
                return false;
            }

            return true;
        }
        
        public static void ApplyParams(this BulletEmitter emitter, OffensiveModuleStats stats, Entity entity, OffensiveModule module)
        {
            if (emitter.rootBullet != null)
            {
                int projectiles = stats.projectileCount.GetValueInt() * stats.projectileMultiplier.GetValueInt() + module.temporaryProjectilesUntilNextShot;
                
                emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, projectiles);
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpeed, stats.projectileSpeed.GetValue());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.WaitDuration, GetFireRate(entity, stats));
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpread, stats.projectileSpreadAngle.GetValue());
                
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileDelayBetweenConsecutiveShots, stats.projectileDelayBetweenConsecutiveShots.GetValue());

                emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileShotsPerShot, stats.shotsPerShot.GetValueInt());

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
        
        public static float GetFireRate(Entity entity, OffensiveModuleStats stats)
        {
            var retValue = stats.fireRate.GetValue();
            if (entity is Player player)
            {
                retValue *= player.stats.moduleFireRateMul.GetValue();
            }

            return retValue;
        }
    }
}