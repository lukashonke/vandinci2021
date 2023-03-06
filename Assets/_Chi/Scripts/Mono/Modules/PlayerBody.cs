using _Chi.Scripts.Mono.Entities;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _Chi.Scripts.Mono.Modules
{
    public class PlayerBody : MonoBehaviour
    {
        public Collider2D damageAroundCollider;

        public float mass = 1f;
        
        public void Initialise(Player player)
        {
            player.rb.mass = mass;
        }
    }
}