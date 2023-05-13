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

        [Range(0, 1)] public float spawnChance = 1f;

        public override bool Apply(Entity target, Vector3 targetPosition, Entity sourceEntity, Item sourceItem, Module sourceModule, float defaultStrength, ImmediateEffectParams parameters, ImmediateEffectFlags flags = ImmediateEffectFlags.None)
        {
            if (Random.value > spawnChance)
            {
                return true;
            }

            if (target != null)
            {
                DoSpawn(targetPosition, target.transform.rotation);
            }
            else
            {
                DoSpawn(targetPosition, null);
            }
            
            return true;
        }   
        
        private void DoSpawn(Vector3 position, Quaternion? rotation)
        {
            var instance = Gamesystem.instance.poolSystem.SpawnGo(prefab);
            instance.transform.position = position;

            if (rotation.HasValue)
            {
                instance.transform.rotation = rotation.Value;
            }
            
            if (despawnAfter > 0)
            {
                Gamesystem.instance.Schedule(Time.time + despawnAfter, () => Gamesystem.instance.poolSystem.DespawnGo(prefab, instance));
            }
        }
    }
}