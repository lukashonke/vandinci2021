using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Damage", menuName = "Gama/Immediate Effects/Damage")]
    public class DamageEffect : ImmediateEffect
    {
        public float baseDamage;

        public Color? damageTextColor;

        public float damageMul = 1.0f;
        
        public DamageType damageType;

        public override bool Apply(EffectSourceData data, float defaultStrength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (data.target == null) return false;
            
            flags |= forcedFlags;
            
            var sourceDamage = baseDamage * defaultStrength;

            if (data.sourceModule is OffensiveModule offensiveModule)
            {
                if (!flags.HasFlag(ImmediateEffectFlags.FixedDamage) && effect == ImmediateEffectType.Damage)
                {
                    sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
                }
            }
            
            /*if (flags.HasFlag(ImmediateEffectFlags.DamageFromModuleProjectileStrength))
            {
                sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
            }*/
            
            sourceDamage *= damageMul;

            if (damageType == DamageType.PercentOfMaxHp)
            {
                sourceDamage = data.target.entityStats.maxHp * sourceDamage;
            }
            
            var dmgWithFlags = DamageExtensions.CalculateEffectDamage(sourceDamage, data.target, data.sourceEntity, data.sourceModule, flags);
            data.target.ReceiveDamage(dmgWithFlags.damage, data.sourceEntity, dmgWithFlags.flags, damageTextColor, data);
            return true;
        }    
    }

    public enum DamageType
    {
        Fixed,
        PercentOfMaxHp
    }
}