using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using BulletPro;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module, IBulletEmitterEntityParameters
    {
        [NonSerialized] public BulletEmitter emitter;
        
        public OffensiveModuleStats stats;
        
        public List<ImmediateEffect> effects;

        public TargetType affectType;

        protected bool activated;

        public override void Awake()
        {
            base.Awake();

            emitter = GetComponent<BulletEmitter>();
            emitter.entityParameters = this;
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
        public virtual int? GetProjectileCount()
        {
            return null;
        }

        public virtual float? GetProjectileForwardSpeed()
        {
            //TODO derive in Crossbow and use it in the emitter 
            return null;
        }

        public float? GetProjectileFireInterval()
        {
            return null;
        }
    }
}