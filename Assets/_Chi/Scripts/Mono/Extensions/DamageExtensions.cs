using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class DamageExtensions
    {
        public static float CalculateMonsterContactDamage(this Npc npc, Entity entity)
        {
            return npc.stats.contactDamage;
        }

        public static float CalculateEffectDamage(float baseEffectDamage, Entity target, Entity source)
        {
            return baseEffectDamage;
        }

        public static float CalculateProjectileLifetime(float baseLifetime, Module module)
        {
            return baseLifetime;
        }
    }

    //TODO use
    public enum DamageType
    {
        Unknown
    }
}