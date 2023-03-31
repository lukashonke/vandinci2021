using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Scriptables
{
    public class UpgradeItem : SerializedScriptableObject
    {
        public string uiName;
        public string uiDescription;
        
        public List<EntityStatsEffect> effects;

        public Rarity rarity;

        public List<int> unlocksModulePrefabIds;
        
        public List<int> disablesModulePrefabIds;
        
        public List<int> replacesModulePrefabIds;

        public List<(string title, string value)> GetUiStats(int level)
        {
            List<(string title, string value)> retValue = new();

            foreach (var effect in effects)
            {
                var effectStats = effect.GetUiStats(level);
                if (effectStats != null)
                {
                    retValue.AddRange(effectStats);
                }
            }

            return retValue;
        }
        
        public void ApplyToPlayer(Player player)
        {
            if (player != null)
            {
                foreach (var effect in effects)
                {
                    effect.Apply(player, this, 1);
                }
            }
        }

        public void RemoveFromPlayer(Player player)
        {
            if (player != null)
            {
                foreach (var effect in effects)
                {
                    effect.Remove(player, this);
                }
            }
        }
    }
}