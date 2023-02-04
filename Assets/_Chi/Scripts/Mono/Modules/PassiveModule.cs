using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Scriptables;
using NotImplementedException = System.NotImplementedException;

namespace _Chi.Scripts.Mono.Modules
{
    public class PassiveModule : Module
    {
        public List<ModuleStatsEffect> effects;

        private IEnumerator NextFrame(Action action)
        {
            yield return null;

            action();
        }

        public override bool ActivateEffects()
        {
            if (!base.ActivateEffects()) return false;

            StartCoroutine(NextFrame(() =>
            {
                if (slot != null)
                {
                    foreach (var moduleSlot in slot.connectedTo)
                    {
                        if (moduleSlot.currentModule != null)
                        {
                            foreach (var effect in effects)
                            {
                                effect.Apply(moduleSlot.currentModule, this, level);
                            }
                        }
                    }
                }
            }));

            return true;
        }

        public override bool DeactivateEffects()
        {
            if (!base.DeactivateEffects()) return false;
            if (slot != null)
            {
                foreach (var moduleSlot in slot.connectedTo)
                {
                    if (moduleSlot.currentModule != null)
                    {
                        foreach (var effect in effects)
                        {
                            effect.Remove(moduleSlot.currentModule, this);
                        }
                    }
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