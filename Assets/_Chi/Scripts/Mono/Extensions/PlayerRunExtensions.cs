using System.Collections.Generic;
using _Chi.Scripts.Persistence;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class PlayerRunExtensions
    {
        public static int GetCountOfPrefabs(this PlayerRun run, int prefabId, bool countLevelAsExtraItems = true)
        {
            int count = 0;

            foreach (var slot in run.modulesInSlots)
            {
                if (slot.moduleId == prefabId)
                {
                    if (countLevelAsExtraItems)
                    {
                        count += slot.level;
                    }
                    else
                    {
                        count++;
                    }
                }
                
                count += CountPrefabs(slot.upgradeItems, prefabId);
            }
            
            count += CountPrefabs(run.skillPrefabIds, prefabId);
            count += CountPrefabs(run.mutatorPrefabIds, prefabId);
            count += CountPrefabs(run.playerUpgradeItems, prefabId);
            count += CountPrefabs(run.skillUpgradeItems, prefabId);
            count += CountPrefabs(run.moduleUpgradeItems, prefabId);

            return count;
        }

        private static int CountPrefabs(List<SlotItem> items, int prefabId)
        {
            if (items == null) return 0;
            
            int count = 0;
            foreach (var item in items)
            {
                if (item.prefabId == prefabId)
                {
                    count++;
                }
            }
            return count;
        }
    }
}