using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Misc;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class AffectRandomEnemy : SubEmitter
    {
        public GameObject dropProjectile;
        public float dropProjectileLifetime;

        public float dropProjectileRadius;

        public AffectRandomEnemyTriggerType triggerType;
        public AffectRandomEnemyTargetType targetType;

        public float maxDistance2ToTarget;
        
        private WaitForSeconds dropProjectileWaiter;

        private List<Entity> potentialTargets;

        public override void Awake()
        {
            base.Awake();

            potentialTargets = new();
            
            dropProjectileWaiter = new WaitForSeconds(dropProjectileLifetime);
        }

        public override IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                if (targetType == AffectRandomEnemyTargetType.RandomEnemy)
                {
                    if (parentModule.parent is Player player)
                    {
                        potentialTargets.Clear();
                        var playerPos = player.GetPosition();

                        foreach (var entity in player.targetableEnemies)
                        {
                            if (CanTarget(entity, player) && Utils.Dist2(entity.GetPosition(), playerPos) < maxDistance2ToTarget)
                            {
                                potentialTargets.Add(entity);
                            }
                        }
                    }
                }
            }
        }

        public override void OnParentShoot(object source)
        {
            base.OnParentShoot(source);
            
            if (triggerType == AffectRandomEnemyTriggerType.OnShoot)
            {
                if (parentModule.parent is Player player && potentialTargets.Count > 0 && parentModule is OffensiveModule offensiveModule)
                {
                    for (int i = 0; i < offensiveModule.stats.projectileCount.GetValueInt(); i++)
                    {
                        var randomTarget = potentialTargets[Random.Range(0, potentialTargets.Count)];

                        if (CanTarget(randomTarget, player))
                        {
                            Fire(randomTarget);
                        }
                    }
                }
            }
        }

        private bool CanTarget(Entity entity, Player player)
        {
            return entity is Npc npc && npc != null && npc.activated && npc.AreEnemies(player);
        }

        private void Fire(Entity target)
        {
            var spawnPosition = target.GetPosition();
            
            var dropProjectileInstance = Gamesystem.instance.poolSystem.SpawnPoolable(dropProjectile);
            dropProjectileInstance.MoveTo(spawnPosition + new Vector3(Random.Range(0, dropProjectileRadius), 0, 0));
            //dropProjectileInstance.Rotate(Quaternion.identity);
            dropProjectileInstance.Run();
            
            if (dropProjectileInstance is DropFromAbove dropFromAbove)
            {
                dropFromAbove.actionWhenDropped = () =>
                {
                    Affect(target, dropProjectileInstance);

                    Gamesystem.instance.poolSystem.Despawn(dropProjectile, dropProjectileInstance);
                };
            }
        }

        private void Affect(Entity target, IPoolable dropProjectileInstance)
        {
            if (target != null && target.activated && parentModule is OffensiveModule offensiveModule)
            {
                var effects = offensiveModule.effects;
                
                foreach (var effect in effects)
                {
                    effect.Apply(target, target.GetPosition(), parentModule.parent, null, parentModule, 1, new ImmediateEffectParams());
                }
                
                var additionalEffects = offensiveModule.additionalEffects;

                if (additionalEffects.Count > 0)
                {
                    ListPool<ImmediateEffect>.Get(out var list);

                    for (var index = 0; index < additionalEffects.Count; index++)
                    {
                        var effect = additionalEffects[index].Item2;

                        if (!list.Contains(effect))
                        {
                            effect.Apply(target, target.GetPosition(), parentModule.parent, null, parentModule, 1, new ImmediateEffectParams());
                            list.Add(effect);
                        } 
                    }
					
                    list.Clear();
                    ListPool<ImmediateEffect>.Release(list);
                }
                
                //offensiveModule.OnBulletEffectGiven(bullet, this, bulletWillDie: deactivate);
            }
        }
    }
    
    public enum AffectRandomEnemyTriggerType
    {
        OnShoot,
    }
    
    public enum AffectRandomEnemyTargetType
    {
        RandomEnemy,
    }
}