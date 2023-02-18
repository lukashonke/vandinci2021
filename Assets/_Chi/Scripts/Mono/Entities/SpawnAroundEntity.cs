using System;
using System.Collections;
using _Chi.Scripts.Mono.Mission;
using UnityEngine;

namespace _Chi.Scripts.Mono.Entities
{
    public class SpawnAroundEntity : Npc
    {
        //TODO make this part of variant
        //TODO cool effects - like a ring of evil or something
        public string spawnGroupName;

        public float delay;

        public bool despawnOnSpawn;

        public override void Start()
        {
            base.Start();
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public override void ApplyVariant(string variant)
        {
            base.ApplyVariant(variant);

            var variantInstance = Gamesystem.instance.prefabDatabase.GetVariant(variant);

            {
            if (variantInstance.parameters != null)
                spawnGroupName = variantInstance.parameters.spawnAroundEntityGroupName;
            }
        }

        public override void Setup(Vector3 position, Quaternion rotation)
        {
            base.Setup(position, rotation);
            
            StartCoroutine(RunCoroutine());
        }

        private IEnumerator RunCoroutine()
        {
            yield return new WaitForSeconds(delay);
            
            Gamesystem.instance.spawnAroundSettings.Spawn(spawnGroupName, transform.position);

            if (despawnOnSpawn)
            {
                Gamesystem.instance.poolSystem.Despawn(this);
            }
        }
    }
}