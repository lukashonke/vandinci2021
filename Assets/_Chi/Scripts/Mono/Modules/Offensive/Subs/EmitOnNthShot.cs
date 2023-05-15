using System;
using System.Collections;
using _Chi.Scripts.Mono.Common;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class EmitOnNthShot : SubEmitter
    {
        private Vector3 lastPos;

        public float projectileMul = 1;

        public int everyNth;

        [NonSerialized] private int currentShot = 0;

        public override void OnParentShoot(object source)
        {
            base.OnParentShoot(source);

            if (source == this || source is EmitByMovement em || source is EmitOnTarget || source is EmitOnProjectile || source is EmitOnNthShot) return;

            currentShot++;
            
            if (currentShot >= everyNth)
            {
                Emit();
                currentShot = 0;
            }
        }

        private void Emit()
        {
            if (parentModule is OffensiveModule offensiveModule)
            {
                int projectiles = (int) Math.Ceiling(projectileMul * offensiveModule.stats.projectileCount.GetValueInt() * offensiveModule.stats.projectileMultiplier.GetValueInt());
                
                emitter.applyBulletParamsAction = () =>
                {
                    ApplyParentParameters();
            
                    if (emitter.rootBullet != null)
                    {
                        emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, projectiles);
                    }
                };
            
                PlayEmitter(triggerShootInstruction: false, applyParentModuleParameters: false);

                for (int i = 0; i < projectileMul; i++)
                {
                    offensiveModule.OnShootInstruction(this);
                }
            }
        }

        public override IEnumerator UpdateCoroutine()
        {
            yield break;
        }
    }
}