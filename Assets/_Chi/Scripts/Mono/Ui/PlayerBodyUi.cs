using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Persistence;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class PlayerBodyUi : MonoBehaviour
    {
        public List<ModuleSlotUi> slots;

        public void Initialise()
        {
            var run = Gamesystem.instance.progress.progressData.run;

            var db = Gamesystem.instance.prefabDatabase;

            slots = GetComponentsInChildren<ModuleSlotUi>().ToList();

            foreach (var slot in slots)
            {
                slot.Initialise(this);
            }
            
            if (run.modulesInSlots != null)
            {
                foreach (var moduleInSlot in run.modulesInSlots)
                {
                    var uiSlot = GetSlotById(moduleInSlot.slotId);
                    if (uiSlot == null)
                    {
                        Debug.LogError($"Slot {moduleInSlot.slotId} does not exist.");
                        continue;
                    }

                    var modulePrefab = db.GetById(moduleInSlot.moduleId);
                    if (modulePrefab == null)
                    {
                        Debug.LogError($"Prefab {moduleInSlot.moduleId} does not exist.");
                        continue;
                    }
                
                    uiSlot.SetModulePrefab(modulePrefab, moduleInSlot.level, moduleInSlot.rotation);
                }
            }
        }

        public ModuleSlotUi GetSlotById(int id)
        {
            return slots.FirstOrDefault(s => s.slotId == id);
        }
        
        public void SetModuleInSlotRotation(ModuleSlotUi slot)
        {
            var run = Gamesystem.instance.progress.progressData.run;

            var persistedSlot = run.modulesInSlots.FirstOrDefault(s => s.slotId == slot.slotId);
            if (persistedSlot != null)
            {
                persistedSlot.rotation = slot.rotation;
            }
        }

        public void RemoveModuleFromSlot(ModuleSlotUi slot)
        {
            slot.SetModulePrefab(null);
            
            var run = Gamesystem.instance.progress.progressData.run;

            var persistedSlot = run.modulesInSlots.FirstOrDefault(s => s.slotId == slot.slotId);

            if (persistedSlot != null)
            {
                run.modulesInSlots.Remove(persistedSlot);
            }
        }

        public bool MergeModuleInSlot(ModuleSlotUi slot, AddingUiItem module)
        {
            return AddModuleToSlot(slot, module, slot.moduleLevel + module.level);
        }

        public bool AddModuleToSlot(ModuleSlotUi uiSlot, AddingUiItem module, int level)
        {
            RemoveModuleFromSlot(uiSlot);
            
            var run = Gamesystem.instance.progress.progressData.run;

            var db = Gamesystem.instance.prefabDatabase;

            var prefab = db.GetById(module.prefab.id);
            
            if (prefab != null)
            {
                run.modulesInSlots.Add(new ModuleInSlot()
                {
                    slotId = uiSlot.slotId,
                    moduleId = prefab.id,
                    level = level
                });
                
                uiSlot.SetModulePrefab(prefab, level, 0);
                
                module.finishCallback?.Invoke();
                
                Gamesystem.instance.uiManager.SetAddingUiItem(null);

                return true;
            }

            return false;
        }
    }
}