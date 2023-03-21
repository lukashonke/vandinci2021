using System;
using UnityEngine;

public class Statusbar : MonoBehaviour
{
    public Transform parent;

    public Vector3 offset;
    
    public GameObject statusGo;

    public float value;
    public float maxValue;
    public float scalePer1 = 0.01f;

    public void Awake()
    {
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
            transform.position = parent.position + offset;
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
        var scale = (value / (float)maxValue) * 1f;

        var transform1 = this.transform;
        var localScale = transform1.localScale;
        localScale = new Vector3(maxValue * scalePer1, localScale.y, localScale.z);
        transform1.localScale = localScale;
        statusGo.transform.localScale = new Vector3(scale, 1, 1);
    }
}
