using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using UnityEngine;

namespace _Chi.Scripts.Mono.Entities
{
    public class EffectCloud : MonoBehaviour
    {
        public ImmediateEffect effect;

        public Teams team;

        public float effectStrength = 1;

        public float despawnDelay;

        void Start()
        {
            StartCoroutine(Despawn());
        }

        private IEnumerator Despawn()
        {
            yield return new WaitForSeconds(despawnDelay);
            
            Destroy(this.gameObject);
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            //TODO check if inside
            
            if (col != null && col.gameObject != null)
            {
                var currentPlayer = Gamesystem.instance.objects.currentPlayer;
                var entity = col.gameObject.GetEntity();
                
                if (team == Teams.Monster && entity is Player player)
                {
                    effect.Apply(entity, null, null, null, effectStrength);
                }
                else if (team == Teams.Player && entity is Npc npc && npc.AreEnemies(currentPlayer))
                {
                    effect.Apply(entity, null, null, null, effectStrength);
                }
            }
        }
    }
}