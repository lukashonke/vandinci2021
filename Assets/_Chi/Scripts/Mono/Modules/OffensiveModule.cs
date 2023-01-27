using System;
using System.Collections;
using _Chi.Scripts.Statistics;
using BulletPro;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module
    {
        [NonSerialized] public BulletEmitter emitter;
        
        public OffensiveModuleStats stats;

        protected bool activated;

        public override void Awake()
        {
            base.Awake();

            emitter = GetComponent<BulletEmitter>();
        }

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