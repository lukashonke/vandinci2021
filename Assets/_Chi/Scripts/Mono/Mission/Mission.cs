using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Mission
{
    public class Mission : SerializedMonoBehaviour
    {
        [TableList(ShowIndexLabels = true)]
        public List<MissionEvent> events;

        public int startIndex = 0;

        public bool forceStartAtIndex;

        public bool loopEvents;
        
        public MissionProgressSettings progressSettings;

        [ReadOnly] public MissionEvent currentEvent;

        private bool alive;

        void Start()
        {
            alive = true;

            foreach (var missionEvent in events)
            {
                missionEvent.Initialise(this);
            }

            StartCoroutine(UpdateLoop());
            
            if (progressSettings.shops.Count > 0)
            {
                var initialShop = progressSettings.shops[0];
            
                Gamesystem.instance.uiManager.goldProgressBar.SetMaxValue(initialShop.goldAcumulatedRequired);
                Gamesystem.instance.uiManager.goldProgressBar.ResetValue();
                progressSettings.lastGoldTriggeredShopLevelIndex = -1;
            }
            
            Gamesystem.instance.missionManager.currentMission = this;
        }
        
        public void OnAddedGold()
        {
            var gold = Gamesystem.instance.progress.GetAcumulatedGold();

            var shopIndex = progressSettings.lastGoldTriggeredShopLevelIndex;
                
            if(shopIndex + 1 < progressSettings.shops.Count)
            {
                var nextShop = progressSettings.shops[shopIndex + 1];

                if (gold >= nextShop.goldAcumulatedRequired)
                {
                    shopIndex++;
                        
                    //TODO trigger shop
                    Gamesystem.instance.uiManager.OpenRewardSetWindow(nextShop.shopSet, "Shop", nextShop);
                        
                    nextShop = progressSettings.shops[shopIndex + 1];

                    if (shopIndex >= 0)
                    {
                        var prevShop = progressSettings.shops[shopIndex];
                        Gamesystem.instance.uiManager.goldProgressBar.SetMaxValue(nextShop.goldAcumulatedRequired - prevShop.goldAcumulatedRequired);
                    }
                    else
                    {
                        Gamesystem.instance.uiManager.goldProgressBar.SetMaxValue(nextShop.goldAcumulatedRequired);
                    }
                        
                    Gamesystem.instance.uiManager.goldProgressBar.ResetValue();
                    
                    progressSettings.lastGoldTriggeredShopLevelIndex = shopIndex;
                }
            }
        }

        private IEnumerator UpdateLoop()
        {
            float timePassed = 0;
            const float loopInterval = 0.5f;
            var waiter = new WaitForSeconds(loopInterval);

            int currentEventIndex = startIndex;
            
            while (alive)
            {
                if (currentEvent != null && currentEvent.CanEnd(timePassed))
                {
                    currentEvent.End(timePassed);
                    currentEvent = null;
                }
                else if (currentEvent != null)
                {
                    currentEvent.Update();
                }

                if (currentEvent == null)
                {
                    if (currentEventIndex >= events.Count)
                    {
                        if (loopEvents)
                        {
                            currentEventIndex = 0;
                        }
                        else
                        {
                            yield return waiter;
                            continue;
                        }
                    }
                    
                    for (var index = currentEventIndex; index < events.Count; index++)
                    {
                        var ev = events[index];
                        if (ev.CanStart(timePassed))
                        {
                            ev.Start(timePassed);
                            currentEvent = ev;
                            currentEventIndex = index + 1;
                            break;
                        }
                    }
                }

                yield return waiter;
                timePassed += loopInterval;
            }
        }

        public void SimulateUpToEvent(MissionEvent currentEvent)
        {
            StartCoroutine(SimulateUpToEventCoroutine(currentEvent));
        }

        private IEnumerator SimulateUpToEventCoroutine(MissionEvent currentEvent)
        {
            var index = events.IndexOf(currentEvent);

            for (int i = 0; i <= index; i++)
            {
                var ev  = events[i];
                yield return ev.Simulate();
            }
        }
        
        private void OnDestroy()
        {
            alive = false;
        }
    }

    [Serializable]
    public class MissionProgressSettings
    {
        public List<TriggeredShop> shops;

        public List<int> rerollPrices;

        [NonSerialized] public int lastGoldTriggeredShopLevelIndex = 0;
    }
    
    [Serializable]
    public class TriggeredShop
    {
        public int goldAcumulatedRequired;
        
        [HorizontalGroup("ShopSet")]
        public List<TriggeredShopSet> shopSet;

        [HorizontalGroup("ShopSet")]
        public float priceMultiplier;

        [HideInEditorMode]
        [Button]
        public void Show()
        {
            Gamesystem.instance.uiManager.OpenRewardSetWindow(shopSet, "Shop", this);
        }
    }
    
    [Serializable]
    public class TriggeredShopSet
    {
        public string name;

        public bool showOnlyPreviouslyLocked;
    }
}