using System;
using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class LineRendererToTarget : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Transform target;
        
        public void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void LateUpdate()
        {
            lineRenderer.SetPosition(0, transform.position + new Vector3(0, 0, -1));
            lineRenderer.SetPosition(1, target.position + new Vector3(0, 0, -1));
        }
    }
}