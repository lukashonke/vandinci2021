using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Statistics;
using DamageNumbersPro;
using Pathfinding.RVO;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Prefab Database", menuName = "Gama/Configuration/Prefab Database")]
    public class PrefabDatabase : SerializedScriptableObject
    {
        [AssetsOnly]
        [TableList(AlwaysExpanded = true)]
        [TabGroup("Prefabs")]
        public List<PrefabItem> prefabs;

        [TableList(AlwaysExpanded = true)]
        [TabGroup("RewardSets")]
        public List<RewardSet> rewardSets;

        [TableList(AlwaysExpanded = true)]
        [TabGroup("Variants")]
        public List<PrefabVariant> variants;
        [NonSerialized] public Dictionary<string, PrefabVariant> variantsLookup;

        [TabGroup("Prefabs")]
        [Required] public DamageNumber playerCriticalDealtDamage;
        [TabGroup("Prefabs")]
        [Required] public DamageNumber playerDealtDamage;
        [TabGroup("Prefabs")]
        [Required] public DamageNumber playerGoldReceived;

        public void Initialise()
        {
            variantsLookup = variants.ToDictionary(v => v.variant, v => v);
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

        [TabGroup("Prefabs")]
        [Button]
        public void Reorder()
        {
            prefabs = prefabs.OrderBy(p => p.id).ToList();
        }

        public void ApplyPrefabVariant(Npc npc, string variant)
        {
            npc.ApplyVariant(variant);
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

    [Serializable]
    public class RewardSet
    {
        [Required]
        [VerticalGroup("Name")]
        public string name;

        [Multiline]
        [VerticalGroup("Name")]
        public string note;

        [Required]
        [VerticalGroup("Items")]
        public List<RewardSetItemWithWeight> prefabs;

        [VerticalGroup("Name")]
        [MinValue(1)] 
        public int minCountShownItems = 1;

        [VerticalGroup("Items")]
        public bool closeOnFirstPurchase;

        public List<RewardSetItemWithWeight> CalculateShownItems(Player player)
        {
            List<RewardSetItemWithWeight> kv = new();

            foreach (var prefab in prefabs)
            {
                for (int i = 0; i < prefab.weight; i++)
                {
                    kv.Add(prefab);
                }
            }

            var itemsToShow = minCountShownItems;

            var retValue = new List<RewardSetItemWithWeight>();

            for (int i = 0; i < itemsToShow; i++)
            {
                if (kv.Count == 0) break;
                
                var random = Random.Range(0, kv.Count);
                var prefab = kv[random];
                if (!CanApply(player, prefab))
                {
                    kv.RemoveAt(random);
                    i--;
                    continue;
                }
                
                retValue.Add(prefab);
                kv.RemoveAll(i => i == prefab);
            }

            return retValue;
        }

        private bool CanApply(Player player, RewardSetItemWithWeight item)
        {
            var prefab = Gamesystem.instance.prefabDatabase.GetById(item.prefabId);
            var run = Gamesystem.instance.progress.progressData.run;
            
            if (prefab == null) return false;

            switch (prefab.type)
            {
                case PrefabItemType.UpgradeItemPlayer:
                {
                    if (run.playerUpgradeItems != null && run.playerUpgradeItems.Any(i => i.prefabId == item.prefabId))
                    {
                        return false;
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
        [HorizontalGroup("Prefab")]
        public int prefabId;
        [HorizontalGroup("Prefab")]
        public int weight;
        
        [HorizontalGroup("Price")]
        public float price;
        [HorizontalGroup("Price")]
        public float priceMultiplyForEveryOwned = 0f;
    }
}