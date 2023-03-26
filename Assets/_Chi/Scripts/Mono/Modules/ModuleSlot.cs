using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using UnityEngine;

public class ModuleSlot : MonoBehaviour
{
    [NonSerialized] public int slotId;
    
    [NonSerialized] public Entity parent;
    [NonSerialized] public Module currentModule;
    
    public ModuleSlotType slotType;

    public List<ModuleSlot> connectedTo;
    [NonSerialized] public List<ModuleSlot> connectedFrom;

    public void Awake()
    {
        parent = GetComponentInParent<Entity>();

        slotId = this.transform.GetSiblingIndex();

        if (connectedTo != null)
        {
            foreach (var slot in connectedTo)
            {
                if (slot.connectedFrom == null) slot.connectedFrom = new();
                slot.connectedFrom.Add(this);
            }
        }
        
        var module = GetComponentInChildren<Module>();
        if (module != null)
        {
            SetModuleInSlot(module, activate: false);
        }
    }
    
    public void Start()
    {
        if (currentModule != null)
        {
            ActivateModuleInSlot();
        }
    }

    public void SetModuleInSlot(Module module, bool activate = true, bool destroyCurrent = false)
    {
        if (currentModule != module)
        {
            if (currentModule != null && module != null)
            {
                throw new Exception("Cannot add two modules into one slot! Remove previous module first.");
            }

            if (currentModule != null) // nastavuji na null
            {
                DeactivateModuleInSlot();
                currentModule.slot = null;

                if (destroyCurrent)
                {
                    Destroy(currentModule);
                }
            }

            currentModule = module;

            if (currentModule != null)
            {
                currentModule.slot = this;
                if (activate)
                {
                    ActivateModuleInSlot();
                }
            }
        }
    }
    
    public void ActivateModuleInSlot()
    {
        if (currentModule != null)
        {
            currentModule.OnAddedToSlot();
                
            if (connectedFrom != null && connectedFrom.Any())
            {
                // aktivuj sloty napojené na tento slot
                foreach (var moduleSlot in connectedFrom)
                {
                    if (moduleSlot.currentModule != null)
                    {
                        moduleSlot.currentModule.ActivateEffects();
                    }
                }
            }
        }
    }

    public void DeactivateModuleInSlot()
    {
        if (connectedFrom != null && connectedFrom.Any())
        {
            // deaktivuj sloty napojené na tento slot
            foreach (var moduleSlot in connectedFrom)
            {
                if (moduleSlot.currentModule != null)
                {
                    moduleSlot.currentModule.DeactivateEffects();
                }
            }
        }
                
        currentModule.OnRemovedFromSlot();
    }

    public Vector3 GetModulePosition()
    {
        return transform.position;
    }
}
