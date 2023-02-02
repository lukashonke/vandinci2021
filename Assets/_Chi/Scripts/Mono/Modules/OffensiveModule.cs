using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using BulletPro;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module, IBulletEmitterEntityParameters
    {
        [NonSerialized] public BulletEmitter emitter;
        
        public OffensiveModuleStats stats;
        
        public List<ImmediateEffect> effects;

        public ParticleSystem shootVfx;

        public TargetType affectType;

        protected bool activated;

        public override void Awake()
        {
            base.Awake();

            emitter = GetComponent<BulletEmitter>();
        }

        public override void Start()
        {
            base.Start();
        }

        public override bool ActivateEffects()
        {
            if (!base.ActivateEffects() || activated) return false;
            
            emitter.shootInstruction.AddListener(OnShootInstruction);
            
            activated = true;
            StartCoroutine(UpdateLoop());
            
            return true;
        }

        public override bool DeactivateEffects()
        {
            if (!base.DeactivateEffects() || !activated) return false;
            emitter.shootInstruction.RemoveListener(OnShootInstruction);
            activated = false;
            return true;
        }

        public abstract IEnumerator UpdateLoop();
        
        public void ShootEffect()
        {
            if (shootVfx != null)
            {
                shootVfx.Play();
            }
        }

        public virtual void OnShootInstruction()
        {
            
        }
    }
}