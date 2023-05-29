using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Chi.Scripts.Mono.Mission
{
    public class Mission : SerializedMonoBehaviour
    {
        [TableList(ShowIndexLabels = true)]
        public List<MissionEvent> events;

        public Vector3 center;

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

                if (initialShop.GetGoldAcumulatedRequired() > 0)
                {
                    Gamesystem.instance.uiManager.rewardProgressBar.SetMaxValue(initialShop.GetGoldAcumulatedRequired());
                }
                else
                {
                    Gamesystem.instance.uiManager.rewardProgressBar.SetMaxValue(initialShop.GetExpAcumulatedRequired());
                }
                
                Gamesystem.instance.uiManager.rewardProgressBar.ResetValue();
                progressSettings.lastGoldTriggeredShopLevelIndex = -1;
                progressSettings.lastExpTriggeredShopLevelIndex = -1;
            }
            
            Gamesystem.instance.missionManager.currentMission = this;
        }
        
        public void OnAddedExp()
        {
            var exp = Gamesystem.instance.progress.GetAcumulatedExp();

            var shopIndex = progressSettings.lastExpTriggeredShopLevelIndex;
                
            if(shopIndex + 1 < progressSettings.shops.Count)
            {
                var nextShop = progressSettings.shops[shopIndex + 1];

                if (nextShop.GetExpAcumulatedRequired() > 0 && exp >= nextShop.GetExpAcumulatedRequired())
                {
                    shopIndex++;
                    
                    Gamesystem.instance.uiManager.OpenRewardSetWindow(nextShop.shopSet, nextShop.title, nextShop);
                        
                    nextShop = progressSettings.shops[shopIndex + 1];

                    if (shopIndex >= 0)
                    {
                        var prevShop = progressSettings.shops[shopIndex];
                        Gamesystem.instance.uiManager.rewardProgressBar.SetMaxValue(nextShop.GetExpAcumulatedRequired() - prevShop.GetExpAcumulatedRequired());
                    }
                    else
                    {
                        Gamesystem.instance.uiManager.rewardProgressBar.SetMaxValue(nextShop.GetExpAcumulatedRequired());
                    }
                        
                    Gamesystem.instance.uiManager.rewardProgressBar.ResetValue();
                    
                    progressSettings.lastExpTriggeredShopLevelIndex = shopIndex;
                }
            }
        }
        
        public void OnAddedGold()
        {
            var gold = Gamesystem.instance.progress.GetAcumulatedGold();

            var shopIndex = progressSettings.lastGoldTriggeredShopLevelIndex;
                
            if(shopIndex + 1 < progressSettings.shops.Count)
            {
                var nextShop = progressSettings.shops[shopIndex + 1];

                if (nextShop.GetGoldAcumulatedRequired() > 0 && gold >= nextShop.GetGoldAcumulatedRequired())
                {
                    shopIndex++;
                        
                    Gamesystem.instance.uiManager.OpenRewardSetWindow(nextShop.shopSet, nextShop.title, nextShop);
                        
                    /*nextShop = progressSettings.shops[shopIndex + 1];

                    if (shopIndex >= 0)
                    {
                        var prevShop = progressSettings.shops[shopIndex];
                        Gamesystem.instance.uiManager.rewardProgressBar.SetMaxValue(nextShop.GetGoldAcumulatedRequired() - prevShop.GetGoldAcumulatedRequired());
                    }
                    else
                    {
                        Gamesystem.instance.uiManager.rewardProgressBar.SetMaxValue(nextShop.GetGoldAcumulatedRequired());
                    }
                        
                    Gamesystem.instance.uiManager.rewardProgressBar.ResetValue();*/
                    
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

        /*public void SimulateUpToEvent(MissionEvent currentEvent)
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
        }*/
        
        private void OnDestroy()
        {
            alive = false;
        }
    }

    [Serializable]
    public class MissionProgressSettings
    {
        public List<TriggeredShop> shops;
        
        public List<TriggeredShop> simulatedRewards;

        public List<int> rerollPrices;

        [NonSerialized] public int lastGoldTriggeredShopLevelIndex = 0;
        
        [NonSerialized] public int lastExpTriggeredShopLevelIndex = 0;
    }

    public enum TriggeredShopType
    {
        FreeReward,
        Shop
    }
    
    [Serializable]
    public class TriggeredShop
    {
        public TriggeredShopType type;

        public bool closeOnFirstPurchase;

        public int index;
        public string title;
        
        [HideInInspector] // TODO not used now
        public int goldAcumulatedRequired;
        
        public int expAcumulatedRequired;

        /// <summary>
        /// means this shop wont trigger, but its a placeholdere for admin for quick equipment check
        /// </summary>
        public bool simulated;

        [ShowIf("simulated")]
        public int addGold;
        
        [VerticalGroup("ShopSet")]
        public float priceMultiplier;
        
        [VerticalGroup("ShopSet")]
        public List<TriggeredShopSet> shopSet;

        [HideInEditorMode]
        [Button]
        public void Show()
        {
            if (simulated && addGold > 0)
            {
                Gamesystem.instance.progress.AddGold(addGold);
            }
            
            Gamesystem.instance.uiManager.OpenRewardSetWindow(shopSet, title, this);
        }
        
        public int GetGoldAcumulatedRequired()
        {
            return (int) (goldAcumulatedRequired * Gamesystem.instance.goldMul);
        }
        
        public int GetExpAcumulatedRequired()
        {
            return (int) (expAcumulatedRequired * Gamesystem.instance.expMul);
        }
    }
    
    [Serializable]
    public class TriggeredShopSet
    {
        public string name;

        public bool showOnlyPreviouslyLocked;

        public int shownItemsCount;

        public bool forcePriceForAllItems;
        
        [ShowIf("forcePriceForAllItems")]
        public float priceForAllItems;

        [FormerlySerializedAs("hideForNextShopOccurences")] public int ifBoughtHideForNextShopOccurences;
    }
}