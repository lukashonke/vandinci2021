using System;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables.Dtos;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui.Tooltips
{
    public class ModuleTooltip : MonoBehaviour
    {
        [Required]public TextMeshProUGUI title;
        [Required]public TextMeshProUGUI text;
        [Required]public TextMeshProUGUI level;
        [Required]public Image logo;
        
        public void Initialise(PrefabItem modulePrefabItem, int level)
        {
            this.title.text = modulePrefabItem.label;
            if (level > 0)
            {
                this.level.text = $"Lv {level}";
            }
            else
            {
                this.level.text = string.Empty;
            }
        }
    }
}