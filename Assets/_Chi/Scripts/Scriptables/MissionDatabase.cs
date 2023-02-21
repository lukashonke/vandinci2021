using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Scriptables.Dtos;
using DamageNumbersPro;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Mission Database", menuName = "Gama/Configuration/Mission Database")]
    public class MissionDatabase : SerializedScriptableObject
    {
        public List<MissionDatabaseItem> missions;
        
        public MissionDatabaseItem GetMission(int index)
        {
            return missions.FirstOrDefault(m => m.index == index);
        }
    }

    [Serializable]
    public class MissionDatabaseItem
    {
        public int index;

        [Required]
        public string name;

        [Multiline(3)]
        public string description;
        
        [AssetsOnly]
        public List<GameObject> missionScenarios;
    }
}