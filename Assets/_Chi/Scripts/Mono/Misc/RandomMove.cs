using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Misc
{
    public class RandomMove : MonoBehaviour
    {
        public bool chooseOneDirection;

        public float speed;

        private Vector3 direction;

        public void Start()
        {
            if (chooseOneDirection)
            {
                direction = Random.insideUnitCircle.normalized * speed;
            }
        }

        public void FixedUpdate()
        {
            transform.position += (direction * Time.fixedDeltaTime);
        }
    }
}