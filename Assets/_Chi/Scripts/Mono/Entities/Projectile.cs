using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using Unity.Collections;
using UnityEngine;
using Object = System.Object;

namespace _Chi.Scripts.Mono.Entities
{
    public class Projectile : MonoBehaviour
    {
        [ReadOnly] public Entity owner;
        [ReadOnly] public Module ownerModule;
        [ReadOnly] private bool hasOwner;
        [ReadOnly] private bool hasOwnerModule;

        [NonSerialized] public Rigidbody2D rb;
        [NonSerialized] public bool hasRb;
        
        public List<ImmediateEffect> effects;

        public TargetType affectType;

        [NonSerialized] public ProjectileInstanceStats stats;

        [NonSerialized] public int poolId;

        public void Shoot(Vector3 direction)
        {
            
        }
        
        public void Awake()
        {
            poolId = this.gameObject.GetInstanceID();

            stats = new ProjectileInstanceStats();
            rb = GetComponent<Rigidbody2D>();
            hasRb = rb != null;
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            var entity = col.gameObject.GetEntity();
            if (entity != null && CanAffect(entity))
            {
                Affect(entity);
            }
        }

        public void Affect(Entity entity)
        {
            if (!stats.active)
            {
                return;
            }

            if (!entity.isAlive)
            {
                return;
            }
            
            for (var index = 0; index < effects.Count; index++)
            {
                var effect = effects[index];
                effect.Apply(entity, owner, null, null, 1);

                bool deactivate = false;

                if (hasOwnerModule && ownerModule is OffensiveModule offensiveModule)
                {
                    if (offensiveModule.stats.canProjectilePierce > 0)
                    {
                        stats.piercedEnemies++;
                        if (stats.piercedEnemies >= offensiveModule.stats.projectilePierceCount.GetValueInt())
                        {
                            deactivate = true;
                        }
                    }
                    else
                    {
                        deactivate = true;
                    }
                }
                else
                {
                    deactivate = true;
                }

                if (deactivate)
                {
                    Deactivate();
                }
            }
        }

        public void Deactivate()
        {
            this.gameObject.SetActive(false);
            stats.active = false;
        }
        
        public bool CanAffect(Entity entity)
        {
            if (affectType == TargetType.EnemyOnly)
            {
                return owner.team != entity.team;
            }

            if (affectType == TargetType.FriendlyOnly)
            {
                return owner.team == entity.team;
            }

            return true;
        }

        public void Setup(Module module)
        {
            this.owner = module.parent;
            this.ownerModule = module;
            this.transform.position = ownerModule.GetProjectilePosition();
            
            Setup();
        }

        public void Setup(Entity owner)
        {
            this.owner = owner;
            this.transform.position = owner.GetPosition();

            Setup();
        }
        
        private void Setup()
        {
            this.gameObject.SetActive(true);
            
            this.hasOwnerModule = ownerModule != null;
            this.hasOwner = owner != null;

            if (hasRb)
            {
                rb.WakeUp();
            }
            
            stats.Reset();
        }

        public void Cleanup()
        {
            this.owner = null;
            this.ownerModule = null;
            this.gameObject.SetActive(false);

            if (hasRb)
            {
                rb.velocity = Vector2.zero;
                rb.Sleep();
            }
        }
    }
}