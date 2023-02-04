using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Mutators
{
    [CreateAssetMenu(fileName = "Stats", menuName = "Gama/Mutators/Stats")]
    public class StatsMutator : Mutator
    {
        public List<EntityStatsEffect> effects;
        
        public override List<(string title, string value)> GetUiStats(int level)
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
        
        public override void ApplyToPlayer(Player player)
        {
            if (player != null)
            {
                foreach (var effect in effects)
                {
                    effect.Apply(player, this, 1);
                }
            }
        }

        public override void RemoveFromPlayer(Player player)
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