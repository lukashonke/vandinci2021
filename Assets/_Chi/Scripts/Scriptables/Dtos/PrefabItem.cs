using System;
using _Chi.Scripts.Mono.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Dtos
{
    [Serializable]
    public class PrefabItem
    {
        public int id;

        public string label;

        public PrefabItemType type;

        public PredefinedPrefabIds predefinedId;

        public GameObject prefab;

        //TODO show if type == body
        public GameObject prefabUi;

        [ShowIf("type", PrefabItemType.Skill)]
        public Skill skill;

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