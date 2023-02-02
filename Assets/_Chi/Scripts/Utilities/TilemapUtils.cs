using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace _Chi.Scripts.Utilities
{
    public class TilemapUtils
    {
        /// <summary>
        /// returns world position at a center of a tile in a tilemap
        /// </summary>
        public static Vector3 TileCenter(Vector3Int posInt, Tilemap tilemap)
        {
            var pos = tilemap.CellToWorld(posInt);

            var cellSize = tilemap.layoutGrid.cellSize / 2f;

            pos += cellSize;

            return pos;
        }
    }
}