using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Area Effect", menuName = "Gama/Immediate Effects/Area Effect")]
    public class AreaEffect : ImmediateEffect
    {
        public float radius;

        [FormerlySerializedAs("damageDelay")] public float applyDelay;

        private Collider2D[] buffer = new Collider2D[2048];
        
        public List<ImmediateEffect> effects;

        public TargetType targetType;
        
        public AreaType areaType;

        [HideIf("targetType", AreaType.Circle)]
        public float angle;

        public bool excludeTargetEntity;

        [Range(0, 1)] public float chance = 1;
        
        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            Gamesystem.instance.StartCoroutine(Explode(data, strength));

            return true;
        }

        private IEnumerator Explode(EffectSourceData data, float strength)
        {
            if (applyDelay > 0)
            {
                yield return new WaitForSeconds(applyDelay);
            }

            if (data.sourceEntity == null) yield break;
            
            var targets = Utils.GetObjectsAtPosition(data.targetPosition, buffer, radius, data.sourceEntity.GetLayerMask(targetType));
            
            var sourcePosition = data.sourceEntity.GetPosition();

            for (int i = 0; i < targets; i++)
            {
                var coll = buffer[i];
                var entity = coll.gameObject.GetEntity();
                if (entity != null && (!excludeTargetEntity || entity != data.target))
                {
                    bool allow = false;

                    switch (areaType)
                    {
                        case AreaType.Circle:
                            allow = true;
                            break;
                        case AreaType.Cone:
                            var a = Math.Abs(Utils.AngleToTarget(data.sourceEntity.GetRotation(), sourcePosition, entity.GetPosition()));
                            allow = a <= angle;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    if(!allow) continue;
                    if(chance < 1 && Random.value > chance) continue;
                    
                    for (var index = 0; index < effects.Count; index++)
                    {
                        var effect = effects[index];
                        
                        var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                        effectData.target = entity;
                        effectData.targetPosition = entity.GetPosition();
                        effectData.sourceEntity = data.sourceEntity;
                        effectData.sourceItem = data.sourceItem;
                        effectData.sourceModule = data.sourceModule;
                        
                        effect.ApplyWithChanceCheck(effectData, strength, new ImmediateEffectParams());
                        
                        Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
                    }
                }
            }
        }
    }

    public enum AreaType
    {
        Circle,
        Cone
    }
}