using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Chi.Scripts.Mono.Misc
{
    public class DropFromAbove : MonoBehaviour, IPoolable
    {
        public float dist;

        public float duration;

        public float startRandomDurationMin;
        public float startRandomDurationMax;

        public Action actionWhenDropped;

        private TrailRenderer trailRenderer;

        public void Awake()
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }

        public void Reset()
        {
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
            gameObject.SetActive(false);
        }

        public void Setup(GameObject prefab)
        {
            gameObject.SetActive(true);
        }

        public void Run()
        {
            StartCoroutine(Drop());
        }

        public void MoveTo(Vector3 position)
        {
            transform.position = position;
        }

        public void Rotate(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }

        public IEnumerator Drop()
        {
            if (startRandomDurationMax > 0)
            {
                yield return new WaitForSeconds(Random.Range(startRandomDurationMin, startRandomDurationMax));
            }

            var transform1 = transform;
            transform1.position += Vector3.up * dist;

            var time = duration;
            var waiter = new WaitForFixedUpdate();
            
            while(time > 0)
            {
                yield return waiter;
                time -= Time.deltaTime;

                transform1.position += Vector3.down * dist / duration * Time.deltaTime;
            }

            actionWhenDropped?.Invoke();

            Finish();
        }

        public void Finish()
        {
            gameObject.SetActive(false);
        }
    }
}