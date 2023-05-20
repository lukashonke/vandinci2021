using _Chi.Scripts.Mono.Common;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Reward", menuName = "Gama/Immediate Effects/Reward")]
    public class RewardEffect : ImmediateEffect
    {
        public DropType type;

        public int countMin;
        public int countMax;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            int amount = Random.Range(countMin, countMax);
            if (type == DropType.Level1Gold || type == DropType.Level15Gold || type == DropType.Level2Gold ||
                type == DropType.Level3Gold)
            {
                Gamesystem.instance.progress.AddGold(amount);
                Gamesystem.instance.objects.currentPlayer.OnPickupGold(amount);
            }
            else if (type == DropType.Level1Exp || type == DropType.Level15Exp || type == DropType.Level2Exp ||
                     type == DropType.Level3Exp)
            {
                Gamesystem.instance.progress.AddExp(amount);
                Gamesystem.instance.objects.currentPlayer.OnPickupExp(amount);
            }
            return true;
        }  
    }
}