using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Chi.Scripts.Mono.Entities
{
    public class Item : SerializedMonoBehaviour
    {
        public List<ImmediateEffect> effects;

        private void Awake()
        {
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            var entity = col.gameObject.GetEntity();
            if (CanPickup(entity))
            {
                if (OnPickup(entity))
                {
                    DestroyMe();
                }
            }
        }

        public bool CanPickup(Entity e)
        {
            return e is Player;
        }

        public void DestroyMe()
        {
            Destroy(this.gameObject);
        }

        public bool OnPickup(Entity entity)
        {
            for (var index = 0; index < effects.Count; index++)
            {
                var effect = effects[index];
                effect.Apply(entity, null, this, null);
            }

            return true;
        }
    }
}