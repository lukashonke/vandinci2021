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
    [Obsolete("Use DOTEffect instead")]
    public class FireEffect : ImmediateEffect
    {
        //public bool stackDuration;
        //public bool stackStrength;

        public GameObject vfxPrefab;

        public float interval;
        public int intervalsCount;
        public ImmediateEffect applyEffectOnInterval;
        public float applyEffectOnIntervalStrength;
        
        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (data.target != null && data.sourceEntity != null && data.target.AreEnemies(data.sourceEntity))
            {
                if(fires == null) fires = new Dictionary<Entity, FireData>();
                
                if (fires.ContainsKey(data.target))
                {
                    var fireData = fires[data.target];
                    fireData.strength += applyEffectOnIntervalStrength;
                    fireData.remainingIntervals = intervalsCount;
                    fires[data.target] = fireData;
                }
                else
                {
                    fires.Add(data.target, new FireData()
                    {
                        remainingIntervals = intervalsCount,
                        strength = applyEffectOnIntervalStrength
                    });

                    Schedule(data.target, data.sourceEntity, data.sourceItem, data.sourceModule);
                }
                
                if (vfxPrefab != null)
                {
                    data.target.AddVfx(vfxPrefab);
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
                    
                    var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                    effectData.sourceEntity = source;
                    effectData.sourceItem = sourceItem;
                    effectData.sourceModule = sourceModule;
                    effectData.target = target;
                    effectData.targetPosition = target.GetPosition();
                    
                    applyEffectOnInterval.ApplyWithChanceCheck(effectData, data.strength, new ImmediateEffectParams());
                    
                    Gamesystem.instance.poolSystem.ReturnEffectData(effectData);

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