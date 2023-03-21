using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

namespace _Chi.Scripts.Mono.Entities
{
    public class Projectile : MonoBehaviour
    {
        [Unity.Collections.ReadOnly] public Entity owner;
        [Unity.Collections.ReadOnly] public Module ownerModule;
        [Unity.Collections.ReadOnly] private bool hasOwner;
        [Unity.Collections.ReadOnly] private bool hasOwnerModule;

        [NonSerialized] public Rigidbody2D rb;
        [NonSerialized] public bool hasRb;
        [NonSerialized] public Collider2D projectileCollider;
        
        public List<ImmediateEffect> effects;

        public TargetType affectType;

        [NonSerialized] public ProjectileInstanceStats stats;

        public int poolId;
        
        [Button]
        [HideInPlayMode]
        public void RandomPoolId()
        {
            poolId = GetInstanceID();
        }
        
        public float baseStrength = 1;

        public bool getHitsOnSpawn;
        public bool noDespawnAfterHit;

        public void Shoot(Vector3 direction)
        {
            
        }
        
        public void Awake()
        {
            stats = new ProjectileInstanceStats();
            projectileCollider = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
            hasRb = rb != null;
        }
        
        private List<Collider2D> buffer = new List<Collider2D>(256);

        public void Start()
        {
            
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            if (getHitsOnSpawn) return;
            
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
                effect.Apply(entity, owner, null, ownerModule, baseStrength, new ImmediateEffectParams());

                if (!noDespawnAfterHit)
                {
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
            if (module != null)
            {
                this.owner = module.parent;
                this.ownerModule = module;
                this.transform.position = ownerModule.GetProjectilePosition();
            }
            
            Setup();
        }

        public void Setup(Entity owner)
        {
            this.owner = owner;
            this.transform.position = owner.GetPosition();

            Setup();
        }

        public void SetScale(float f)
        {
            transform.localScale = new Vector3(f, f, f);
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

            if (getHitsOnSpawn)
            {
                StartCoroutine(GetHitsCoroutine());
            }
        }

        private IEnumerator GetHitsCoroutine()
        {
            yield return new WaitForFixedUpdate();
            
            if (getHitsOnSpawn)
            {
                Physics2D.OverlapCollider(projectileCollider, new ContactFilter2D()
                {
                    useTriggers = true
                }, buffer);
                
                for (var index = 0; index < buffer.Count; index++)
                {
                    var col = buffer[index];
                    var entity = col.gameObject.GetEntity();
                    if (entity != null && CanAffect(entity))
                    {
                        Affect(entity);
                    }
                }
            }
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