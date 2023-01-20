using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.System;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gamesystem : MonoBehaviour
{
    public static Gamesystem instance;
    
    [HideInInspector] public GameobjectHolder objects;
    [HideInInspector] public Tests tests;
    [HideInInspector] public PoolSystem poolSystem;

    [Required] public UiManager uiManager;
    [Required] public GameObject world;
    [Required] public MiscSettings miscSettings;
    [Required] public PrefabDatabase prefabDatabase;

    [NonSerialized] public Dictionary<int, PrefabItem> prefabs;
    [NonSerialized] public Dictionary<PredefinedPrefabIds, PrefabItem> predefinedPrefabs;

    private SortedList<float, Action> schedules;
    private List<int> toRemoveSchedules;

    private void Awake()
    {
        instance = this;
        
        schedules = new SortedList<float, Action>();
        toRemoveSchedules = new List<int>();
        
        this.objects = GetComponent<GameobjectHolder>();
        this.tests = GetComponent<Tests>();
        this.poolSystem = GetComponent<PoolSystem>();

        prefabs = prefabDatabase.prefabs.ToDictionary(t => t.id, t => t);
        predefinedPrefabs = prefabDatabase.prefabs.Where(t => t.predefinedId != PredefinedPrefabIds.Custom)
            .ToDictionary(t => t.predefinedId, t => t);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Update()
    {
        var time = Time.time;
        int index = 0;
        foreach (var schedule in schedules)
        {
            if (time < schedule.Key)
            {
                return;
            }

            schedule.Value();
            toRemoveSchedules.Add(index);
            index++;
        }

        if (toRemoveSchedules.Any())
        {
            for (var i = toRemoveSchedules.Count - 1; i >= 0; i--)
            {
                var removeSchedule = toRemoveSchedules[i];
                schedules.RemoveAt(removeSchedule);
            }

            toRemoveSchedules.Clear();
        }
    }
    
    public void Schedule(float time, Action action)
    {
        schedules.Add(time, action);
    }
}
