using System;
using _Chi.Scripts.Mono.Common;

namespace _Chi.Scripts.Statistics
{
    [Serializable]
    public class PlayerStats
    {
        public Stat speed = new Stat();
        
        public Stat rotationSpeed = new Stat();

        public Stat nearbyEnemyRangeSqrt = new Stat();

        public Stat velocityToDamageMul = new Stat();
        
        public Stat minVelocityToDamage = new Stat();

        public Stat maxDistanceToReceiveContactDamage = new Stat();

        public Stat hpRegenPerSecond = new Stat();

        public Stat shieldChargesCount = new Stat();

        public Stat singleShieldRechargeDelay = new Stat();
    }
}