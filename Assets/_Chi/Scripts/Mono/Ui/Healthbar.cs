using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [NonSerialized] public Entity parent;

    public GameObject healthGo;

    public void Awake()
    {
        parent = GetComponentInParent<Entity>();
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
    }

    private void Update()
    {
        Recalculate();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        try
        {
            transform.position = parent.GetPosition() + parent.healthbarOffset;
        }
        catch (Exception e)
        {
            if (parent == null)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    public void Recalculate()
    {
        var value = parent.entityStats.hp;
        var maxValue = parent.GetMaxHp();
        
        var scale = (value / (float)maxValue) * 1f;

        healthGo.transform.localScale = new Vector3(scale, 1, 1);
    }
}
