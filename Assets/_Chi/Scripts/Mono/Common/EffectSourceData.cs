using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Mono.Modules.Offensive.Subs;
using _Chi.Scripts.Scriptables;
using BulletPro;
using UnityEngine;

namespace _Chi.Scripts.Mono.Common
{
    public class EffectSourceData
    {
        public Entity target;
        public Vector3 targetPosition;

        public bool hasKilledTarget;

        public Entity sourceEntity;
        public Module sourceModule;
        public Item sourceItem;
        public ImmediateEffect sourceEffect;

        public BulletBehavior sourceBullet;
        public BulletEmitter sourceEmitter;

        public void Cleanup()
        {
            target = null;
            sourceModule = null;
            sourceEntity = null;
            sourceEffect = null;
            sourceEmitter = null;
            sourceItem = null;
            sourceBullet = null;
            
            hasKilledTarget = false;
        }

        public void Copy(EffectSourceData data)
        {
            target = data.target;
            targetPosition = data.targetPosition;
            sourceEntity = data.sourceEntity;
            sourceModule = data.sourceModule;
            sourceItem = data.sourceItem;
            sourceEffect = data.sourceEffect;
            sourceEmitter = data.sourceEmitter;
            hasKilledTarget = data.hasKilledTarget;
            sourceBullet = data.sourceBullet;
        }
    }
}