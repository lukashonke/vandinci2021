using System.Collections;
using _Chi.Scripts.Mono.Common;
using UnityEngine;

namespace _Chi.Scripts.Mono.Misc
{
    public class Poolable : MonoBehaviour, IPoolable
    {
        public void Reset()
        {
            gameObject.SetActive(false);
        }

        public void Setup(GameObject prefab)
        {
            gameObject.SetActive(true);
        }

        public void Run()
        {
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

        public void Finish()
        {
            gameObject.SetActive(false);
        }
    }
}