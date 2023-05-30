using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Chi.Scripts.Mono.Common
{
    [Serializable]
    public class SpawnPositions
    {
        public List<Vector3> positions;
        
        public Vector3 GetRandomPosition()
        {
            return positions[UnityEngine.Random.Range(0, positions.Count)];
        }
    }
}