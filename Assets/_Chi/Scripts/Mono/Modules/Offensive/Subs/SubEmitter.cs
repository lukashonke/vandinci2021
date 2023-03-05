using System;
using System.Collections;
using _Chi.Scripts.Mono.Extensions;
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

        public void PlayEmitter()
        {
            if (parentModule is OffensiveModule offensiveModule)
            {
                emitter.ApplyParams(offensiveModule.stats, parentModule.parent);
            }
            emitter.Play();
        }

        public void RotateRandomly()
        {
            Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            transform.rotation = rotation;
        }

        public virtual void OnParentShoot()
        {
            
        }
    }
}