using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "DOT", menuName = "Gama/Immediate Effects/DOT")]
    public class DOTEffect : ImmediateEffect
    {
        //public bool stackDuration;
        //public bool stackStrength;

        public GameObject vfxPrefab;

        public float interval;
        public int intervalsCount;
        public ImmediateEffect applyEffectOnInterval;
        public float applyEffectOnIntervalStrength;

        public DOTType dotType;
        
        [NonSerialized] private Dictionary<Entity, DOTData> dots;
        
        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (data.target != null && data.sourceEntity != null && data.target.AreEnemies(data.sourceEntity))
            {
                if(dots == null) dots = new Dictionary<Entity, DOTData>();
                
                if (dots.ContainsKey(data.target))
                {
                    var fireData = dots[data.target];
                    fireData.strength += applyEffectOnIntervalStrength;
                    fireData.remainingIntervals = intervalsCount;
                    dots[data.target] = fireData;
                }
                else
                {
                    dots.Add(data.target, new DOTData()
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
            if(dots == null) dots = new Dictionary<Entity, DOTData>();
            
            Gamesystem.instance.Schedule(Time.time + interval, () =>
            {
                if (dots.TryGetValue(target, out var data))
                {
                    if (target == null || !target.activated || !target.isAlive)
                    {
                        dots.Remove(target);
                        return;
                    }
                    
                    if(data.remainingIntervals <= 0)
                    {
                        dots.Remove(target);
                        return;
                    }
                    
                    data.remainingIntervals--;
                    
                    dots[target] = data;
                    
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
                        dots.Remove(target);
                        if (vfxPrefab != null)
                        {
                            target.RemoveVfx(vfxPrefab);
                        }
                    }
                }
            });
        }

        public struct DOTData
        {
            public int remainingIntervals;

            public float strength;
        }
    }

    public enum DOTType
    {
        Poison,
        Acid,
        Plague,
        Fire
    }
}