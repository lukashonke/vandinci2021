using System.Collections;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Offensive  
{
    public class ShootNearestModule : OffensiveModule
    {
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
                    
                    var nearest = ((Player) parent).GetNearestEnemy(GetPosition());

                    if (nearest != null && Utils.Dist2(nearest.GetPosition(), GetPosition()) < Mathf.Pow(stats.targetRange.GetValue(), 2))
                    {
                        currentTarget = nearest.transform;
                        emitter.Play();
                    }
                    else
                    {
                        currentTarget = null;
                        emitter.Stop();
                    }

                    if (emitter.rootBullet != null)
                    {
                        emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, stats.projectileCount.GetValueInt());
                        emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpeed, stats.projectileSpeed.GetValue());
                        emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.WaitDuration, stats.fireRate.GetValue());

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
                    
                    RotateTowards(currentTarget.position, true);
                }
            }
        }

        //public override int? GetProjectileCount() => stats.projectileCount.GetValueInt();

        //public override float? GetProjectileForwardSpeed() => stats.projectileSpeed.GetValue();
    }
}