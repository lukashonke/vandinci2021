using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Modules.Offensive  
{
    public class SpawnProjectilesModule : OffensiveModule
    {
        public SpawnProjectilesTargetType targetType;

        public Projectile prefabProjectile;
        public GameObject dropProjectile;
        [ShowIf("prefabProjectile")]
        public float dropProjectileLifetime;
        
        public float minDistanceFromModule = 1f;

        public float projectileScaleToStatsScaleMultiplier = 1f;

        public float projectileRandomDelayMax = 0.1f;

        public bool projectileRandomRotation;

        private Func<Entity, bool> nearestTargetFilterFunc;
        private WaitForSeconds dropProjectileWaiter;

        public override void Awake()
        {
            base.Awake();
            
            nearestTargetFilterFunc = entity => Utils.Dist2(entity.GetPosition(), transform.position) < Mathf.Pow(stats.targetRange.GetValue(), 2);
            dropProjectileWaiter = new WaitForSeconds(dropProjectileLifetime);
        }

        private IEnumerator Shoot(int projectileCount)
        {
            yield return new WaitForSeconds(Random.Range(0, projectileRandomDelayMax));
            
            List<Vector3> positions = ListPool<Vector3>.Get();
            List<Quaternion> rotations = ListPool<Quaternion>.Get();

            for (int i = 0; i < projectileCount; i++)
            {
                var spawnRotation = projectileRandomRotation ? Quaternion.Euler(0, 0, Random.Range(0, 360)) : Quaternion.identity;
                Vector2 spawnPosition;

                if (currentTarget != null)
                {
                    spawnPosition = currentTarget.position;
                }
                else
                {
                    spawnPosition = Utils.GetRandomPositionAround(transform.position, minDistanceFromModule, stats.projectileRange.GetValue());
                }
            
                if (dropProjectile != null)
                {
                    var dropProjectileInstance = Gamesystem.instance.poolSystem.SpawnPoolable(dropProjectile);
                    dropProjectileInstance.MoveTo(spawnPosition);
                    dropProjectileInstance.Rotate(spawnRotation);
                    dropProjectileInstance.Run();
                    Gamesystem.instance.Schedule(Time.time + dropProjectileLifetime, () => Gamesystem.instance.poolSystem.Despawn(dropProjectile, dropProjectileInstance));
                }
                
                positions.Add(spawnPosition);
                rotations.Add(spawnRotation);
            }

            if (dropProjectile != null)
            {
                yield return dropProjectileWaiter;
            }

            for (int i = 0; i < projectileCount; i++)
            {
                var projectile = prefabProjectile.SpawnProjectile(this);
                    
                if (stats.projectileScale.GetValue() > 0)
                {
                    projectile.SetScale(stats.projectileScale.GetValue() * projectileScaleToStatsScaleMultiplier);
                }

                var transform1 = projectile.transform;
                transform1.position = positions[i];
                transform1.rotation = rotations[i];
                    
                projectile.ScheduleUnspawn(DamageExtensions.CalculateProjectileLifetime(stats.projectileLifetime.GetValue(), this));     
            }
            
            ListPool<Vector3>.Release(positions);
            ListPool<Quaternion>.Release(rotations);
        }
        
        public override IEnumerator UpdateLoop()  
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.5f));
            
            var waiter = new WaitForFixedUpdate();

            float nextTargetUpdate = Time.time + targetUpdateInterval;

            float nextFire = Time.time + GetReloadDuration();

            //var prefabProjectile = prefab.GetComponent<Projectile>();
            
            while (activated && parent.CanShoot())
            {
                yield return waiter;

                if (nextTargetUpdate > Time.time)
                {
                    nextTargetUpdate = Time.time + targetUpdateInterval + Random.Range(0.1f, 0.2f);

                    if (targetType == SpawnProjectilesTargetType.RandomEnemyAroundPlayer)
                    {
                        var nearest = ((Player) parent).GetFirstEnemy(GetPosition(), nearestTargetFilterFunc);
                        if (nearest != null)
                        {
                            currentTarget = nearest.transform;
                        }
                        else
                        {
                            currentTarget = null;
                        }
                    }

                    ApplyParams();
                }
                
                if (nextFire < Time.time/* && ProjectileExtensions.CanFire(currentTarget.position, transform.rotation, transform.position, 5f)*/)
                {
                    nextFire = Time.time + GetReloadDuration();

                    for (int i = 0; i < stats.projectileCount.GetValueInt(); i++)
                    {
                        StartCoroutine(Shoot(1));
                    }

                    //yield return Shoot(stats.projectileCount.GetValueInt());
                }

                /*if (currentTarget != null)
                {
                    
                }
                else
                {
                    this.transform.localRotation = originalRotation;
                }*/
            }
        }

        private void ApplyParams()
        {
            if (hasEmitter && emitter.rootBullet != null)
            {
                emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, stats.projectileCount.GetValueInt() * stats.projectileMultiplier.GetValueInt());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpeed, stats.projectileSpeed.GetValue());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.WaitDuration, GetReloadDuration());
                emitter.rootBullet.moduleParameters.SetFloat(BulletVariables.ProjectileSpread, stats.projectileSpreadAngle.GetValue());
                
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

    public enum SpawnProjectilesTargetType
    {
        RandomPositionAroundPlayer,
        RandomEnemyAroundPlayer
    }
}