using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
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

        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            var pushStrength = basePush;
            
            Vector3 source = target.GetPosition();
            if (sourceModule is OffensiveModule offensiveModule)
            {
                pushStrength = offensiveModule.stats.projectilePushForce.GetValue();
                source = offensiveModule.GetPosition();
            }
            else if (sourceEntity != null)
            {
                source = sourceEntity.GetPosition();
            }
            
            var sourcePush = pushStrength * strength;
            target.ReceivePush((target.GetPosition() - source).normalized * sourcePush, pushDuration);
            if (setCannotBePushedFor > 0)
            {
                target.SetCannotBePushed(pushDuration);
            }
            
            return true;
        }    
    }
}