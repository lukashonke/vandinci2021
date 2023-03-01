using System.Collections.Generic;
using _Chi.Scripts.Mono.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    /// <summary>
    /// an item that is applied to a module
    /// </summary>
    [CreateAssetMenu(fileName = "Module Upgrade Item", menuName = "Gama/Upgrade Items/Module")]
    public class ModuleUpgradeItem : UpgradeItem
    {
        public int modulePrefabId;
        
        public List<ModuleStatsEffect> moduleEffects;

        public int effectsLevel = 1;
        
        public bool ApplyEffects(Module module)
        {
            foreach (var effect in moduleEffects)
            {
                effect.Apply(module, this, effectsLevel);
            }

            return true;
        }

        public bool RemoveEffects(Module module)
        {
            foreach (var effect in moduleEffects)
            {
                effect.Remove(module, this);
            }
            
            return true;
        }
        
        public List<(string title, string value)> GetUiStats(int level)
        {
            List<(string title, string value)> retValue = new();

            foreach (var effect in moduleEffects)
            {
                var effectStats = effect.GetUiStats(level);
                if (effectStats != null)
                {
                    retValue.AddRange(effectStats);
                }
            }

            return retValue;
        }
    }
}