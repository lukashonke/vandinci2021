using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

public class Tests : MonoBehaviour
{
    public bool moveToTestTarget;
    
    public GameObject testMoveTarget;

    public void Start()
    {
        StartCoroutine(Targeter());
    }

    private IEnumerator Targeter()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(2);
            
            if (moveToTestTarget)
            {
                foreach (var entitiesValue in Gamesystem.instance.objects.npcEntitiesList)
                {
                    if (entitiesValue is Npc m && !m.goDirectlyToPlayer)
                    {
                        m.SetMoveTarget(() => testMoveTarget.transform.position);
                    }
                }
            }
            else
            {
                foreach (var entitiesValue in Gamesystem.instance.objects.npcEntitiesList)
                {
                    if (entitiesValue is Npc m && !m.goDirectlyToPlayer)
                    {
                        m.SetMoveTarget(() => Gamesystem.instance.objects.currentPlayer.GetPosition());
                    }
                }
            }
        }
    }
}
