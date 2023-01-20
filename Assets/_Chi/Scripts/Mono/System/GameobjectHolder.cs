using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Movement;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.Serialization;

public class GameobjectHolder : MonoBehaviour
{
    public Dictionary<long, Entity> entities;
    public Player mainPlayer;
    public List<Npc> npcEntitiesList;

    private PathJob pathJob;

    public void Awake()
    {
        entities = new();
        npcEntitiesList = new();

        pathJob = new PathJob(this, RVOSimulator.active);
    }

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        for (var index = npcEntitiesList.Count - 1; index >= 0; index--)
        {
            Entity entity = npcEntitiesList[index];

            try
            {
                entity.DoUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError("Error in Update: " + e);
            }
        }
    }

    public void FixedUpdate()
    {
        pathJob.OnFixedUpdate();
    }

    public void RegisterEntity(Entity e)
    {
        entities.Add(e.GetInstanceID(), e);
        if (e is Npc npc)
        {
            npcEntitiesList.Add(npc);
        }
        else
        {
            mainPlayer = (Player) e;
        }
    }

    public void UnregisterEntity(Entity e)
    {
        entities.Remove(e.GetInstanceID());
        if (e is Npc npc)
        {
            npcEntitiesList.Remove(npc);
        }
        else if(mainPlayer == e)
        {
            mainPlayer = null;
        }
    }
}
