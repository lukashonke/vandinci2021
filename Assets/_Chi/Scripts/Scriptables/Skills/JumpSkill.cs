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
        
        public override bool Trigger(Entity entity)
        {
            if (!CanTrigger(entity)) return false;

            if (entity is Player player)
            {
                player.StartCoroutine(Jump(player));
                
                SetNextSkillUse(entity, reuseDelay);
                return true;
            }

            return false;
        }

        private IEnumerator Jump(Player player)
        {
            var direction = (Utils.GetMousePosition() - player.GetPosition()).normalized;
            var jumpUntil = Time.time + jumpLength;
            
            player.SetCanMove(false);
            var waiter = new WaitForFixedUpdate();

            while (jumpUntil >= Time.time)
            {
                player.rb.velocity = direction * jumpForce;
                
                //player.rb.MovePosition((Vector3) player.rb.position + (direction * jumpForce * Time.fixedDeltaTime));
                
                yield return waiter;
            }
            
            player.SetCanMove(true);
            
            player.rb.velocity = Vector2.zero;
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