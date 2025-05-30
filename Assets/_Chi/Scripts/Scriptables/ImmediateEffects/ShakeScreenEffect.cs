﻿using _Chi.Scripts.Mono.Common;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Shake Screen", menuName = "Gama/Immediate Effects/Shake Screen")]
    public class ShakeScreenEffect : ImmediateEffect
    {
        public string preset;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            ProCamera2DShake.Instance.Shake(preset);
            return true;
        }    
    }
}