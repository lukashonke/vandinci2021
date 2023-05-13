using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Mono.Modules.Offensive.Subs;
using _Chi.Scripts.Scriptables;
using BulletPro;

namespace _Chi.Scripts.Mono.Common
{
    public class EffectSourceData
    {
        public Entity target;

        public bool hasKilledTarget;


        public Entity sourceEntity;
        public Module sourceModule;
        public Item sourceItem;
        public ImmediateEffect sourceEffect;

        public BulletEmitter sourceEmitter;

        public void Cleanup()
        {
            target = null;
            sourceModule = null;
            sourceEntity = null;
            sourceEffect = null;
            sourceEmitter = null;
            sourceItem = null;
            
            hasKilledTarget = false;
        }
    }
}