using System;
using _Chi.Scripts.Mono.Common;

namespace _Chi.Scripts.Statistics
{
    [Serializable]
    public class OffensiveModuleStats
    {
        public Stat projectileCount = new Stat();
        
        public Stat projectileSpeed = new Stat();

        public Stat fireRate = new Stat();
        
        public float detectRange = 5f;

        public float projectileLifetime = 1f;

        public bool canProjectilePierce = false;
        public int projectilePierceCount;
        public float projectilePierceChance;

        public bool hasAreaEffect = false;
        public bool hasProjectile = true;

        public float areaEffectDamage;

        public float accuracy = 1f;

        public float pushForce = 0;

        public float effectDuration = 1f;
    }
}