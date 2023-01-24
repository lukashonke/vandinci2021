using System;
using _Chi.Scripts.Utilities;
using Pathfinding.Ionic.Zip;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class VehicleSettingsWindow : MonoBehaviour
    {
        [Required] public GameObject container;

        [NonSerialized] public PlayerBodyUi ui;

        public void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(false);
                Gamesystem.instance.Unpause();
                
                ApplyOnClose();
            }
            else
            {
                Initialise();
                this.gameObject.SetActive(true);
                Gamesystem.instance.Pause();
            }
            
            Gamesystem.instance.uiManager.UpdateFullscreenOverlay();
        }

        public void Close()
        {
            ApplyOnClose();
            
            this.gameObject.SetActive(false);
            Gamesystem.instance.Unpause();
            
            Gamesystem.instance.uiManager.UpdateFullscreenOverlay();
        }

        public void ApplyOnClose()
        {
            Gamesystem.instance.progress.ApplyRunToPlayer(Gamesystem.instance.objects.currentPlayer, Gamesystem.instance.progress.progressData.run);
        }

        public void Initialise()
        {
            var currentBody = Gamesystem.instance.progress.progressData.run.bodyId;

            var db = Gamesystem.instance.prefabDatabase;

            var bodyUi = db.GetById(currentBody).prefabUi;
            
            container.transform.RemoveAllChildren();

            var newBodyUi = Instantiate(bodyUi, container.transform);

            ui = newBodyUi.GetComponent<PlayerBodyUi>();
            
            ui.Initialise();
        }
    }
}