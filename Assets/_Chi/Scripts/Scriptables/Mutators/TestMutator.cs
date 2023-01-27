using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.Mutators
{
    [CreateAssetMenu(fileName = "Test", menuName = "Gama/Mutators/Test")]
    public class TestMutator : Mutator
    {
        public override void ApplyToPlayer(Player player)
        {
            Debug.Log("applying mutator");
        }

        public override void RemoveFromPlayer(Player player)
        {
            Debug.Log("removing mutator");
        }
    }
}