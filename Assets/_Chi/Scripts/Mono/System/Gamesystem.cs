using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.System;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gamesystem : MonoBehaviour
{
    public static Gamesystem instance;
    
    [HideInInspector] public GameobjectHolder objects;
    [HideInInspector] public Tests tests;
    [HideInInspector] public PoolSystem poolSystem;
    [HideInInspector] public PlayerProgressManager progress;
    [HideInInspector] public KillEffectManager killEffectManager;

    [Required] public UiManager uiManager;
    [Required] public GameObject world;
    [Required] public MiscSettings miscSettings;
    [Required] public PrefabDatabase prefabDatabase;
    [Required] public MissionManager missionManager;

    [NonSerialized] public Dictionary<int, PrefabItem> prefabs;
    [NonSerialized] public Dictionary<PredefinedPrefabIds, PrefabItem> predefinedPrefabs;

    private List<FloatWithAction> schedules;
    private List<int> toRemoveSchedules;

    [NonSerialized] public float levelStartedTime;

    private void Awake()
    {
        instance = this;
        
        schedules = new List<FloatWithAction>();
        toRemoveSchedules = new List<int>();
        
        this.objects = GetComponent<GameobjectHolder>();
        this.tests = GetComponent<Tests>();
        this.poolSystem = GetComponent<PoolSystem>();
        this.progress = GetComponent<PlayerProgressManager>();
        this.killEffectManager = GetComponent<KillEffectManager>();

        prefabs = prefabDatabase.prefabs.ToDictionary(t => t.id, t => t);
        predefinedPrefabs = prefabDatabase.prefabs.Where(t => t.predefinedId != PredefinedPrefabIds.Custom)
            .ToDictionary(t => t.predefinedId, t => t);
    }

    // Start is called before the first frame update
    void Start()
    {
        RestartLevelClock();
    }

    public void Update()
    {
        var time = Time.time;
        int index = 0;
        /*for (int i = 0; i < schedules.Count; i++)
        {
            var schedule = schedules[i];
            
            
        }*/

        bool anyRun = false;
        
        foreach (var schedule in schedules)
        {
            if (time > schedule.time)
            {
                schedule.action();
                anyRun = true;
            }
            
            index++;
        }

        if (anyRun)
        {
            schedules.RemoveAll(t => time > t.time);
        }
    }
    
    public void Schedule(float time, Action action)
    {
        //TODO same time = error
        schedules.Add(new FloatWithAction()
        {
            action = action,
            time = time
        });
    }

    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Unpause()
    {
        Time.timeScale = 1;
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void RestartLevelClock()
    {
        levelStartedTime = Time.time;
    }

    public void OnKilled(Entity e)
    {
        if (e is Npc npc)
        {
            progress.progressData.run.killed++;
        }
    }
}
