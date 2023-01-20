using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Damage", menuName = "Gama/Immediate Effects/Damage")]
    public class DamageEffect : ImmediateEffect
    {
        public float baseDamage;

        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem)
        {
            var dmg = DamageExtensions.CalculateEffectDamage(baseDamage, target, sourceEntity);
            target.ReceiveDamage(dmg, sourceEntity);
            return true;
        }    
    }
}