using System;
using System.Collections;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Scriptables;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class EmitOnSkillUse : SubEmitter
    {
        private Vector3 lastPos;

        public int projectileMul = 1;

        public bool fireBeforeSkill;
        public bool fireAfterSkill;

        public Skill skillToUse;

        public override void OnSkillUse(Skill skill)
        {
            base.OnSkillUse(skill);

            if (fireBeforeSkill && (skillToUse == null || skillToUse == skill))
            {
                Emit();
            }
        }

        public override void OnAfterSkillUse(Skill skill)
        {
            base.OnAfterSkillUse(skill);

            if (fireAfterSkill && (skillToUse == null || skillToUse == skill))
            {
                Emit();
            }
        }

        private void Emit()
        {
            emitter.applyBulletParamsAction = () =>
            {
                ApplyParentParameters();
            
                if (emitter.rootBullet != null && parentModule is OffensiveModule offensiveModule)
                {
                    emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, projectileMul * offensiveModule.stats.projectileCount.GetValueInt() * offensiveModule.stats.projectileMultiplier.GetValueInt());
                }
            };
            
            PlayEmitter(applyParentModuleParameters: false);
        }

        public override IEnumerator UpdateCoroutine()
        {
            yield break;
        }
    }
}