using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Fire", menuName = "Gama/Immediate Effects/Fire")]
    public class FireEffect : ImmediateEffect
    {
        //public bool stackDuration;
        //public bool stackStrength;

        public GameObject vfxPrefab;

        public float interval;
        public int intervalsCount;
        public ImmediateEffect applyEffectOnInterval;
        public float applyEffectOnIntervalStrength;
        
        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            if (target != null && sourceEntity != null && target.AreEnemies(sourceEntity))
            {
                if(fires == null) fires = new Dictionary<Entity, FireData>();
                
                if (fires.ContainsKey(target))
                {
                    var fireData = fires[target];
                    fireData.strength += applyEffectOnIntervalStrength;
                    fireData.remainingIntervals = intervalsCount;
                    fires[target] = fireData;
                }
                else
                {
                    fires.Add(target, new FireData()
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
            if(fires == null) fires = new Dictionary<Entity, FireData>();
            
            Gamesystem.instance.Schedule(Time.time + interval, () =>
            {
                if (fires.TryGetValue(target, out var data))
                {
                    if (target == null || !target.activated || !target.isAlive)
                    {
                        fires.Remove(target);
                        return;
                    }
                    
                    if(data.remainingIntervals <= 0)
                    {
                        fires.Remove(target);
                        return;
                    }
                    
                    data.remainingIntervals--;
                    
                    fires[target] = data;
                    
                    applyEffectOnInterval.Apply(target, source, sourceItem, sourceModule, data.strength);

                    if (data.remainingIntervals > 0)
                    {
                        Schedule(target, source, sourceItem, sourceModule);
                    }
                    else
                    {
                        fires.Remove(target);
                        if (vfxPrefab != null)
                        {
                            target.RemoveVfx(vfxPrefab);
                        }
                    }
                }
            });
        }

        [NonSerialized] private Dictionary<Entity, FireData> fires;

        public struct FireData
        {
            public int remainingIntervals;

            public float strength;
        }
    }
}