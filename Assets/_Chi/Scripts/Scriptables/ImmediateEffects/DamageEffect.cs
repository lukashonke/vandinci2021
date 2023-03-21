using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Damage", menuName = "Gama/Immediate Effects/Damage")]
    public class DamageEffect : ImmediateEffect
    {
        public float baseDamage;

        public Effects? effect;

        public Color? damageTextColor;

        public float damageMul = 1.0f;

        public ImmediateEffectFlags forcedFlags = ImmediateEffectFlags.None;

        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float defaultStrength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            flags |= forcedFlags;
            
            var sourceDamage = baseDamage * defaultStrength;
            if (!flags.HasFlag(ImmediateEffectFlags.FixedDamage) && sourceModule is OffensiveModule offensiveModule && !effect.HasValue)
            {
                sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
            }

            /*if (flags.HasFlag(ImmediateEffectFlags.DamageFromModuleProjectileStrength))
            {
                sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
            }*/
            
            sourceDamage *= damageMul;
            
            var dmgWithFlags = DamageExtensions.CalculateEffectDamage(sourceDamage, target, sourceEntity);
            target.ReceiveDamage(dmgWithFlags.damage, sourceEntity, dmgWithFlags.flags, damageTextColor);
            return true;
        }    
    }
}