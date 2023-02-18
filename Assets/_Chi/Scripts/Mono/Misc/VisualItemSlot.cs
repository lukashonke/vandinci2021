using System.ComponentModel;
using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class VisualItemSlot : MonoBehaviour
    {
        public VisualItemSlotType slotType;
        
        private GameObject currentInstance;
        
        public void SetContent(GameObject prefab)
        {
            if (currentInstance != null)
            {
                Destroy(currentInstance);
                currentInstance = null;
            }
            
            currentInstance = Instantiate(prefab, transform, false);
            currentInstance.transform.position = transform.position;
            currentInstance.transform.rotation = transform.rotation;
        }
    }

    public enum VisualItemSlotType
    {
        SpikesAndBlade
    }
}