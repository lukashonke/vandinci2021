using System;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Misc;
using _Chi.Scripts.Mono.Modules.Offensive.Subs;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules
{
    public abstract class Module : SerializedMonoBehaviour
    {
        [NonSerialized] public Dictionary<object, List<SubEmitter>> subEmitters;
        
        [ReadOnly] public Entity parent;
        [ReadOnly] public ModuleSlot slot;

        public List<ModuleUpgradeItem> upgrades;
        
        public float targetUpdateInterval = 0.2f;

        public float rotationSpeed = 500;

        public bool instantRotation = true;

        public List<SetVisualItemSlot> visualItems;

        [NonSerialized] public bool effectsActivated;

        [NonSerialized] public Transform currentTarget;

        [NonSerialized] public int level = 1;
        
        [NonSerialized] public Quaternion originalRotation;

        public virtual void Awake()
        {
            parent = GetComponentInParent<Entity>();

            originalRotation = transform.rotation;
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

            if (visualItems != null && parent is Player player)
            {
                foreach (var item in visualItems)
                {
                    player.SetVisualItems(item, false);
                }
            }
        }

        public virtual void OnRemovedFromSlot()
        {
            DeactivateEffects();
            
            // chci deaktivovat tento slot
            if (slot.connectedFrom != null && slot.connectedFrom.Any())
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
            
            if (visualItems != null && parent is Player player)
            {
                foreach (var item in visualItems)
                {
                    player.SetVisualItems(item, true);
                }
            }
        }

        public virtual bool ActivateEffects()
        {
            if (effectsActivated) return false;
            effectsActivated = true;
            
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplyEffects(this);
            }
            
            return true;
        }

        public virtual bool DeactivateEffects()
        {
            if (!effectsActivated) return false;
            effectsActivated = false;
            
            foreach (var upgrade in upgrades)
            {
                upgrade.RemoveEffects(this);
            }

            return true;
        }
        
        public void AddSubEmitter(object source, SubEmitter go)
        {
            if (!subEmitters.ContainsKey(source))
            {
                subEmitters.Add(source, new List<SubEmitter>());
            }
            
            subEmitters[source].Add(go);
        }

        public void DestroySubEmitters(object source)
        {
            if (subEmitters.ContainsKey(source))
            {
                foreach (var su in subEmitters[source])
                {
                    Destroy(su.gameObject);
                }

                subEmitters.Remove(source);
            }
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

        public void SetOriginalRotation(int angle)
        {
            originalRotation = Quaternion.Euler(0, 0, angle);
            this.transform.rotation = originalRotation;
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

        public virtual List<(string title, string value)> GetUiStats(int level) => null;
    }

    public class SetVisualItemSlot
    {
        public VisualItemSlotType slotType;
        public GameObject prefab;
    }
}