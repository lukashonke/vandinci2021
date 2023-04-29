using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Affect Nearby", menuName = "Gama/Skills/Affect Nearby")]
    public class AffectNearbySkill : Skill
    {
        public List<ImmediateEffect> effects;

        public float baseRadius = 2f;

        public float effectStrength = 10;

        public float effectsDelay = 0.2f;

        public TargetType targetType;
        
        private Collider2D[] buffer = new Collider2D[100];

        public override bool Trigger(Entity entity, bool force = false)
        {
            bool usedExtraCharge = false;
            
            if (!force && !CanTrigger(entity, out usedExtraCharge)) return false;
            
            if (entity is Player player)
            {
                player.StartCoroutine(Run(player));

                if (!usedExtraCharge)
                {
                    SetNextSkillUse(entity, GetReuseDelay(player));
                }
                return true;
            }

            return false;
        }

        private IEnumerator Run(Player player)
        {
            SetActivated(player, true);

            SpawnPrefabVfx(player.GetPosition(), player.transform.rotation, null);
            player.OnSkillUse(this);

            if (effectsDelay > 0)
            {
                yield return new WaitForSeconds(effectsDelay);
            }
            
            var nearby = Physics2D.OverlapCircleNonAlloc(player.GetPosition(), baseRadius * player.stats.skillPowerMul.GetValue(), buffer, 1 << Layers.enemiesLayer | 1 << Layers.playersLayer);
            
            for (var i = 0; i < nearby; i++)
            {
                var target = buffer[i].GetComponent<Entity>();
                if (target == null) continue;

                if (targetType == TargetType.EnemyOnly && !target.AreEnemies(player)) continue;
                if (targetType == TargetType.FriendlyOnly && target.AreEnemies(player)) continue;
                
                foreach (var effect in effects)
                {
                    effect.ApplyWithChanceCheck(target, target.GetPosition(), player, null, null, effectStrength * player.stats.skillPowerMul.GetValue(), new ImmediateEffectParams());
                }
            }
            
            player.OnAfterSkillUse(this);
            
            SetActivated(player, false);
        }

        public override SkillData CreateDefaultSkillData()
        {
            return new AffectNearbySkillData();
        }
    }

    public class AffectNearbySkillData : SkillData
    {
        
    }
}