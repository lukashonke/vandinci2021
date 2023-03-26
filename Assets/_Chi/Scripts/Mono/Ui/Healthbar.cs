using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [NonSerialized] public Entity parent;

    public GameObject healthGo;

    public float scalePer1Hp = 0.01f;

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
            if (!parent.activated)
            {
                Destroy(gameObject);
                return;
            }
            
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

        var transform1 = this.transform;
        var localScale = transform1.localScale;
        localScale = new Vector3(maxValue * scalePer1Hp, localScale.y, localScale.z);
        transform1.localScale = localScale;
        healthGo.transform.localScale = new Vector3(scale, 1, 1);
    }
}
