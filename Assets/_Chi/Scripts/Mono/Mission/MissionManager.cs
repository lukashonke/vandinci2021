using System;
using System.Collections.Generic;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Chi.Scripts.Mono.Mission
{
    public class MissionManager : MonoBehaviour
    {
        [ReadOnly] public MissionDatabaseItem mission;
        
        [NonSerialized] public IMissionHandler[] handlers;
        
        public void Awake()
        {
            Gamesystem.instance.missionManager = this;
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
    }
}