using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using UnityEngine;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class DamageExtensions
    {
        public static float CalculateMonsterContactDamage(this Npc npc, Entity entity)
        {
            var dmg = npc.stats.contactDamage;
            
            if (entity is Player player)
            {
                dmg = ApplyPlayerReceivedDamage(player, dmg);
            }

            return dmg;
        }

        public static Vector3 CalculatePushForce(this Entity entity, Vector3 baseForce)
        {
            if (entity is Npc npc)
            {
                return baseForce * npc.stats.pushForceMultiplier;
            }

            return baseForce;
        }

        // used for pre-calculations and checks, not actual damage computation
        public static float CalculatePotentialModuleDamage(Module module, Player player, bool isModuleCritical, bool isPlayerCritical)
        {
            float damage = 0;
            
            if(module is OffensiveModule offensiveModule)
            {
                damage = offensiveModule.stats.projectileDamage.GetValue();
                damage *= player.stats.dealtDamageMul.GetValue();
                damage *= player.stats.nonCriticalDamageMul.GetValue();
                
                if (isModuleCritical)
                {
                    damage = CalculateModuleCriticalDamageMultiplier((OffensiveModule) module, damage);
                }
                else if(isPlayerCritical)
                {
                    damage = CalculateCriticalDamageMultiplier(player, damage);
                }
            }
            
            return damage;
        }

        public static (float damage, DamageFlags flags) CalculateEffectDamage(float baseEffectDamage, Entity target, Entity source, Module module, ImmediateEffectFlags immediateEffectFlags)
        {
            var damage = baseEffectDamage;
            DamageFlags flags = DamageFlags.None;
            if (source is Player player)
            {
                damage *= player.stats.dealtDamageMul.GetValue();

                bool moduleCritical = false;
                
                if (module is OffensiveModule offensiveModule)
                {
                    if (target is Npc npc)
                    {
                        var targetArmor = npc.stats.armor;
                        var targetArmorAfterNegation = Mathf.Min(0, targetArmor - offensiveModule.stats.armorPiercing.GetValue());
                        
                        //TODO mul damage when armored
                        
                        if (targetArmorAfterNegation > 0.99f)
                        {
                            damage *= offensiveModule.stats.armoredDamageMul.GetValue();
                        }
                        else
                        {
                            damage *= offensiveModule.stats.nonArmorDamageMul.GetValue();
                        }
                    }

                    if (offensiveModule.upgrades != null)
                    {
                        foreach (var moduleUpgradeItem in offensiveModule.upgrades)
                        {
                            if (moduleUpgradeItem.moduleEffects != null)
                            {
                                foreach (var statsEffect in moduleUpgradeItem.moduleEffects)
                                {
                                    damage = statsEffect.AlterEffectDamage(damage, player, target, module, immediateEffectFlags);
                                }
                            }
                        }
                    }
                    
                    moduleCritical = IsModuleDamageCritical(offensiveModule);
                }

                bool forcedModuleCritical = immediateEffectFlags.HasFlag(ImmediateEffectFlags.ForceModuleCritical);
                bool playerCritical = IsPlayerDamageCritical(player, damage);
                
                if (moduleCritical || playerCritical || forcedModuleCritical)
                {
                    if (moduleCritical || forcedModuleCritical)
                    {
                        damage = CalculateModuleCriticalDamageMultiplier((OffensiveModule) module, damage);
                    }
                    else
                    {
                        damage = CalculateCriticalDamageMultiplier(player, damage);
                    }
                    
                    flags |= DamageFlags.Critical;
                }
                else
                {
                    damage *= player.stats.nonCriticalDamageMul.GetValue();
                }
            }
            return (damage, flags);
        }

        public static float CalculateProjectileLifetime(float baseLifetime, Module module)
        {
            return baseLifetime;
        }

        public static float CalculateCriticalDamageMultiplier(Player player, float damage)
        {
            return damage * player.stats.criticalDamageMul.GetValue();
        }
        
        public static float CalculateModuleCriticalDamageMultiplier(OffensiveModule module, float damage)
        {
            return damage * module.stats.projectileCriticalDamage.GetValue();
        }

        public static bool IsPlayerDamageCritical(Player player, float damage)
        {
            var chance = player.stats.baseCriticalRate.GetValue();

            if (chance > 0)
            {
                return Random.value < chance;
            }

            return false;
        }

        public static bool IsModuleDamageCritical(OffensiveModule module)
        {
            var chance = module.stats.projectileCriticalRate.GetValue();

            if (chance > 0)
            {
                return Random.value < chance;
            }

            return false;
        }

        public static float ApplyPlayerReceivedDamage(Player player, float damage)
        {
            var skillUseReduceDamageDuration = player.stats.takeDamageFaterSkillUseDuration.GetValue();

            if (skillUseReduceDamageDuration > 0 &&
                (Time.time - player.lastSkillUseTime) < skillUseReduceDamageDuration)
            {
                damage *= player.stats.takeDamageAfterSkillUseMul.GetValue();
            }

            damage *= player.stats.receiveDamageMul.GetValue();
            
            damage += player.stats.receiveDamageAdd.GetValue();

            return damage;
        }

        public static bool CanTarget(this OffensiveModule module, Entity target)
        {
            if (module.targetAffectConditions.Count > 0)
            {
                foreach (var condition in module.targetAffectConditions)
                {
                    var canTarget = condition.Item2(target);
                    if (!canTarget)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    //TODO use
    public enum DamageType
    {
        Unknown
    }
}