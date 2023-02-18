using System;
using System.Collections.Generic;
using Sirenix.Serialization;

namespace _Chi.Scripts.Persistence
{
    [Serializable]
    public class PlayerProgressData
    {
        public int level;

        public PlayerRun run;
    }

    [Serializable]
    public class PlayerRun
    {
        public int bodyId;

        public int gold;

        public int killed;

        public int missionIndex;
        
        public int missionWaweIndex;
        
        public List<ModuleInSlot> modulesInSlots;

        public List<SlotItem> skillPrefabIds;

        public List<SlotItem> mutatorPrefabIds;
    }

    [Serializable]
    public class ModuleInSlot
    {
        public int slotId;
        public int moduleId;
        public int level = 1;
        public int rotation;
    }

    [Serializable]
    public class SlotItem
    {
        public int slot;
        public int prefabId;
    }
}