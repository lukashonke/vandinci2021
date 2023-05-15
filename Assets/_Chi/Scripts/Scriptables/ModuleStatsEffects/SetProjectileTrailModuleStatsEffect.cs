using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Set Projectile Trail", menuName = "Gama/Module Stats Effect/Set Projectile Trail")]
    public class SetProjectileTrailModuleStatsEffect : ModuleStatsEffect
    {
        public TrailParameters trailParameters;

        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.trailParameters.useTrail = trailParameters.useTrail;
                offensiveModule.trailParameters.material = trailParameters.material;
                offensiveModule.trailParameters.trailLengthTime = trailParameters.trailLengthTime;
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            //TODO backup, needed?
            return true;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                //("Change Bullet Behavior", $""),
            };
        }
    }
}