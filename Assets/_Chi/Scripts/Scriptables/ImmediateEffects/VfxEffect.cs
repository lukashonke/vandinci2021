using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
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

        public override bool Apply(Entity target, Entity sourceEntity, Item sourceItem, Module sourceModule, float strength)
        {
            var obj = Gamesystem.instance.poolSystem.SpawnVfx(vfxPrefab, vfxPoolSize);

            obj.transform.position = target.GetPosition();
            
            Gamesystem.instance.Schedule(Time.time + vfxDespawn, () => Gamesystem.instance.poolSystem.DespawnVfx(vfxPrefab, obj));
            return true;
        }    
    }
}