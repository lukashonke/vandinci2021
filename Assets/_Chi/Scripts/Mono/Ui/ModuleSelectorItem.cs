using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui
{
    public class ModuleSelectorItem : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public Image icon;

        private List<ActionsPanelButton> buttons;
        private Action abort;
        
        public void Initialise(string title, Image icon, string description, List<ActionsPanelButton> buttons, Action abort)
        {
            this.title.text = title;
            this.description.text = description;
            this.icon.sprite = icon.sprite;

            this.buttons = buttons;
            this.abort = abort;
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
    }
}