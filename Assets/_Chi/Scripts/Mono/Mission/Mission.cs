using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Mission
{
    public class Mission : SerializedMonoBehaviour
    {
        public List<MissionEvent> events;

        public bool loopEvents;

        [ReadOnly] public MissionEvent currentEvent;

        private bool alive;

        void Start()
        {
            alive = true;

            StartCoroutine(UpdateLoop());
        }

        private IEnumerator UpdateLoop()
        {
            float timePassed = 0;
            const float loopInterval = 0.5f;
            var waiter = new WaitForSeconds(loopInterval);

            int currentEventIndex = 0;
            
            while (alive)
            {
                if (currentEvent != null && currentEvent.CanEnd(timePassed))
                {
                    currentEvent.End(timePassed);
                    currentEvent = null;
                }
                else if (currentEvent != null)
                {
                    currentEvent.Update();
                }

                if (currentEvent == null)
                {
                    if (currentEventIndex + 1 >= events.Count)
                    {
                        if (loopEvents)
                        {
                            currentEventIndex = 0;
                        }
                        else
                        {
                            yield return waiter;
                            continue;
                        }
                    }
                    
                    for (var index = currentEventIndex; index < events.Count; index++)
                    {
                        var ev = events[index];
                        if (ev.CanStart(timePassed))
                        {
                            ev.Start(timePassed);
                            currentEvent = ev;
                            currentEventIndex = index + 1;
                            break;
                        }
                    }
                }

                yield return waiter;
                timePassed += loopInterval;
            }
        }
        
        private void OnDestroy()
        {
            alive = false;
        }
    }
}