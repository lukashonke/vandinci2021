using System;
using System.Collections;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Utilities;
using BulletPro;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class EmitOnProjectile : SubEmitter
    {
        public float projectileMul = 1;
        
        public EmitterProfile[] whiteListEmitters;

        public EmitOnProjectileType type;

        public float delay;

        public override void OnBulletDeath(OffensiveModule offensiveModule, Bullet bullet, BulletBehavior behavior)
        {
            base.OnBulletDeath(offensiveModule, bullet, behavior);
            
            bool canEmit = false;
            
            if(whiteListEmitters.Length > 0)
            {
                foreach (var e in whiteListEmitters)
                {
                    if (e == bullet.emitter.emitterProfile)
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

            if (type != EmitOnProjectileType.OnProjectileDeath) return;
            if(offensiveModule != parentModule) return;

            if (delay > 0.01f)
            {
                StartCoroutine(EmitCoroutine(bullet.transform.position, offensiveModule.parent.transform));
            }
            else
            {
                Emit(bullet.transform.position, offensiveModule.parent.transform);    
            }
        }
        
        private IEnumerator EmitCoroutine(Vector3 position, Transform target)
        {
            yield return new WaitForSeconds(delay);
            
            Emit(position, target);
        }

        private void Emit(Vector3 position, Transform target)
        {
            emitter.applyBulletParamsAction = () =>
            {
                ApplyParentParameters();
            
                if (emitter.rootBullet != null && parentModule is OffensiveModule offensiveModule)
                {
                    int projectileCount = (int) Math.Ceiling(projectileMul * offensiveModule.stats.projectileCount.GetValueInt() * offensiveModule.stats.projectileMultiplier.GetValueInt());
                    
                    emitter.rootBullet.moduleParameters.SetInt(BulletVariables.ProjectileCount, projectileCount);
                    
                    //emitter.rootBullet.moduleParameters.SetObjectReference(BulletVariables.IgnoreTarget1, target);
                }
            };
            
            var temp = emitter.patternOrigin;
            
            emitter.patternOrigin = transform;
            emitter.patternOrigin.SetPositionAndRotation(position, Utils.GetRotationTowards(position, target.position));
            
            PlayEmitter(applyParentModuleParameters: false);
            
            //transform.SetPositionAndRotation(backupPosition, backupRotation);
            
            emitter.patternOrigin = temp;
        }

        public override IEnumerator UpdateCoroutine()
        {
            yield break;
        }
    }

    public enum EmitOnProjectileType
    {
        OnProjectileDeath
    }
}