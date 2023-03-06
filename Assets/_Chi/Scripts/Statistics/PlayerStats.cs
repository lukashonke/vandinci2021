using System;
using _Chi.Scripts.Mono.Common;
using UnityEngine.Serialization;

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

        public Stat shieldEffectsStrength = new Stat();
        
        public Stat shieldEffectsRadius = new Stat();

        public Stat skillReuseMul = new Stat();

        public Stat skillPowerMul = new Stat();
        
        public Stat weightMul = new Stat();
        
        public Stat skillExtraChargeCount = new Stat();
        
        public Stat skillExtraChargeLoadMul = new Stat();

        public Stat skillUseHealthPercent = new Stat();

        public Stat baseCriticalRate = new Stat();

        public Stat criticalDamageMul = new Stat();

        public Stat nonCriticalDamageMul = new Stat();

        public Stat takeDamageAfterSkillUseMul = new Stat();

        public Stat takeDamageFaterSkillUseDuration = new Stat();

        public Stat receiveDamageMinInterval = new Stat();

        public Stat moduleFireRateMul = new Stat();

        public Stat visibilityRange = new Stat();

        public Stat pickupAttractRange = new Stat();
        
        public Stat pickupRange = new Stat();

        public Stat playerGoldDropped = new Stat();
    }
}