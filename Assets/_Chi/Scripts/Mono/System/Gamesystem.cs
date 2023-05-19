using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.System;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Gamesystem : MonoBehaviour
{
    public static Gamesystem instance;

    [HideInPlayMode]
    [Button]
    public void InitInEditor()
    {
        instance = this;
    }
    
    [NonSerialized] public MissionManager missionManager;
    
    [HideInInspector] public GameobjectHolder objects;
    [HideInInspector] public Tests tests;
    [HideInInspector] public PlayerProgressManager progress;
    [HideInInspector] public KillEffectManager killEffectManager;

    [Required] public PoolSystem poolSystem;
    [Required] public UiManager uiManager;
    [Required] public MiscSettings miscSettings;
    [Required] public PrefabDatabase prefabDatabase;
    [Required] public MissionDatabase missionDatabase;
    [Required] public MapGenSettings mapGenSettings;
    [Required] public Tilemap mapGenTilemap;
    [Required] public DropManager dropManager;
    [Required] public TextDatabase textDatabase;
    [Required] public LocationManager locationManager;
    
    [Required]
    public SpawnAroundSettings spawnAroundSettings;

    [Required] public GameObject world;
    [Required] public GameObject worldGenerated;

    [NonSerialized] public Dictionary<int, PrefabItem> prefabs;
    [NonSerialized] public Dictionary<PredefinedPrefabIds, PrefabItem> predefinedPrefabs;

    private List<FloatWithAction> schedules;
    private List<int> toRemoveSchedules;
    GraphNode centerNode;

    [NonSerialized] public float levelStartedTime;

    public float HorizontalToBorderDistance;
    [FormerlySerializedAs("VerticalToborderDistance")] public float VerticalToBorderDistance;
    
    [FoldoutGroup("Difficulty")]
    public float monsterCountMul = 1;

    [FoldoutGroup("Difficulty")] 
    public float goldMul = 1f;
    
    [FoldoutGroup("Difficulty")] 
    public float expMul = 1f;

    private void Awake()
    {
        instance = this;
        
        prefabDatabase.Initialise();
        spawnAroundSettings.Initialise();
        
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
        
        AstarPath.active.Scan();
        
        centerNode = AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node;
    }

    private List<FloatWithAction> schedulesCopy = new();

    public void Update()
    {
        //DebugDistances();

        //Debug.Log(CanBeAccessed(Utils.GetMousePosition()));
        
        var time = Time.time;
        int index = 0;
        /*for (int i = 0; i < schedules.Count; i++)
        {
            var schedule = schedules[i];
            
            
        }*/

        bool anyRun = false;
        
        schedulesCopy.AddRange(schedules);
        
        foreach (var schedule in schedulesCopy)
        {
            if (time > schedule.time)
            {
                schedule.action();
                anyRun = true;
            }
            
            index++;
        }
        
        schedulesCopy.Clear();

        if (anyRun)
        {
            schedules.RemoveAll(t => time > t.time);
        }
    }
    
    public void Schedule(float time, Action action)
    {
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

            if (npc.currentVariantInstance?.reward != null)
            {
                foreach (var item in npc.currentVariantInstance.reward.items ?? Enumerable.Empty<RewardItem>())
                {
                    var chance = item.dropChance;
                    var doDrop = UnityEngine.Random.Range(0f, 100f) < chance;
                    if (doDrop)
                    {
                        var player = Gamesystem.instance.objects.currentPlayer;
                        var drops = Random.Range(item.amountMin, item.amountMax) * Math.Max(1, player.stats.playerGoldDropped.GetValueInt());

                        for (int i = 0; i < drops; i++)
                        {
                            Vector3 position;
                            const float spread = 0.4f;
                            position = npc.GetPosition() + new Vector3(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread));
                        
                            dropManager.Drop(item.dropType, position, item.bypassGlobalDropChance);
                        }
                    }
                }
            }
        }
    }

    public bool CanBeAccessed(Vector3 position)
    {
        GraphNode node1 = AstarPath.active.GetNearest(position, NNConstraint.None).node;
        return PathUtilities.IsPathPossible(centerNode, node1);
    }
    
    private void DebugDistances()
    {
#if UNITY_EDITOR
        Debug.DrawLine(objects.currentPlayer.GetPosition(), objects.currentPlayer.GetPosition() + new Vector3(HorizontalToBorderDistance, 0, 0), Color.red);
        
        Debug.DrawLine(objects.currentPlayer.GetPosition(), objects.currentPlayer.GetPosition() + new Vector3(0, VerticalToBorderDistance, 0), Color.blue);
#endif
    }

    public Vector3 playerMoveDirection;
    private Vector3 previousPlayerPosition;
    private Vector3 currentPlayerPosition;

    public void TrackPlayerPosition(Player player)
    {
        currentPlayerPosition = player.GetPosition();
        playerMoveDirection = currentPlayerPosition - previousPlayerPosition;
        
        previousPlayerPosition = currentPlayerPosition;
    }

    public PlayerMoveDirection GetPlayerMoveDirection()
    {
        var result = PlayerMoveDirection.Idle;
        if(playerMoveDirection.x > 0) result |= PlayerMoveDirection.Right;
        if(playerMoveDirection.x < 0) result |= PlayerMoveDirection.Left;
        if(playerMoveDirection.y > 0) result |= PlayerMoveDirection.Up;
        if(playerMoveDirection.y < 0) result |= PlayerMoveDirection.Down;
        return result;
    }

    [Flags]
    public enum PlayerMoveDirection
    {
        Idle = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
}
