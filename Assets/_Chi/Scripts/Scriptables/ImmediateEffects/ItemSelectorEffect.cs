using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Item Selector", menuName = "Gama/Immediate Effects/Item Selector")]
    public class ItemSelectorEffect : ImmediateEffect
    {
        public TriggeredShop shop;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            shop.Show();
            return true;
        }    
    }
}