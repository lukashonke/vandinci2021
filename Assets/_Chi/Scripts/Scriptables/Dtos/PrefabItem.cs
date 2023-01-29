using System;
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

        public string label;

        public PrefabItemType type;

        public PredefinedPrefabIds predefinedId;

        [VerticalGroup("Prefab")]
        public GameObject prefab;

        //TODO show if type == body
        public GameObject prefabUi;

        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.Skill)]
        public Skill skill;

        [VerticalGroup("Prefab")]
        [ShowIf("type", PrefabItemType.Mutator)]
        public Mutator mutator;
    }

    public enum PrefabItemType
    {
        Unknown,
        Entity,
        Body,
        Module,
        Skill,
        Mutator
    }
}