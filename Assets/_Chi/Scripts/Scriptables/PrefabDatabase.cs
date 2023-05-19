using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Statistics;
using DamageNumbersPro;
using Pathfinding.RVO;
using QFSW.QC;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Prefab Database", menuName = "Gama/Configuration/Prefab Database")]
    public class PrefabDatabase : SerializedScriptableObject
    {
        [TabGroup("Prefabs")]
        [HorizontalGroup("Buttons")]
        [Button]
        public void ClearFilter()
        {
            if (hiddenPrefabs == null)
            {
                hiddenPrefabs = new();
            }
            
            prefabs = prefabs.Union(hiddenPrefabs).GroupBy(g => g.id).Select(g => g.First()).ToList();
            hiddenPrefabs.Clear();
            Reorder();
        }

        [TabGroup("Prefabs")]
        [HorizontalGroup("Buttons")]
        [Button]
        public void Filter(PrefabItemType type)
        {
            if (hiddenPrefabs == null)
            {
                hiddenPrefabs = new();
            }
            
            prefabs = prefabs.Union(hiddenPrefabs).GroupBy(g => g.id).Select(g => g.First()).ToList();

            hiddenPrefabs.Clear();
            hiddenPrefabs.AddRange(prefabs);
            
            prefabs = prefabs.Where(p => p.type == type).ToList();
            Reorder();
        }
        
        [TabGroup("Prefabs")]
        [HorizontalGroup("Buttons")]
        [Button]
        public void Reorder()
        {
            prefabs = prefabs.OrderBy(p => p.id).ToList();
        }
        
        [AssetsOnly]
        [TableList()]
        [TabGroup("Prefabs")]
        public List<PrefabItem> prefabs;

        [HideInInspector] public List<PrefabItem> hiddenPrefabs;

        [TableList(AlwaysExpanded = true)]
        [TabGroup("RewardSets")]
        public List<RewardSet> rewardSets;

        [TableList(AlwaysExpanded = true)]
        [TabGroup("Variants")]
        public List<PrefabVariant> variants;
        [NonSerialized] public Dictionary<string, PrefabVariant> variantsLookup;

        [TabGroup("Misc")]
        [Required] public DamageNumber playerCriticalDealtDamage;
        [TabGroup("Misc")]
        [Required] public DamageNumber playerDealtDamage;
        [TabGroup("Misc")]
        [Required] public DamageNumber playerGoldReceived;
        [TabGroup("Misc")]
        [Required] public DamageNumber playerExpReceived;
        [TabGroup("Misc")]
        [Required] public DamageNumber selfEffect;

        public void Initialise()
        {
            variantsLookup = variants.ToDictionary(v => v.variant, v => v);

            if (hiddenPrefabs == null)
            {
                hiddenPrefabs = new();
            }

            prefabs = prefabs.Union(hiddenPrefabs).GroupBy(g => g.id).Select(g => g.First()).ToList();
            hiddenPrefabs.Clear();
            Reorder();
        }

        public PrefabItem GetById(int id)
        {
            //TODO optimise
            return prefabs.FirstOrDefault(p => p.id == id);
        }

        public RewardSet GetRewardSet(string setName) => rewardSets.First(r => r.name == setName);

        public PrefabVariant GetVariant(string variant)
        {
            if (variantsLookup.TryGetValue(variant, out var val))
            {
                return val;
            }

            return null;
        }
        
        public void ApplyPrefabVariant(Npc npc, string variant)
        {
            npc.ApplyVariant(variant);
        }

        [Button]
        public void Export(PrefabItemType type)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var prefab in prefabs.Where(t => t.type == type))
            {
                sb.AppendLine($"{prefab.label}; {prefab.description}");
            }
            
            Debug.Log(sb.ToString());
        }
    }

    [Serializable]
    public class PrefabVariant
    {
        [Required]
        [VerticalGroup("Title")]
        public string variant;

        [Required]
        [VerticalGroup("Visuals")]
        public Sprite sprite;

        [Required]
        [VerticalGroup("Visuals")]
        public Material spriteMaterial;
        
        [VerticalGroup("Visuals")]
        public RuntimeAnimatorController animatorController;
        
        [Required]
        [VerticalGroup("Stats")]
        public EntityStats entityStats;
        
        [Required]
        [VerticalGroup("Stats")]
        public NpcStats npcStats;

        [VerticalGroup("Stats")]
        [FoldoutGroup("Stats/Skills")]
        public List<PrefabVariantSkill> skills;

        [VerticalGroup("Stats")]
        [FoldoutGroup("Stats/Skills")]
        public List<Skill> skillsOnDie;
        
        [VerticalGroup("Stats")]
        [FoldoutGroup("Stats/Skills")]
        public List<ImmediateEffect> effectsOnDie;
        
        [VerticalGroup("Stats")]
        public SpawnPrefabParameters parameters;
        
        [VerticalGroup("Stats")]
        public Reward reward;
    }

    [Serializable]
    public class Reward
    {
        public List<RewardItem> items;
    }

    [Serializable]
    public class RewardItem
    {
        public DropType dropType;
        public float dropChance = 100;

        public int amountMin = 1;
        public int amountMax = 1;

        public bool bypassGlobalDropChance = false;
    }
    
    [Serializable]
    public class SpawnPrefabParameters
    {
        public string spawnAroundEntityGroupName;

        public float rvoPriority = 0.5f;

        public bool disableRvoCollision;

        public bool setRvoLayers;
        [ShowIf("setRvoLayers")]
        public RVOLayer rvoLayer;
        [ShowIf("setRvoLayers")]
        public RVOLayer rvoCollidesWith;
    }

    [Serializable]
    public class PrefabVariantSkill
    {
        public PrefabVariantSkillTrigger trigger;
        
        public Skill skill;
    }

    [Serializable]
    public class PrefabVariantSkillTrigger
    {
        //TODO
    }

    public enum RewardSetType
    {
        FreeReward,
        Shop
    }

    [Serializable]
    public class RewardSet
    {
        [Required]
        [VerticalGroup("Name")]
        public string name;

        [Multiline]
        [VerticalGroup("Name")]
        public string note;

        [VerticalGroup("Name")]
        [MinValue(1)] 
        public int minCountShownItems = 1;
        
        [Required]
        [VerticalGroup("Items")]
        public List<RewardSetItemWithWeight> prefabs;

        [VerticalGroup("Items")]
        public List<string> nestedRewardSets;

        [VerticalGroup("Items")] 
        public List<float> priceMultiplierPerCountOwned;

        [NonSerialized] public int dontShowTimes;
        [NonSerialized] private bool hiddenInCurrentRoll;

        [VerticalGroup("Name")]
        public string lockId;

        public List<(RewardSetItemWithWeight item, float priceMul)> CalculateShownItems(Player player, Dictionary<int, bool> lockedPrefabIds, bool showOnlyLocked, bool isReroll, int? shownItems)
        {
            if(dontShowTimes > 0 || (isReroll && hiddenInCurrentRoll))
            {
                if (!isReroll)
                {
                    dontShowTimes--;
                }

                hiddenInCurrentRoll = true;
                return new List<(RewardSetItemWithWeight item, float priceMul)>();
            }

            hiddenInCurrentRoll = false;
            
            if (!prefabs.Any()) return new List<(RewardSetItemWithWeight item, float priceMul)>();
            
            List<PrefabItem> ownedItems = GetPlayerItems();
            var disabledPrefabIds = GetDisabledItems(ownedItems);
            var unlockedPrefabIds = GetUnlockedItems(ownedItems);
            var unlockedIds = GetUnlockedIds(ownedItems);
            var replacedPrefabIds = GetReplacedItems(ownedItems);
            Dictionary<string, RewardSetItemWithWeight> alreadySelectedGroups = new();

            // set is locked, check if another item unlocked it
            if (!string.IsNullOrWhiteSpace(lockId))
            {
                if(!unlockedIds.Contains(lockId))
                {
                    return new List<(RewardSetItemWithWeight item, float priceMul)>();
                }
            }

            Dictionary<int, WeightSettingsItem> weightAlters = new();
            foreach (var item in ownedItems)
            {
                if (item.weightSettings?.alteredWeights != null)
                {
                    foreach (var aw in item.weightSettings.alteredWeights)
                    {
                        if (!weightAlters.ContainsKey(aw.prefabId))
                        {
                            weightAlters.Add(aw.prefabId, aw);
                        }
                    }
                }
            }

            var db = Gamesystem.instance.prefabDatabase;
            float averageCount = 0;
            int totalCount = 0;

            foreach (var alter in weightAlters)
            {
                var owned = ownedItems.Count(i => i.id == alter.Key);
                averageCount += owned;
                totalCount += owned;
            }

            if (totalCount > 0)
            {
                averageCount /= totalCount;
            }

            var retValue = new List<(RewardSetItemWithWeight item, float priceMul)>();
            
            if (showOnlyLocked)
            {
                foreach (var prefab in prefabs)
                {
                    if (lockedPrefabIds.TryGetValue(prefab.prefabId, out var val) && val)
                    {
                        retValue.Add((prefab, GetPriceMul(prefab, ownedItems)));                        
                    }
                }

                return retValue;
            }
            
            List<RewardSetItemWithWeight> allItemsWithWeights = new();

            foreach (var prefab in prefabs)
            {
                var weight = prefab.weight;
                if(weightAlters.TryGetValue(prefab.prefabId, out var val))
                {
                    var owned = ownedItems.Count(i => i.id == prefab.prefabId);
                    if (owned < averageCount)
                    {
                        weight += val.additionalWeightWhenHavingLessThanAverage + val.addWeight;
                    }
                    else
                    {
                        weight += val.addWeight;
                    }
                }
                
                for (int i = 0; i < weight; i++)
                {
                    allItemsWithWeights.Add(prefab);
                }
            }

            var itemsToShow = shownItems ?? minCountShownItems;
            
            for (int i = 0; i < itemsToShow; i++)
            {
                if (allItemsWithWeights.Count == 0) break;

                var index = Random.Range(0, allItemsWithWeights.Count);
                for (var index1 = 0; index1 < allItemsWithWeights.Count; index1++)
                {
                    var itemWithWeight = allItemsWithWeights[index1];
                    if (lockedPrefabIds.TryGetValue(itemWithWeight.prefabId, out var isLocked) && isLocked)
                    {
                        index = index1;
                    }
                }

                var prefab = allItemsWithWeights[index];
                if (!CanApply(player, prefab, replacedPrefabIds, disabledPrefabIds, unlockedPrefabIds, alreadySelectedGroups, unlockedIds))
                {
                    allItemsWithWeights.RemoveAt(index);
                    i--;
                    continue;
                }

                if (prefab.isInGroup)
                {
                    if (alreadySelectedGroups.TryGetValue(prefab.group, out var currentSelectedPrefab))
                    {
                        // replace the current item
                        retValue.RemoveAll(i => i.item == currentSelectedPrefab);
                    }
                    
                    alreadySelectedGroups[prefab.group] = prefab;
                }

                retValue.Add((prefab, GetPriceMul(prefab, ownedItems)));
                allItemsWithWeights.RemoveAll(i => i == prefab);
            }

            return retValue;
        }

        private float GetPriceMul(RewardSetItemWithWeight item, List<PrefabItem> ownedItems)
        {
            var prefab = Gamesystem.instance.prefabDatabase.GetById(item.prefabId);

            List<int> replaces = new();
            if (prefab.playerUpgradeItem != null && prefab.playerUpgradeItem.replacesModulePrefabIds.HasValues()) replaces.AddRange(prefab.playerUpgradeItem.replacesModulePrefabIds);
            else if(prefab.moduleUpgradeItem != null && prefab.moduleUpgradeItem.replacesModulePrefabIds.HasValues()) replaces.AddRange(prefab.moduleUpgradeItem.replacesModulePrefabIds);
            else if(prefab.skillUpgradeItem != null && prefab.skillUpgradeItem.replacesModulePrefabIds.HasValues()) replaces.AddRange(prefab.skillUpgradeItem.replacesModulePrefabIds);

            var ownedItemsCount = prefabs.Count(c => ownedItems.Any(oi => oi.id == c.prefabId));

            var multiplier = 1f;
            if (priceMultiplierPerCountOwned.HasValues())
            {
                if(priceMultiplierPerCountOwned.Count > ownedItemsCount)
                {
                    multiplier = priceMultiplierPerCountOwned[ownedItemsCount];
                }
                else
                {
                    multiplier = priceMultiplierPerCountOwned.Last();
                }
            }
            
            if (replaces.HasValues() && ownedItems.Any(i => replaces.Contains(i.id)))
            {
                // player owns an item which this new item replaces - so this new item has a discount
                return multiplier * 0.5f;
            }

            return multiplier;
        }

        public List<PrefabItem> GetPlayerItems()
        {
            var retValue = new List<PrefabItem>();
            var db = Gamesystem.instance.prefabDatabase;
            var run = Gamesystem.instance.progress.progressData.run;

            foreach (var slot in run.modulesInSlots ?? Enumerable.Empty<ModuleInSlot>())
            {
                if (slot.moduleId > 0)
                {
                    for (int i = 0; i < Math.Max(1, slot.level); i++)
                    {
                        retValue.Add(db.GetById(slot.moduleId));
                    }
                }
                foreach (var slotItem in slot.upgradeItems)
                {
                    if(slotItem.prefabId > 0) retValue.Add(db.GetById(slotItem.prefabId));
                }
            }

            foreach (var item in run.skillPrefabIds ?? Enumerable.Empty<SlotItem>())
            {
                if(item.prefabId > 0) retValue.Add(db.GetById(item.prefabId));
            }

            foreach (var item in run.playerUpgradeItems ?? Enumerable.Empty<SlotItem>())
            {
                if(item.prefabId > 0) retValue.Add(db.GetById(item.prefabId));
            }
            
            foreach (var item in run.mutatorPrefabIds ?? Enumerable.Empty<SlotItem>())
            {
                if(item.prefabId > 0) retValue.Add(db.GetById(item.prefabId));
            }
            
            foreach (var item in run.moduleUpgradeItems ?? Enumerable.Empty<SlotItem>())
            {
                if(item.prefabId > 0) retValue.Add(db.GetById(item.prefabId));
            }
            
            foreach (var item in run.skillUpgradeItems ?? Enumerable.Empty<SlotItem>())
            {
                if(item.prefabId > 0) retValue.Add(db.GetById(item.prefabId));
            }

            return retValue;
        }

        public HashSet<int> GetDisabledItems(List<PrefabItem> items)
        {
            List<int> disabled = new();
            foreach (var item in items)
            {
                if (item.moduleUpgradeItem != null && item.moduleUpgradeItem.disablesModulePrefabIds != null)
                {
                    disabled.AddRange(item.moduleUpgradeItem.disablesModulePrefabIds);
                }
                if (item.playerUpgradeItem != null && item.playerUpgradeItem.disablesModulePrefabIds != null)
                {
                    disabled.AddRange(item.playerUpgradeItem.disablesModulePrefabIds);
                }
                if (item.skillUpgradeItem != null && item.skillUpgradeItem.disablesModulePrefabIds != null)
                {
                    disabled.AddRange(item.skillUpgradeItem.disablesModulePrefabIds);
                }
            }

            return disabled.ToHashSet();
        }
        
        public HashSet<int> GetUnlockedItems(List<PrefabItem> items)
        {
            List<int> unlocked = new();
            foreach (var item in items)
            {
                if (item.moduleUpgradeItem != null && item.moduleUpgradeItem.unlocksModulePrefabIds != null)
                {
                    unlocked.AddRange(item.moduleUpgradeItem.unlocksModulePrefabIds);
                }
                if (item.playerUpgradeItem != null && item.playerUpgradeItem.unlocksModulePrefabIds != null)
                {
                    unlocked.AddRange(item.playerUpgradeItem.unlocksModulePrefabIds);
                }
                if (item.skillUpgradeItem != null && item.skillUpgradeItem.unlocksModulePrefabIds != null)
                {
                    unlocked.AddRange(item.skillUpgradeItem.unlocksModulePrefabIds);
                }
            }

            return unlocked.ToHashSet();
        }
        
        public HashSet<string> GetUnlockedIds(List<PrefabItem> items)
        {
            List<string> unlocked = new();
            foreach (var item in items)
            {
                unlocked.AddRange(item.unlockSettings?.unlockedIds ?? Enumerable.Empty<string>());
            }

            return unlocked.ToHashSet();
        }
        
        public HashSet<int> GetReplacedItems(List<PrefabItem> items)
        {
            List<int> replaced = new();
            foreach (var item in items)
            {
                if (item.moduleUpgradeItem != null && item.moduleUpgradeItem.replacesModulePrefabIds.HasValues())
                {
                    replaced.AddRange(item.moduleUpgradeItem.replacesModulePrefabIds);
                }
                if (item.playerUpgradeItem != null && item.playerUpgradeItem.replacesModulePrefabIds.HasValues())
                {
                    replaced.AddRange(item.playerUpgradeItem.replacesModulePrefabIds);
                }
                if (item.skillUpgradeItem != null && item.skillUpgradeItem.replacesModulePrefabIds.HasValues())
                {
                    replaced.AddRange(item.skillUpgradeItem.replacesModulePrefabIds);
                }
            }

            return replaced.ToHashSet();
        }

        private bool CanApply(Player player, RewardSetItemWithWeight item, HashSet<int> replacedItems, HashSet<int> disabledItems, HashSet<int> unlockedItems, Dictionary<string, RewardSetItemWithWeight> alreadySelectedGroups, HashSet<string> unlockedIds)
        {
            var prefab = Gamesystem.instance.prefabDatabase.GetById(item.prefabId);
            var run = Gamesystem.instance.progress.progressData.run;
            
            if (prefab == null) return false;
            
            if(item.mustBeUnlocked && !unlockedItems.Contains(item.prefabId)) return false;
            if(disabledItems.Contains(item.prefabId)) return false;
            if(replacedItems.Contains(item.prefabId)) return false;
            if (!string.IsNullOrWhiteSpace(item.lockId))
            {
                if (!unlockedIds.Contains(item.lockId))
                {
                    return false;
                }
            }

            if (item.isInGroup)
            {
                // an item from the same group has already been selected
                if (alreadySelectedGroups.TryGetValue(item.group, out var currentSelectedPrefab))
                {
                    // and the selected item is of higher or same level
                    if (currentSelectedPrefab.levelInGroup >= item.levelInGroup) return false;
                }
            }
            
            switch (prefab.type)
            {
                case PrefabItemType.UpgradeItemPlayer:
                {
                    if (!prefab.playerUpgradeItem.canBeStacked)
                    {
                        if (run.playerUpgradeItems != null && run.playerUpgradeItems.Any(i => i.prefabId == item.prefabId))
                        {
                            return false;
                        }    
                    }

                    break;
                }
                case PrefabItemType.UpgradeItemSkill:
                {
                    if (run.skillUpgradeItems != null && run.skillUpgradeItems.Any(i => i.prefabId == item.prefabId))
                    {
                        return false;
                    }
                    
                    break;
                }
                case PrefabItemType.Skill:
                {
                    if (run.skillPrefabIds != null && run.skillPrefabIds.Any(i => i.prefabId == item.prefabId))
                    {
                        return false;
                    }
                    
                    break;
                }
                case PrefabItemType.UpgradeItemModule:
                {
                    var moduleId = prefab.moduleUpgradeItem.modulePrefabId;

                    var moduleInSlot = run.modulesInSlots.FirstOrDefault(m => m.moduleId == moduleId);
                    if (moduleInSlot == null || moduleInSlot.upgradeItems.Any(u => u.prefabId == prefab.id))
                    {
                        return false;
                    }

                    break;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class RewardSetItemWithWeight
    {
        [HorizontalGroup("Name")][ReadOnly] public string name;
        
        [HorizontalGroup("Prefab")] [OnValueChanged("OnPrefabIdChanged")][LabelWidth(100)]
        public int prefabId;

        [HorizontalGroup("Prefab")][LabelWidth(100)]
        public int weight;
        
        [HorizontalGroup("Group")] public bool isInGroup;

        [FoldoutGroup("GroupSettings")] [ShowIf("isInGroup")]
        public string group;

        [FoldoutGroup("GroupSettings")] [ShowIf("isInGroup")]
        public int levelInGroup;


        [FoldoutGroup("Price")] public float price;
        [FoldoutGroup("Price")] public float priceMultiplyForEveryOwned = 0f;

        [FoldoutGroup("Lock")]
        public string lockId;
        [FoldoutGroup("Price")][InfoBox("Use lockId instead")]
        public bool mustBeUnlocked;

        private void OnPrefabIdChanged()
        {
            var item = Gamesystem.instance.prefabDatabase.GetById(prefabId);
            if (item != null)
            {
                if (item.playerUpgradeItem != null) name = $"{item.playerUpgradeItem.rarity} {item.label}";
                else if (item.skillUpgradeItem != null) name = $"{item.skillUpgradeItem.rarity} {item.label}";
                else if (item.moduleUpgradeItem != null) name = $"{item.moduleUpgradeItem.rarity} {item.label}";
                else name = item.label;
            }
        }
    }
}