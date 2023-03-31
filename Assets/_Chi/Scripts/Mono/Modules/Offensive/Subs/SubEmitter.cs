using System;
using System.Collections;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using BulletPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public abstract class SubEmitter : MonoBehaviour
    {
        [NonSerialized] public BulletEmitter emitter;

        [NonSerialized] public Module parentModule;

        [NonSerialized] public bool isEnabled;
        
        public virtual void Awake()
        {
            emitter = GetComponent<BulletEmitter>();
        }

        public virtual void Start()
        {
            parentModule = this.transform.parent.gameObject.GetModule();
            
            StartCoroutine(UpdateCoroutine());
        }

        public abstract IEnumerator UpdateCoroutine();

        public void Enable(bool b)
        {
            isEnabled = b;
        }

        public void ApplyParentParameters()
        {
            if (parentModule is OffensiveModule offensiveModule)
            {
                emitter.ApplyParams(offensiveModule.stats, parentModule.parent, offensiveModule);
            }
        }

        public void PlayEmitter(bool applyParentModuleParameters = true, bool triggerShootInstruction = true)
        {
            if (parentModule is OffensiveModule offensiveModule)
            {
                if (applyParentModuleParameters)
                {
                    ApplyParentParameters();
                }

                if (triggerShootInstruction)
                {
                    offensiveModule.OnShootInstruction(this);
                }
            }
            emitter.Play();
        }

        public void RotateRandomly()
        {
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            transform.rotation = rotation;
        }

        public virtual void OnParentShoot(object source)
        {
            
        }

        public virtual void OnSkillUse(Skill skill)
        {
            
        }
        
        public virtual void OnAfterSkillUse(Skill skill)
        {
            
        }
    }
}