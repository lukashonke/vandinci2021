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
        [TableList]
        public List<PrefabItem> prefabs;

        [TableList]
        public List<RewardSet> rewardSets;

        [TableList]
        public List<PrefabVariant> variants;
        [NonSerialized] public Dictionary<string, PrefabVariant> variantsLookup;

        [Required] public DamageNumber playerCriticalDealtDamage;

        [Required] public DamageNumber playerDealtDamage;

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
        public List<PrefabVariantSkill> skills;

        [VerticalGroup("Stats")]
        public List<Skill> skillsOnDie;
        
        [VerticalGroup("Stats")]
        public List<ImmediateEffect> effectsOnDie;
        
        [VerticalGroup("Stats")]
        public SpawnPrefabParameters parameters;
        
        [VerticalGroup("Stats")]
        public Reward reward;
    }

    [Serializable]
    public class Reward
    {
        public DropType dropType;
        public float dropChance;
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
                retValue.Add(prefab);
                kv.RemoveAll(i => i == prefab);
            }

            return retValue;
        }
    }

    [Serializable]
    public class RewardSetItemWithWeight
    {
        public int prefabId;
        public int weight;
        public int price;
    }
}