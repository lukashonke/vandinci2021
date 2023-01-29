using System;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class Module : SerializedMonoBehaviour
    {
        [ReadOnly] public Entity parent;
        [ReadOnly] public ModuleSlot slot;

        public float targetUpdateInterval = 0.2f;

        public float rotationSpeed = 500;

        [NonSerialized] public bool effectsActivated;

        [NonSerialized] public Transform currentTarget;

        public int level = 1;

        public virtual void Awake()
        {
            parent = GetComponentInParent<Entity>();
        }

        public virtual void Start()
        {
            
        }

        public virtual void OnDestroy()
        {
            if (slot != null)
            {
                slot.SetModuleInSlot(null);
            }
        }

        public virtual void OnAddedToSlot()
        {
            ActivateEffects();
        }

        public virtual void OnRemovedFromSlot()
        {
            DeactivateEffects();
            
            // chci deaktivovat tento slot
            if (slot.connectedFrom.Any())
            {
                // sloty napojené na tento slot
                foreach (var moduleSlot in slot.connectedFrom)
                {
                    if (moduleSlot.currentModule != null)
                    {
                        moduleSlot.currentModule.DeactivateEffects();
                    }
                }
            }
        }

        public virtual bool ActivateEffects()
        {
            if (effectsActivated) return false;
            effectsActivated = true;
            
            return true;
        }

        public virtual bool DeactivateEffects()
        {
            if (!effectsActivated) return false;
            effectsActivated = false;

            return true;
        }

        public Vector3 GetProjectilePosition()
        {
            return transform.position;
        }
        
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void SetRotation(Quaternion q)
        {
            this.transform.rotation = q;
        }

        public void RotateTowards(Vector3 rotationTarget, bool instantly = false)
        {
            if (instantly)
            {
                SetRotation(EntityExtensions.GetRotationTo(GetPosition(), rotationTarget));
            }
            else
            {
                SetRotation(EntityExtensions.RotateTowards(GetPosition(), rotationTarget, transform.rotation, rotationSpeed));
            }
        }
    }
}