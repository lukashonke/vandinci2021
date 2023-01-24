using System.Collections;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _Chi.Scripts.Mono.Modules.Offensive
{
    public class CrossbowModule : OffensiveModule
    {
        public GameObject prefab;

        public override IEnumerator UpdateLoop()
        {
            var waiter = new WaitForFixedUpdate();

            float nextTargetUpdate = Time.time + targetUpdateInterval;

            float nextFireRate = Time.time + stats.fireRate;

            var prefabProjectile = prefab.GetComponent<Projectile>();
            
            while (activated && parent.CanShoot())
            {
                yield return waiter;

                if (nextTargetUpdate > Time.time)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval;
                    
                    var nearest = ((Player) parent).GetNearestEnemy(GetPosition());

                    if (nearest != null)
                    {
                        currentTarget = nearest.transform;
                    }
                    else
                    {
                        currentTarget = null;
                    }
                }

                if (currentTarget != null)
                {
                    if (nextFireRate < Time.time && ProjectileExtensions.CanFire(currentTarget.position, transform.rotation, transform.position, 5f))
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
                    }
                    
                    RotateTowards(currentTarget.position);
                }
            }
        }
    }
}