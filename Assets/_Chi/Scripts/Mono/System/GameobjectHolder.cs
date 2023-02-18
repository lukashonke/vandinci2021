using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Movement;
using _Chi.Scripts.Scriptables;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.Serialization;

public class GameobjectHolder : MonoBehaviour
{
    public Dictionary<long, Entity> entities;
    public Player currentPlayer;
    public List<Npc> npcEntitiesList;
    private List<Npc> npcWithSkill;

    private PathJob pathJob;

    public void Awake()
    {
        entities = new();
        npcEntitiesList = new();
        npcWithSkill = new();

        pathJob = new PathJob(this, RVOSimulator.active);
    }

    // Start is called before the first frame update
    public void Start()
    {
        StartCoroutine(NpcSkillCoroutine());
    }

    private IEnumerator NpcSkillCoroutine()
    {
        yield return new WaitForSeconds(1f);
        
        while (this != null)
        {
            for (var index = 0; index < npcWithSkill.Count; index++)
            {
                Npc npc = npcWithSkill[index];

                foreach (var variantSkill in npc.currentVariantInstance.skills)
                {
                    var skill = variantSkill.skill;
                    var trigger = variantSkill.trigger;
                    if (!npc.skillDatas.ContainsKey(skill))
                    {
                        var skillData = skill.CreateDefaultSkillData();
                        npc.skillDatas.Add(skill, skillData);
                    }

                    //TODO more sophisticated triggering by npcs, use 'trigger' obj
                    if (skill.CanTrigger(npc, out _))
                    {
                        skill.Trigger(npc);
                    }
                }
            }

            yield return null;
        }
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

    public void RegisterNpcWithSkills(Npc npc)
    {
        if (npcWithSkill.Contains(npc))
        {
            Debug.LogError("DEBUG SHOULD NOT BE HERE TODO REMOVE THIS LATER");
        }
        
        npcWithSkill.Add(npc);
    }

    public void UnregisterNpcWithSkills(Npc npc)
    {
        npcWithSkill.Remove(npc);
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
            currentPlayer = (Player) e;
        }
    }

    public void UnregisterEntity(Entity e)
    {
        entities.Remove(e.GetInstanceID());
        if (e is Npc npc)
        {
            npcEntitiesList.Remove(npc);
        }
        else if(currentPlayer == e)
        {
            currentPlayer = null;
        }
    }
}
