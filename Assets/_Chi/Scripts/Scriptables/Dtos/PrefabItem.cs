using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Dtos
{
    [Serializable]
    public class PrefabItem
    {
        [TableColumnWidth(57, Resizable = false)]
        public int id;

        [VerticalGroup("Text")]
        public string label;
        
        [VerticalGroup("Text")]
        public bool enabled;
        
        [VerticalGroup("Text")]
        [Multiline(2)]
        public string description;

        [VerticalGroup("Type")]
        public PrefabItemType type;

        [VerticalGroup("Type")]
        public PredefinedPrefabIds predefinedId;

        [VerticalGroup("Type")] 
        public WeightSettings weightSettings;

        [VerticalGroup("Prefab")]
        public GameObject prefab;

        //TODO show if type == body
        [VerticalGroup("Prefab")]
        public GameObject prefabUi;

        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.Skill)]
        public Skill skill;

        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.Mutator)]
        public Mutator mutator;
        
        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.UpgradeItemModule)]
        public ModuleUpgradeItem moduleUpgradeItem;
        
        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.UpgradeItemPlayer)]
        public PlayerUpgradeItem playerUpgradeItem;
        
        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.UpgradeItemSkill)]
        public SkillUpgradeItem skillUpgradeItem;

        [VerticalGroup("Text")]
        [FoldoutGroup("Text/Additional")]
        public List<string> additionalTexts;

        [VerticalGroup("Text")]
        [FoldoutGroup("Text/Additional")]
        [Multiline(2)]
        public string story;
    }

    [Serializable]
    public class WeightSettings
    {
        public List<WeightSettingsItem> alteredWeights;
    }

    [Serializable]
    public class WeightSettingsItem
    {
        public int prefabId;
        public int addWeight;

        public int additionalWeightWhenHavingLessThanAverage;
    }

    public enum PrefabItemType
    {
        Unknown,
        Entity,
        Body,
        Module,
        Skill,
        Mutator,
        Tree,
        Resource,
        UpgradeItemPlayer,
        UpgradeItemModule,
        UpgradeItemSkill,
    }
}