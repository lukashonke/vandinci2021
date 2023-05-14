using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Ignore Certain Targets", menuName = "Gama/Module Stats Effect/Ignore Certain Targets")]
    public class ProjectileIgnoreTargetsEffect : ModuleStatsEffect
    {
        public IgnoreCertainTargetsType ignoreType;
        
        [ShowIf("ignoreType", IgnoreCertainTargetsType.ImmediateEffectWithDuration)]
        public List<ImmediateEffectWithDuration> effectToIgnore;
        
        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.targetAffectConditions.Add((source, CanTargetFunc));
                return true;
            }
            
            return false;
        }

        private bool CanTargetFunc(Entity target)
        {
            if (ignoreType == IgnoreCertainTargetsType.ImmediateEffectWithDuration)
            {
                foreach (var effect in effectToIgnore)
                {
                    if (target.currentEffects.ContainsKey(effect))
                    {
                        return false;
                    }
                }
            }
            else if (ignoreType == IgnoreCertainTargetsType.Stunned)
            {
                if (target.IsStunned())
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.targetAffectConditions.RemoveAll(c => c.Item1 == source);
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                //("Projectile ", $"{AddLevelValueUI(value, level)}"),
            };
        }
    }

    public enum IgnoreCertainTargetsType
    {
        ImmediateEffectWithDuration,
        Stunned
    }
}