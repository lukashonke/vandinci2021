using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;
using Random = System.Random;

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

        public float randomAngleMax;
        
        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (data.target != null && data.sourceEntity != null && data.target.AreEnemies(data.sourceEntity) && data.target.CanReceiveEffect(this))
            {
                if(fears == null) fears = new Dictionary<Entity, FearData>();
                
                data.target.SetFearing(true);
                data.target.SetFearEscapeAngle(UnityEngine.Random.Range(-randomAngleMax, randomAngleMax));
                
                if (fears.ContainsKey(data.target))
                {
                    var fireData = fears[data.target];
                    fireData.strength += applyEffectOnIntervalStrength;
                    fireData.remainingIntervals = intervalsCount;
                    fears[data.target] = fireData;
                }
                else
                {
                    fears.Add(data.target, new FearData()
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
                        var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                        effectData.sourceEntity = source;
                        effectData.sourceItem = sourceItem;
                        effectData.sourceModule = sourceModule;
                        effectData.target = target;
                        effectData.targetPosition = target.GetPosition();
                        
                        applyEffectOnInterval.ApplyWithChanceCheck(effectData, data.strength, new ImmediateEffectParams());
                        
                        Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
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