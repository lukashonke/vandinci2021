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
        
        public List<ModuleInSlot> modulesInSlots;
    }

    [Serializable]
    public class ModuleInSlot
    {
        public int slotId;
        public int moduleId;
        public int level = 1;
    }
}