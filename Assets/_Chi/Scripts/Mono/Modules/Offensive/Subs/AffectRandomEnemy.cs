using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Misc;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class AffectRandomEnemy : SubEmitter
    {
        public GameObject dropProjectile;
        public float dropProjectileLifetime;

        public float dropProjectileRadius;

        public AffectRandomEnemyTriggerType triggerType;
        public AffectRandomEnemyTargetType targetType;
        public AffectCountType targetCountType;

        public float maxDistance2ToTarget;
        
        private WaitForSeconds dropProjectileWaiter;

        private List<Entity> potentialTargets;

        [Range(0, 1)] public float triggerChance = 1f;

        [ShowIf("triggerType", AffectRandomEnemyTriggerType.OnSkillUse)]
        public Skill trigeringSkill;

        [ShowIf("targetCountType", AffectCountType.Fixed)]
        public int hitTargets = 1;

        public ImmediateEffectFlags effectFlags;
        
        //TODO hp condition
        
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

                if (parentModule.parent is Player player)
                {
                    potentialTargets.Clear();
                    var playerPos = player.GetPosition();
                    
                    if (targetType == AffectRandomEnemyTargetType.RandomEnemy || targetType == AffectRandomEnemyTargetType.RandomEnemyWithHpLessThanDamage);
                    {
                        var maxHp = targetType == AffectRandomEnemyTargetType.RandomEnemyWithHpLessThanDamage 
                            ? DamageExtensions.CalculatePotentialModuleDamage(parentModule, player, true, false) 
                            : int.MaxValue;
                        
                        foreach (var entity in player.targetableEnemies)
                        {
                            if (CanTarget(entity, player) 
                                && Utils.Dist2(entity.GetPosition(), playerPos) < maxDistance2ToTarget 
                                && entity.entityStats.hp <= maxHp)
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
            
            if (triggerType == AffectRandomEnemyTriggerType.OnShoot && Random.Range(0f, 1f) < triggerChance)
            {
                Shoot();
            }
        }
        
        public override void OnSkillUse(Skill skill)
        {
            base.OnSkillUse(skill);

            if (triggerType == AffectRandomEnemyTriggerType.OnSkillUse && (trigeringSkill == null || trigeringSkill == skill) && Random.Range(0f, 1f) < triggerChance)
            {
                Shoot();
            }
        }
        
        private void Shoot()
        {
            if (parentModule.parent is Player player && potentialTargets.Count > 0 && parentModule is OffensiveModule offensiveModule)
            {
                var count = 0;

                switch (targetCountType)
                {
                    case AffectCountType.ProjectilesShot:
                        count = offensiveModule.stats.projectileCount.GetValueInt();
                        break;
                    case AffectCountType.Fixed:
                        count = hitTargets;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                List<Entity> hits = ListPool<Entity>.Get();
                
                hits.AddRange(potentialTargets);

                for (int i = 0; i < count; i++)
                {
                    if (hits.Count == 0)
                    {
                        break;
                    }
                    
                    var randomTarget = hits[Random.Range(0, hits.Count)];
                    
                    hits.Remove(randomTarget);
                    
                    if (CanTarget(randomTarget, player))
                    {
                        Fire(randomTarget);
                    }
                }
                
                ListPool<Entity>.Release(hits);
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
                    effect.ApplyWithChanceCheck(target, target.GetPosition(), parentModule.parent, null, parentModule, 1, new ImmediateEffectParams(), effectFlags);
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
                            effect.ApplyWithChanceCheck(target, target.GetPosition(), parentModule.parent, null, parentModule, 1, new ImmediateEffectParams(), effectFlags);
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
        OnSkillUse
    }
    
    public enum AffectRandomEnemyTargetType
    {
        RandomEnemy,
        RandomEnemyWithHpLessThanDamage
    }

    public enum AffectCountType
    {
        ProjectilesShot,
        Fixed
    }
}