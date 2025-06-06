﻿using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Continuous Buff", menuName = "Gama/Skills/Continuous Buff")]
    public class ContinuousBuff : Skill
    {
        public List<EntityStatsEffect> effectsWhileActive;
        
        public List<ImmediateEffect> receivedEffectsWhileActive;
        public float receiveEffectsInterval;
        public bool firstReceiveEffectImmediately;
        public float receivedEffectsStrength;

        public int effectsLevel = 1;

        public override bool Trigger(Entity entity, bool force = false)
        {
            bool usedExtraCharge = false;            
            if (!force && !CanTrigger(entity, out usedExtraCharge)) return false;

            if (entity is Player player)
            {
                player.StartCoroutine(Run(player));

                if (!usedExtraCharge)
                {
                    SetNextSkillUse(entity, GetReuseDelay(player));
                }
                else
                {
                    OnUseExtraCharge(entity, GetReuseDelay(player));
                }
                return true;
            }

            return false;
        }

        private IEnumerator Run(Player player)
        {
            Debug.Log("start");
            
            SetActivated(player, true);
            player.OnSkillUse(this);
            
            var vfxInstance = SpawnPrefabVfx(player.GetPosition(), player.transform.rotation, null);
            vfxInstance.transform.SetParent(player.transform);
            
            foreach (var effect in effectsWhileActive)
            {
                effect.Apply(player, this, effectsLevel);
            }

            float nextReceiveEffect = firstReceiveEffectImmediately ? Time.time : Time.time + receiveEffectsInterval;
            
            while (player.IsStillActivated(this))
            {
                if (nextReceiveEffect <= Time.time)
                {
                    nextReceiveEffect = Time.time + receiveEffectsInterval;
                    foreach (var effect in receivedEffectsWhileActive)
                    {
                        var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                        effectData.target = player;
                        effectData.targetPosition = player.GetPosition();
                        
                        effect.ApplyWithChanceCheck(effectData, receivedEffectsStrength, new ImmediateEffectParams());
                        
                        Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
                    }
                }
                
                yield return null;
                
                Debug.Log("activated");
            }
            
            foreach (var effect in effectsWhileActive)
            {
                effect.Remove(player, this);
            }

            if (vfxInstance != null && vfxDespawnAfter <= 0)
            {
                Gamesystem.instance.poolSystem.DespawnGo(vfx, vfxInstance);
            }
            
            player.OnAfterSkillUse(this);
            SetActivated(player, false);
            
            Debug.Log("end");
        }

        public override SkillData CreateDefaultSkillData()
        {
            return new OverdriveSkillData();
        }
    }

    public class OverdriveSkillData : SkillData
    {
    }
}