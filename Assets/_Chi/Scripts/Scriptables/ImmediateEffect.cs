using System;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    public abstract class ImmediateEffect : SerializedScriptableObject
    {
        public StatOrders order = StatOrders.ImmediateEffect;
        
        public ImmediateEffectFlags forcedFlags = ImmediateEffectFlags.None;
        
        public abstract bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None);
    }

    [Flags]
    public enum ImmediateEffectFlags
    {
        None = 0,
        FixedDamage = 1 << 0,
        DamageFromModuleProjectileStrength = 1 << 1,
    }

    public struct ImmediateEffectParams
    {
        
    }
}