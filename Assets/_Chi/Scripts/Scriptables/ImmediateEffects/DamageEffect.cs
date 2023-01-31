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

        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            var sourceDamage = baseDamage;
            if (sourceModule is OffensiveModule offensiveModule)
            {
                sourceDamage = offensiveModule.stats.projectileDamage.GetValue();
            }
            
            var dmg = DamageExtensions.CalculateEffectDamage(sourceDamage, target, sourceEntity);
            target.ReceiveDamage(dmg, sourceEntity);
            return true;
        }    
    }
}