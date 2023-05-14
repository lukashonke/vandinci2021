using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Pickup Magnet", menuName = "Gama/Immediate Effects/Pickup Magnet")]
    public class PickupMagnetEffect : ImmediateEffect
    {
        public float range;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            Gamesystem.instance.dropManager.Pickup(range);
            return true;
        }    
    }
}