using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using _Chi.Scripts.Utilities;
using BulletPro;
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
        [NonSerialized] public bool hasRenderer;
        [NonSerialized] public bool hasAnimator;
        [NonSerialized] public bool animatorSetup;
        [NonSerialized] public Animator animator;
        [NonSerialized] public bool hasBulletReceiver;
        [NonSerialized] public BulletReceiver bulletReceiver;
        
        #endregion

        #region publics

        [NonSerialized] public bool activated;

        public Vector3? rotationTarget;
        
        [NonSerialized] public bool canMove;
        [NonSerialized] public bool isAlive = true;
        [NonSerialized] public float immobilizedUntil = 0;
        [NonSerialized] public int immobilizedCounter = 0;
        [NonSerialized] public bool canReceiveDamage = true;
        [NonSerialized] public bool isFearing;
        [NonSerialized] public bool isInBlackHole;

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

            animator = GetComponent<Animator>();
            hasAnimator = animator != null;
            if (hasAnimator && animator.runtimeAnimatorController != null && animator.gameObject.activeSelf)
            {
                GetComponent<Animator>().SetFloat("Offset", Random.Range(0.0f, 1.0f));
            }
            
            bulletReceiver = GetComponent<BulletReceiver>();
            hasBulletReceiver = bulletReceiver != null;

            Register();

            miscSettingsReference = Gamesystem.instance.miscSettings;
            
            renderer = GetComponent<SpriteRenderer>();
            hasRenderer = renderer != null;
            if (hasRenderer)
            {
                originalMaterial = renderer.material;
            }

            currentEffects = new Dictionary<ImmediateEffect, float>(32);
            vfx = new Dictionary<GameObject, GameObject>(16);
            SetCanMove(true);
        }
        
        public virtual void Start()
        {
            entityStats.hp = GetMaxHp();
            isAlive = entityStats.hp > 0;
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
        
        public Quaternion GetRotation()
        {
            if (hasRb)
            {
                return Quaternion.Euler(0, 0, rb.rotation);
            }
            else
            {
                return transform.rotation;
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

        public virtual bool ReceiveDamage(float damage, Entity damager, DamageFlags damageFlags = DamageFlags.None, Color? damageTextColor = null)
        {
            if (damage <= 0 || !canReceiveDamage) return false;

            if (!CanReceiveDamage(damage, damager)) return false;
            
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
                if (damageTextColor.HasValue)
                {
                    Gamesystem.instance.prefabDatabase.playerCriticalDealtDamage.Spawn(transform.position, damage, damageTextColor.Value);
                }
                else
                {
                    Gamesystem.instance.prefabDatabase.playerCriticalDealtDamage.Spawn(transform.position, damage);
                }
            }
            else
            {
                if (damageTextColor.HasValue)
                {
                    Gamesystem.instance.prefabDatabase.playerDealtDamage.Spawn(transform.position, damage, damageTextColor.Value);
                }
                else
                {
                    Gamesystem.instance.prefabDatabase.playerDealtDamage.Spawn(transform.position, damage);
                }
            }

            return true;
        }

        public virtual void ReceivePush(Vector3 force, float pushDuration)
        {
            if (hasRb && CanBePushed())
            {
                rb.AddForce(force);
                SetImmobilizedUntil(Time.time + pushDuration);
            }
        }

        public virtual void OnDie(DieCause cause)
        {
            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                Debug.LogError("3 destroying npc " + name);
                Debug.LogError(e);
            }
            
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
        
        public Vector3 GetForwardVector()
        {
            return Utils.GetHeading(this.transform);
        }

        public Vector3 GetForwardVector(float angle)
        {
            return Quaternion.Euler(new Vector3(0, 0, angle)) * GetForwardVector();
        }

        public virtual void SetCanMove(bool b)
        {
            canMove = b;
        }

        public void SetIsInBlackHole(bool b)
        {
            isInBlackHole = b;
        }

        public void SetCanReceiveDamage(bool b)
        {
            canReceiveDamage = b;
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
            
            var instance = Gamesystem.instance.poolSystem.SpawnGo(prefab);
            vfx.Add(prefab, instance);

            instance.transform.position = this.transform.position;
            instance.transform.parent = this.transform;

            if (duration > 0)
            {
                Gamesystem.instance.Schedule(Time.time + duration, () => Gamesystem.instance.poolSystem.DespawnGo(prefab, instance));
            }
        }

        public void RemoveVfx(GameObject prefab)
        {
            if (vfx.TryGetValue(prefab, out var instance))
            {
                vfx.Remove(prefab);
                Gamesystem.instance.poolSystem.DespawnGo(prefab, instance);
            }
        }
        
        public void SetTemporaryMaterial(Material material, float duration = 0)
        {
            if (hasRenderer && renderer.material == originalMaterial)
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
            if (hasRenderer && renderer.sharedMaterial == material)
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

        public void SetFearing(bool b)
        {
            isFearing = b;
        }

        public void SetImmobilizedUntil(float time)
        {
            immobilizedUntil = time;
        }

        public virtual void UpdateImmobilized()
        {
            
        }

        public void SetMoving(float speed)
        {
            //TODO disable for optimization
            /*if (animatorSetup)
            {
                animator.SetFloat("MovementSpeed", speed);
            }*/
        }
        
        public virtual void OnSkillUse(Skill skill)
        {
        }

        public virtual void OnAfterSkillUse(Skill skill)
        {
        }
        
        public virtual SkillData GetSkillData(Skill skill)
        {
            return null;
        }
    }
}
