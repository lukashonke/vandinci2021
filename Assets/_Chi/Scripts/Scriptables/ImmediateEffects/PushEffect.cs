using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Push", menuName = "Gama/Immediate Effects/Push")]
    public class PushEffect : ImmediateEffect
    {
        public float basePush;

        public float pushDuration = 0.5f;

        public float setCannotBePushedFor = 0.5f;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (data.target == null) return false;
            
            var pushStrength = basePush;

            Vector3 source = data.target.GetPosition();
            if (data.sourceModule is OffensiveModule offensiveModule)
            {
                if (!forcedFlags.HasFlag(ImmediateEffectFlags.FixedDamage))
                {
                    pushStrength = offensiveModule.stats.projectilePushForce.GetValue();
                }

                source = offensiveModule.GetPosition();
            }
            else if (data.sourceEntity != null)
            {
                source = data.sourceEntity.GetPosition();
            }
            
            var sourcePush = pushStrength * strength;
            data.target.ReceivePush((data.target.GetPosition() - source).normalized * sourcePush, pushDuration);

            var target = data.target;

            Gamesystem.instance.Schedule(Time.time + pushDuration, () =>
            {
                target.StopRb();
            });
            
            if (setCannotBePushedFor > 0)
            {
                data.target.SetCannotBePushed(setCannotBePushedFor);
            }
            
            return true;
        }    
    }
}