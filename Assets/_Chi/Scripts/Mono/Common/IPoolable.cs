using UnityEngine;

namespace _Chi.Scripts.Mono.Common
{
    public interface IPoolable
    {
        public void Reset();

        public void Setup(GameObject prefab);

        public void Run();

        public void MoveTo(Vector3 position);

        public void Rotate(Quaternion rotation);

        public void Destroy();
    }
}