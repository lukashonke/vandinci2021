using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Fear", menuName = "Gama/Immediate Effects/Fear")]
    public class FearEffect : ImmediateEffect
    {
        [NonSerialized] private Dictionary<Entity, FearData> fears;
        
        //public bool stackDuration;
        //public bool stackStrength;

        public GameObject vfxPrefab;

        public float interval;
        public int intervalsCount;
        public ImmediateEffect applyEffectOnInterval;
        public float applyEffectOnIntervalStrength;
        
        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (target != null && sourceEntity != null && target.AreEnemies(sourceEntity))
            {
                if(fears == null) fears = new Dictionary<Entity, FearData>();
                
                target.SetFearing(true);
                
                if (fears.ContainsKey(target))
                {
                    var fireData = fears[target];
                    fireData.strength += applyEffectOnIntervalStrength;
                    fireData.remainingIntervals = intervalsCount;
                    fears[target] = fireData;
                }
                else
                {
                    fears.Add(target, new FearData()
                    {
                        remainingIntervals = intervalsCount,
                        strength = applyEffectOnIntervalStrength
                    });

                    Schedule(target, sourceEntity, sourceItem, sourceModule);
                }
                
                if (vfxPrefab != null)
                {
                    target.AddVfx(vfxPrefab);
                }
                return true;
            }

            return false;        
        }
        
        public void Schedule(Entity target, Entity source, Item sourceItem, Module sourceModule)
        {
            if(fears == null) fears = new Dictionary<Entity, FearData>();
            
            Gamesystem.instance.Schedule(Time.time + interval, () =>
            {
                if (fears.TryGetValue(target, out var data))
                {
                    if (target == null || !target.activated || !target.isAlive)
                    {
                        fears.Remove(target);
                        return;
                    }
                    
                    if(data.remainingIntervals <= 0)
                    {
                        fears.Remove(target);
                        target.SetFearing(false);
                        return;
                    }
                    
                    data.remainingIntervals--;
                    
                    fears[target] = data;

                    if (applyEffectOnInterval != null)
                    {
                        applyEffectOnInterval.Apply(target, target.GetPosition(), source, sourceItem, sourceModule, data.strength, new ImmediateEffectParams());
                    }

                    if (data.remainingIntervals > 0)
                    {
                        Schedule(target, source, sourceItem, sourceModule);
                    }
                    else
                    {
                        fears.Remove(target);
                        if (vfxPrefab != null)
                        {
                            target.RemoveVfx(vfxPrefab);
                        }
                        
                        target.SetFearing(false);
                    }
                }
            });
        }

        public struct FearData
        {
            public int remainingIntervals;

            public float strength;
        }
    }
}