using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui.Tooltips
{
    public class ModuleTooltip : MonoBehaviour
    {
        [Required] public GameObject topRowContainer;
        [Required] public GameObject contentContainer; 
        [Required]public TextMeshProUGUI title;
        [Required]public TextMeshProUGUI text;
        [Required]public TextMeshProUGUI level;
        [Required]public Image logo;

        [Required] public GameObject statsContainer;
        [Required] public GameObject statsItem;
        
        [Required] public GameObject upgradesContainer;
        [Required] public GameObject upgradesItem;
        
        public void Initialise(PrefabItem modulePrefabItem, int level, UiManager.TooltipType tooltipType, List<UpgradeItem> upgradeItems)
        {
            if (tooltipType == UiManager.TooltipType.ExcludeTitleLogoDescription)
            {
                topRowContainer.SetActive(false);
                contentContainer.SetActive(false);
            }
            else
            {
                this.title.text = modulePrefabItem.label;
                this.text.text = modulePrefabItem.description;
                this.logo.sprite = modulePrefabItem.prefabUi.GetComponent<Image>().sprite;
            
                if (level > 0)
                {
                    this.level.text = $"Lv {level}";
                }
                else
                {
                    this.level.text = string.Empty;
                }
            }
            
            if (!InitialiseStats(modulePrefabItem, level))
            {
                statsContainer.SetActive(false);
            }
            
            if (!InitialiseUpgrades(upgradeItems))
            {
                upgradesContainer.SetActive(false);
            }
        }
        
        private bool InitialiseUpgrades(List<UpgradeItem> upgradeItems)
        {
            upgradesItem.SetActive(false);
            
            if (upgradeItems != null && upgradeItems.Any())
            {
                foreach (var item in upgradeItems)
                {
                    var go = Instantiate(upgradesItem, upgradesContainer.transform);
                    go.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = item.uiName;
                    go.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = item.uiDescription;
                    go.SetActive(true);
                }

                return true;
            }

            return false;
        }

        private bool InitialiseStats(PrefabItem modulePrefabItem, int level)
        {
            statsItem.SetActive(false);
            
            if (modulePrefabItem.prefab != null)
            {
                var module = modulePrefabItem.prefab.GetComponent<Module>();
                if (module != null)
                {
                    var stats = module.GetUiStats(level);

                    if (stats != null)
                    {
                        foreach (var stat in stats)
                        {
                            var go = Instantiate(statsItem, statsContainer.transform);
                            go.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = stat.title;
                            go.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = stat.value;
                            go.SetActive(true);
                        }

                        return true;
                    }
                }
            }
            else if (modulePrefabItem.mutator != null)
            {
                var stats = modulePrefabItem.mutator.GetUiStats(1);
                if (stats != null)
                {
                    foreach (var stat in stats)
                    {
                        var go = Instantiate(statsItem, statsContainer.transform);
                        go.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = stat.title;
                        go.transform.Find("Value").GetComponent<TextMeshProUGUI>().text = stat.value;
                        go.SetActive(true);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}