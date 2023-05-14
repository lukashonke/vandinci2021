using System;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Spawn Wawe Effect", menuName = "Gama/Immediate Effects/Spawn Wawe Effect")]
    public class SpawnWaweEffect : ImmediateEffect
    {
        public SpawnWaweData spawnData;

        [NonSerialized] private bool initialised = false;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (!initialised)
            {
                spawnData.Initialise(Time.time);
                initialised = true;
            }
            
            SpawnWawe.Spawn(spawnData, data.target.GetPosition(), Time.time, data.target, null);
            return true;
        }    
    }
}