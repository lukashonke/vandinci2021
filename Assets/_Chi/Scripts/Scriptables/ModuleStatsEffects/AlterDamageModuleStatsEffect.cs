using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Alter Damage", menuName = "Gama/Module Stats Effect/Alter Damage")]
    public class AlterDamageModuleStatsEffect : ModuleStatsEffect
    {
        public AlterDamageType type;
        
        [FormerlySerializedAs("damageByDistanceMultiplier")] [ShowIf("type", AlterDamageType.DamageByDistance)]
        public AnimationCurve damageByDistance2Multiplier;

        public override float AlterEffectDamage(float damage, Entity target, Entity source, Module module, ImmediateEffectFlags immediateEffectFlags)
        {
            if (type == AlterDamageType.DamageByDistance)
            {
                var distance = Utils.Dist2(target.GetPosition(), source.GetPosition());
                damage *= damageByDistance2Multiplier.Evaluate(distance);
                return damage;
            }

            return damage;
        }

        public override bool Apply(Module target, object source, int level)
        {
            return true;
        }

        public override bool Remove(Module target, object source)
        {
            return true;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            var ret = new List<(string title, string value)>();

            //ret.Add(("Add Effect", effect.name));

            return ret;
        }
    }

    public enum AlterDamageType
    {
        DamageByDistance
    }
}