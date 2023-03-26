using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Defensive
{
    public class EffectAroundDefensiveModule : DefensiveModule
    {
        public List<ImmediateEffect> immediateEffects;
        public float effectStrength = 1;

        private bool activated = false;
        private WaitForSeconds waiter;

        public float damageInterval;
        
        public TargetType targetType;
        
        public override bool ActivateEffects()
        {
            if (!base.ActivateEffects()) return false;

            activated = true;
            waiter = new WaitForSeconds(damageInterval);
            StartCoroutine(UpdateCoroutine());

            return true;
        }

        public override bool DeactivateEffects()
        {
            if (!base.DeactivateEffects()) return false;

            activated = false;
            
            return true;
        }

        private IEnumerator UpdateCoroutine()
        {
            var buffer = new List<Collider2D>();
            var player = parent as Player;
            var col = player.body.damageAroundCollider;
            
            while (activated)
            {
                var colliders = Physics2D.OverlapCollider(col, new ContactFilter2D()
                {
                    useTriggers = true
                }, buffer);

                for (int i = 0; i < colliders; i++)
                {
                    var target = buffer[i].GetComponent<Entity>();
                    if (target == null) continue;

                    if (targetType == TargetType.EnemyOnly && !target.AreEnemies(player)) continue;
                    if (targetType == TargetType.FriendlyOnly && target.AreEnemies(player)) continue;
                
                    foreach (var effect in immediateEffects)
                    {
                        effect.Apply(target, target.GetPosition(), player, null, this, effectStrength, new ImmediateEffectParams());
                    }
                }

                yield return waiter;
            }
        }
    }
}