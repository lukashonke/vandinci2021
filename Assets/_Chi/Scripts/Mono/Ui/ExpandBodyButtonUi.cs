using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class ExpandBodyButtonUi : MonoBehaviour
    {
        public int bodyPrefabId = 1000;

        public void OnClick()
        {
            Gamesystem.instance.progress.progressData.run.bodyId = bodyPrefabId;
            
            Gamesystem.instance.uiManager.vehicleSettingsWindow.OnBodyChange();
            
            Gamesystem.instance.uiManager.HideTooltip();
        }

        public void OnHoverEnter()
        {
            var db = Gamesystem.instance.prefabDatabase;
            
            var body = db.GetById(bodyPrefabId);
            
            Gamesystem.instance.uiManager.ShowItemTooltip((RectTransform) this.transform, body, 0);
        }

        public void OnHoverExit()
        {
            Gamesystem.instance.uiManager.HideTooltip();
        }
    }
}