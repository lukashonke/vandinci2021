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

        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float defaultStrength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (target == null) return false;
            
            flags |= forcedFlags;
            
            var sourceDamage = baseDamage * defaultStrength;

            if (sourceModule is OffensiveModule offensiveModule)
            {
                if (!flags.HasFlag(ImmediateEffectFlags.FixedDamage) && !effect.HasValue)
                {
                    sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
                }
            }
            
            /*if (flags.HasFlag(ImmediateEffectFlags.DamageFromModuleProjectileStrength))
            {
                sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
            }*/
            
            sourceDamage *= damageMul;
            
            var dmgWithFlags = DamageExtensions.CalculateEffectDamage(sourceDamage, target, sourceEntity, sourceModule, flags);
            target.ReceiveDamage(dmgWithFlags.damage, sourceEntity, dmgWithFlags.flags, damageTextColor);
            return true;
        }    
    }
}