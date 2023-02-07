using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using BulletPro;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class OffensiveModule : Module, IBulletEmitterEntityParameters
    {
        [NonSerialized] public BulletEmitter emitter;

        public List<Transform> muzzles;
        private int lastMuzzle;

        public OffensiveModuleStats stats;
        
        public List<ImmediateEffect> effects;

        [ReadOnly] public List<(object, ImmediateEffect)> additionalEffects;

        public ParticleSystem shootVfx;

        public TargetType affectType;

        public TrailParameters trailParameters;

        protected bool activated;

        public override void Awake()
        {
            base.Awake();

            additionalEffects = new();
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
                if (muzzles != null && muzzles.Count > 0)
                {
                    lastMuzzle++;
                    if (lastMuzzle >= muzzles.Count) lastMuzzle = 0;
                    shootVfx.transform.position = muzzles[lastMuzzle].position;
                }
                
                shootVfx.Play();
            }
        }

        public virtual void OnShootInstruction()
        {
            
        }
    }

    [Serializable]
    public class TrailParameters
    {
        public bool useTrail;

        public Material material;

        public float trailLengthTime;
    }
}