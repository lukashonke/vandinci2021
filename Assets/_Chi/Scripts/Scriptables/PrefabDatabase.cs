using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Scriptables.Dtos;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Prefab Database", menuName = "Gama/Configuration/Prefab Database")]
    public class PrefabDatabase : SerializedScriptableObject
    {
        [AssetsOnly]
        public List<PrefabItem> prefabs;

        public PrefabItem GetById(int id)
        {
            return prefabs.FirstOrDefault(p => p.id == id);
        }
    }
}