using System;
using System.Collections;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Scriptables.Skills;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class RotateTowardsMouse : MonoBehaviour
    {
        
        public Transform follow;
        
        public float delta;

        public bool useJumpDistanceAsDelta;
        
        [ShowIf("useJumpDistanceAsDelta")]
        public Player player;
        
        public void Start()
        {
            // decouple
            transform.parent = null;

            if (useJumpDistanceAsDelta)
            {
                StartCoroutine(UpdateJumpDist());
            }
        }

        private IEnumerator UpdateJumpDist()
        {
            var waiter = new WaitForSeconds(1f);
            while (player != null)
            {
                var skill = player.GetSkill(0);

                if (skill is JumpSkill jump)
                {
                    var weight = player.rb.mass;
                    weight = Math.Max(1, player.stats.weightMul.GetValue() * weight); // TODO make it better somehow
                
                    delta = (jump.GetJumpForce(player) * player.stats.skillPowerMul.GetValue() * (1/(weight))) * jump.jumpLength;
                }

                yield return waiter;
            }
        }
        
        public void Update()
        {
            var basePosition = follow.transform.position;

            Vector3 mousePosition = Utils.GetMousePosition();
            Vector2 direction = mousePosition - basePosition;
            
            if (Utils.Dist2(basePosition, Utils.GetMousePosition()) > 0.1f)
            {
                var rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
                transform.rotation = rotation;
                transform.position = follow.transform.position + (Vector3)(direction.normalized * delta);
            }
        }
    }
}