﻿using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui
{
    public class ModuleSelector : MonoBehaviour
    {
        [Required] public GameObject titlePrefab;
        [Required] public GameObject itemInfoPrefab;
        [Required] public GameObject exitGo;

        private TextMeshProUGUI rerollPriceText;

        private ModuleSelectorMode currentMode;
        private List<RewardSet> currentRewardSets;
        [NonSerialized] public Dictionary<PrefabItem, OptionData> options;
        
        private List<TriggeredShopSet> rewardSets;
        private TriggeredShop triggeredShop;
        private string title;

        public class OptionData
        {
            public GameObject go;
            public RewardSetItemWithWeight rewardSetItemWithWeight;
            public PrefabItem prefabItem;
            public TriggeredShop triggeredShop;
            public float priceMul;
            public Action onBuy;
            public TriggeredShopSet rewardSet;
        }

        public void Start()
        {
            currentRewardSets = new();
            //Initialise(ModuleSelectorMode.ShowAllItems, true);
        }

        public void Initialise(ModuleSelectorMode mode, bool canExit, List<TriggeredShopSet> rewardSets = null, string title = null, TriggeredShop triggeredShop = null)
        {
            exitGo.SetActive(canExit);

            this.title = title;
            this.rewardSets = rewardSets;
            this.triggeredShop = triggeredShop;
            this.currentMode = mode;
            
            ShowItems(false);
            
            UpdateRerollPrice();
        }

        private void ShowItems(bool isReroll)
        {
            var db = Gamesystem.instance.prefabDatabase;
            var run = Gamesystem.instance.progress.progressData.run;
            
            transform.RemoveAllChildren();
            if (title != null)
            {
                var newTitle = Instantiate(titlePrefab, transform.position, Quaternion.identity, transform);
                var text = newTitle.GetComponentInChildren<TextMeshProUGUI>();
                text.text = title;
                text.enabled = true;
                
                rerollPriceText = newTitle.transform.Find("PriceValue").GetComponent<TextMeshProUGUI>();
                newTitle.transform.Find("RerollButton").GetComponent<Button>().onClick.AddListener(() => Reroll());

                newTitle.gameObject.GetComponentsInChildren<Image>().ForEach(s => s.enabled = true);
            }
            else
            {
                rerollPriceText = null;
            }

            if (currentRewardSets == null)
            {
                currentRewardSets = new();
            }
            currentRewardSets.Clear();
            if(options == null) options = new();
            options.Clear();

            if (currentMode == ModuleSelectorMode.ShowAllItems)
            {
                foreach (var item in db.prefabs.Where(t => 
                             t.enabled &&
                             (t.type == PrefabItemType.Module
                              || t.type == PrefabItemType.Skill
                              || t.type == PrefabItemType.Mutator
                              || t.type == PrefabItemType.UpgradeItemModule
                              || t.type == PrefabItemType.UpgradeItemSkill
                              || t.type == PrefabItemType.UpgradeItemPlayer)
                         ))
                {
                    var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
                    var option = new OptionData()
                    {
                        go = newItem,
                        prefabItem = item,
                        triggeredShop = triggeredShop,
                    };
                    options.Add(item, option);
                    var newItemItem = newItem.GetComponent<ModuleSelectorItem>();
                    
                    newItemItem.Initialise(item, new List<ActionsPanelButton>()
                    {
                        new ActionsPanelButton("Add", () => StartAddingItem(option))
                    }, AbortAddingItem, 0);
                }
            }
            else if (currentMode == ModuleSelectorMode.ShopSet)
            {
                ShowRandomRewardSetItems(isReroll);
            }
        }
        
        private void ShowRandomRewardSetItems(bool isReroll)
        {
            foreach (TriggeredShopSet rewardSet in rewardSets)
            {
                RewardSet setTemplate = Gamesystem.instance.prefabDatabase.GetRewardSet(rewardSet.name);
                
                ShowRewardSetItems(rewardSet, setTemplate, isReroll);

                if (setTemplate.nestedRewardSets != null && setTemplate.nestedRewardSets.Any())
                {
                    foreach (var nested in setTemplate.nestedRewardSets)
                    {
                        RewardSet nestedSetTemplate = Gamesystem.instance.prefabDatabase.GetRewardSet(nested);

                        ShowRewardSetItems(rewardSet, nestedSetTemplate, isReroll);
                    }
                }
            }
        }

        private void ShowRewardSetItems(TriggeredShopSet rewardSet, RewardSet set, bool isReroll)
        {
            var db = Gamesystem.instance.prefabDatabase;
            
            bool showOnlyLocked = rewardSet.showOnlyPreviouslyLocked;

            currentRewardSets.Add(set);

            var shownItems = set.CalculateShownItems(Gamesystem.instance.objects.currentPlayer, locks, showOnlyLocked, isReroll, rewardSet.shownItemsCount > 0 ? rewardSet.shownItemsCount : null).ToHashSet();
            
            //var shownItemsHash = shownItems.Select(s => s.item.prefabId).ToHashSet();

            foreach (var rewardSetItemWithWeight in shownItems)
            {
                var item = db.GetById(rewardSetItemWithWeight.item.prefabId);
                if (!item.enabled) continue;
                    
                var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
                var option = new OptionData()
                {
                    go = newItem,
                    prefabItem = item,
                    rewardSetItemWithWeight = rewardSetItemWithWeight.item,
                    priceMul = rewardSetItemWithWeight.priceMul,
                    triggeredShop = triggeredShop,
                    rewardSet = rewardSet,
                    onBuy = () =>
                    {
                        if (rewardSet.ifBoughtHideForNextShopOccurences > 0)
                        {
                            set.dontShowTimes = rewardSet.ifBoughtHideForNextShopOccurences;    
                        }
                    }
                };
                options.Add(item, option);
                var newItemItem = newItem.GetComponent<ModuleSelectorItem>();

                var price = GetPrice(option);

                newItemItem.Initialise(item, new List<ActionsPanelButton>()
                {
                    new ActionsPanelButton("Buy", () => StartAddingItem(option))
                }, AbortAddingItem, price);
            }
        }

        private void UpdateRerollPrice()
        {
            if (rerollPriceText == null) return;
            
            if (Gamesystem.instance.missionManager.currentMission?.progressSettings?.rerollPrices != null)
            {
                var progress = Gamesystem.instance.progress.progressData;
                var run = progress.run;

                var nextRerollPrice = Gamesystem.instance.missionManager.currentMission.progressSettings.rerollPrices[run.rerolls];
                rerollPriceText.text = nextRerollPrice == 0 ? "Free" : nextRerollPrice.ToString();
            }
            else
            {
                rerollPriceText.text = "Free";
            }
        }

        public void Reroll()
        {
            var progress = Gamesystem.instance.progress.progressData;
            var run = progress.run;

            var nextRerollPrice = Gamesystem.instance.missionManager.currentMission.progressSettings.rerollPrices[run.rerolls];

            if (nextRerollPrice <= run.gold)
            {
                Gamesystem.instance.progress.RemoveGold(nextRerollPrice);
                Gamesystem.instance.uiManager.vehicleSettingsWindow.UpdateCurrentMoney();
                run.rerolls++;
                
                ShowItems(true);
                UpdateRerollPrice();
            }
        }

        private int? GetPrice(OptionData option)
        {
            if (triggeredShop != null && triggeredShop.type == TriggeredShopType.FreeReward) return null;
            if (option.rewardSetItemWithWeight == null) return 0;
            
            var run = Gamesystem.instance.progress.progressData.run;

            var count = run.GetCountOfPrefabs(option.prefabItem.id);
                        
            if (option.rewardSet.forcePriceForAllItems)
            {
                return (int?) (option.rewardSet.priceForAllItems);
            }
            else
            {
                var price = (int?) (option.rewardSetItemWithWeight.price * (option.triggeredShop?.priceMultiplier ?? 1));
                int? priceToAdd = (int?) (price * (count * option.rewardSetItemWithWeight.priceMultiplyForEveryOwned));
                price += priceToAdd;
                return (int?) (price * option.priceMul * Gamesystem.instance.goldMul);
            }
        }

        public void StartAddingItem(OptionData option)
        {
            var prefabItem = option.prefabItem;
            
            var price = GetPrice(option);
            
            var info = new AddingUiItem()
            {
                prefab = prefabItem,
                prefabModule = prefabItem.prefab != null ? prefabItem.prefab.GetComponent<Module>() : null,
                type = AddingModuleInfoType.Add,
                level = 1
            };

            if (prefabItem.playerUpgradeItem != null)
            {
                Gamesystem.instance.uiManager.ShowConfirmDialog("Confirm upgrade", "Are you sure you want to buy this upgrade?", () =>
                {
                    if (Gamesystem.instance.uiManager.vehicleSettingsWindow.AddPlayerUpgradeItem(prefabItem))
                    {
                        ModuleAdded(info, price);
                    }
                }, () =>
                {

                }, () =>
                {

                }, "Confirm", "Cancel");
            }
            else
            {
                info.finishCallback = () => ModuleAdded(info, price);
                Gamesystem.instance.uiManager.SetAddingUiItem(info);
            }
        }

        public void ModuleAdded(AddingUiItem addingUiItem, int? price)
        {
            if (price.HasValue)
            {
                Gamesystem.instance.progress.RemoveGold(price.Value);
                Gamesystem.instance.uiManager.vehicleSettingsWindow.UpdateCurrentMoney();
            }
            
            if (triggeredShop != null && triggeredShop.closeOnFirstPurchase)
            {
                Gamesystem.instance.uiManager.vehicleSettingsWindow.CloseWindow();
            }
            
            if (options.TryGetValue(addingUiItem.prefab, out var option))
            {
                option.onBuy?.Invoke();

                if (addingUiItem.prefab.playerUpgradeItem != null || addingUiItem.prefab.skillUpgradeItem != null ||
                    addingUiItem.prefab.moduleUpgradeItem != null || addingUiItem.prefabModule is OffensiveModule)
                {
                    Destroy(option.go);
                    options.Remove(addingUiItem.prefab);
                }
                else
                {
                    var item = option.go.GetComponent<ModuleSelectorItem>();
                    item.SetPrice(GetPrice(option));
                }
            }
            
            SetLocked(addingUiItem.prefab, false);
        }

        public void AbortAddingItem()
        {
            Gamesystem.instance.uiManager.SetAddingUiItem(null);
        }

        public bool CanClose()
        {
            return currentMode == ModuleSelectorMode.ShowAllItems;
        }

        private Dictionary<int, bool> locks = new(); 

        public bool IsLocked(PrefabItem item)
        {
            if(locks == null) locks = new();
            return locks.TryGetValue(item.id, out var locked) && locked;
        }

        public void SetLocked(PrefabItem item, bool locked)
        {
            if(locks == null) locks = new();
            locks[item.id] = locked;
        }

        public void UnlockAll()
        {
            foreach (var key in locks.Keys.ToArray())
            {
                locks[key] = false;
            }

            foreach (var option in options)
            {
                option.Value.go.GetComponent<ModuleSelectorItem>().SetLocked(false);
            }
        }
    }

    public enum ModuleSelectorMode
    {
        ShowAllItems,
        ShopSet,
    }
}