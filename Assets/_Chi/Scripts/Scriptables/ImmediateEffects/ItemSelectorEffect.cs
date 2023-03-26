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

        public string uiTitle = "Treasure Chest";
        
        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            shop.Show(uiTitle);
            return true;
        }    
    }
}