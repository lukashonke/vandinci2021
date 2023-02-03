using System;
using _Chi.Scripts.Mono.Common;

namespace _Chi.Scripts.Statistics
{
    [Serializable]
    public class OffensiveModuleStats
    {
        public Stat projectileCount = new Stat();
        
        public Stat projectileSpreadAngle = new Stat();
        
        public Stat projectileSpeed = new Stat();

        public Stat fireRate = new Stat();

        public Stat projectileDamage = new Stat();
        
        public Stat targetRange = new Stat();
        
        public Stat projectileRange = new Stat();
        
        public Stat projectileLifetime = new Stat();

        public int canProjectilePierce = 0;
        
        public Stat projectilePierceCount = new Stat();
        
        public float projectilePierceChance;

        public bool hasAreaEffect = false;
        public bool hasProjectile = true;

        public float areaEffectDamage;

        public float accuracy = 1f;

        public float pushForce = 0;

        public float effectDuration = 1f;
    }
}