using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Move Speed", menuName = "Gama/Immediate Effects/Move Speed")]

    public class MoveSpeedImmediateEffect : ImmediateEffectWithDuration
    {
        public float value;
        public StatModifierType modifier;
        
        public override bool ApplyEffect(EffectSourceData data)
        {
            if (data.target is Player player)
            {
                player.stats.speed.AddModifier(new StatModifier(data.sourceEntity, value, modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override void RemoveEffect(EffectSourceData data)
        {
            if (data.target is Player player)
            {
                player.stats.speed.RemoveModifiersBySource(data.sourceEntity);
            }
        }
    }
}