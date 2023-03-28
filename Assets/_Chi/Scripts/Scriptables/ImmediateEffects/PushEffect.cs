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

        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (target == null) return false;
            
            var pushStrength = basePush;

            Vector3 source = target.GetPosition();
            if (sourceModule is OffensiveModule offensiveModule)
            {
                if (!forcedFlags.HasFlag(ImmediateEffectFlags.FixedDamage))
                {
                    pushStrength = offensiveModule.stats.projectilePushForce.GetValue();
                }

                source = offensiveModule.GetPosition();
            }
            else if (sourceEntity != null)
            {
                source = sourceEntity.GetPosition();
            }
            
            var sourcePush = pushStrength * strength;
            target.ReceivePush((target.GetPosition() - source).normalized * sourcePush, pushDuration);

            Gamesystem.instance.Schedule(Time.time + pushDuration, () =>
            {
                target.StopRb();
            });
            
            if (setCannotBePushedFor > 0)
            {
                target.SetCannotBePushed(setCannotBePushedFor);
            }
            
            return true;
        }    
    }
}