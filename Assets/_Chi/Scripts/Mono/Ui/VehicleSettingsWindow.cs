using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Pathfinding.Ionic.Zip;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class VehicleSettingsWindow : MonoBehaviour
    {
        [Required] public GameObject container;

        [Required] public ModuleSelector moduleSelector;

        [NonSerialized] public PlayerBodyUi ui;

        [NonSerialized] public SkillSlotUi[] skillSlots;
        
        [NonSerialized] public MutatorSlotUi[] mutatorSlots;

        [Required] public TextMeshProUGUI playerGold;

        public void Awake()
        {
            gameObject.SetActive(false);
        }

        public void OpenWindow(List<TriggeredShopSet> rewardSet, string title, TriggeredShop triggeredShop)
        {
            UpdateCurrentMoney();
                
            if (rewardSet == null && triggeredShop == null)
            {
                moduleSelector.Initialise(ModuleSelectorMode.ShowAllItems, true, null, title, null);
            }
            else if(triggeredShop != null)
            {
                moduleSelector.Initialise(ModuleSelectorMode.ShopSet, true, rewardSet, title, triggeredShop);
            }
            else
            {
                moduleSelector.Initialise(ModuleSelectorMode.SingleRewardSet, true, rewardSet, title, null);
            }

            Initialise();
            this.gameObject.SetActive(true);
            Gamesystem.instance.Pause();
            
            Gamesystem.instance.uiManager.UpdateFullscreenOverlay();
        }

        public void UpdateCurrentMoney()
        {
            playerGold.text = Gamesystem.instance.progress.progressData.run.gold.ToString();
        }

        public void CloseWindow()
        {
            this.gameObject.SetActive(false);
            Gamesystem.instance.Unpause();
                
            ApplyOnClose();
            
            Gamesystem.instance.uiManager.UpdateFullscreenOverlay();
        }
        
        public void Toggle(bool showAllModules)
        {
            if (this.gameObject.activeSelf)
            {
                CloseWindow();
            }
            else
            {
                OpenWindow(null, null, null);
            }
        }

        public bool Opened()
        {
            return this.gameObject.activeSelf;
        }

        public void Close()
        {
            if (moduleSelector.CanClose() || moduleSelector.options.Count == 0)
            {
                DoClose();
            }
            else
            {
                Gamesystem.instance.uiManager.ShowConfirmDialog("Leave Shop?", "Have you spent your gold wisely?", 
                    DoClose, () => {}, () => {});
            }
        }

        private void DoClose()
        {
            ApplyOnClose();
            
            this.gameObject.SetActive(false);
            Gamesystem.instance.Unpause();
            
            Gamesystem.instance.uiManager.UpdateFullscreenOverlay();
        }

        public void ApplyOnClose()
        {
            Gamesystem.instance.progress.Save();
            Gamesystem.instance.progress.ApplyRunToPlayer(Gamesystem.instance.objects.currentPlayer, Gamesystem.instance.progress.progressData.run);
        }

        public void OnBodyChange()
        {
            Initialise();
        }

        public void Initialise()
        {
            var currentBody = Gamesystem.instance.progress.progressData.run.bodyId;

            var db = Gamesystem.instance.prefabDatabase;

            var bodyUi = db.GetById(currentBody).prefabUi;
            
            container.transform.RemoveAllChildren();

            var newBodyUi = Instantiate(bodyUi, container.transform);

            ui = newBodyUi.GetComponent<PlayerBodyUi>();
            
            ui.Initialise();

            skillSlots = GetComponentsInChildren<SkillSlotUi>();

            foreach (var slot in skillSlots)
            {
                slot.Initialise();
            }
            
            mutatorSlots = GetComponentsInChildren<MutatorSlotUi>();

            foreach (var slot in mutatorSlots)
            {
                slot.Initialise();
            }
        }

        public bool SetSkill(SkillSlotUi slot, PrefabItem skillItem)
        {
            var run = Gamesystem.instance.progress.progressData.run;

            if (run.skillPrefabIds == null) run.skillPrefabIds = new List<SlotItem>();

            var existing = run.skillPrefabIds.FirstOrDefault(s => s.slot == slot.index);

            if (existing != null)
            {
                run.skillPrefabIds.Remove(existing);
            }
            
            run.skillPrefabIds.Add(new SlotItem()
            {
                prefabId = skillItem.id,
                slot = slot.index
            });

            slot.SetPrefab(skillItem);

            if (run.skillUpgradeItems != null)
            {
                run.skillUpgradeItems = new List<SlotItem>();
            }

            return true;
        }

        public bool AddSkillUpgrade(SkillSlotUi skillSlotUi, PrefabItem item)
        {
            var run = Gamesystem.instance.progress.progressData.run;

            if (run.skillUpgradeItems == null) run.skillUpgradeItems = new List<SlotItem>();

            if (item != null && item.skillUpgradeItem != null)
            {
                if(run.skillUpgradeItems.Any(i => i.prefabId == item.id)) return false;
                
                run.skillUpgradeItems.Add(new SlotItem()
                {
                    prefabId = item.id,
                    slot = 0,
                });

                return true;
            }

            return false;
        }

        public bool AddPlayerUpgradeItem(PrefabItem item) 
        {
            var run = Gamesystem.instance.progress.progressData.run;

            if (run.playerUpgradeItems == null) run.playerUpgradeItems = new List<SlotItem>();

            if (item != null && item.playerUpgradeItem != null)
            {
                if(!item.playerUpgradeItem.canBeStacked && run.playerUpgradeItems.Any(i => i.prefabId == item.id)) return false;
                
                run.playerUpgradeItems.Add(new SlotItem()
                {
                    prefabId = item.id,
                    slot = 0,
                });

                return true;
            }

            return false;
        }
        
        public bool SetMutator(MutatorSlotUi slot, PrefabItem skillItem)
        {
            var run = Gamesystem.instance.progress.progressData.run;

            if (run.mutatorPrefabIds == null) run.mutatorPrefabIds = new List<SlotItem>();

            if (skillItem != null)
            {
                run.mutatorPrefabIds.RemoveAll(s => s.slot == slot.index);
                
                run.mutatorPrefabIds.Add(new SlotItem()
                {
                    slot = slot.index,
                    prefabId = skillItem.id
                });    
            }
            else
            {
                run.mutatorPrefabIds.RemoveAll(s => s.slot == slot.index);    
            }

            slot.SetPrefab(skillItem);

            return true;
        }
    }
}