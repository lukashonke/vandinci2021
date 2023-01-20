using System.Collections.Generic;
using _Chi.Scripts.Scriptables.Dtos;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Prefab Database", menuName = "Gama/Configuration/Prefab Database")]
    public class PrefabDatabase : SerializedScriptableObject
    {
        public List<PrefabItem> prefabs;
    }
}