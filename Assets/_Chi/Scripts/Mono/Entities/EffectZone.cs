using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Utilities;
using DamageNumbersPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Entities
{
    public class EffectZone : MonoBehaviour
    {
        public List<ImmediateEffect> effects;
        
        public List<EntityStatsEffect> statsEffects;

        public int statsEffectLevel = 1;

        public float effectInterval;

        public Teams team;

        public float effectStrength = 1;

        public float loseInterestAfterDist2 = 4;
        
        public float despawnDelay;

        public float firstEffectAfterSpawnDelay;

        private float applySinceTime;

        private Collider2D collider;
        private Dictionary<Entity, float> entitiesInside;
        private HashSet<Entity> statsEffectsApplied;
        
        List<Entity> toRemove;
        List<Entity> toUpdate;

        private void Awake()
        {
            collider = GetComponent<Collider2D>();
            statsEffectsApplied = new();
            
            toRemove = new List<Entity>();
            toUpdate = new List<Entity>();
        }

        void Start()
        {
            entitiesInside = new();
            
            StartCoroutine(Despawn());
            StartCoroutine(Updater());
            
            applySinceTime = Time.time + firstEffectAfterSpawnDelay;
        }

        public void OnDestroy()
        {
            foreach (var kp in entitiesInside)
            {
                var entity = kp.Key;
                if (entity != null)
                {
                    foreach (var effect in statsEffects)
                    {
                        if (statsEffectsApplied.Contains(entity))
                        {
                            effect.Remove(entity, this);
                            statsEffectsApplied.Remove(entity);
                        }
                    }
                }
            }
        }

        private void UpdaterApply(Player player)
        {
            if (Time.time < applySinceTime)
            {
                return;
            }
            
            var position = transform.position;
            foreach (var kp in entitiesInside)
            {
                var entity = kp.Key;
                if (entity == null)
                {
                    toRemove.Add(entity);
                    continue;
                }
                if (kp.Value < Time.time)
                {
                    if ((team == Teams.Monster && entity is Player) 
                        || team == Teams.Player && entity is Npc npc && npc.AreEnemies(player)
                        || team == Teams.Neutral)
                    {
                        if (IsInsideEffectArea(entity))
                        {
                            Debug.Log("Applied to " + entity.name + "");
                            foreach (var effect in effects)
                            {
                                effect.Apply(entity, null, null, null, effectStrength);
                            }
                            
                            foreach (var effect in statsEffects)
                            {
                                if (!statsEffectsApplied.Contains(entity))
                                {
                                    effect.Apply(entity, this, statsEffectLevel);
                                    statsEffectsApplied.Add(entity);
                                }
                            }
                        }
                        else
                        {
                            foreach (var effect in statsEffects)
                            {
                                if (statsEffectsApplied.Contains(entity))
                                {
                                    effect.Remove(entity, this);
                                    statsEffectsApplied.Remove(entity);
                                }
                            }
                        }
                    }
                    
                    toUpdate.Add(entity);
                }
                else
                {
                    var dist2 = Utils.Dist2(entity.GetPosition(), position);
                    if (dist2 > loseInterestAfterDist2)
                    {
                        toRemove.Add(entity);
                    }
                }
            }
            
            foreach (var entity in toRemove)
            {
                entitiesInside.Remove(entity);
            }
            
            foreach (var entity in toUpdate)
            {
                entitiesInside[entity] = Time.time + effectInterval;
            }
            
            toUpdate.Clear();
            toRemove.Clear();
        }

        private IEnumerator Updater()
        {
            yield return new WaitForSeconds(Random.Range(0, 0.2f));

            var waiter = new WaitForSeconds(0.10f);
            while (this != null)
            {
                UpdaterApply(Gamesystem.instance.objects.currentPlayer);
                
                yield return waiter;
            }
        }

        private bool IsInsideEffectArea(Entity entity)
        {
            return collider.OverlapPoint(entity.GetPosition());
        }

        private IEnumerator Despawn()
        {
            if (despawnDelay > 0)
            {
                yield return new WaitForSeconds(despawnDelay);
                
                Destroy(this.gameObject);
            }
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            if (col != null && col.gameObject != null)
            {
                var entity = col.gameObject.GetEntity();

                if (entity != null && !entitiesInside.ContainsKey(entity))
                {
                    entitiesInside.Add(entity, 0f);
                    
                    UpdaterApply(Gamesystem.instance.objects.currentPlayer);
                }
            }
        }
    }
}