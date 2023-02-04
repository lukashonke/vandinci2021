using System.Collections.Generic;
using _Chi.Scripts.Scriptables;
using Unity.VisualScripting;

namespace _Chi.Scripts.Mono.Modules
{
    public class DefensiveModule : Module
    {
        public List<EntityStatsEffect> effects;
        
        public override bool ActivateEffects()
        {
            if (!base.ActivateEffects()) return false;
            
            if (parent != null)
            {
                foreach (var effect in effects)
                {
                    effect.Apply(parent, this, level);
                }
            }

            return true;
        }

        public override bool DeactivateEffects()
        {
            if (!base.DeactivateEffects()) return false;
            if (parent != null)
            {
                foreach (var effect in effects)
                {
                    effect.Remove(parent, this);
                }
            }
            
            return true;
        }

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
    }
}