using System;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class ProjectileExtensions
    {
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
    }
}