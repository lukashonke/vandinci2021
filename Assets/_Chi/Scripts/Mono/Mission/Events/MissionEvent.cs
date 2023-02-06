using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Chi.Scripts.Mono.Mission.Events
{
    [Serializable]
    public abstract class MissionEvent
    {
        public abstract void Start(float currentTime);

        public abstract bool CanStart(float currentTime);

        public abstract bool CanEnd(float currentTime);

        public virtual void End(float currentTime)
        {
            
        }
        
        public virtual void Update()
        {
            
        }

        public virtual void TrackAliveEntity(Entity e)
        {
            
        }
    }

    public class EndAllMissionHandlersEvent : MissionEvent
    {
        public bool waitUntilAllEnemiesDead;
        
        public override void Start(float currentTime)
        {
            var toDelete = new List<Transform>();
            foreach (Transform transform in Gamesystem.instance.missionManager.transform)
            {
                if (transform.GetComponent<Mission>() != null) continue;
                
                toDelete.Add(transform);
            }

            foreach (var transform in toDelete)
            {
                Object.Destroy(transform.gameObject);
            }
        }

        public override bool CanStart(float currentTime)
        {
            return true;
        }

        public override bool CanEnd(float currentTime)
        {
            if (waitUntilAllEnemiesDead)
            {
                return Gamesystem.instance.missionManager.AreTrackedEntitiesDead();
            }
            return true;
        }
    }

    public class StartMissionHandlerEvent : MissionEvent
    {
        public List<GameObject> handlers;

        //public bool hasFixedDuration;
        //[ShowIf("hasFixedDuration")]
        
        public float fixedDuration;

        public bool waitTillAllSpawnedAreDead;

        public bool waitTillAllWawesSpawn = true;

        private bool allDead;

        [NonSerialized] private float startedAt;
        [NonSerialized] private List<Entity> trackAliveEntities;

        [NonSerialized] private List<IMissionHandler> handlerInstances;

        public override void Start(float currentTime)
        {
            var parent = Gamesystem.instance.missionManager.transform;

            handlerInstances = new();

            trackAliveEntities = new();
            allDead = false;
            
            foreach (var prefab in handlers)
            {
                var go = GameObject.Instantiate(prefab, parent);

                foreach (var handler in go.GetComponents<IMissionHandler>())
                {
                    handler.OnStart(this);
                    handlerInstances.Add(handler);
                }
                
            }
            
            startedAt = currentTime;
        }

        public override bool CanStart(float currentTime)
        {
            return true;
        }

        public override bool CanEnd(float currentTime)
        {
            if (waitTillAllWawesSpawn && handlerInstances.Any(h => !h.IsFinished()))
            {
                return false;
            }
            
            if ((currentTime-startedAt) > fixedDuration)
            {
                if (waitTillAllSpawnedAreDead)
                {
                    return allDead;
                }
                
                return true;
            }

            return false;
        }

        public override void Update()
        {
            base.Update();

            bool anyAlive = false;
            foreach (var entity in trackAliveEntities)
            {
                if (entity != null && entity.isAlive && entity.gameObject.activeSelf)
                {
                    anyAlive = true;
                    break;
                }
            }

            allDead = !anyAlive;
            
            trackAliveEntities.RemoveAll(e => !e.isAlive);
        }

        public override void TrackAliveEntity(Entity e)
        {
            base.TrackAliveEntity(e);
            
            trackAliveEntities.Add(e);
            allDead = false;
        }
    }

    public class ShowMessage : MissionEvent
    {
        public string text;
        
        public override void Start(float currentTime)
        {
            Gamesystem.instance.uiManager.SetMissionText(text);
        }

        public override bool CanStart(float currentTime) => true;

        public override bool CanEnd(float currentTime) => true;
    }

    public class Delay : MissionEvent
    {
        public float delay;
        private float startedAt;
        
        public override void Start(float currentTime)
        {
            startedAt = currentTime;
        }

        public override bool CanStart(float currentTime) => true;

        public override bool CanEnd(float currentTime)
        {
            return currentTime - startedAt > delay;
        }
    }

    public class WaitIfAllSpawnedAreDead : MissionEvent
    {
        public override void Start(float currentTime)
        {
            
        }

        public override bool CanStart(float currentTime)
        {
            return true;
        }

        public override bool CanEnd(float currentTime)
        {
            return Gamesystem.instance.missionManager.AreTrackedEntitiesDead();
        }
    }

    public class NextMission : MissionEvent
    {
        public override void Start(float currentTime)
        {
            var run = Gamesystem.instance.progress.progressData.run;
            Gamesystem.instance.missionManager.ChangeMission(run.missionIndex + 1);
        }

        public override bool CanStart(float currentTime)
        {
            return true;
        }

        public override bool CanEnd(float currentTime)
        {
            return true;
        }
    }

    public class ReceiveRewardHandler : MissionEvent
    {
        public override void Start(float currentTime)
        {
            Time.timeScale = 0f;
            
            Gamesystem.instance.uiManager.ShowConfirmDialog("Reward Time!", "Here is when you pick a new reward.", 
                () => Time.timeScale = 1f, () => Time.timeScale = 1f, () => Time.timeScale = 1f);
        }

        public override bool CanStart(float currentTime)
        {
            return true;
        }

        public override bool CanEnd(float currentTime)
        {
            return true;
        }
    }
    
}