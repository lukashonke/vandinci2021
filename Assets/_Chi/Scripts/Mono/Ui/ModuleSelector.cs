using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.Modules;
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
        
        public void Start()
        {
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

            if (currentMode == ModuleSelectorMode.ShowAllItems)
            {
                foreach (var item in db.prefabs.Where(t => 
                             t.enabled &&
                             (t.type == PrefabItemType.Module
                             || t.type == PrefabItemType.Skill
                             || t.type == PrefabItemType.Mutator)
                         ))
                {
                    var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
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

                    var shownItems = set.CalculateShownItems(Gamesystem.instance.objects.currentPlayer).ToHashSet();
                    var shownItemsHash = shownItems.Select(s => s.prefabId).ToHashSet();

                    foreach (var rewardSetItemWithWeight in shownItems)
                    {
                        var item = db.GetById(rewardSetItemWithWeight.prefabId);
                    
                        var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
                        var newItemItem = newItem.GetComponent<ModuleSelectorItem>();

                        newItemItem.Initialise(item, new List<ActionsPanelButton>()
                        {
                            new ActionsPanelButton("Buy", () => StartAddingItem(item, rewardSetItemWithWeight.price))
                        }, AbortAddingItem, (int?) (rewardSetItemWithWeight.price * (triggeredShop?.priceMultiplier ?? 1)));
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

            info.finishCallback = () => ModuleAdded(info, price);
            
            Gamesystem.instance.uiManager.SetAddingUiItem(info);
        }

        public void ModuleAdded(AddingUiItem addingUiItem, int? price)
        {
            if (price.HasValue)
            {
                Gamesystem.instance.progress.RemoveGold(price.Value);
                Gamesystem.instance.uiManager.vehicleSettingsWindow.UpdateCurrentMoney();
            }
            
            if (currentMode == ModuleSelectorMode.SingleRewardSet)
            {
                Gamesystem.instance.uiManager.vehicleSettingsWindow.CloseWindow();
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