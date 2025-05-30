﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Utilities
{
    public static class FormationsUtils
    {
        public static int GetGridRows(int count)
        {
            return Mathf.CeilToInt(Mathf.Sqrt(count));
        }

        public static int GetHordeRows(int count)
        {
            var gridRows = GetGridRows(count);
            
            if(gridRows % 2 == 0)
            {
                gridRows++;
            }
            
            return gridRows;
        }

        public static float GetArcTheta(int agents)
        {
            return Mathf.PI / (agents * 2);
        }

        public static float GetCircleTheta(int agents)
        {
            return 2 * Mathf.PI / agents;
        }
        
        public static Vector3 GetGridTargetPosition(Transform around, int index, float zLookAhead, int rows, Vector2 separation)
        {
            var row = index % rows;
            var column = index / rows;
            
            return around.TransformPoint(separation.x * column, 0, -separation.y * row + zLookAhead);
        }
        
        public static Vector3 GetGridTargetPosition(Vector3 around, Quaternion rotation, int index, float zLookAhead, int rows, Vector2 separation, float randomSpreadMax = 0f)
        {
            var row = index % rows;
            var column = index / rows;

            return Transform(around,
                new Vector3(separation.x * column + Random.Range(-randomSpreadMax, randomSpreadMax), -separation.y * row + zLookAhead + Random.Range(-randomSpreadMax, randomSpreadMax), 0),
                rotation,
                new Vector3(1, 1));
        }
        
        public static Vector3 GetRandomPackTargetPosition(Vector3 around, Quaternion rotation, int index, float zLookAhead, int rows, Vector2 separation)
        {
            throw new NotImplementedException();
        }
        
        public static Vector3 GetHordeTargetPosition(Vector3 around, Quaternion rotation, int index, float zLookAhead, int rows, Vector2 separation, float shift = 0.5f, float randomSpreadMax = 0f)
        {
            var row = index % rows;
            var column = index / rows;
            
            var shiftBy = row % 2 == 0 ? 0 : shift;

            return Transform(around,
                new Vector3(separation.x * column + shiftBy + Random.Range(-randomSpreadMax, randomSpreadMax), -separation.y * row + zLookAhead + Random.Range(-randomSpreadMax, randomSpreadMax), 0),
                rotation,
                new Vector3(1, 1));
        }

        public static Vector3 GetArcPosition(Vector3 around, Quaternion rotation, int index, float theta, float radius, float zLookAhead, bool concave = true)
        {
            var radians = theta * (((index - 1) / 2) + 1) + (concave ? 0 : Mathf.PI);
            var v = new Vector3(radius * Mathf.Sin(radians) * (index % 2 == 0 ? -1 : 1),
                radius * Mathf.Cos(radians) + radius * (concave ? -1 : 1) + zLookAhead, 0);
            
            return Transform(around, v, rotation, new Vector3(1, 1));
        }

        public static Vector3 GetCirclePosition(Vector3 around, Quaternion rotation, int index, float theta,
            float radius, float zLookAhead)
        {
            var v = new Vector3(radius * Mathf.Sin(theta * index),
                radius * Mathf.Cos(theta * index) - radius + zLookAhead, 0);
            return Transform(around, v, rotation, new Vector3(1, 1));
        }

        public static Vector3 GetLinePosition(Vector3 around, Quaternion rotation, int index, float separation, float zLookAhead, bool right = true)
        {
            var v = new Vector3(separation * index * (right ? 1 : -1), zLookAhead);
            return Transform(around, v, rotation, new Vector3(1, 1));
        }

        public static Vector3 Transform(Vector3 position, Vector3 transformVector, Quaternion rotation, Vector3 scale)
        {
            return rotation * Vector3.Scale(transformVector, scale) + position;
        }
    }
}