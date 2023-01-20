using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.EntityStatsEffects
{
    [CreateAssetMenu(fileName = "Max HP", menuName = "Gama/Stats Effect/Max HP")]
    public class MaxHpStatsEffect : EntityStatsEffect
    {
        public float value;

        public override bool Apply(Entity target, object source)
        {
            if (modifier == StatModifierType.Add)
            {
                target.entityStats.maxHpAdd += value;
            }
            else if (modifier == StatModifierType.Mul)
            {
                target.entityStats.maxHpMul += value;
            }
            else
            {
                Debug.LogError("No support for setting max HP directly.");
                return false;
            }

            return true;
        }

        public override bool Remove(Entity target, object source)
        {
            if (modifier == StatModifierType.Add)
            {
                target.entityStats.maxHpAdd -= value;
            }
            else if (modifier == StatModifierType.Mul)
            {
                target.entityStats.maxHpMul -= value;
            }
            else
            {
                Debug.LogError("No support for setting max HP directly.");
                return false;
            }

            return true;
        }
    }
}