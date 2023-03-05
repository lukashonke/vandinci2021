using System.Collections;
using UnityEngine;

namespace _Chi.Scripts.Mono.Modules.Offensive.Subs
{
    public class EmitByMovement : SubEmitter
    {
        private Vector3 lastPos;

        public bool randomRotation;
        
        public AnimationCurve projectilesPerSecondBySpeedCurve;

        public override IEnumerator UpdateCoroutine()
        {
            Vector3 lastPosition = transform.position;
            var waiter = new WaitForFixedUpdate();
            var lastShoot = Time.time;
            
            while (true)
            {
                if (!isEnabled)
                {
                    yield return waiter;
                }

                var currentPosition = transform.position;
                
                var velocity = (currentPosition - lastPosition).magnitude / Time.fixedDeltaTime;
                if (velocity > 0)
                {
                    Debug.Log(velocity);
                }
                
                if(lastShoot + GetFireRate(velocity) < Time.time)
                {
                    if (randomRotation)
                    {
                        RotateRandomly();
                    }
                    PlayEmitter();
                    lastShoot = Time.time;
                }
                
                lastPosition = currentPosition;

                yield return waiter;
            }
        }

        public float GetFireRate(float velocity)
        {
            return 1 / projectilesPerSecondBySpeedCurve.Evaluate(velocity);
        }
    }
}