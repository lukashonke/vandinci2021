using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class MutatorSlotUi : MonoBehaviour
    {
        public int index;
        
        public PrefabItem currentMutator;
        public GameObject moduleGo;
        
        public GameObject highlightGo;

        public MutatorCategory category;

        public void Initialise()
        {
            SetHighlighted(false);
            
            var run = Gamesystem.instance.progress.progressData.run;

            var db = Gamesystem.instance.prefabDatabase;
            
            if (run.mutatorPrefabIds != null)
            {
                foreach (var mutatorInSlot in run.mutatorPrefabIds)
                {
                    if (mutatorInSlot.slot != index)
                    {
                        continue;
                    }

                    var mutatorPrefab = db.GetById(mutatorInSlot.prefabId);
                    if (mutatorPrefab == null)
                    {
                        Debug.LogError($"Prefab {mutatorInSlot.prefabId} does not exist.");
                        continue;
                    }
                
                    SetPrefab(mutatorPrefab);
                }
            }
        }

        public void OnClick()
        {
            if (TryAddCurrentlyAddingSkill())
            {
                Gamesystem.instance.uiManager.RemoveActionsPanel();
                return;
            }
            
            var buttons = new List<ActionsPanelButton>();

            if (moduleGo != null)
            {
                buttons.Add(new ActionsPanelButton("Remove", RemoveMutatorFromSlot, ActionsPanelButtonType.Default));
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
        
        public void RemoveMutatorFromSlot()
        {
            Gamesystem.instance.uiManager.vehicleSettingsWindow.SetMutator(this, null);
            
            Gamesystem.instance.uiManager.RemoveActionsPanel();
        }
        
        private bool TryAddCurrentlyAddingSkill()
        {
            var addingMutator = Gamesystem.instance.uiManager.addingUiItem;
            
            if (addingMutator == null || !CanAccept(addingMutator))
            {
                return false;
            }
            
            //TODO merge passive modules
            
            string title = "Confirm";
            string text = "Are you sure?";
            
            //TODO hlaska podle typu
            
            Gamesystem.instance.uiManager.ShowConfirmDialog(title, text, () => TrySetMutator(addingMutator), () =>
            {
                Gamesystem.instance.uiManager.SetAddingUiItem(null);

            }, () =>
            {
                Gamesystem.instance.uiManager.SetAddingUiItem(null);
            });

            return true;
        }
        
        public void TrySetMutator(AddingUiItem module)
        {
            if (Gamesystem.instance.uiManager.vehicleSettingsWindow.SetMutator(this, module.prefab))
            {
            }
        }
        
        public void OnHoverEnter()
        {
            if (moduleGo != null && currentMutator != null)
            {
                //TODO use actual module instance if available to show level
                Gamesystem.instance.uiManager.ShowModuleTooltip((RectTransform) this.transform, currentMutator, 0);
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
                currentMutator = item;
            }
        }

        public void OnHoverExit()
        {
            if (moduleGo != null && currentMutator != null)
            {
                Gamesystem.instance.uiManager.HideTooltip();
            }
        }
        
        public void NotifyAddingItem(AddingUiItem candidate)
        {
            bool highlight = candidate != null && CanAccept(candidate);
            
            SetHighlighted(highlight);
        }

        public bool CanAccept(AddingUiItem moduleCandidate)
        {
            return moduleCandidate.prefab.type == PrefabItemType.Mutator
                   && moduleCandidate.prefab.mutator.category == category;
        }
        
        public void SetHighlighted(bool b)
        {
            highlightGo.SetActive(b);
        }
    }
}