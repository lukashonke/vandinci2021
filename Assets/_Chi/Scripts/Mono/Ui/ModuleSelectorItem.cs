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
        public TextMeshProUGUI price;
        public Image icon;

        public Button lockButton;

        public Color weaponSubtitleColor;
        public Color defensiveModuleSubtitleColor;
        public Color passiveModuleSubtitleColor;
        public Color mutatorSubtitleColor;
        public Color skillSubtitleColor;

        public Color moduleUpgradeItemColor;
        public Color playerUpgradeItemColor;
        public Color skillUpgradeItemColor;

        private List<ActionsPanelButton> buttons;
        private Action abort;

        private PrefabItem item;
        private int? priceValue;
        
        public void Initialise(PrefabItem item, List<ActionsPanelButton> buttons, Action abort, int? price)
        {
            this.item = item;

            InitialiseUi(item.label, item.prefabUi.GetComponent<Image>(), item.description, buttons, abort, price);
            
            /*if(item.playerUpgradeItem != null || item.moduleUpgradeItem != null || item.skillUpgradeItem != null)
            {
                lockButton.gameObject.SetActive(true);
            }
            else
            {
                lockButton.gameObject.SetActive(false);
            }*/
        }
        
        private void InitialiseUi(string title, Image icon, string description, List<ActionsPanelButton> buttons, Action abort, int? price)
        {
            this.title.text = title;
            var subtitleColor = GetSubtitle();
            this.subTitle.text = subtitleColor.Item1;
            if(subtitleColor.Item2.HasValue) this.subTitle.color = subtitleColor.Item2.Value;
            this.description.text = description ?? "";
            this.icon.sprite = icon.sprite;
            this.icon.material = icon.material;

            this.buttons = buttons;
            this.abort = abort;

            SetPrice(price);
            
            var locked = Gamesystem.instance.uiManager.vehicleSettingsWindow.moduleSelector.IsLocked(item);
            
            SetLocked(locked);
        }

        public void SetPrice(int? price)
        {
            this.priceValue = price;
            
            if (price.HasValue)
            {
                this.price.gameObject.transform.parent.gameObject.SetActive(true);
                this.price.text = price.Value.ToString();
            }
            else
            {
                this.price.gameObject.transform.parent.gameObject.SetActive(false);
            }
        }

        private (string, Color?) GetSubtitle()
        {
            if (item.prefab != null)
            {
                var module = item.prefab.GetComponent<Module>();

                if (module is OffensiveModule)
                {
                    return ("Weapon", weaponSubtitleColor);
                }
                if (module is DefensiveModule)
                {
                    return ("Defense Module", defensiveModuleSubtitleColor);
                }
                if (module is PassiveModule)
                {
                    return ("Weapon Upgrade", passiveModuleSubtitleColor);
                }
            }

            if (item.skill != null)
            {
                return ("Skill", skillSubtitleColor);
            }

            if (item.mutator != null)
            {
                return ("Mutator", mutatorSubtitleColor);
            }

            if (item.moduleUpgradeItem != null)
            {
                return ("Chaos Weapon Upgrade", moduleUpgradeItemColor);
            }
            
            if (item.skillUpgradeItem != null)
            {
                return ("Skill Upgrade", skillUpgradeItemColor);
            }
            
            if (item.playerUpgradeItem != null)
            {
                return ("Player Upgrade", playerUpgradeItemColor);
            }

            return (this.item.type.ToString(), null);
        }
        
        public void OnClick()
        {
            var playerGold = Gamesystem.instance.progress.GetGold();

            if (priceValue == null || playerGold >= priceValue)
            {
                if (this.buttons.Count == 1)
                {
                    buttons[0].action();
                    
                    /*Gamesystem.instance.uiManager.SetActionsPanel(new ActionsPanel()
                    {
                        source = this,
                        buttons = this.buttons,
                        abortFunction = abort
                    }, (RectTransform) transform);*/
                }
                else
                {
                    Gamesystem.instance.uiManager.SetActionsPanel(new ActionsPanel()
                    {
                        source = this,
                        buttons = this.buttons,
                        abortFunction = abort
                    }, (RectTransform) transform);
                }
            }
        }

        public void OnLockItem()
        {
            var doLock = !Gamesystem.instance.uiManager.vehicleSettingsWindow.moduleSelector.IsLocked(item);

            if (doLock)
            {
                Gamesystem.instance.uiManager.vehicleSettingsWindow.moduleSelector.UnlockAll();
            }
            
            Gamesystem.instance.uiManager.vehicleSettingsWindow.moduleSelector.SetLocked(item, doLock);
            
            SetLocked(doLock);
        }

        public void SetLocked(bool b)
        {
            lockButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = b ? "Unlock" : "Lock";
        }
        
        public void OnHoverEnter()
        {
            if (item != null)
            {
                if ((item.prefab != null && item.prefab.GetComponent<Module>() != null) || item.mutator != null || item.moduleUpgradeItem != null || item.skillUpgradeItem != null || item.playerUpgradeItem != null)
                {
                    Gamesystem.instance.uiManager.ShowItemTooltip((RectTransform) this.transform, item, 1, UiManager.TooltipAlign.BottomLeft, UiManager.TooltipType.ExcludeTitleLogoDescription);
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
                if ((item.prefab != null && item.prefab.GetComponent<Module>() != null) || item.mutator != null || item.moduleUpgradeItem != null || item.skillUpgradeItem != null || item.playerUpgradeItem != null)
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