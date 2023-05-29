using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables;
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
        public bool resetStatsOnStart = false;

        [NonSerialized] public bool disabledRewards = false;
        
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

        public void ResetRun()
        {
            progressData.run = new PlayerRun();
            progressData.run.bodyId = 1002;
            
            progressData.run.modulesInSlots = new ();
            progressData.run.moduleUpgradeItems = new();
            progressData.run.skillUpgradeItems = new();
            progressData.run.playerUpgradeItems = new();
            progressData.run.skillPrefabIds = new();
            progressData.run.mutatorPrefabIds = new();
            
            Gamesystem.instance.missionManager.OnPlayerDie();
        }

        public void ApplyRunToPlayer(Player player, PlayerRun run)
        {
            var db = Gamesystem.instance.prefabDatabase;
            var body = db.GetById(run.bodyId);
            
            player.SetBody(body.prefab);
            player.RemoveSkills();
            player.ToggleModuleUpgradesForPlayer(false);
            
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

                    var upgrades = moduleInSlot.upgradeItems?.Select(si => db.GetById(si.prefabId).moduleUpgradeItem).ToList() ?? new List<ModuleUpgradeItem>();
                
                    player.SetModuleInSlot(slot, modulePrefab.prefab, moduleInSlot.level, moduleInSlot.rotation, upgrades);
                }
            }
            
            player.ToggleModuleUpgradesForPlayer(true);
            
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
            
            player.RemovePlayerUpgradeItems();

            if (run.playerUpgradeItems != null)
            {
                foreach (var playerUpgradeItem in run.playerUpgradeItems)
                {
                    var prefabItem = db.GetById(playerUpgradeItem.prefabId);
                    if (prefabItem == null)
                    {
                        Debug.LogError($"Prefab {playerUpgradeItem.prefabId} does not exist.");
                        continue;
                    }
                    
                    player.AddPlayerUpgradeItem(prefabItem.playerUpgradeItem);
                }
            }
            
            player.RemoveSkillUpgradeItems();
            
            if (run.skillUpgradeItems != null)
            {
                foreach (var skillUpgradeItem in run.skillUpgradeItems)
                {
                    var prefabItem = db.GetById(skillUpgradeItem.prefabId);
                    if (prefabItem == null)
                    {
                        Debug.LogError($"Prefab {skillUpgradeItem.prefabId} does not exist.");
                        continue;
                    }
                    
                    player.AddSkillUpgradeItem(prefabItem.skillUpgradeItem);
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

            if (resetStatsOnStart)
            {
                progressData.run.gold = 0;
                progressData.run.acumulatedGold = 0;
                progressData.run.killed = 0;
                progressData.run.rerolls = 0;
            }
        }

        [Button]
        public void Reset()
        {
            PersistenceUtils.ResetState(PersistenceUtils.GetDefaultSaveName());
            this.progressData = LoadFile();
            
            this.InitializeProgressData();
        }
        
        public void AddExp(int exp, bool countToProgress = true)
        {
            this.progressData.run.exp += exp;
            this.progressData.run.acumulatedExp += exp;
            
            if (countToProgress)
            {
                Gamesystem.instance.uiManager.rewardProgressBar.AddValue(exp);
                
                Gamesystem.instance.prefabDatabase.playerExpReceived.Spawn(Gamesystem.instance.objects.currentPlayer.GetPosition(), exp);
            }
            
            Gamesystem.instance.missionManager.currentMission.OnAddedExp();
        }

        public void AddGold(int gold, bool countToProgress = true)
        {
            this.progressData.run.gold += gold;
            this.progressData.run.acumulatedGold += gold;
            
            if (countToProgress)
            {
                Gamesystem.instance.uiManager.rewardProgressBar.AddValue(gold);
                
                Gamesystem.instance.prefabDatabase.playerGoldReceived.Spawn(Gamesystem.instance.objects.currentPlayer.GetPosition(), gold);
            }
            
            Gamesystem.instance.missionManager.currentMission.OnAddedGold();
        }

        public int GetGold()
        {
            return this.progressData.run.gold;
        }
        
        public int GetAcumulatedGold()
        {
            return this.progressData.run.acumulatedGold;
        }
        
        public int GetExp()
        {
            return this.progressData.run.exp;
        }
        
        public int GetAcumulatedExp()
        {
            return this.progressData.run.acumulatedExp;
        }
        
        public long GetChaos()
        {
            return this.progressData.run.chaos;
        }
        
        public void AddChaos(long chaos)
        {
            this.progressData.run.chaos += chaos;
        }
        
        public void RemoveGold(int gold)
        {
            this.progressData.run.gold -= gold;
        }
        
        public void RemoveExp(int exp)
        {
            this.progressData.run.exp -= exp;
        }
    }
}