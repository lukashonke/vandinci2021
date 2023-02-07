using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Statistics;
using DamageNumbersPro;
using Sirenix.OdinInspector;
using UnityEditor.Animations;
using UnityEngine;

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
            npc.currentVariant = variant;
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
        public AnimatorController animatorController;

        [Required]
        [VerticalGroup("Stats")]
        public EntityStats entityStats;
        
        [Required]
        [VerticalGroup("Stats")]
        public NpcStats npcStats;
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
        public List<int> prefabIds;
    }
}