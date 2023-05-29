using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Mission;
using QFSW.QC;
using UnityEngine;

namespace _Chi.Scripts.Mono.System
{
    public class Commands : MonoBehaviour
    {
        [Command()]
        public void AddGold(int amount)
        {
            Gamesystem.instance.progress.AddGold(amount);
        }
        
        [Command()]
        public void AddExp(int amount)
        {
            Gamesystem.instance.progress.AddExp(amount);
        }
        
        [Command()]
        public void ResetGold()
        {
            Gamesystem.instance.progress.RemoveGold(Gamesystem.instance.progress.GetGold());
        }
        
        [Command()]
        public void ResetExp()
        {
            Gamesystem.instance.progress.RemoveExp(Gamesystem.instance.progress.GetExp());
        }

        [Command()]
        public void Pause()
        {
            Time.timeScale = 0;
        }
        
        [Command()]
        public void Resume()
        {
            Time.timeScale = 1;
        }

        [Command()]
        public void Rewards(int upToIndex)
        {
            StartCoroutine(RewardCoroutine(upToIndex, false));
        }
        
        [Command()]
        public void Reward(int index)
        {
            StartCoroutine(RewardCoroutine(index, true));
        }

        [Command()]
        public void DisableReward()
        {
            Gamesystem.instance.progress.disabledRewards = true;
        }

        [Command()]
        public void EnableRewards()
        {
            Gamesystem.instance.progress.disabledRewards = false;
        }

        private IEnumerator RewardCoroutine(int index, bool equal)
        {
            if (Gamesystem.instance.missionManager.currentMission == null) yield break;
            
            var mission = Gamesystem.instance.missionManager.currentMission;

            int shopIndex = 0;

            for (int i = 0; i <= index; i++)
            {
                if(equal && i != index) continue;
                
                foreach (var reward in mission.progressSettings.simulatedRewards ?? new List<TriggeredShop>())
                {
                    if (reward.index == i)
                    {
                        reward.Show();
                
                        // wait until its closed
                        while (Gamesystem.instance.uiManager.vehicleSettingsWindow.Opened())
                        {
                            yield return null;
                        }
                    }
                }
                
                foreach (var reward in mission.progressSettings.shops)
                {
                    if (reward.index == i)
                    {
                        reward.Show();
                
                        // wait until its closed
                        while (Gamesystem.instance.uiManager.vehicleSettingsWindow.Opened())
                        {
                            yield return null;
                        }
                
                        mission.progressSettings.lastExpTriggeredShopLevelIndex = shopIndex++;
                    }
                }
            }
            
            // reset gold
            Gamesystem.instance.progress.RemoveGold(Gamesystem.instance.progress.GetGold());
        }
    }
}