using System;
using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class RandomRotate : MonoBehaviour
    {
        public float rotateSpeed;
        
        public void FixedUpdate()
        {
            this.transform.Rotate(Vector3.forward, rotateSpeed * Time.fixedDeltaTime);
        }
    }
}