using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Misc
{
    public class RandomMove : MonoBehaviour
    {
        public bool chooseOneDirection;

        public int chanceToGoAgainstPlayer;

        public float speedMin;
        
        public float speedMax;

        private Vector3 direction;

        public void Start()
        {
            if (chooseOneDirection)
            {
                if (chanceToGoAgainstPlayer > 0 && Random.Range(0, 100) < chanceToGoAgainstPlayer)
                {
                    direction = (Gamesystem.instance.objects.currentPlayer.GetPosition() - transform.position).normalized * Random.Range(speedMin, speedMax);
                }
                else
                {
                    direction = Random.insideUnitCircle.normalized * Random.Range(speedMin, speedMax);
                }
            }
        }

        public void FixedUpdate()
        {
            transform.position += (direction * Time.fixedDeltaTime);
        }
    }
}