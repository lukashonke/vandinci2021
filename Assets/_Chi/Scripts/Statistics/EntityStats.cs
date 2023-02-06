using System;

namespace _Chi.Scripts.Statistics
{
    /// <summary>
    /// Contains basic stats shared by all monsters, structures, players, etc
    /// Must be optimised and memory-light
    /// </summary>
    [Serializable]
    public class EntityStats
    {
        // not using Stat because this class will exist in many thousands instances
        
        public float maxHp = 10;
        public float maxHpAdd = 0;
        public float maxHpMul = 1;
        
        public float hp = 10;

        public void CopyFrom(EntityStats prefab)
        {
            this.maxHp = prefab.maxHp;
            this.maxHpAdd = prefab.maxHpAdd;
            this.maxHpMul = prefab.maxHpMul;
            this.hp = prefab.hp;
        }
    }
}