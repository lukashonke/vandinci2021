using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
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

        public static (float damage, DamageFlags flags) CalculateEffectDamage(float baseEffectDamage, Entity target, Entity source)
        {
            var damage = baseEffectDamage;
            DamageFlags flags = DamageFlags.None;
            bool isCrit = false;
            if (source is Player player)
            {
                damage *= player.stats.dealtDamageMul.GetValue();
                
                isCrit = IsPlayerDamageCritical(player, damage);
                if (isCrit)
                {
                    damage = CalculateCriticalDamageMultiplier(player, damage);
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

        public static bool IsPlayerDamageCritical(Player player, float damage)
        {
            var chance = player.stats.baseCriticalRate.GetValue();

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
    }

    //TODO use
    public enum DamageType
    {
        Unknown
    }
}