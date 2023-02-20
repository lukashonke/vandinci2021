using System;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
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

        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            if (!initialised)
            {
                spawnData.Initialise(Time.time);
                initialised = true;
            }
            
            SpawnWawe.Spawn(spawnData, target.GetPosition(), Time.time, target, null);
            return true;
        }    
    }
}