using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using BulletPro;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Pool;

namespace _Chi.Scripts.Mono.Entities
{
    public class DestructibleStructure : Npc
    {
        public BulletEmitter emitter;

        public bool canShoot;
        
        [ShowIf("canShoot")]
        public Projectile prefabProjectile;
        
        [ShowIf("canShoot")]
        public GameObject projectilePreview;

        [ShowIf("canShoot")]
        public float projectilePreviewDuration;
        
        [ShowIf("canShoot")]
        public float projectilePreviewLifetime;
        
        [ShowIf("canShoot")]
        public GameObject dropProjectile;

        [ShowIf("canShoot")]
        public float projectileLifetime;

        [ShowIf("canShoot")]
        public bool projectileRandomRotation;
        
        [ShowIf("canShoot")]
        public float dropProjectileLifetime;
        
        [ShowIf("canShoot")]
        public float projectileShootInterval;

        [ShowIf("canShoot")]
        public float distanceToPlayerToShoot;
        
        public override void Awake()
        {
            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            SetCanMove(false);

            StartCoroutine(UpdateJob());
        }

        public override bool CanBePushed() => false;

        private IEnumerator UpdateJob()
        {
            var waiter = new WaitForSeconds(0.2f);

            var nextShoot = Time.time + projectileShootInterval;

            while (isAlive)
            {
                var player = Gamesystem.instance.objects.currentPlayer;

                var dist = Utils.Dist2(GetPosition(), player.GetPosition());

                SetDistanceToPlayer(dist, player);

                if (canShoot)
                {
                    if(dist <= distanceToPlayerToShoot)
                    {
                        if (emitter != null)
                        {
                            var rotation = Utils.GetRotationTowards(emitter.gameObject.transform.position, player.GetPosition());
                            emitter.gameObject.transform.rotation = rotation;
                            emitter.Play();
                        }

                        if (prefabProjectile != null)
                        {
                            if (nextShoot < Time.time)
                            {
                                nextShoot = Time.time + projectileShootInterval;
                                
                                StartCoroutine(ShootProjectile(player.GetPosition()));
                            }
                        }
                    }
                    else
                    {
                        if (emitter != null)
                        {
                            emitter.Stop();
                        }
                    }
                }
                
                yield return waiter;
            }
        }

        private IEnumerator ShootProjectile(Vector3 target)
        {
            var spawnRotation = projectileRandomRotation ? Quaternion.Euler(0, 0, Random.Range(0, 360)) : Quaternion.identity;
            Vector2 spawnPosition = target;

            if (projectilePreview != null)
            {
                var previewProjectileInstance = Gamesystem.instance.poolSystem.SpawnPoolable(projectilePreview);
                previewProjectileInstance.MoveTo(spawnPosition);
                previewProjectileInstance.Rotate(spawnRotation);
                previewProjectileInstance.Run();
                Gamesystem.instance.Schedule(Time.time + projectilePreviewLifetime, () => Gamesystem.instance.poolSystem.Despawn(projectilePreview, previewProjectileInstance));
                
                yield return new WaitForSeconds(projectilePreviewDuration);
            }

            if (dropProjectile != null)
            {
                var dropProjectileInstance = Gamesystem.instance.poolSystem.SpawnPoolable(dropProjectile);
                dropProjectileInstance.MoveTo(spawnPosition);
                dropProjectileInstance.Rotate(spawnRotation);
                dropProjectileInstance.Run();
                Gamesystem.instance.Schedule(Time.time + dropProjectileLifetime, () => Gamesystem.instance.poolSystem.Despawn(dropProjectile, dropProjectileInstance));
                
                yield return new WaitForSeconds(dropProjectileLifetime);
            }

            var projectile = prefabProjectile.SpawnProjectile(this);
                    
            /*if (stats.projectileScale.GetValue() > 0)
            {
                projectile.SetScale(stats.projectileScale.GetValue() * projectileScaleToStatsScaleMultiplier);
            }*/

            var transform1 = projectile.transform;
            transform1.position = spawnPosition;
            transform1.rotation = spawnRotation;
                    
            projectile.ScheduleUnspawn(projectileLifetime);     
        }
    }
}