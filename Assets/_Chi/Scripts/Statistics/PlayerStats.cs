using System;
using _Chi.Scripts.Mono.Common;

namespace _Chi.Scripts.Statistics
{
    [Serializable]
    public class PlayerStats
    {
        public Stat speed = new Stat(1);
        
        public Stat rotationSpeed = new Stat(500);
    }
}