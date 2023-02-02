using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Scriptables;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace _Chi.Scripts.Utilities
{
    public static class MapGenUtils
    {
        public static List<GameObject> ProcessTilemap(Tilemap tilemap, int? seed=null)
        {
            Random random;
            if (seed != null)
            {
                random = new Random(seed.Value);
            }
            else
            {
                random = new Random();
            }

            List<GameObject> spawnedList = new List<GameObject>();

            var parentGo = Gamesystem.instance.worldGenerated;

            foreach (var tileSettings in Gamesystem.instance.mapGenSettings.settings)
            {
                ILookup<Vector2Int, MapGenReplaceSettingsItem> tilePrefabSettingsLookup = tileSettings.items.ToLookup(t => t.rectSize);

                var containsRectItems = tileSettings.ContainsRectItems();
                
                List<Vector2Int> pointsToCalculateRectFrom = new List<Vector2Int>();
                
                foreach (var tilePosInt in tilemap.cellBounds.allPositionsWithin)
                {   
                    var tile = tilemap.GetTile(tilePosInt);
                    if (tile != null)
                    {
                        if (tileSettings.tile == tile)
                        {
                            if (containsRectItems)
                            {
                                pointsToCalculateRectFrom.Add(new Vector2Int(tilePosInt.x, tilePosInt.y));
                            }
                            else
                            {
                                var spawnPos = TilemapUtils.TileCenter(tilePosInt, tilemap);
                                
                                var prefabs = tilePrefabSettingsLookup[Vector2Int.one].OrderBy(p => p.chance).ToList();

                                var selectedPrefab = SelectPrefabFromCompoundChance(prefabs, random);
                                
                                spawnPos += new Vector3(random.Next(-selectedPrefab.randomPosition.x, selectedPrefab.randomPosition.x), random.Next(-selectedPrefab.randomPosition.y, selectedPrefab.randomPosition.y), 0);
                                
                                var spawned = Object.Instantiate(selectedPrefab.prefab, spawnPos, Quaternion.identity, parentGo.transform);

                                if (selectedPrefab.sizeVariation > 0)
                                {
                                    var rnd = UnityEngine.Random.Range(-selectedPrefab.sizeVariation, selectedPrefab.sizeVariation);
                                    spawned.transform.localScale = new Vector3(1 + rnd, 1 + rnd, 1);
                                }
                                
                                spawnedList.Add(spawned);
                            }
                        }
                    }
                }

                if (pointsToCalculateRectFrom.Any())
                {
                    var prefabs = tilePrefabSettingsLookup.SelectMany(l => l).OrderBy(p => p.chance).ToList();
                    
                    var rectsWithPrefabItems = CreateRects(prefabs, pointsToCalculateRectFrom, random);

                    foreach (var rectWithPrefabItem in rectsWithPrefabItems)
                    {
                        //var spawnPosInt = rectWithPrefabItem.rect.position; // -23, 15
                        var center = rectWithPrefabItem.rect.center;
                        
                        //var spawnPos = TilemapUtils.TileCenter(new Vector3Int(spawnPosInt.x, spawnPosInt.y, 0), tilemap);
                        //spawnPos = tilemap.CellToWorld(new Vector3Int(spawnPosInt.x, spawnPosInt.y, 0));
                        var spawnPos = tilemap.LocalToWorld(center);
                        
                        spawnPos += new Vector3(random.Next(-rectWithPrefabItem.item.randomPosition.x, rectWithPrefabItem.item.randomPosition.x), random.Next(-rectWithPrefabItem.item.randomPosition.y, rectWithPrefabItem.item.randomPosition.y), 0);
                        
                        var spawned = Object.Instantiate(rectWithPrefabItem.item.prefab, spawnPos, Quaternion.identity, parentGo.transform);
                        
                        if (rectWithPrefabItem.item.sizeVariation > 0)
                        {
                            var rnd = UnityEngine.Random.Range(-rectWithPrefabItem.item.sizeVariation, rectWithPrefabItem.item.sizeVariation);
                            spawned.transform.localScale = new Vector3(1 + rnd, 1 + rnd, 1);
                        }
                        
                        spawnedList.Add(spawned);
                    }
                }
            }

            return spawnedList;
        }

        public static List<RectWithPrefabItem> CreateRects(List<MapGenReplaceSettingsItem> prefabs, List<Vector2Int> pointsList, Random random)
        {
            if (!prefabs.Any()) return null;
            
            var rectsList = new List<RectWithPrefabItem>();
            
            var points = new Stack<Vector2Int>(pointsList
                .OrderByDescending(p => TilemapUtils.TileCenter(new Vector3Int(p.x, p.y, 0), Gamesystem.instance.mapGenTilemap).x)
                .ThenByDescending(p => TilemapUtils.TileCenter(new Vector3Int(p.x, p.y, 0), Gamesystem.instance.mapGenTilemap).y));

            HashSet<Vector2Int> usedUp = new HashSet<Vector2Int>();

            var maxPrio = prefabs.Max(p => p.priority);
            var prefabsThisIteration = prefabs.Where(p => p.priority == maxPrio);
            prefabs = prefabs.Except(prefabsThisIteration).ToList();
            
            var pointsSet = pointsList.ToHashSet();

            List<Vector2Int> remainingPoints = new List<Vector2Int>();

            while (points.Count > 0)
            {
                List<MapGenReplaceSettingsItem> selectable = new List<MapGenReplaceSettingsItem>(prefabsThisIteration);
                    
                var point = points.Pop();
                if(usedUp.Contains(point)) continue;

                while (true)
                {
                    if (!selectable.Any())
                    {
                        remainingPoints.Add(point);
                        break;
                    }
                    
                    MapGenReplaceSettingsItem selectedPrefab = SelectPrefabFromCompoundChance(selectable, random);

                    if (selectedPrefab == null)
                    {
                        remainingPoints.Add(point);
                        break;
                    }

                    var rectWidth = selectedPrefab.rectSize.x;
                    var rectHeight = selectedPrefab.rectSize.y;


                    var x = point.x;
                    var y = point.y;
                
                    var rect = new RectInt(x, y, rectWidth, rectHeight);

                    var validRect = true;
                
                    foreach (var rectPoint in rect.allPositionsWithin)
                    {
                        if (usedUp.Contains(rectPoint) || !pointsSet.Contains(rectPoint))
                        {
                            validRect = false;
                            break;
                        }
                    }
                
                    if (!validRect)
                    {
                        // this rect cannot be placed on this point, but try again with another rect
                        selectable.Remove(selectedPrefab);
                        continue;
                    }

                    pointsSet.Remove(point);
                        
                    foreach (var rectPoint in rect.allPositionsWithin)
                    {
                        usedUp.Add(rectPoint);
                    }
                
                    rectsList.Add(new RectWithPrefabItem()
                    {
                        item = selectedPrefab,
                        rect = rect
                    });
                        
                    break;
                }
                
                //TODO support overlap
            }

            if (remainingPoints.Any())
            {
                var newRects = CreateRects(prefabs, remainingPoints, random);
                if (newRects != null)
                {
                    rectsList.AddRange(newRects);
                }
            }

            return rectsList;
        }

        public class RectWithPrefabItem
        {
            public RectInt rect;
            public MapGenReplaceSettingsItem item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefabs">must be ordered by chance, asc</param>
        /// <param name="random"></param>
        /// <param name="prefabsToIgnore">list of prefabs that will never be selected</param>
        /// <returns></returns>
        private static MapGenReplaceSettingsItem SelectPrefabFromCompoundChance(List<MapGenReplaceSettingsItem> prefabs, Random random, List<MapGenReplaceSettingsItem> prefabsToIgnore=null)
        {
            var maxChance = prefabs.Sum(p => p.chance); 
            int randomChance = random.Next(maxChance); 
            int currentChanceSkip = 0;

            IEnumerable<MapGenReplaceSettingsItem> prefabsEnum = prefabs;
            if (prefabsToIgnore != null)
            {
                prefabsEnum = prefabs.Except(prefabsToIgnore);
            }

            foreach (var prefab in prefabsEnum)
            {
                if (randomChance < prefab.chance + currentChanceSkip)
                {
                    return prefab;
                }

                currentChanceSkip += prefab.chance;
            }

            return null;
        }
    }
}