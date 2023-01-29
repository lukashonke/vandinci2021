using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Scriptables.Dtos;
using DamageNumbersPro;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Prefab Database", menuName = "Gama/Configuration/Prefab Database")]
    public class PrefabDatabase : SerializedScriptableObject
    {
        [AssetsOnly]
        [TableList]
        public List<PrefabItem> prefabs;

        [Required] public DamageNumber playerDealtDamage;

        public PrefabItem GetById(int id)
        {
            return prefabs.FirstOrDefault(p => p.id == id);
        }

        [Button]
        public void Reorder()
        {
            prefabs = prefabs.OrderBy(p => p.id).ToList();
        }
    }
}