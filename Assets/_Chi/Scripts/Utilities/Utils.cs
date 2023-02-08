using UnityEngine;

namespace _Chi.Scripts.Utilities
{
    public static class Utils
    {
        public static Vector3 GetMousePosition()
        {
            var v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(v.x, v.y);
        }
        
        /// <summary>
        /// distance between x and y. 
        /// - faster - vrati druhou mocninu
        /// </summary>
        public static float Dist2(Vector3 x, Vector3 y)
        {
            return (x - y).sqrMagnitude;
        }
        
        public static Vector3 GetHeading(Transform transform)
        {
            float angleRad = (transform.rotation.eulerAngles.z + 90) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
        }

        /// <summary>
        /// vypocita presnout vzdalenost (vyuziva odmocninu, tato metoda je vyrazne pomalejsi nez dist2)
        /// </summary>
        public static float Dist1(Vector3 x, Vector3 y)
        {
            return (x - y).magnitude;
        }
        
        public static int GetObjectsAtPosition(Vector3 pos, Collider2D[] buffer, float radius, int layerMask)
        {
            return Physics2D.OverlapCircleNonAlloc(pos, radius, buffer, layerMask);
        }

        public static Quaternion GetRotationTowards(Vector3 pos, Vector3 target)
        {
            Quaternion newRotation = Quaternion.LookRotation(pos - target, Vector3.forward);
            newRotation.x = 0;
            newRotation.y = 0;
            return newRotation;
        }
        
        public static Vector3 GenerateRandomPositionAround(Vector3 pos, float maxRange, float minRange)
        {
            //LH TODO rework

            int limit = 6;
            while (--limit > 0)
            {
                float randX = Random.Range(minRange, maxRange);
                float randY = Random.Range(minRange, maxRange);

                if (Random.Range(0, 2) == 0)
                    randX *= -1;

                if (Random.Range(0, 2) == 0)
                    randY *= -1;

                Vector3 v = new Vector3(pos.x + randX, pos.y +randY, 0);

                //TODO check if position is not within walls or smth? 

                return v;
            }

            return pos;
        }
        
        public static float AngleToTarget(Quaternion fromRotation, Vector3 from, Vector3 target)
        {
            Quaternion newRotation = Quaternion.LookRotation(from - target, Vector3.forward);
            newRotation.x = 0;
            newRotation.y = 0;

            return Utils.RotationAngleDiff(fromRotation, newRotation);
        }
        
        public static float RotationAngleDiff(Quaternion a, Quaternion b)
        {
            float angleA = a.eulerAngles.z;
            float angleB = b.eulerAngles.z;
            return Mathf.DeltaAngle(angleA, angleB);
        }

        public static void RemoveAllChildren(this Transform transform)
        {
            foreach (Transform c in transform) 
            {
                Object.Destroy(c.gameObject);
            }
        }
    }
}