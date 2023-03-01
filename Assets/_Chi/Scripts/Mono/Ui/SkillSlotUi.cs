using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class SkillSlotUi : MonoBehaviour
    {
        public int index;
        
        public PrefabItem currentSkill;
        public GameObject moduleGo;
        
        public GameObject highlightGo;

        public void Initialise()
        {
            SetHighlighted(false);
            
            var run = Gamesystem.instance.progress.progressData.run;

            var db = Gamesystem.instance.prefabDatabase;
            
            if (run.skillPrefabIds != null)
            {
                foreach (var skillInSlot in run.skillPrefabIds)
                {
                    if (skillInSlot.slot != index)
                    {
                        continue;
                    }

                    var skillPrefab = db.GetById(skillInSlot.prefabId);
                    if (skillPrefab == null)
                    {
                        Debug.LogError($"Prefab {skillInSlot.prefabId} does not exist.");
                        continue;
                    }
                
                    SetPrefab(skillPrefab);
                }
            }
        }
        
        private bool TryAddCurrentlyAddingSkill()
        {
            var addingSkill = Gamesystem.instance.uiManager.addingUiItem;
            
            if (addingSkill == null)
            {
                return false;
            }

            if (addingSkill.prefab.skillUpgradeItem == null && addingSkill.prefab.type != PrefabItemType.Skill)
            {
                return false;
            }
            
            if(addingSkill.prefab.skillUpgradeItem != null && currentSkill != null && !CanApplyUpgradeItem(addingSkill))
            {
                return false;
            }

            if (addingSkill.prefab.skillUpgradeItem != null)
            {
                string title = "Confirm";
                string text = "Are you sure?";
            
                //TODO hlaska podle typu
            
                Gamesystem.instance.uiManager.ShowConfirmDialog(title, text, () => TrySetSkillUpgradeItem(addingSkill), () =>
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
            
                //TODO hlaska podle typu
            
                Gamesystem.instance.uiManager.ShowConfirmDialog(title, text, () => TrySetSkill(addingSkill), () =>
                {
                    Gamesystem.instance.uiManager.SetAddingUiItem(null);

                }, () =>
                {
                    Gamesystem.instance.uiManager.SetAddingUiItem(null);
                });
            }
            return true;
        }
        
        public void TrySetSkill(AddingUiItem module)
        {
            if (Gamesystem.instance.uiManager.vehicleSettingsWindow.SetSkill(this, module.prefab))
            {
                module.finishCallback?.Invoke();
            }
        }
        
        public void TrySetSkillUpgradeItem(AddingUiItem module)
        {
            if (Gamesystem.instance.uiManager.vehicleSettingsWindow.AddSkillUpgrade(this, module.prefab))
            {
                module.finishCallback?.Invoke();
            }
        }
        
        public void SetPrefab(PrefabItem item)
        {
            if (moduleGo != null)
            {
                Destroy(moduleGo);
            }

            if (item != null)
            {
                var newModule = Instantiate(item.prefabUi, this.transform.position, Quaternion.identity, this.transform);
                
                moduleGo = newModule;
                currentSkill = item;
            }
        }
        
        public void NotifyAddingItem(AddingUiItem item)
        {
            bool highlight = item != null 
                             && (item.prefab.type == PrefabItemType.Skill || (currentSkill != null && item.prefab.skillUpgradeItem != null && CanApplyUpgradeItem(item)));
            
            SetHighlighted(highlight);
        }

        private bool CanApplyUpgradeItem(AddingUiItem item)
        {
            if (item.prefab.skillUpgradeItem.target != currentSkill.skill) return false;
            
            var run = Gamesystem.instance.progress.progressData.run;

            if (run.skillUpgradeItems != null)
            {
                if (run.skillUpgradeItems.Any(s => s.prefabId == item.prefab.id))
                {
                    return false;
                }
            }

            return true;
        }

        public void OnClick()
        {
            if (TryAddCurrentlyAddingSkill())
            {
                Gamesystem.instance.uiManager.RemoveActionsPanel();
                return;
            }
        }
        
        public void OnHoverEnter()
        {
            if (moduleGo != null && currentSkill != null)
            {
                var upgrades = new List<UpgradeItem>();
                
                var run = Gamesystem.instance.progress.progressData.run;
                var db = Gamesystem.instance.prefabDatabase;

                if (run.skillUpgradeItems != null)
                {
                    foreach (var upgradeItem in run.skillUpgradeItems)
                    {
                        var upgradeItemPrefab = db.GetById(upgradeItem.prefabId);
                        if (upgradeItemPrefab == null)
                        {
                            Debug.LogError($"Prefab {upgradeItem.prefabId} does not exist.");
                            continue;
                        }
                        
                        if (upgradeItemPrefab.skillUpgradeItem.target == currentSkill.skill)
                        {
                            upgrades.Add(upgradeItemPrefab.skillUpgradeItem);
                        }
                    }
                }
                
                Gamesystem.instance.uiManager.ShowItemTooltip((RectTransform) this.transform, currentSkill, 0, UiManager.TooltipAlign.TopRight, UiManager.TooltipType.Default, upgrades);
            }
        }

        public void OnHoverExit()
        {
            if (moduleGo != null && currentSkill != null)
            {
                Gamesystem.instance.uiManager.HideTooltip();
            }
        }
        
        public void SetHighlighted(bool b)
        {
            highlightGo.SetActive(b);
        }
    }
}