using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Empty", menuName = "Gama/Skills/Empty")]
    public class EmptySkill : Skill
    {
        public override bool Trigger(Entity entity, bool force = false)
        {
            bool usedExtraCharge = false;            
            if (!force && !CanTrigger(entity, out usedExtraCharge)) return false;

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
                
                player.OnSkillUse(this);
                player.OnAfterSkillUse(this);
                return true;
            }

            return false;
        }

        public override SkillData CreateDefaultSkillData()
        {
            return new EmptySkillData();
        }
    }

    public class EmptySkillData : SkillData
    {
        
    }
}