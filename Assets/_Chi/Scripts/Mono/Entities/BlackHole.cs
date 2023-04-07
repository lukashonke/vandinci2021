using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class BlackHole : MonoBehaviour, IPooledGameobject
    {
        private Collider2D coll;
        public Teams team;

        public int attachedLimit = 100;

        public float minDistance2 = 2.5f;
        
        public float holdInsideCircleRadius = 0.35f;

        public float distToStopPullingThreshold = 0.01f;

        public float force;

        public float rotationForce = 300;
        
        [NonSerialized] private List<Rigidbody2D> attached = new();

        public void Awake()
        {
            coll = GetComponent<Collider2D>();
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
                if(dir.sqrMagnitude > minDistance2)
                {
                    attached.RemoveAt(index);
                    var entity = rb.gameObject.GetEntity();
                    entity.SetCanMove(true);
                    entity.SetIsInBlackHole(false);
                    continue;
                }

                var randomPosition = GetRandomPosition(index);

                var position1 = rb.position;
                var direction = (randomPosition - position1).normalized;
                if (Utils.Dist2(randomPosition, rb.position) > distToStopPullingThreshold)
                {
                    var distanceToTarget = Vector3.Distance(position1, randomPosition);
    
                    if (distanceToTarget > distToStopPullingThreshold)
                    {
                        var target = rb.position + direction * (force * Time.fixedDeltaTime);
                        rb.MovePosition(target);
                    }
                    else
                    {
                        // We're within range of the target, so just move directly to it
                        rb.MovePosition(randomPosition);
                    }
                }
                
                var directionToTarget = ((Vector2) transform.position - rb.position).normalized;
                var angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                rb.rotation = angleToTarget + 90f; // Rotate 180 degrees to face away from the target
            }
        }

        public float rotationSpeed = 2f;

        private Vector2 GetRandomPosition(int index)
        {
            var position = (Vector2) transform.position;
            var angle = index * 360f / attachedLimit;

            if (rotationSpeed > 0)
            {
                angle += (Time.time) * rotationSpeed * 360f;
            }
            
            var dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            return position + dir * holdInsideCircleRadius;
        }

        public void OnDestroy()
        {
            ReleaseAttached();
        }

        private void ReleaseAttached()
        {
            foreach (var rb in attached)
            {
                if (rb != null)
                {
                    var entity = rb.gameObject.GetEntity();
                    entity.SetCanMove(true);
                    entity.SetIsInBlackHole(false);
                }
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if(attached.Count >= attachedLimit) return;
            
            var entity = other.gameObject.GetEntity();
            if (entity != null && entity.team != team && entity.hasRb && !entity.isInBlackHole && entity.CanGoToBlackHole())
            {
                entity.SetCanMove(false);
                entity.SetIsInBlackHole(true);
                attached.Add(entity.rb);
            }
        }

        public void OnReturnedToPool()
        {
            ReleaseAttached();
        }

        public void OnTakeFromPool()
        {
            
        }
    }
}