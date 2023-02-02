using System;
using System.Collections.Generic;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace _Chi.Scripts.Mono.Mission
{
    public class MissionManager : SerializedMonoBehaviour
    {
        [ReadOnly] public MissionDatabaseItem mission;
        
        [NonSerialized] public IMissionHandler[] handlers;
        
        public bool processOnStart = true;

        public bool hideTilemapOnStart = true;

        private List<GameObject> spawnedObjects = new List<GameObject>();
        
        public void Awake()
        {
            Gamesystem.instance.missionManager = this;
            
            if (processOnStart)
            {
                ProcessMapGenTilemap();
                ClearMapGenTilemap();
            }
            
            if (hideTilemapOnStart)
            {
                Gamesystem.instance.mapGenTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
            }
        }

        public void Start()
        {
            var run = Gamesystem.instance.progress.progressData.run;
            
            mission = Gamesystem.instance.missionDatabase.GetMission(run.missionIndex);
            
            this.transform.RemoveAllChildren();

            foreach (var prefab in mission.missionScenarios)
            {
                Instantiate(prefab, this.gameObject.transform);
            }
            
            handlers = GetComponentsInChildren<IMissionHandler>();
            
            foreach (var handler in handlers)
            {
                handler.OnStart();
            }
        }

        public void OnDestroy()
        {
            foreach (var handler in handlers)
            {
                handler.OnStop();
            }
        }

        [Button]
        public void ChangeMission(int index)
        {
            var run = Gamesystem.instance.progress.progressData.run;
            run.missionIndex = index;
            Gamesystem.instance.progress.Save();
            
            ReloadScene();
        }

        public void OnPlayerDie()
        {
            Time.timeScale = 0;
            
            Gamesystem.instance.progress.Save();

            ReloadScene();
            
            Time.timeScale = 1f;
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        [BoxGroup("Process Map")]
        [Button(ButtonSizes.Gigantic), GUIColor(0.2f, 0.9f, 0.3f)]
        public void ProcessMapGenTilemap()
        {
            spawnedObjects = MapGenUtils.ProcessTilemap(Gamesystem.instance.mapGenTilemap);
        }
        
        [BoxGroup("Process Map")]
        [ButtonGroup("Process Map/Process Map Buttons")]
        [Button(ButtonSizes.Large), GUIColor(1, 0, 0)]
        public void ClearMapGenTilemap()
        {
            Gamesystem.instance.mapGenTilemap.ClearAllTiles();
        }
        
        [BoxGroup("Process Map")]
        [ButtonGroup("Process Map/Process Map Buttons")]
        [Button("$ToggleMapGenTilemapLabel", ButtonSizes.Large)]
        public void ToggleMapGenTilemap()
        {
            Gamesystem.instance.mapGenTilemap.gameObject.GetComponent<TilemapRenderer>().enabled =
                !Gamesystem.instance.mapGenTilemap.gameObject.GetComponent<TilemapRenderer>().enabled;
        }
        
        private string ToggleMapGenTilemapLabel()
        {
            return Gamesystem.instance.mapGenTilemap.gameObject.GetComponent<TilemapRenderer>().enabled ? "Hide MapGen Tilemap" : "Show MapGen Tilemap";
        }
        
        [BoxGroup("Process Map")]
        [ButtonGroup("Process Map/Process Map Buttons")]
        [Button(ButtonSizes.Large)]
        public void ClearGeneratedGameobjects()
        {
            if (spawnedObjects == null) return;
            
            foreach (GameObject spawnedObject in spawnedObjects)
            {
                GameObject.DestroyImmediate(spawnedObject);
            }

            var all = new List<Transform>();
            foreach (Transform t in Gamesystem.instance.worldGenerated.transform)
                all.Add(t);

            foreach (Transform child in all)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }
    }
}