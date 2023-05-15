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
        
        public ImmediateEffectType effect;
        
        public ImmediateEffectFlags forcedFlags = ImmediateEffectFlags.None;

        [Range(0,1)]
        public float chance = 1.0f;

        public bool ApplyWithChanceCheck(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if(chance < 1.0f && UnityEngine.Random.value > chance)
            {
                return false;
            }
            
            return Apply(data, strength, parameters, flags);
        }
        
        public abstract bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None);
    }

    [Flags]
    public enum ImmediateEffectFlags
    {
        None = 0,
        FixedDamage = 1 << 0,
        DamageFromModuleProjectileStrength = 1 << 1,
        ForceModuleCritical = 1 << 2,
    }

    public struct ImmediateEffectParams
    {
        
    }
}