using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Jump", menuName = "Gama/Skills/Jump")]
    public class JumpSkill : Skill
    {
        public float jumpForce = 10;

        public float jumpLength;

        public ImmediateEffect shockwaveEffect;
        
        public override bool Trigger(Entity entity, bool force = false)
        {
            bool usedExtraCharge = false;            
            if (!force && !CanTrigger(entity, out usedExtraCharge)) return false;

            if (entity is Player player)
            {
                player.StartCoroutine(Jump(player));

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

        private float GetJumpDuration(Player player)
        {
            var length = jumpLength;

            foreach (var upgradeItem in player.skillUpgradeItems)
            {
                if (upgradeItem.target == this)
                {
                    if (upgradeItem.parameters != null && upgradeItem.parameters.TryGetValue("jumpLength", out var jumpLength))
                    {
                        length += jumpLength;
                    }
                }
            }

            return length;
        }

        public float GetJumpForce(Player player)
        {
            var force = jumpForce;

            foreach (var upgradeItem in player.skillUpgradeItems)
            {
                if (upgradeItem.target == this)
                {
                    if (upgradeItem.parameters != null && upgradeItem.parameters.TryGetValue("jumpForce", out var jumpForce))
                    {
                        force += jumpForce;
                    }
                }
            }

            return force;
        }

        public bool GetJumpCanPush(Player player)
        {
            foreach (var upgradeItem in player.skillUpgradeItems)
            {
                if (upgradeItem.target == this)
                {
                    if (upgradeItem.parameters != null && upgradeItem.parameters.TryGetValue("jumpNoPush", out var jumpNoPush))
                    {
                        if (jumpNoPush > 0.1f)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public float GetJumpShockwaveStrength(Player player)
        {
            float force = 0;

            foreach (var upgradeItem in player.skillUpgradeItems)
            {
                if (upgradeItem.target == this)
                {
                    if (upgradeItem.parameters != null && upgradeItem.parameters.TryGetValue("shockwave-strength", out var strength))
                    {
                        force += strength;
                    }
                }
            }

            return force;
        }
        
        public float GetJumpShockwaveRadius(Player player)
        {
            float shockwaveRadius = 0;

            foreach (var upgradeItem in player.skillUpgradeItems)
            {
                if (upgradeItem.target == this)
                {
                    if (upgradeItem.parameters != null && upgradeItem.parameters.TryGetValue("shockwave-radius", out var radius))
                    {
                        shockwaveRadius += radius;
                    }
                }
            }

            return shockwaveRadius;
        }

        private IEnumerator Jump(Player player)
        {
            SetActivated(player, true);

            SpawnPrefabVfx(player.GetPosition(), player.transform.rotation, null);
            player.OnSkillUse(this);

            var weight = player.rb.mass;
            weight = Math.Max(1, player.stats.weightMul.GetValue() * weight); // TODO make it better somehow
            
            var force = GetJumpForce(player) * player.stats.skillPowerMul.GetValue() * (1/(weight));
            
            var direction = (Utils.GetMousePosition() - player.GetPosition()).normalized;
            var jumpUntil = Time.time + GetJumpDuration(player);
            
            player.SetCanMove(false);
            player.SetCanReceiveDamage(false);
            player.SetCanDealPushDamage(GetJumpCanPush(player));
            var waiter = new WaitForFixedUpdate();

            while (jumpUntil >= Time.time)
            {
                player.rb.velocity = direction * force;
                
                //player.rb.MovePosition((Vector3) player.rb.position + (direction * jumpForce * Time.fixedDeltaTime));
                
                yield return waiter;
            }
            
            player.SetCanMove(true);
            player.SetCanReceiveDamage(true);
            player.SetCanDealPushDamage(true);
            
            player.rb.velocity = Vector2.zero;
            
            var shockwaveStrength = GetJumpShockwaveStrength(player);
            if (shockwaveStrength > 0.1f)
            {
                var shockwaveRadius = GetJumpShockwaveRadius(player);
                
                var count = EntityExtensions.GetNearest(player, player.GetPosition(), shockwaveRadius, TargetType.EnemyOnly, buffer);
                for (int i = 0; i < count; i++)
                {
                    var col = buffer[i];
                    var entity = col.gameObject.GetEntity();
                    if (entity is Npc npc && player.AreEnemies(npc))
                    {
                        if (npc.CanBePushed())
                        {
                            var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                            effectData.target = npc;
                            effectData.targetPosition = npc.GetPosition();
                            effectData.sourceEntity = player;
                            
                            shockwaveEffect.ApplyWithChanceCheck(effectData, shockwaveStrength, new ImmediateEffectParams());
                            
                            Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
                        }

                        var additionalEffects = GetAdditionalEffects(player);
                        if (additionalEffects != null)
                        {
                            foreach (var effect in additionalEffects)
                            {
                                var effectData = Gamesystem.instance.poolSystem.GetEffectData();
                                effectData.target = npc;
                                effectData.targetPosition = npc.GetPosition();
                                effectData.sourceEntity = player;
                                
                                effect.ApplyWithChanceCheck(effectData, 1, new ImmediateEffectParams());
                                
                                Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
                            }
                        }
                    }
                }
            }
            
            player.OnAfterSkillUse(this);
            
            SetActivated(player, false);
        }
        
        private Collider2D[] buffer = new Collider2D[4096];

        public override SkillData CreateDefaultSkillData()
        {
            return new JumpSkillData();
        }
    }

    public class JumpSkillData : SkillData
    {
        
    }
}