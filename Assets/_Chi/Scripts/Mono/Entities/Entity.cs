using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Movement;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public abstract class Entity : MonoBehaviour
    {
        #region references

        [NonSerialized] public Rigidbody2D rb;
        [NonSerialized] public bool hasRb;
        [NonSerialized] public Collider2D triggerCollider; 
        [NonSerialized] public SpriteRenderer renderer;

        #endregion

        #region publics

        [NonSerialized] public bool activated;

        public Vector3? rotationTarget;
        
        [NonSerialized] public bool canMove;
        [NonSerialized] public bool isAlive = true;
        [NonSerialized] public float immobilizedUntil = 0;
        [NonSerialized] public int immobilizedCounter = 0;

        public EntityStats entityStats;
    
        public Teams team = Teams.Neutral;
        
        [NonSerialized] public Dictionary<ImmediateEffect, float> currentEffects;
        [NonSerialized] public Dictionary<GameObject, GameObject> vfx;

        #endregion

        #region configuration

        public Vector3 healthbarOffset = Vector3.zero;

        #endregion

        #region privates

        protected MiscSettings miscSettingsReference;
        
        protected Material originalMaterial;

        #endregion
    
        public virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            hasRb = rb != null;

            triggerCollider = GetComponent<Collider2D>();

            Register();

            miscSettingsReference = Gamesystem.instance.miscSettings;
            
            renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                originalMaterial = renderer.material;
            }
            
            currentEffects = new Dictionary<ImmediateEffect, float>(32);
            vfx = new Dictionary<GameObject, GameObject>(16);
        }
        
        public virtual void Start()
        {
            entityStats.hp = entityStats.maxHp;
            isAlive = entityStats.hp > 0;
            canMove = true;
        }

        // Update is called once per frame
        public virtual void DoUpdate()
        {
            
        }

        public virtual void FixedUpdate()
        {
        
        }

        public virtual void OnDestroy()
        {
            Unregister();
        }

        public void Register()
        {
            if (activated) return;
            activated = true;
            Gamesystem.instance.objects.RegisterEntity(this);
        }

        public void Unregister()
        {
            if (!activated) return;
            activated = false;
            Gamesystem.instance.objects.UnregisterEntity(this);
        }

        public Vector3? GetRotationTarget()
        {
            return rotationTarget;
        }

        public void SetRotationTarget(Vector3? target)
        {
            rotationTarget = target;
        }
        
        public void Move(Vector2 direction)
        {
            if (hasRb)
            {
                rb.MovePosition(rb.position + (direction * Time.fixedDeltaTime));
            }
            else
            {
                transform.position = transform.position + (Vector3)(direction * Time.fixedDeltaTime);
            }
        }
    
        public void MoveTo(Vector3 destination)
        {
            if (hasRb)
            {
                rb.MovePosition(destination);
            }
            else
            {
                transform.position = destination;
            }
        }

        public void SetRotation(Quaternion q)
        {
            if (hasRb)
            {
                rb.rotation = q.eulerAngles.z;
            }
            else
            {
                transform.rotation = q;
            }
        }

        public virtual void OnTriggerEnter2D(Collider2D col)
        {
        
        }

        public virtual bool CanReceiveDamage(float damage, Entity damager)
        {
            return true;
        }

        public void ReceiveDamage(float damage, Entity damager, DamageFlags damageFlags = DamageFlags.None)
        {
            if (damage <= 0) return;

            if (!CanReceiveDamage(damage, damager)) return;
            
            entityStats.hp -= damage;
            
            if (entityStats.hp <= 0)
            {
                entityStats.hp = 0;

                if (isAlive)
                {
                    isAlive = false;
                    OnDie(DieCause.Killed);
                }
            }
            else if (entityStats.hp > GetMaxHp())
            {
                entityStats.hp = GetMaxHp();
            }

            if (entityStats.hp > 0 && !isAlive)
            {
                isAlive = true;
            }

            if (damageFlags.HasFlag(DamageFlags.Critical))
            {
                Gamesystem.instance.prefabDatabase.playerCriticalDealtDamage.Spawn(transform.position, damage);
            }
            else
            {
                Gamesystem.instance.prefabDatabase.playerDealtDamage.Spawn(transform.position, damage);
            }

            //if (damager is Player)
            {
            }
        }

        public virtual void ReceivePush(Vector3 force, float pushDuration)
        {
            if (hasRb && CanBePushed())
            {
                rb.AddForce(force);
                immobilizedUntil = Time.time + pushDuration;
            }
        }

        public virtual void OnDie(DieCause cause)
        {
            Destroy(this.gameObject);
        }
        
        public Vector3 GetPosition()
        {
            if (hasRb)
            {
                return rb.position;
            }
            else
            {
                return transform.position;
            }
        }

        public void SetCanMove(bool b)
        {
            canMove = b;
        }
        
        public bool CanMove()
        {
            return canMove && immobilizedUntil < Time.time && immobilizedCounter == 0;
        }

        public bool CanShoot()
        {
            return isAlive;
        }

        public float GetMaxHp()
        {
            return (entityStats.maxHp + entityStats.maxHpAdd) * entityStats.maxHpMul;
        }
        
        public virtual bool CanBePushed()
        {
            return true;
        }
        
        public virtual void SetCannotBePushed(float duration)
        {
        }

        public float GetHp() => entityStats.hp;

        /// <summary>
        ///  returns false if the effect already existed and its duration should be extended
        /// </summary>
        public bool AddImmediateEffect(ImmediateEffect effect, float addDuration, bool stackDuration)
        {
            if (currentEffects.ContainsKey(effect))
            {
                if (stackDuration)
                {
                    currentEffects[effect] += addDuration;
                }
                else
                {
                    currentEffects[effect] = Time.time + addDuration;
                }
                return false;
            }
            else
            {
                currentEffects.Add(effect, Time.time + addDuration);
                return true;
            }
        }

        public float TryRemoveImmediateEffect(ImmediateEffect effect)
        {
            if (currentEffects.TryGetValue(effect, out var until))
            {
                if (until - 0.1f < Time.time)
                {
                    currentEffects.Remove(effect);
                    return 0;
                }
                
                // not yet, duration was propably extended
                return until;
            }

            // effect no longer exists
            return -1;
        }

        public void AddVfx(GameObject prefab, float duration = 0)
        {
            if (vfx.ContainsKey(prefab)) return;
            
            var instance = Gamesystem.instance.poolSystem.SpawnVfx(prefab);
            vfx.Add(prefab, instance);

            instance.transform.position = this.transform.position;
            instance.transform.parent = this.transform;

            if (duration > 0)
            {
                Gamesystem.instance.Schedule(Time.time + duration, () => Gamesystem.instance.poolSystem.DespawnVfx(prefab, instance));
            }
        }

        public void RemoveVfx(GameObject prefab)
        {
            if (vfx.TryGetValue(prefab, out var instance))
            {
                vfx.Remove(prefab);
                Gamesystem.instance.poolSystem.DespawnVfx(prefab, instance);
            }
        }
        
        public void SetTemporaryMaterial(Material material, float duration = 0)
        {
            if (renderer.material == originalMaterial)
            {
                renderer.material = material;

                if (duration > 0)
                {
                    Gamesystem.instance.Schedule(Time.time + duration, () => ResetTemporaryMaterial(material));
                }
            } 
        }

        public void ResetTemporaryMaterial(Material material)
        {
            if (renderer.sharedMaterial == material)
            {
                renderer.material = originalMaterial;
            }
        }

        public void SetImmobilized(bool b)
        {
            if (b) immobilizedCounter++;
            else immobilizedCounter--;
            UpdateImmobilized();
        }

        public virtual void UpdateImmobilized()
        {
            
        }
    }
}
