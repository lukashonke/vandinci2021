using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Recharge Weapon", menuName = "Gama/Immediate Effects/Recharge Weapon")]
    public class RechargeWeaponEffect : ImmediateEffect
    {
        public float addSeconds;
        public float addPercent;

        public bool replenishMagazine;

        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float defaultStrength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (sourceModule is OffensiveModule module)
            {
                if (addSeconds > 0)
                {
                    module.AddRechargeFireProgressTime(addSeconds * defaultStrength);
                }
                if(addPercent > 0)
                {
                    module.AddRechargeFireProgressPercent(addPercent * defaultStrength);
                }

                if (replenishMagazine)
                {
                    module.ReplenishMagazine();
                    
                    Gamesystem.instance.prefabDatabase.selfEffect.Spawn(targetPosition, "Reloaded!");
                }
            }
            return true;
        }   
    }
}