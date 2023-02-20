using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using UnityEngine;

namespace _Chi.Scripts.Mono.Entities
{
    public class InvertedBlackHole : MonoBehaviour
    {
        private CircleCollider2D coll;
        public Teams team;

        public int attachedLimit = 100;

        public float radius2;

        public float force;

        public void Awake()
        {
            coll = GetComponent<CircleCollider2D>();
        }
        
        public void Start()
        {
            
        }

        public void FixedUpdate()
        {
            var position = (Vector2) transform.position;
            for (var index = attached.Count - 1; index >= 0; index--)
            {
                Rigidbody2D rb = attached[index];
                var dir = (position - rb.position);
                if (dir.sqrMagnitude > radius2)
                {
                    attached.Remove(rb);
                }
                else
                {
                    rb.MovePosition(rb.position + (rb.position - position).normalized * (force * Time.fixedDeltaTime));
                }
            }
        }

        public void OnDestroy()
        {
            foreach (var rb in attached)
            {
                if (rb != null)
                {
                    var entity = rb.gameObject.GetEntity();
                    //entity.SetCanMove(true);
                }
            }
        }

        [NonSerialized] private List<Rigidbody2D> attached = new();

        public void OnTriggerEnter2D(Collider2D other)
        {
            if(attached.Count >= attachedLimit) return;
            
            var entity = other.gameObject.GetEntity();
            if (entity != null && entity.team != team && entity.hasRb)
            {
                //entity.SetCanMove(false);
                attached.Add(entity.rb);
            }
        }
    }
}