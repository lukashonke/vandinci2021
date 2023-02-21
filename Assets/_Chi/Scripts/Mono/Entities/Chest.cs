using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class Chest : DestructibleStructure
    {
        public List<Drop> drops;

        public override void OnDie(DieCause cause)
        {
            base.OnDie(cause);

            if (cause == DieCause.Killed)
            {
                foreach (var drop in drops)
                {
                    if (Random.Range(0, 100f) < drop.chance)
                    {
                        var count = Random.Range(drop.dropCountMin, drop.dropCountMax);
                        for (int i = 0; i < count; i++)
                        {
                            Gamesystem.instance.dropManager.Drop(drop.dropType, GetPosition() + (Vector3) (Random.insideUnitCircle.normalized * Random.Range(0.2f, 1)));
                        }    
                    }
                }
            }
        }
    }

    [Serializable]
    public class Drop
    {
        public int dropCountMin;
        public int dropCountMax;
        public DropType dropType;

        public float chance;
    }
}