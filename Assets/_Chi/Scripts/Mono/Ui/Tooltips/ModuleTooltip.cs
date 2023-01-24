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
            var module = modulePrefabItem.prefab.GetComponent<Module>();
                
            this.title.text = module.name;
            this.level.text = $"Lv {level}";
        }
    }
}