using System;
using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class Spinner : MonoBehaviour
    {
        public float speed = 10;
        
        public void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, speed * Time.fixedDeltaTime);
        }
    }
}