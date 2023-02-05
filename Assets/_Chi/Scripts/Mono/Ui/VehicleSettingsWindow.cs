using System;
using System.Collections.Generic;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Pathfinding.Ionic.Zip;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class VehicleSettingsWindow : MonoBehaviour
    {
        [Required] public GameObject container;

        [NonSerialized] public PlayerBodyUi ui;

        [NonSerialized] public SkillSlotUi[] skillSlots;
        
        [NonSerialized] public MutatorSlotUi[] mutatorSlots;

        public void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
                Gamesystem.instance.Unpause();
                
                ApplyOnClose();
            }
            else
            {
                Initialise();
                this.gameObject.SetActive(true);
                Gamesystem.instance.Pause();
            }
            
            Gamesystem.instance.uiManager.UpdateFullscreenOverlay();
        }

        public void Close()
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
            
            run.skillPrefabIds.Add(new SlotItem()
            {
                prefabId = skillItem.id,
                slot = slot.index
            });

            slot.SetPrefab(skillItem);

            return true;
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