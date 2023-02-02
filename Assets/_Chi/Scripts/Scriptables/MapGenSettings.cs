using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "MapGen Settings", menuName = "Gama/Configuration/MapGen Settings")]
    public class MapGenSettings : SerializedScriptableObject
    {
        [OdinSerialize][NonSerialized][TableList]
        public List<MapGenReplaceSettings> settings;
    }

    [Serializable]
    public class MapGenReplaceSettings
    {
        [TableColumnWidth(180, false)]
        public Tile tile;

        [TableList]
        public List<MapGenReplaceSettingsItem> items = new List<MapGenReplaceSettingsItem>();

        public bool ContainsRectItems()
        {
            return items.Any(i => i.rectSize.x > 1 || i.rectSize.y > 1);
        }
    }

    [Serializable]
    public class MapGenReplaceSettingsItem
    {
        public Vector2Int rectSize;
        public GameObject prefab;
        
        [TableColumnWidth(100, false)]
        public int chance = 100;
        
        [TableColumnWidth(100, false)]
        public int priority;

        public Vector2 randomPosition;
        
        public float sizeVariation = 0;
    }
}