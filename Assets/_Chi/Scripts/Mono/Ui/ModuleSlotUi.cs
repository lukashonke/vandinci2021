using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class ModuleSlotUi : MonoBehaviour
    {
        [NonSerialized] public int slotId;
        
        public GameObject highlightGo;

        public int rotation = 0;
        
        public ModuleSlotType slotType;
        
        public List<ModuleSlotUi> connectedTo;

        public GameObject moduleGo;
        public GameObject onHoverGo;
        public PrefabItem modulePrefabItem;
        public int moduleLevel;

        private PlayerBodyUi parentBody;

        public void Initialise(PlayerBodyUi parentBody)
        {
            this.parentBody = parentBody;
            slotId = this.transform.GetSiblingIndex();
            
            SetHighlighted(false);
        }

        public void SetModulePrefab(PrefabItem item, int level = 1, int rotation = 0)
        {
            if (moduleGo != null)
            {
                Destroy(moduleGo);
            }

            if (item != null)
            {
                var newModule = Instantiate(item.prefabUi, this.transform.position, Quaternion.identity, this.transform);
                
                moduleGo = newModule;
                onHoverGo = moduleGo.transform.Find("OnHover")?.gameObject;
                modulePrefabItem = item;
                moduleLevel = level;
                SetRotation(rotation);

                if (onHoverGo != null)
                {
                    onHoverGo.SetActive(false);
                }
            }
        }

        private bool TryAddCurrentlyAddingModule()
        {
            var addingModule = Gamesystem.instance.uiManager.addingUiItem;
            
            if (addingModule == null)
            {
                return false;
            }
            
            if (!CanAcceptModule(addingModule))
            {
                return false;
            }

            if (addingModule.prefab.moduleUpgradeItem != null)
            {
                if (!CanApplyUpgradeItem(addingModule))
                {
                    return false;
                }
                
                string title = "Confirm";
                string text = "Do you want to upgrade this weapon?";
            
                Gamesystem.instance.uiManager.ShowConfirmDialog(title, text, () => AddModuleUpgradeItemToSlot(addingModule), () =>
                {
                    Gamesystem.instance.uiManager.SetAddingUiItem(null);

                }, () =>
                {
                    Gamesystem.instance.uiManager.SetAddingUiItem(null);
                });
            }
            else
            {
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
            }

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
                buttons.Add(new ActionsPanelButton("Rotate Left", RotateLeft, ActionsPanelButtonType.Default));
                buttons.Add(new ActionsPanelButton("Rotate Right", RotateRight, ActionsPanelButtonType.Default));
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
                var upgrades = new List<UpgradeItem>();
                
                var run = Gamesystem.instance.progress.progressData.run;
                var db = Gamesystem.instance.prefabDatabase;
                
                var slot = run.modulesInSlots?.FirstOrDefault(x => x.slotId == slotId);

                if (slot != null)
                {
                    foreach (var upgradeItem in slot.upgradeItems)
                    {
                        var upgradeItemPrefab = db.GetById(upgradeItem.prefabId);
                        if (upgradeItemPrefab == null)
                        {
                            Debug.LogError($"Prefab {upgradeItem.prefabId} does not exist.");
                            continue;
                        }
                        
                        upgrades.Add(upgradeItemPrefab.moduleUpgradeItem);
                    }
                }

                var tooltipLevel = moduleLevel;

                if (modulePrefabItem.prefab.GetComponent<Module>() is OffensiveModule o)
                {
                    tooltipLevel = GetUpgradeItems();
                }
                
                //TODO use actual module instance if available to show level
                Gamesystem.instance.uiManager.ShowItemTooltip((RectTransform) this.transform, modulePrefabItem, tooltipLevel, UiManager.TooltipAlign.TopRight, UiManager.TooltipType.Default, upgrades, modulePrefabItem.prefab.GetComponent<Module>().maxLevel);
            }
            
            if (onHoverGo != null)
            {
                onHoverGo.SetActive(true);
            }
        }
        
        private bool CanApplyUpgradeItem(AddingUiItem item)
        {
            var run = Gamesystem.instance.progress.progressData.run;

            var slot = run.modulesInSlots?.FirstOrDefault(x => x.slotId == slotId);
            if (slot != null)
            {
                var db = Gamesystem.instance.prefabDatabase;
                var module = db.GetById(slot.moduleId).prefab.GetComponent<Module>();
                
                if (slot.upgradeItems.Count >= module.maxUpgrades)
                {
                    if (item.prefab.moduleUpgradeItem.replacesModulePrefabIds.HasValues())
                    {
                        if (!slot.upgradeItems.Any(s => item.prefab.moduleUpgradeItem.replacesModulePrefabIds.Contains(s.prefabId)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                
                if (slot.upgradeItems.Any(si => si.prefabId == item.prefab.id))
                {
                    return false;
                }
            }

            return true;
        }

        private int GetUpgradeItems()
        {
            var run = Gamesystem.instance.progress.progressData.run;

            var slot = run.modulesInSlots?.FirstOrDefault(x => x.slotId == slotId);
            if (slot != null)
            {
                return slot.upgradeItems.Count;
            }

            return 0;
        }

        public void OnHoverExit()
        {
            if (moduleGo != null && modulePrefabItem != null)
            {
                Gamesystem.instance.uiManager.HideTooltip();
            }
            
            if (onHoverGo != null)
            {
                onHoverGo.SetActive(false);
            }
        }

        public void RotateLeft()
        {
            rotation += 90;
            rotation %= 360;
            moduleGo.transform.rotation = Quaternion.Euler(0, 0, rotation);
            parentBody.SetModuleInSlotRotation(this);
        }

        public void RotateRight()
        {
            rotation -= 90;
            rotation %= 360;
            moduleGo.transform.rotation = Quaternion.Euler(0, 0, rotation);
            parentBody.SetModuleInSlotRotation(this);
        }

        public void SetRotation(int angle)
        {
            rotation = angle;
            moduleGo.transform.rotation = Quaternion.Euler(0, 0, rotation);
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

        public bool AddModuleUpgradeItemToSlot(AddingUiItem item)
        {
            return parentBody.AddModuleUpgradeItemToSlot(this, item);
        }

        public bool AddModuleToSlot(AddingUiItem module)
        {
            if (CanMergeModule(module))
            {
                return parentBody.MergeModuleInSlot(this, module);
            }
            else
            {
                return parentBody.AddModuleToSlot(this, module, Math.Max(1, module.level));
            }
        }

        public void NotifyAddingModule(AddingUiItem moduleCandidate)
        {
            bool highlight = moduleCandidate != null && CanAcceptModule(moduleCandidate);
            
            if (highlight && moduleGo != null && !CanMergeModule(moduleCandidate) && moduleCandidate.prefab.moduleUpgradeItem == null)
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
            if (info != null && info.prefabModule != null)
            {
                int newLevel = moduleLevel + info.level;
                
                if(newLevel > info.prefabModule.maxLevel) return false;
            }
            
            if (moduleGo != null && modulePrefabItem != null && info != null)
            {
                return modulePrefabItem.id == info.prefab.id;
            }

            return false;
        }

        public bool CanAcceptModule(AddingUiItem item)
        {
            var module = item.prefabModule;

            if (module != null)
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
            }

            if (item.prefab.moduleUpgradeItem != null)
            {
                if (modulePrefabItem != null && modulePrefabItem.id == item.prefab.moduleUpgradeItem.modulePrefabId && CanApplyUpgradeItem(item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}