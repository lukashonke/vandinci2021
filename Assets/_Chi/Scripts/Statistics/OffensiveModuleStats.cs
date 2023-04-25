using System;
using _Chi.Scripts.Mono.Common;
using UnityEngine.Serialization;

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
        
        public Stat stationaryFireRateBoost = new Stat();

        public Stat movingFireRateBoost = new Stat();

        public Stat projectileDamage = new Stat();
        
        public Stat targetRange = new Stat();
        
        public Stat projectileRange = new Stat();
        
        public Stat projectileLifetime = new Stat();

        public int canProjectilePierce = 0;
        
        public int canProjectilePierceUsingDamage = 0;
        
        public Stat projectilePierceCount = new Stat();

        public Stat projectilePushForce = new Stat();

        public Stat projectileScale = new Stat();
        
        public Stat shotsPerShot = new Stat();

        public Stat shootOnSkillUse = new Stat();
        
        public Stat magazineSize = new Stat();

        public Stat fireRate = new Stat();
        
        public float projectilePierceChance;

        public bool hasAreaEffect = false;
        public bool hasProjectile = true;

        public float areaEffectDamage;

        public float accuracy = 1f;

        public float pushForce = 0;

        public float effectDuration = 1f;
    }
}