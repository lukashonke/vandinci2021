using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui
{
    public class ModuleSelector : MonoBehaviour
    {
        [Required] public GameObject itemInfoPrefab;
        
        public void Start()
        {
            Initialise(ModuleSelectorMode.ShowAllItems);
        }

        public void Initialise(ModuleSelectorMode mode)
        {
            var db = Gamesystem.instance.prefabDatabase;

            transform.RemoveAllChildren();

            if (mode == ModuleSelectorMode.ShowAllItems)
            {
                foreach (var item in db.prefabs.Where(t => 
                             t.type == PrefabItemType.Module
                             || t.type == PrefabItemType.Skill
                             || t.type == PrefabItemType.Mutator
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
            
        }

        public void AbortAddingItem()
        {
            Gamesystem.instance.uiManager.SetAddingUiItem(null);
        }
    }

    public enum ModuleSelectorMode
    {
        ShowAllItems,
        
    }
}