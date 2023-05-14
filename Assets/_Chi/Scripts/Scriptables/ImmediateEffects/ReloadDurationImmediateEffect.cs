using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Reload Duration", menuName = "Gama/Immediate Effects/Reload Duration")]

    public class ReloadDurationImmediateEffect : ImmediateEffectWithDuration
    {
        public float value;
        public StatModifierType modifier;
        
        public override bool ApplyEffect(EffectSourceData data)
        {
            if (data.target is Player player)
            {
                player.stats.moduleReloadDurationMul.AddModifier(new StatModifier(data.sourceEntity, value, modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override void RemoveEffect(EffectSourceData data)
        {
            if (data.target is Player player)
            {
                player.stats.moduleReloadDurationMul.RemoveModifiersBySource(data.sourceEntity);
            }
        }
    }
}