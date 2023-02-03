using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables.ImmediateEffects;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Projectile Increase Push Force", menuName = "Gama/Module Stats Effect/Projectile Increase Push Force")]
    public class ProjectileIncreasePushForceEffect : ModuleStatsEffect
    {
        public ImmediateEffect applyPushEffectIfNotPresent;
        
        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                if (applyPushEffectIfNotPresent != null)
                {
                    bool hasPushEffect = offensiveModule.effects.Any(e => e is PushEffect);
                    if (!hasPushEffect)
                    {
                        offensiveModule.additionalEffects.Add((this, applyPushEffectIfNotPresent));
                    }
                }
                
                offensiveModule.stats.projectilePushForce.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                if (applyPushEffectIfNotPresent != null)
                {
                    if (offensiveModule.additionalEffects.Contains((this, applyPushEffectIfNotPresent)))
                    {
                        offensiveModule.additionalEffects.Remove((this, applyPushEffectIfNotPresent));
                    }
                }
                offensiveModule.stats.projectilePushForce.RemoveModifiersBySource(source);
                return true;
            }

            return false;
        }
        
        
    }
}