namespace _Chi.Scripts.Mono.Common
{
    public interface IPooledGameobject
    {
        void OnReturnedToPool();

        void OnTakeFromPool();
    }
}