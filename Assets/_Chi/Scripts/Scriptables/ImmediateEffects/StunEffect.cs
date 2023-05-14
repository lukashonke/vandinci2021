using _Chi.Scripts.Mono.Common;
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
        //public bool stackDuration;
        
        public Material stunMaterial;

        public GameObject vfxPrefab;
        
        public override bool ApplyEffect(EffectSourceData data)
        {
            if (data.target != null && data.sourceEntity != null && data.target.AreEnemies(data.sourceEntity) && data.target.CanReceiveEffect(this))
            {
                if (setStunned)
                {
                    data.target.SetStunned(true);
                }
                
                if (setImmobilized)
                {
                    data.target.SetImmobilized(true);
                }

                if (Mathf.Abs(slowDownBy) > 0 && data.target is Npc npc)
                {
                    npc.AddToSpeed(-slowDownBy);
                }
            
                if (stunMaterial != null)
                {
                    data.target.SetTemporaryMaterial(stunMaterial);
                }

                if (vfxPrefab != null)
                {
                    data.target.AddVfx(vfxPrefab);
                }
                return true;
            }

            return false;
        }

        public override void RemoveEffect(EffectSourceData data)
        {
            if (data.target != null)
            {
                if (setStunned)
                {
                    data.target.SetStunned(false);
                }
                    
                if (setImmobilized)
                {
                    data.target.SetImmobilized(false);
                }
                    
                if (stunMaterial != null)
                {
                    data.target.ResetTemporaryMaterial(stunMaterial);
                }
                    
                if (vfxPrefab != null)
                {
                    data.target.RemoveVfx(vfxPrefab);
                }
                    
                if (Mathf.Abs(slowDownBy) > 0 && data.target is Npc npc)
                {
                    npc.AddToSpeed(slowDownBy);
                }
            }
        }
    }
}