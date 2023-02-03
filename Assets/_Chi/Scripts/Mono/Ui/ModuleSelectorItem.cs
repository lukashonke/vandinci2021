using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables.Dtos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui
{
    public class ModuleSelectorItem : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI subTitle;
        public TextMeshProUGUI description;
        public Image icon;

        private List<ActionsPanelButton> buttons;
        private Action abort;

        private PrefabItem item;
        
        public void Initialise(PrefabItem item, List<ActionsPanelButton> buttons, Action abort)
        {
            this.item = item;
            
            InitialiseUi(item.label, item.prefabUi.GetComponent<Image>(), item.description, buttons, abort);
        }
        
        private void InitialiseUi(string title, Image icon, string description, List<ActionsPanelButton> buttons, Action abort)
        {
            this.title.text = title;
            this.subTitle.text = GetSubtitle();
            this.description.text = description ?? "";
            this.icon.sprite = icon.sprite;

            this.buttons = buttons;
            this.abort = abort;
        }

        private string GetSubtitle()
        {
            if (item.prefab != null)
            {
                var module = item.prefab.GetComponent<Module>();

                if (module is OffensiveModule)
                {
                    return "Weapon";
                }
                else if (module is DefensiveModule)
                {
                    return "Defense Module";
                }
                else if (module is PassiveModule)
                {
                    return "Weapon Upgrade";
                }
            }
            
            return this.item.type.ToString();
        }
        
        public void OnClick()
        {
            Gamesystem.instance.uiManager.SetActionsPanel(new ActionsPanel()
            {
                source = this,
                buttons = this.buttons,
                abortFunction = abort
            }, (RectTransform) transform);
        }
        
        public void OnHoverEnter()
        {
            if (item != null)
            {
                if (item.prefab != null && item.prefab.GetComponent<Module>() != null)
                {
                    Gamesystem.instance.uiManager.ShowModuleTooltip((RectTransform) this.transform, item, 1, UiManager.TooltipAlign.BottomLeft, UiManager.TooltipType.ExcludeTitleLogoDescription);
                }
            }
            
            /*if (moduleGo != null && modulePrefabItem != null)
            {
                //TODO use actual module instance if available to show level
                Gamesystem.instance.uiManager.ShowModuleTooltip((RectTransform) this.transform, modulePrefabItem, moduleLevel);
            }
            
            if (onHoverGo != null)
            {
                onHoverGo.SetActive(true);
            }*/
        }

        public void OnHoverExit()
        {
            if (item != null)
            {
                if (item.prefab != null && item.prefab.GetComponent<Module>() != null)
                {
                    Gamesystem.instance.uiManager.HideTooltip();
                }
            }
            
            /*if (moduleGo != null && modulePrefabItem != null)
            {
                Gamesystem.instance.uiManager.HideTooltip();
            }
            
            if (onHoverGo != null)
            {
                onHoverGo.SetActive(false);
            }*/
        }
    }
}