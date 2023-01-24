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
            Initialise(ModuleSelectorMode.ShowAllModules);
        }

        public void Initialise(ModuleSelectorMode mode)
        {
            var db = Gamesystem.instance.prefabDatabase;

            transform.RemoveAllChildren();

            if (mode == ModuleSelectorMode.ShowAllModules)
            {
                foreach (var item in db.prefabs.Where(t => t.type == PrefabItemType.Module))
                {
                    var newItem = Instantiate(itemInfoPrefab, transform.position, Quaternion.identity, transform);
                    var newItemItem = newItem.GetComponent<ModuleSelectorItem>();
                    
                    newItemItem.Initialise(item.prefab.name, item.prefabUi.GetComponent<Image>(), "test", new List<ActionsPanelButton>()
                    {
                        new ActionsPanelButton("Add", () => StartAddingModule(item))
                    }, AbortAddingModule);
                }
            }
        }

        public void StartAddingModule(PrefabItem item)
        {
            var info = new AddingModuleInfo()
            {
                prefab = item,
                prefabModule = item.prefab.GetComponent<Module>(),
                type = AddingModuleInfoType.Add,
                level = 1
            };

            info.finishCallback = () => ModuleAdded(info);
            
            Gamesystem.instance.uiManager.SetAddingModule(info);
        }

        public void ModuleAdded(AddingModuleInfo addingModuleInfo)
        {
            
        }

        public void AbortAddingModule()
        {
            Gamesystem.instance.uiManager.SetAddingModule(null);
        }
    }

    public enum ModuleSelectorMode
    {
        ShowAllModules,
        
    }
}