﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Ui
{
    public class LocationManager : SerializedMonoBehaviour
    {
        [NonSerialized] public List<(GameObject go, Vector3)> targets;
        [NonSerialized] public Dictionary<GameObject, GameObject> arrows;

        public Dictionary<LocationTargetType, GameObject> uiArrowPrefabs;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public void Awake()
        {
            targets = new ();
            arrows = new();
        }

        public void Update()
        {
            foreach (var (go, target) in targets)
            {
                Vector3 screenPos = mainCamera.WorldToViewportPoint(target);
                GameObject arrow = arrows[go];

                if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1)
                {
                    arrow.SetActive(false);
                    // Target is on screen; no need for arrow
                    continue;
                }
                
                arrow.SetActive(true);

                screenPos.x = Mathf.Clamp(screenPos.x, 0.1f, 0.9f);
                screenPos.y = Mathf.Clamp(screenPos.y, 0.1f, 0.9f);
                screenPos.z = 0;

                Vector3 worldPos = mainCamera.ViewportToWorldPoint(screenPos);
                worldPos.z = 0;

                float angle = Mathf.Atan2(target.y - worldPos.y, target.x - worldPos.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                arrow.transform.position = worldPos;
                arrow.transform.rotation = rotation;
                arrow.transform.SetParent(go.transform);
            }
        }

        public void AddTarget(Vector3 target, GameObject go, LocationTargetType type)
        {
            targets.Add((go, target));
            arrows.Add(go, Instantiate(uiArrowPrefabs[type]));
        }
        
        public void RemoveTarget(GameObject go)
        {
            targets.RemoveAll(t => t.go == go);
            
            if(arrows.TryGetValue(go, out var arrow))
            {
                Destroy(arrow);
                arrows.Remove(go);
            }
        }
    }

    public enum LocationTargetType
    {
        Item,
        Npc
    }
}