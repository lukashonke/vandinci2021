using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class DestructibleStructure : Npc
    {
        public override void Start()
        {
            base.Start();

            canMove = false;

            StartCoroutine(UpdateJob());
        }

        public override bool CanBePushed() => false;

        private IEnumerator UpdateJob()
        {
            var waiter = new WaitForSeconds(0.2f);

            while (isAlive)
            {
                var player = Gamesystem.instance.objects.currentPlayer;

                var dist = Utils.Dist2(GetPosition(), player.GetPosition());

                SetDistanceToPlayer(dist, player);
                
                yield return waiter;
            }
        }
    }
}