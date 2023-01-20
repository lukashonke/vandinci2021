using System;
using _Chi.Scripts.Mono.Common;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Dtos
{
    [Serializable]
    public class PrefabItem
    {
        public int id;

        public PredefinedPrefabIds predefinedId;

        public GameObject prefab;
    }
}