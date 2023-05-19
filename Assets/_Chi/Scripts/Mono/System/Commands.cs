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
    }
}