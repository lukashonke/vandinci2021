using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ImmediateEffects
{
    [CreateAssetMenu(fileName = "Spawn", menuName = "Gama/Immediate Effects/Spawn")]
    public class SpawnEffect : ImmediateEffect
    {
        public GameObject prefab;

        public float despawnAfter;

        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float defaultStrength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            DoSpawn(targetPosition);
            return true;
        }   
        
        private void DoSpawn(Vector3 position)
        {
            var instance = Gamesystem.instance.poolSystem.SpawnGo(prefab);
            instance.transform.position = position;
            
            if (despawnAfter > 0)
            {
                Gamesystem.instance.Schedule(Time.time + despawnAfter, () => Gamesystem.instance.poolSystem.DespawnGo(prefab, instance));
            }
        }
    }
}