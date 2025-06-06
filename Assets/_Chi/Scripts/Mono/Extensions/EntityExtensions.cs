﻿using System;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Extensions
{
    public static class EntityExtensions
    {
        public static Npc SpawnPooledNpc(this Npc prefab, Vector3 position, Quaternion rotation)
        {
            var npc = Gamesystem.instance.poolSystem.Spawn(prefab);

            npc.Setup(position, rotation);

            return npc;
        }

        public static bool DeletePooledNpc(this Npc instance)
        {
            return Gamesystem.instance.poolSystem.Despawn(instance);
        }
        
        public static void Heal(this Entity e)
        {
            e.entityStats.hp = e.GetMaxHp();
            if (e.GetHp() > 0)
            {
                e.isAlive = true;
            }
        }
        
        public static void Heal(this Entity e, float amount)
        {
            e.entityStats.hp += amount;

            if (e.entityStats.hp > e.GetMaxHp())
            {
                e.entityStats.hp = e.GetMaxHp();
            }
            
            if (e.GetHp() > 0)
            {
                e.isAlive = true;
            }
        }
        
        public static Entity GetEntity(this GameObject obj)
        {
            return obj.GetComponent<Entity>();
        }
        
        public static Module GetModule(this GameObject obj)
        {
            if (obj.CompareTag(Tags.LookAtParent))
            {
                return obj.transform.parent.GetComponent<Module>();                
            }

            return obj.GetComponent<Module>();
        }
        
        public static bool AreEnemies(this Entity e, Entity other)
        {
            return e.team != other.team;
        }

        public static int GetNearest(this Entity source, Vector3 from, float range, TargetType targetType, Collider2D[] buffer)
        {
            var count = Utils.GetObjectsAtPosition(from, buffer, range, GetLayerMask(source, targetType));

            return count;
        }

        public static Entity GetNearestEnemy(this Entity source, Vector3 from, float maxRange, TargetType targetType, Collider2D[] buffer)
        {
            var count = GetNearest(source, from, maxRange, targetType, buffer);

            Entity nearest = null;
            float nearestDistance = float.MaxValue;
            
            for (int i = 0; i < count; i++)
            {
                var col = buffer[i];

                var entity = col.gameObject.GetEntity();
                if (entity is Npc npc && npc.activated && npc.AreEnemies(source) && npc != null)
                {
                    var dist = Utils.Dist2(npc.GetPosition(), from);
                    if (dist < nearestDistance)
                    {
                        nearest = entity;
                        nearestDistance = dist;
                    }
                }
            }

            return nearest;
        }
        
        public static Entity GetFirstEnemy(this Player player, Vector3 from, Func<Entity, bool> condition)
        {
            Entity target = null;

            foreach (var entity in player.targetableEnemies)
            {
                if (entity is Npc npc && npc.activated && npc.AreEnemies(player) && npc != null)
                {
                    //var dist = Utils.Dist2(npc.GetPosition(), from);
                    if (condition == null || condition(npc))
                    {
                        target = entity;
                        break;
                    }
                }
            }

            return target;
        }

        public static Entity GetNearestEnemy(this Player player, Vector3 from, Func<Entity, bool> condition)
        {
            Entity nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var entity in player.targetableEnemies)
            {
                if (entity is Npc npc && npc != null && npc.activated && npc.AreEnemies(player))
                {
                    var dist = Utils.Dist2(npc.GetPosition(), from);
                    if (dist < nearestDistance && (condition == null || condition(npc)))
                    {
                        nearest = entity;
                        nearestDistance = dist;
                    }
                }
            }

            return nearest;
        }
        
        public static Entity GetRandomEnemy(this Player player, Vector3 from, Func<Entity, bool> condition, float maxDist2)
        {
            var possibleEnemies = ListPool<Entity>.Get();

            try
            {
                foreach (var entity in player.targetableEnemies)
                {
                    if (entity is Npc npc && npc != null && npc.activated && npc.AreEnemies(player))
                    {
                        var dist = Utils.Dist2(npc.GetPosition(), from);
                        if (dist < maxDist2 && (condition == null || condition(npc)))
                        {
                            possibleEnemies.Add(entity);
                        }
                    }
                }
            
                if (possibleEnemies.Count == 0)
                {
                    return null;
                }
                
                return possibleEnemies[Random.Range(0, possibleEnemies.Count)];
            }
            finally
            {
                ListPool<Entity>.Release(possibleEnemies);
            }
        }
        
        public static int GetLayerMask(this Entity source, TargetType type)
        {
            int layerMask;
            if (type == TargetType.EnemyOnly)
            {
                if (source.team == Teams.Monster)
                {
                    layerMask = 1 << Layers.playersLayer;
                }
                else
                {
                    layerMask = 1 << Layers.enemiesLayer;
                }
            }
            else if (type == TargetType.FriendlyOnly)
            {
                if (source.team == Teams.Monster)
                {
                    layerMask = 1 << Layers.enemiesLayer;
                }
                else
                {
                    layerMask = 1 << Layers.playersLayer;
                }
            }
            else 
            {
                layerMask = 1 << Layers.enemiesLayer | 1 << Layers.playersLayer;
            }

            return layerMask;
        }

        public static Quaternion GetRotationTo(Vector3 from, Vector3 rotationTarget)
        {
            Quaternion newRotation = Quaternion.LookRotation(from - rotationTarget, Vector3.forward);
            newRotation.x = 0;
            newRotation.y = 0;

            return newRotation;
        }
        
        public static Quaternion RotateTowards(Vector3 from, Vector3 rotationTarget, Quaternion rotation, float rotationSpeed)
        {
            Quaternion newRotation = Quaternion.LookRotation(from - rotationTarget, Vector3.forward);
            newRotation.x = 0;
            newRotation.y = 0;

            var nextRotation = Quaternion.RotateTowards(rotation, newRotation, rotationSpeed * Time.fixedDeltaTime);
            return nextRotation;
        }
    }
}