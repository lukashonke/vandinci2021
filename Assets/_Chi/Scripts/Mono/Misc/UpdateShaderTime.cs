using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Misc
{
    public class UpdateShaderTime : MonoBehaviour
    {
        public Image image;

        public string propertyName;
        
        private int propertyId;

        public void Start()
        {
            image = GetComponent<Image>();
            
            if (image != null)
            {
                propertyId = Shader.PropertyToID(propertyName);
            }
        }
        public void Update()
        {
            if (image != null)
            {
                image.material.SetFloat(propertyId, Time.unscaledTime);
                image.materialForRendering.SetFloat(propertyId, Time.unscaledTime);
                image.defaultMaterial.SetFloat(propertyId, Time.unscaledTime);
            }    
        }
    }
}