using System;

namespace _Chi.Scripts.Statistics
{
    /// <summary>
    /// Must be optimised and memory-light
    /// </summary>
    [Serializable]
    public class NpcStats
    {
        // not using Stat because this class will exist in many thousands instances
        
        public float speed = 1;
        public int rotationSpeed = 500;
    
        public float contactDamage;
        public float contactDamageInterval = 1f;
        
        public void CopyFrom(NpcStats prefab)
        {
            this.speed = prefab.speed;
            this.rotationSpeed = prefab.rotationSpeed;
            this.contactDamage = prefab.contactDamage;
            this.contactDamageInterval = prefab.contactDamageInterval;
        }
    }
}