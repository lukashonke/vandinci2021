using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    /// <summary>
    /// an item that is applied to a skill
    /// </summary>
    [CreateAssetMenu(fileName = "Skill Upgrade Item", menuName = "Gama/Upgrade Items/Skill")]
    public class SkillUpgradeItem : UpgradeItem
    {
        public Skill target;

        public string upgradeId;
        public Dictionary<string, float> parameters;
    }
}