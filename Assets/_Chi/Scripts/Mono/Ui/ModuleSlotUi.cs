using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables.Dtos;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class ModuleSlotUi : MonoBehaviour
    {
        [NonSerialized] public int slotId;
        
        public GameObject highlightGo;
        
        public ModuleSlotType slotType;
        
        public List<ModuleSlotUi> connectedTo;

        public GameObject moduleGo;
        public PrefabItem modulePrefabItem;
        public int moduleLevel;

        private PlayerBodyUi parentBody;

        public void Initialise(PlayerBodyUi parentBody)
        {
            this.parentBody = parentBody;
            slotId = this.transform.GetSiblingIndex();
            
            SetHighlighted(false);
        }

        public void SetModulePrefab(PrefabItem item, int level = 1)
        {
            if (moduleGo != null)
            {
                Destroy(moduleGo);
            }

            if (item != null)
            {
                var newModule = Instantiate(item.prefabUi, this.transform.position, Quaternion.identity, this.transform);
                
                moduleGo = newModule;
                modulePrefabItem = item;
                moduleLevel = level;
            }
        }

        private bool TryAddCurrentlyAddingModule()
        {
            var addingModule = Gamesystem.instance.uiManager.addingUiItem;
            
            if (addingModule == null)
            {
                return false;
            }
            
            if (!CanAcceptModule(addingModule.prefabModule))
            {
                return false;
            }

            //TODO merge passive modules
            
            string title = "Confirm";
            string text = "Are you sure?";
            
            if (moduleGo != null)
            {
                if (CanMergeModule(addingModule))
                {
                    text = "Do you want to merge two modules?";
                }
                else
                {
                    return false;
                }
            }
            
            //TODO hlaska podle typu
            
            Gamesystem.instance.uiManager.ShowConfirmDialog(title, text, () => AddModuleToSlot(addingModule), () =>
            {
                Gamesystem.instance.uiManager.SetAddingUiItem(null);

            }, () =>
            {
                Gamesystem.instance.uiManager.SetAddingUiItem(null);
            });

            return true;
        }

        public void OnClickModule()
        {
            if (TryAddCurrentlyAddingModule())
            {
                Gamesystem.instance.uiManager.RemoveActionsPanel();
                return;
            }
            
            var buttons = new List<ActionsPanelButton>();

            if (moduleGo != null)
            {
                buttons.Add(new ActionsPanelButton("Remove", RemoveModuleFromSlot, ActionsPanelButtonType.Default));
                buttons.Add(new ActionsPanelButton("Move", MoveModuleToAnotherSlot, ActionsPanelButtonType.Default));
            }

            if (buttons.Any())
            {
                Gamesystem.instance.uiManager.SetActionsPanel(new ActionsPanel
                {
                    source = this,
                    buttons = buttons
                }, (RectTransform) transform);
            }
            else
            {
                Gamesystem.instance.uiManager.RemoveActionsPanel();
            }
        }

        public void OnHoverEnter()
        {
            if (moduleGo != null && modulePrefabItem != null)
            {
                //TODO use actual module instance if available to show level
                Gamesystem.instance.uiManager.ShowModuleTooltip((RectTransform) this.transform, modulePrefabItem, moduleLevel);
            }
        }

        public void OnHoverExit()
        {
            if (moduleGo != null && modulePrefabItem != null)
            {
                Gamesystem.instance.uiManager.HideTooltip();
            }
        }

        public void RemoveModuleFromSlot()
        {
            parentBody.RemoveModuleFromSlot(this);
            
            Gamesystem.instance.uiManager.RemoveActionsPanel();
        }

        public void MoveModuleToAnotherSlot()
        {
            var info = new AddingUiItem()
            {
                prefab = modulePrefabItem,
                prefabModule = modulePrefabItem.prefab.GetComponent<Module>(),
                type = AddingModuleInfoType.Move,
                level = moduleLevel
            };

            info.finishCallback = () => OnModuleMoved(info);
            
            Gamesystem.instance.uiManager.SetAddingUiItem(info);
            
            Gamesystem.instance.uiManager.RemoveActionsPanel();
        }

        public void OnModuleMoved(AddingUiItem info)
        {
            if (info != null)
            {
                //TODO remove from this slot
                parentBody.RemoveModuleFromSlot(this);    
            }
        }

        public bool AddModuleToSlot(AddingUiItem module)
        {
            if (CanMergeModule(module))
            {
                return parentBody.MergeModuleInSlot(this, module);
            }
            else
            {
                return parentBody.AddModuleToSlot(this, module, 1);
            }
        }

        public void NotifyAddingModule(AddingUiItem moduleCandidate)
        {
            bool highlight = moduleCandidate != null && CanAcceptModule(moduleCandidate.prefabModule);
            
            if (moduleGo != null && !CanMergeModule(moduleCandidate))
            {
                highlight = false;
            }
                
            SetHighlighted(highlight);
        }

        public void SetHighlighted(bool b)
        {
            highlightGo.SetActive(b);
        }

        public bool CanMergeModule(AddingUiItem info)
        {
            if (moduleGo != null && modulePrefabItem != null && info != null)
            {
                return modulePrefabItem.id == info.prefab.id;
            }

            return false;
        }

        public bool CanAcceptModule(Module module)
        {
            if (module is OffensiveModule)
            {
                return slotType == ModuleSlotType.Offensive;
            }

            if (module is DefensiveModule)
            {
                return slotType == ModuleSlotType.Defensive;
            }

            if (module is PassiveModule)
            {
                return slotType == ModuleSlotType.Passive;
            }

            return false;
        }
    }
}