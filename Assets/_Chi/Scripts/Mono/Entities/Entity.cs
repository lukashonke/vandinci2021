using System;
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

        [HideInInspector] public Rigidbody2D rb;
        [NonSerialized] public bool hasRb;

        #endregion

        #region publics

        [NonSerialized] public bool activated;

        public Vector3? rotationTarget;
        
        [NonSerialized] public bool canMove;
        [NonSerialized] public bool isAlive = true;

        public EntityStats entityStats;
    
        public Teams team = Teams.Neutral;

        #endregion

        #region configuration

        public float healthbarOffset = 1;

        #endregion

        #region privates

        protected MiscSettings miscSettingsReference;

        #endregion
    
        public virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            hasRb = rb != null;

            Register();

            miscSettingsReference = Gamesystem.instance.miscSettings;
        }
        
        public virtual void Start()
        {
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

        public void ReceiveDamage(float damage, Entity damager)
        {
            entityStats.hp -= damage;
            
            if (entityStats.hp <= 0)
            {
                entityStats.hp = 0;
                isAlive = false;
                OnDie();
            }
            else if (entityStats.hp > GetMaxHp())
            {
                entityStats.hp = GetMaxHp();
            }

            if (entityStats.hp > 0 && !isAlive)
            {
                isAlive = true;
            }
        }

        public virtual void OnDie()
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
        
        public bool CanMove()
        {
            return canMove;
        }

        public bool CanShoot()
        {
            return isAlive;
        }

        public float GetMaxHp()
        {
            return (entityStats.maxHp + entityStats.maxHpAdd) * entityStats.maxHpMul;
        }

        public float GetHp() => entityStats.hp;
    }
}
