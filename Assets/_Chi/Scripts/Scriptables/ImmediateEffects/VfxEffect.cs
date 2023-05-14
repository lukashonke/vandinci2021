using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "VFX", menuName = "Gama/Immediate Effects/VFX")]
    public class VfxEffect : ImmediateEffect
    {
        public GameObject vfxPrefab;

        public int vfxPoolSize = 100;

        public float vfxDespawn = 0.5f;

        public override bool Apply(EffectSourceData data, float strength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            var obj = Gamesystem.instance.poolSystem.SpawnGo(vfxPrefab, vfxPoolSize);

            obj.transform.position = data.targetPosition;
            
            Gamesystem.instance.Schedule(Time.time + vfxDespawn, () => Gamesystem.instance.poolSystem.DespawnGo(vfxPrefab, obj));
            return true;
        }    
    }
}