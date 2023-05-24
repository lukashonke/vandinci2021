using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui.Dialogs
{
    public class ConfirmDialog : MonoBehaviour
    {
        [Required] public TextMeshProUGUI title;
        [Required] public TextMeshProUGUI text;
        
        [Required] public Button confirmButton;
        [Required] public TextMeshProUGUI confirmButtonLabel;
        
        [Required] public Button rejectButton;
        [Required] public TextMeshProUGUI rejectButtonLabel;
        
        [Required] public Button abortButton;

        public void Initialise(string title, string text, string confirmLabel, string rejectLabel, Action confirm, Action reject, Action abort)
        {
            this.title.text = title;
            this.text.text = text;
            this.confirmButtonLabel.text = confirmLabel;
            this.rejectButtonLabel.text = rejectLabel;

            if (confirm != null)
            {
                this.confirmButton.onClick.AddListener(() =>
                {
                    confirm();
                    Gamesystem.instance.uiManager.HideConfirmDialog();
                });
            }
            
            if (reject != null)
            {
                this.rejectButton.onClick.AddListener(() =>
                {
                    reject();
                    Gamesystem.instance.uiManager.HideConfirmDialog();
                });
            }
            
            if (abort != null)
            {
                this.abortButton.onClick.AddListener(() =>
                {
                    abort();
                    Gamesystem.instance.uiManager.HideConfirmDialog();
                });
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                confirmButton.onClick.Invoke();
            }
        }
    }
}