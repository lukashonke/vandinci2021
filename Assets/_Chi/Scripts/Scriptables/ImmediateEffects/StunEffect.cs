using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Stun", menuName = "Gama/Immediate Effects/Stun")]
    public class StunEffect : ImmediateEffectWithDuration
    {
        public bool setStunned;
        public bool setImmobilized;
        public float slowDownBy;
        public bool stackDuration;
        
        public Material stunMaterial;

        public GameObject vfxPrefab;
        
        public override bool ApplyEffect(Entity target, Entity source, Item sourceItem, Module sourceModule)
        {
            if (target != null && source != null && target.AreEnemies(source) && target.CanReceiveEffect(this) && target.AddImmediateEffect(this, duration, stackDuration))
            {
                if (setStunned)
                {
                    target.SetStunned(true);
                }
                
                if (setImmobilized)
                {
                    target.SetImmobilized(true);
                }

                if (Mathf.Abs(slowDownBy) > 0 && target is Npc npc)
                {
                    npc.AddToSpeed(-slowDownBy);
                }
            
                if (stunMaterial != null)
                {
                    target.SetTemporaryMaterial(stunMaterial);
                }

                if (vfxPrefab != null)
                {
                    target.AddVfx(vfxPrefab);
                }
                return true;
            }

            return false;
        }

        public override void RemoveEffect(Entity target, Entity source, Item sourceItem, Module sourceModule)
        {
            if (target != null)
            {
                var rescheduledUntil = target.TryRemoveImmediateEffect(this);
                if (rescheduledUntil > 0)
                {
                    ScheduleRemove(rescheduledUntil, target, source, sourceItem, sourceModule);
                }
                else if(rescheduledUntil > -1)
                {
                    if (setStunned)
                    {
                        target.SetStunned(false);
                    }
                    
                    if (setImmobilized)
                    {
                        target.SetImmobilized(false);
                    }
                    
                    if (stunMaterial != null)
                    {
                        target.ResetTemporaryMaterial(stunMaterial);
                    }
                    
                    if (vfxPrefab != null)
                    {
                        target.RemoveVfx(vfxPrefab);
                    }
                    
                    if (Mathf.Abs(slowDownBy) > 0 && target is Npc npc)
                    {
                        npc.AddToSpeed(slowDownBy);
                    }
                }
            }
        }
    }
}