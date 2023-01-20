using System.Collections;
using _Chi.Scripts.Statistics;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module
    {
        protected Collider2D[] buffer = new Collider2D[2048];

        public OffensiveModuleStats stats;

        protected bool activated;
        
        public override bool ActivateEffects()
        {
            if (!base.ActivateEffects() || activated) return false;
            activated = true;
            StartCoroutine(UpdateLoop());

            return true;
        }

        public override bool DeactivateEffects()
        {
            if (!base.DeactivateEffects() || !activated) return false;
            activated = false;

            return true;
        }

        public abstract IEnumerator UpdateLoop();
    }
}