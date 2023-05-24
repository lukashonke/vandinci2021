using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Immediate Effects Skill", menuName = "Gama/Skills/Immediate Effects Skill")]
    public class ImmediateEffectsSkill : Skill
    {
        public List<ImmediateEffect> effects;

        public float effectStrength = 10;

        public override bool Trigger(Entity entity, bool force = false)
        {
            bool usedExtraCharge = false;
            
            if (!force && !CanTrigger(entity, out usedExtraCharge)) return false;
            
            entity.OnSkillUse(this);
            
            foreach (var effect in effects)
            {
                var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                effectData.target = entity;
                effectData.targetPosition = entity.GetPosition();
                effectData.sourceEntity = entity;
                
                effect.ApplyWithChanceCheck(effectData, effectStrength, new ImmediateEffectParams());
                
                Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
            }
            
            if (entity is Player player)
            {
                if (!usedExtraCharge)
                {
                    SetNextSkillUse(entity, GetReuseDelay(player));
                }
                else
                {
                    OnUseExtraCharge(entity, GetReuseDelay(player));
                }
                return true;
            }
            
            entity.OnAfterSkillUse(this);

            return false;
        }


        public override SkillData CreateDefaultSkillData()
        {
            return new ImmediateEffectsSkillData();
        }
    }

    public class ImmediateEffectsSkillData : SkillData
    {
        
    }
}