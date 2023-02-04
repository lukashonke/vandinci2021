using System;
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
            
            if (addingSkill == null || addingSkill.prefab.type != PrefabItemType.Skill)
            {
                return false;
            }
            
            //TODO merge passive modules
            
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

            return true;
        }
        
        public void TrySetSkill(AddingUiItem module)
        {
            if (Gamesystem.instance.uiManager.vehicleSettingsWindow.SetSkill(this, module.prefab))
            {
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
        
        public void NotifyAddingItem(AddingUiItem moduleCandidate)
        {
            bool highlight = moduleCandidate != null && moduleCandidate.prefab.type == PrefabItemType.Skill;
            
            SetHighlighted(highlight);
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
                Gamesystem.instance.uiManager.ShowItemTooltip((RectTransform) this.transform, currentSkill, 0);
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