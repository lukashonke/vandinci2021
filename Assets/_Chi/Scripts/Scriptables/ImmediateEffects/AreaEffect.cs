using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

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

        public bool excludeTargetEntity;
        
        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (target == null) return false;
            
            Gamesystem.instance.StartCoroutine(Explode(target.GetPosition(), target, sourceEntity, sourceItem, sourceModule, strength));

            return true;
        }

        private IEnumerator Explode(Vector3 position, Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            if (applyDelay > 0)
            {
                yield return new WaitForSeconds(applyDelay);
            }

            if (sourceEntity == null) yield break;
            
            var targets = Utils.GetObjectsAtPosition(position, buffer, radius, sourceEntity.GetLayerMask(targetType));

            for (int i = 0; i < targets; i++)
            {
                var coll = buffer[i];
                var entity = coll.gameObject.GetEntity();
                if (entity != null && (!excludeTargetEntity || entity != target))
                {
                    for (var index = 0; index < effects.Count; index++)
                    {
                        var effect = effects[index];
                        effect.Apply(entity, sourceEntity, sourceItem, sourceModule, strength, new ImmediateEffectParams());
                    }
                }
            }
        }
    }
}