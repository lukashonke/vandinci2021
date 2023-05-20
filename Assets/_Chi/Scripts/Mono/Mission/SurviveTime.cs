using System;
using System.Collections;
using _Chi.Scripts.Mono.Mission.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _Chi.Scripts.Mono.Mission
{
    /*public class SurviveTime : SerializedMonoBehaviour, IMissionHandler
    {
        public int secondsToSurvive = 60;

        [NonSerialized] public float remainingTime;
        
        private bool stopped;
        
        public void OnStart(MissionEvent ev, float fixedDuration)
        {
            remainingTime = secondsToSurvive;
            StartCoroutine(UpdateLoop());
        }

        private IEnumerator UpdateLoop()
        {
            var waiter = new WaitForSeconds(0.5f);
            while (!stopped && remainingTime > 0)
            {
                remainingTime -= 0.5f;
                
                Gamesystem.instance.uiManager.SetMissionText($"Survive for {(int) remainingTime} sec");

                yield return waiter;
            }
            
            Gamesystem.instance.uiManager.SetMissionText("Mission Complete!");
        }

        public void OnStop()
        {
            stopped = true;
        }

        public bool IsFinished()
        {
            return true;
        }
    }*/
}