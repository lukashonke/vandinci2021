using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Initialise(ModuleSelectorMode mode, bool canExit, string rewardSet = null, string title = null)
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
                        new ActionsPanelButton("Add", () => StartAddingItem(item))
                    }, AbortAddingItem);
                }
            }
            else if (currentMode == ModuleSelectorMode.RewardSet)
            {
                var set = Gamesystem.instance.prefabDatabase.GetRewardSet(rewardSet);

                var shownItems = set.CalculateShownItems(Gamesystem.instance.objects.currentPlayer).ToHashSet();
                
                foreach (var item in db.prefabs.Where(t => shownItems.Contains(t.id) && t.enabled))
                {
                    var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
                    var newItemItem = newItem.GetComponent<ModuleSelectorItem>();
                    
                    newItemItem.Initialise(item, new List<ActionsPanelButton>()
                    {
                        new ActionsPanelButton("Add", () => StartAddingItem(item))
                    }, AbortAddingItem);
                }
            }
        }

        public void StartAddingItem(PrefabItem item)
        {
            var info = new AddingUiItem()
            {
                prefab = item,
                prefabModule = item.prefab != null ? item.prefab.GetComponent<Module>() : null,
                type = AddingModuleInfoType.Add,
                level = 1
            };

            info.finishCallback = () => ModuleAdded(info);
            
            Gamesystem.instance.uiManager.SetAddingUiItem(info);
        }

        public void ModuleAdded(AddingUiItem addingUiItem)
        {
            if (currentMode == ModuleSelectorMode.RewardSet)
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
            return currentMode != ModuleSelectorMode.RewardSet;
        }
    }

    public enum ModuleSelectorMode
    {
        ShowAllItems,
        RewardSet
    }
}