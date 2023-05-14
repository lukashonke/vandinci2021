using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Heal", menuName = "Gama/Immediate Effects/Heal")]
    public class HealEffect : ImmediateEffect
    {
        public float baseHeal;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if(data.target == null) return false;
            data.target.Heal(baseHeal);
            return true;
        }    
    }
}