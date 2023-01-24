using System;
using _Chi.Scripts.Mono.Common;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Dtos
{
    [Serializable]
    public class PrefabItem
    {
        public int id;

        public PrefabItemType type;

        public PredefinedPrefabIds predefinedId;

        public GameObject prefab;

        //TODO show if type == body
        public GameObject prefabUi;
    }

    public enum PrefabItemType
    {
        Unknown,
        Entity,
        Body,
        Module
    }
}