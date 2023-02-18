using System.Collections;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Utilities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Skills
{
    [CreateAssetMenu(fileName = "Jump", menuName = "Gama/Skills/Jump")]
    public class JumpSkill : Skill
    {
        public float jumpForce = 10;

        public float jumpLength;
        
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
                return true;
            }

            return false;
        }

        private IEnumerator Jump(Player player)
        {
            SetActivated(player, true);

            SpawnPrefabVfx(player.GetPosition(), player.transform.rotation, null);
            player.OnSkillUse();
            
            var force = jumpForce * player.stats.skillPowerMul.GetValue();
            
            var direction = (Utils.GetMousePosition() - player.GetPosition()).normalized;
            var jumpUntil = Time.time + jumpLength;
            
            player.SetCanMove(false);
            player.SetCanReceiveDamage(false);
            var waiter = new WaitForFixedUpdate();

            while (jumpUntil >= Time.time)
            {
                player.rb.velocity = direction * force;
                
                //player.rb.MovePosition((Vector3) player.rb.position + (direction * jumpForce * Time.fixedDeltaTime));
                
                yield return waiter;
            }
            
            player.SetCanMove(true);
            player.SetCanReceiveDamage(true);
            
            player.rb.velocity = Vector2.zero;
            
            SetActivated(player, false);
        }

        public override SkillData CreateDefaultSkillData()
        {
            return new JumpSkillData();
        }
    }

    public class JumpSkillData : SkillData
    {
        
    }
}