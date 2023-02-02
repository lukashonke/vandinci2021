using System;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Persistence;
using InControl;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.System
{
    public class PlayerProgressManager : SerializedMonoBehaviour
    {
        public PlayerProgressData progressData;

        public bool loadFileFromStart = true;
        public bool applyRunOnStart = false;
        
        public void Awake()
        {
            if (loadFileFromStart)
            {
                this.progressData = LoadFile();
            }
        }

        public void Start()
        {
            Load(applyRunOnStart);
        }

        private PlayerProgressData LoadFile()
        {
            PlayerProgressData progressData = PersistenceUtils.LoadState(PersistenceUtils.GetDefaultSaveName());

            if (progressData == null)
            {
                progressData = new PlayerProgressData();
            }

            return progressData;
        }

        private void InitializeProgressData()
        {
            //TODO restore player data
        }

        public void ApplyRunToPlayer(Player player, PlayerRun run)
        {
            var db = Gamesystem.instance.prefabDatabase;
            var body = db.GetById(run.bodyId);
            
            player.SetBody(body.prefab);

            if (run.modulesInSlots != null)
            {
                foreach (var moduleInSlot in run.modulesInSlots)
                {
                    var slot = player.GetSlotById(moduleInSlot.slotId);
                    if (slot == null)
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
                
                    player.SetModuleInSlot(slot, modulePrefab.prefab, moduleInSlot.level, moduleInSlot.rotation);
                }
            }
            
            player.RemoveSkills();

            if (run.skillPrefabIds != null)
            {
                foreach (var skill in run.skillPrefabIds)
                {
                    var skillPrefab = db.GetById(skill.prefabId);
                    if (skillPrefab == null)
                    {
                        Debug.LogError($"Prefab {skill} does not exist.");
                        continue;
                    }
                    
                    player.AddSkill(skillPrefab.skill);
                }
            }
            
            player.RemoveMutators();
            
            if (run.mutatorPrefabIds != null)
            {
                foreach (var skill in run.mutatorPrefabIds)
                {
                    var mutatorPrefab = db.GetById(skill.prefabId);
                    if (mutatorPrefab == null)
                    {
                        Debug.LogError($"Prefab {skill} does not exist.");
                        continue;
                    }
                    
                    player.AddMutator(mutatorPrefab.mutator);
                }
            }
        }
        
        [Button]
        public void Save()
        {
            PersistenceUtils.SaveState(PersistenceUtils.GetDefaultSaveName(), this.progressData);
        }
        
        [Button]
        public void Load(bool applyRun)
        {
            this.InitializeProgressData();

            if (applyRun)
            {
                ApplyRunToPlayer(Gamesystem.instance.objects.currentPlayer, progressData.run);
            }
        }

        [Button]
        public void Reset()
        {
            PersistenceUtils.ResetState(PersistenceUtils.GetDefaultSaveName());
            this.progressData = LoadFile();
            
            this.InitializeProgressData();
        }
    }
}