using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "Misc Settings", menuName = "Gama/Configuration/Misc Settings")]
    public class MiscSettings : SerializedScriptableObject
    {
        public float minSeekPathPeriod = 0.3f;
        public float maxSeekPathPeriod = 1.5f;

        public float maxStunnedSpeed = 0.1f;
    }
}