using System;
using System.Collections.Generic;
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

        private bool allDead;

        [NonSerialized] private float startedAt;
        [NonSerialized] private List<Entity> trackAliveEntities;

        public override void Start(float currentTime)
        {
            var parent = Gamesystem.instance.missionManager.transform;

            trackAliveEntities = new();
            allDead = false;
            
            foreach (var prefab in handlers)
            {
                var go = GameObject.Instantiate(prefab, parent);
                var handler = go.GetComponent<IMissionHandler>();
                handler.OnStart(this);
            }
            
            startedAt = currentTime;
        }

        public override bool CanStart(float currentTime)
        {
            return true;
        }

        public override bool CanEnd(float currentTime)
        {
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

    /*public class ReceiveRewardHandler : MissionEvent
    {
        public override void Start()
        {
            //TODO
        }
    }*/
    
}