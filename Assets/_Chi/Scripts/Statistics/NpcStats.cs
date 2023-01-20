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
        
        public int speed = 1;
        public int rotationSpeed = 500;
    
        public float contactDamage;
    }
}