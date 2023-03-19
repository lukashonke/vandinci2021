using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace _Chi.Scripts.Persistence
{
    [Serializable]
    public class PlayerProgressData
    {
        public int level = 1;

        public PlayerRun run;
    }

    [Serializable]
    public class PlayerRun
    {
        public int bodyId;

        public int gold;

        public int rerolls;
        
        public int acumulatedGold;

        public long chaos;

        public int killed;

        public int missionIndex = 1;
        
        public int missionWaweIndex;
        
        public List<ModuleInSlot> modulesInSlots;

        public List<SlotItem> skillPrefabIds;

        public List<SlotItem> mutatorPrefabIds;

        public List<SlotItem> playerUpgradeItems;
        
        public List<SlotItem> skillUpgradeItems;
        
        public List<SlotItem> moduleUpgradeItems;
    }

    [Serializable]
    public class ModuleInSlot
    {
        public int slotId;
        public int moduleId;
        public int level = 1;
        public int rotation;
        
        public List<SlotItem> upgradeItems;
    }

    [Serializable]
    public class SlotItem
    {
        public int slot;
        public int prefabId;
    }
}