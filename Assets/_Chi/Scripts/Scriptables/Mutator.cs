using _Chi.Scripts.Mono.Entities;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Scriptables
{
    public abstract class Mutator : SerializedScriptableObject
    {
        public MutatorCategory category;

        public abstract void ApplyToPlayer(Player player);
        
        public abstract void RemoveFromPlayer(Player player);
    }

    public enum MutatorCategory
    {
        A,
        B,
        C,
        D,
        E
    }
}