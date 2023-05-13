using System.Collections;
using _Chi.Scripts.Mono.Common;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class EmitOnReload : SubEmitter
    {
        public int projectileMul = 1;

        public override void OnMagazineReload(Module module)
        {
            base.OnMagazineReload(module);

            Emit();
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