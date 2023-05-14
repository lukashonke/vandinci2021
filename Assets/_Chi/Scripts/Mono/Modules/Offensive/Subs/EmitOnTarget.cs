using System.Collections;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using BulletPro;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class EmitOnTarget : SubEmitter
    {
        private Vector3 lastPos;

        public int projectileMul = 1;

        public EmitOnTargetType type;
        
        public EmitterProfile[] whiteListEmitters;
        
        public override void OnHitTarget(EffectSourceData data)
        {
            base.OnHitTarget(data);
            
            if(data.sourceModule != parentModule || data.sourceEmitter == null) return;
            
            bool canEmit = false;
            
            if(whiteListEmitters.Length > 0)
            {
                foreach (var e in whiteListEmitters)
                {
                    if (e == data.sourceEmitter.emitterProfile)
                    {
                        canEmit = true;
                    }
                }
            }
            else
            {
                canEmit = true;
            }

            if (!canEmit)
            {
                return;
            }
            
            if (data.hasKilledTarget && type == EmitOnTargetType.OnKill)
            {
                Emit(data.target);
            }
            else if (type == EmitOnTargetType.OnHitButNotKilled)
            {
                Emit(data.target);
            }
        }
        
        private void Emit(Entity target)
        {
            emitter.applyBulletParamsAction = () =>
            {
                ApplyParentParameters();
            
                if (emitter.rootBullet != null && parentModule is OffensiveModule offensiveModule)
                {
                    emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, projectileMul * offensiveModule.stats.projectileCount.GetValueInt() * offensiveModule.stats.projectileMultiplier.GetValueInt());
                    
                    emitter.rootBullet.moduleParameters.SetObjectReference(BulletVariables.IgnoreTarget1, target);
                }
            };
            
            var temp = emitter.patternOrigin;
            emitter.patternOrigin = target.transform;
            
            PlayEmitter(applyParentModuleParameters: false);
            
            emitter.patternOrigin = temp;
        }

        public override IEnumerator UpdateCoroutine()
        {
            yield break;
        }
    }

    public enum EmitOnTargetType
    {
        OnKill,
        OnHitButNotKilled
    }
}