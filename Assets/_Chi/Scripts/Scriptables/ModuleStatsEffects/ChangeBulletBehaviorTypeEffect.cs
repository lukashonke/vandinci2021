using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Change Bullet Behavior Type", menuName = "Gama/Module Stats Effect/Change Bullet Behavior Type")]
    public class ChangeBulletBehaviorTypeEffect : ModuleStatsEffect
    {
        public BulletBehaviorType newValue;

        [NonSerialized] private Dictionary<OffensiveModule, BulletBehaviorType> backups;

        public bool add;

        public override bool Apply(Module target, object source, int level)
        {
            if (backups == null) backups = new();
            
            if (target is OffensiveModule offensiveModule)
            {
                backups[offensiveModule] = offensiveModule.bulletBehavior;
                
                if (add) offensiveModule.bulletBehavior |= newValue;
                else offensiveModule.bulletBehavior = newValue;
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (backups == null) backups = new();
            
            if (target is OffensiveModule offensiveModule)
            {
                offensiveModule.bulletBehavior = backups[offensiveModule];
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            return new List<(string title, string value)>()
            {
                ("Change Bullet Behavior", $""),
            };
        }
    }
}