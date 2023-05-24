using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;

namespace _Chi.Scripts.Statistics
{
    [Serializable]
    public class OffensiveModuleStats
    {
        public Stat projectileCount = new Stat();
        
        public Stat projectileMultiplier = new Stat();
        
        public Stat projectileSpreadAngle = new Stat();

        public Stat projectileDelayBetweenConsecutiveShots = new Stat();
        
        public Stat projectileSpeed = new Stat();

        public Stat reloadDuration = new Stat();
        
        public Stat stationaryReloadDurationBoost = new Stat();

        public Stat movingReloadDurationBoost = new Stat();

        public Stat projectileDamage = new Stat();
        
        public Stat targetRange = new Stat();
        
        public Stat projectileRange = new Stat();
        
        public Stat projectileLifetime = new Stat();

        public int canProjectilePierce = 0;
        
        public int canProjectilePierceUsingDamage = 0;
        
        public Stat projectilePierceCount = new Stat();
        
        public Stat projectilePierceChance = new Stat();
        
        public Stat projectilePierceCountIgnoreKilled = new Stat();
        
        public Stat projectilePierceCountIgnoreIfLessThanHp = new Stat();
        
        public Stat projectilePierceCountIgnoreIfLessThanProjectileDamagePortion = new Stat();

        public Stat projectilePierceDeadChance = new Stat();
        
        public Stat projectilePierceDeadCount = new Stat();
        
        public Stat projectilePushForce = new Stat();

        public Stat projectileScale = new Stat();
        
        public Stat shotsPerShot = new Stat();

        public Stat shootOnSkillUse = new Stat();
        
        public Stat magazineSize = new Stat();

        public Stat fireRate = new Stat();
        
        public Stat stationaryFireRateBoost = new Stat();

        public Stat movingFireRateBoost = new Stat();
        
        public Stat projectileCriticalRate = new Stat();
        
        public Stat projectileCriticalDamage = new Stat();

        // bypass % of armor 
        public Stat armorPiercing = new Stat();
        
        // mul of damage against armored enemies
        public Stat armoredDamageMul = new Stat();
        
        // mul of damage against non armored enemies
        public Stat nonArmorDamageMul = new Stat();
        
        public Stat instantReloadChance = new Stat();
        
        public Stat consumeNoAmmoChance = new Stat();
        public Stat standingConsumeNoAmmoChance = new Stat();
        public Stat movingConsumeNoAmmoChance = new Stat();
        
        public Stat projectileRandomRotation = new Stat();

        //TODO
        public Dictionary<ImmediateEffectType, Stat> effectDurationMuls;
        public Dictionary<ImmediateEffectType, Stat> effectStrengthMuls;
        public Dictionary<ImmediateEffectType, Stat> effectChanceMuls;

        public bool hasAreaEffect = false;
        public bool hasProjectile = true;

        public float areaEffectDamage;

        public float accuracy = 1f;

        public float pushForce = 0;

        public float effectDuration = 1f;
    }
}