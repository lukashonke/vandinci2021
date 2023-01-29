using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Chi.Scripts.Mono.Mission
{
    public class MissionManager : MonoBehaviour
    {
        public IMissionHandler[] handlers;

        public void Awake()
        {
            handlers = GetComponentsInChildren<IMissionHandler>();
        }

        public void Start()
        {
            foreach (var handler in handlers)
            {
                handler.OnStart();
            }
        }

        public void OnDestroy()
        {
            foreach (var handler in handlers)
            {
                handler.OnStop();
            }
        }
    }
}