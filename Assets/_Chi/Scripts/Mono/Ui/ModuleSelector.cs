using System;
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

        private ModuleSelectorMode currentMode;
        private List<RewardSet> currentRewardSets;
        [NonSerialized] public Dictionary<PrefabItem, GameObject> options;

        public void Start()
        {
            currentRewardSets = new();
            //Initialise(ModuleSelectorMode.ShowAllItems, true);
        }

        public void Initialise(ModuleSelectorMode mode, bool canExit, string rewardSets = null, string title = null, TriggeredShop triggeredShop = null)
        {
            var db = Gamesystem.instance.prefabDatabase;

            exitGo.SetActive(canExit);

            transform.RemoveAllChildren();

            currentMode = mode;

            if (title != null)
            {
                var newTitle = Instantiate(titlePrefab, transform.position, Quaternion.identity, transform);
                var text = newTitle.GetComponentInChildren<TextMeshProUGUI>();
                text.text = title;
                text.enabled = true;

                newTitle.gameObject.GetComponentsInChildren<Image>().ForEach(s => s.enabled = true);
            }
            
            var run = Gamesystem.instance.progress.progressData.run;

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
                    options.Add(item, newItem);
                    var newItemItem = newItem.GetComponent<ModuleSelectorItem>();
                    
                    newItemItem.Initialise(item, new List<ActionsPanelButton>()
                    {
                        new ActionsPanelButton("Add", () => StartAddingItem(item, null))
                    }, AbortAddingItem, 0);
                }
            }
            else if (currentMode == ModuleSelectorMode.ShopSet || currentMode == ModuleSelectorMode.SingleRewardSet)
            {
                foreach (var rewardSet in rewardSets.Split(";"))
                {
                    var set = Gamesystem.instance.prefabDatabase.GetRewardSet(rewardSet);
                    
                    currentRewardSets.Add(set);

                    var shownItems = set.CalculateShownItems(Gamesystem.instance.objects.currentPlayer).ToHashSet();
                    var shownItemsHash = shownItems.Select(s => s.prefabId).ToHashSet();

                    foreach (var rewardSetItemWithWeight in shownItems)
                    {
                        var item = db.GetById(rewardSetItemWithWeight.prefabId);
                        if (!item.enabled) continue;
                    
                        var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
                        options.Add(item, newItem);
                        var newItemItem = newItem.GetComponent<ModuleSelectorItem>();

                        var count = run.GetCountOfPrefabs(item.id);
                        
                        int? price = (int?) (rewardSetItemWithWeight.price * (triggeredShop?.priceMultiplier ?? 1));
                        int? priceToAdd = (int?) (price * (count * rewardSetItemWithWeight.priceMultiplyForEveryOwned));
                        
                        price += priceToAdd;

                        newItemItem.Initialise(item, new List<ActionsPanelButton>()
                        {
                            new ActionsPanelButton("Buy", () => StartAddingItem(item, price))
                        }, AbortAddingItem, price);
                    }
                }
            }
        }

        public void StartAddingItem(PrefabItem item, int? price)
        {
            var info = new AddingUiItem()
            {
                prefab = item,
                prefabModule = item.prefab != null ? item.prefab.GetComponent<Module>() : null,
                type = AddingModuleInfoType.Add,
                level = 1
            };

            if (item.playerUpgradeItem != null)
            {
                Gamesystem.instance.uiManager.ShowConfirmDialog("Confirm upgrade", "Are you sure you want to buy this upgrade?", () =>
                {
                    if (Gamesystem.instance.uiManager.vehicleSettingsWindow.AddPlayerUpgradeItem(item))
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
            
            if (currentRewardSets.Any(rs => rs.closeOnFirstPurchase))
            {
                Gamesystem.instance.uiManager.vehicleSettingsWindow.CloseWindow();
            }
            
            if (options.TryGetValue(addingUiItem.prefab, out var go))
            {
                Destroy(go);
                options.Remove(addingUiItem.prefab);
            }
        }

        public void AbortAddingItem()
        {
            Gamesystem.instance.uiManager.SetAddingUiItem(null);
        }

        public bool CanClose()
        {
            return currentMode == ModuleSelectorMode.ShowAllItems;
        }
    }

    public enum ModuleSelectorMode
    {
        ShowAllItems,
        ShopSet,
        SingleRewardSet
    }
}